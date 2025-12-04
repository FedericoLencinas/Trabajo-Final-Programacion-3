<template>
  <div class="edit-transaction-page">
    <div class="content-wrapper">
      
      <h1 class="title">✏️ Editar Transacción (ID: {{ transaccionId }})</h1>
      
      <div v-if="loading" class="info-message">Cargando datos de la transacción...</div>
      <div v-else-if="error" class="error-message">{{ error }}</div>
      
      <div v-else-if="formData.id" class="form-card">
        
        <form @submit.prevent="handleSubmit">

          <div class="input-group">
            <label for="crypto_code" class="label-field required">Criptomoneda</label>
            <select 
              id="crypto_code" 
              v-model="formData.crypto_code" 
              required 
              class="input-field select-field"
            >
              <option value="" disabled>-- Seleccione una moneda --</option>
              
              <option 
                v-for="moneda in monedas || []" 
                :key="moneda.abreviatura" 
                :value="moneda.abreviatura"
              >
                {{ moneda.nombre }} ({{ moneda.abreviatura.toUpperCase() }})
              </option>
            </select>
            <div v-if="monedas && !monedas.length && !loading" class="help-text error-text">
                Error: No se cargó la lista de monedas.
            </div>
          </div>

          <div class="input-group">
            <label class="label-field required">Acción</label>
            <div class="radio-group">
              <input type="radio" id="purchase" value="purchase" v-model="formData.action">
              <label for="purchase" :class="{'radio-checked': formData.action === 'purchase'}">COMPRA</label>
              
              <input type="radio" id="sale" value="sale" v-model="formData.action">
              <label for="sale" :class="{'radio-checked': formData.action === 'sale'}">VENTA</label>
            </div>
          </div>

          <div class="input-group">
            <label for="crypto_amount" class="label-field required">Cantidad de Criptomoneda</label>
            <input id="crypto_amount" v-model.number="formData.crypto_amount" type="number" step="any" required class="input-field">
          </div>
          
          <div class="input-group">
            <label class="label-field">Fecha de Registro</label>
            <input type="text" :value="formatDate(formData.datetime)" disabled class="input-field disabled-field">
          </div>

          <div class="input-group">
            <label class="label-field">Monto en ARS (Estimado)</label>
            <input type="text" :value="formatMoney(formData.money)" disabled class="input-field disabled-field">
            <p class="help-text">El monto en ARS se recalcula en el servidor al guardar.</p>
          </div>

          <button type="submit" :disabled="isSubmitting" class="btn-submit">
            {{ isSubmitting ? 'Guardando...' : 'Guardar Cambios' }}
          </button>
        </form>
      </div>

      <div class="flex-end-wrapper mt-6">
        <button @click="router.push({ name: 'listatransacciones' })" class="btn-secondary">
          Cancelar y Volver al Historial
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth'; 


const API_BASE = 'https://localhost:7273/api'; 
const API_URL_BASE = `${API_BASE}/Transaccion`; 
const API_MONEDAS = `${API_BASE}/Moneda`; 

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const transaccionId = ref(route.params.id);
const formData = ref({
    ClienteId: authStore.user?.id || 0 
});
const loading = ref(true);
const error = ref(null);
const isSubmitting = ref(false);
const monedas = ref([]); 

const formatMoney = (amount) => {
    return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(parseFloat(amount) || 0);
};

const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    if (isNaN(date.getTime())) { return 'Fecha Inválida'; }
    
    const formattedString = date.toLocaleString('es-AR', { 
        year: '2-digit', month: 'numeric', day: 'numeric', 
        hour: '2-digit', minute: '2-digit', hour12: false
    });
    return formattedString.replace(/(\s+hs)$/, '');
};

const fetchMonedas = async () => {
    try {
        const response = await axios.get(API_MONEDAS);
        monedas.value = response.data;
    } catch (err) {
        console.error("Error al cargar la lista de monedas:", err);
    }
}


const fetchTransactionData = async () => {
    if (!transaccionId.value) {
        error.value = "ID de transacción no encontrado en la ruta.";
        loading.value = false;
        return;
    }

    try {
        const response = await axios.get(`${API_URL_BASE}/${transaccionId.value}`);
        
        formData.value = { ...formData.value, ...response.data };

    } catch (err) {
        error.value = `Error al cargar la transacción: ${err.response?.statusText || 'Error de red'}`;
    } finally {
        loading.value = false;
    }
};

const handleSubmit = async () => {
    isSubmitting.value = true;
    error.value = null;

    try {
        if (!formData.value.crypto_code) {
             window.alert('Debe seleccionar una criptomoneda.');
             isSubmitting.value = false;
             return;
        }

        const payload = {
            id: formData.value.id,
            ClienteId: formData.value.ClienteId, 
            crypto_code: formData.value.crypto_code,
            action: formData.value.action,
            crypto_amount: formData.value.crypto_amount,
        };


        await axios.put(`${API_URL_BASE}/${formData.value.id}`, payload);

        window.alert('Transacción actualizada con éxito.');
        router.push({ name: 'listatransacciones' });

    } catch (err) {
        let message = 'Error desconocido al intentar actualizar la transacción.';
        
        if (err.response) {
            const status = err.response.status;
            const serverMessage = err.response.data?.message || err.response.data?.detail || 'Sin mensaje del servidor.';
            message = `Error ${status}: ${serverMessage}`;
        }
        
        window.alert(`Fallo en la actualización: ${message}`);
        error.value = message;
    } finally {
        isSubmitting.value = false;
    }
};

onMounted(() => {
    if (!authStore.isLoggedIn) {
        router.push({ name: 'login' });
        return;
    }
    fetchMonedas();
    fetchTransactionData();
});
</script>

<style scoped>
:root {
    --color-primary: #2563EB;
    --color-primary-dark: #1E40AF;
    --color-success: #059669;
    --color-danger: #EF4444;
    --color-background: #F9FAFB;
    --color-card-bg: #FFFFFF;
    --color-text-dark: #1F2937;
    --color-border: #D1D5DB;
    --color-gray-medium: #6B7280;
}

.edit-transaction-page {
    min-height: 100vh;
    padding: 1rem 1rem 1rem 1rem; 
}
.content-wrapper {
    max-width: 600px; 
    margin: 0 auto;
}
.title {
    margin-top : 100px;
    font-size: 2rem;
    font-weight: 700;
    margin-bottom: 2rem;
    text-align: center;
    color: var(--color-text-dark);
}
.form-card {
    background-color: rgb(14, 14, 88);
    padding: 10px;
    margin-top: 1px;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    border-top: 5px solid var(--color-primary);
}

.input-group {
    margin-top: 15px;
    margin-bottom: 5px;
}
.label-field {
    display: block;
    font-size: 0.9rem;
    font-weight: 600;
    color: var(--color-text-dark);
    margin-bottom: 0.5rem;
}
.required::after {
    content: '*';
    color: var(--color-danger);
    margin-left: 4px;
}
.input-field {
    width: 100%;
    padding: 0.75rem 1rem;
    border: 1px solid var(--color-border);
    border-radius: 8px;
    box-sizing: border-box;
    font-size: 1rem;
    transition: border-color 0.2s;
}
.input-field:focus {
    outline: none;
    border-color: var(--color-primary);
    box-shadow: 0 0 0 1px var(--color-primary);
}
.select-field {
    appearance: none; 
    background-repeat: no-repeat;
    background-position: right 1rem center;
    padding-right: 2.5rem;
}

.disabled-field {
    background-color: #E5E7EB;
    cursor: not-allowed;
    color: black;
}

.radio-group {
    display: flex;
    gap: 15px;
}
.radio-group input[type="radio"] {
    display: none;
}
.radio-group label {
    padding: 0.75rem 1.5rem;
    border: 2px solid var(--color-border);
    border-radius: 8px;
    cursor: pointer;
    font-weight: 600;
    transition: background-color 0.2s, color 0.2s, border-color 0.2s;
    user-select: none;
}
.radio-group input[type="radio"]:checked + label {
    border-color: var(--color-primary);
    background-color: var(--color-primary);
    color: white;
}

.btn-submit {
    width: 100%;
    background-color: rgb(27, 202, 27);
    color: white;
    font-weight: 700;
    padding: 0.8rem;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: background-color 0.2s;
    margin-top: 1rem;
}
.btn-submit:hover:not(:disabled) {
    background-color: #047821;
}
.btn-submit:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}
.btn-secondary {
 background-color: #2563EB; 
    border: 1px solid var(--color-primary);
    color: white;
    padding: 10px 18px;
    border-radius: 8px; 
    font-size: 0.9rem;
    font-weight: 600;
    cursor: pointer;
    transition: background-color 0.2s, color 0.2s;
    text-decoration: none;
}
.btn-secondary:hover {
        background-color: #082470; 

}

.info-message, .error-message {
    padding: 1rem;
    border-radius: 8px;
    text-align: center;
    margin: 1.5rem 0;
}
.info-message { background-color: #DBEAFE; color: var(--color-primary); }
.error-message { background-color: #FEF2F2; color: var(--color-danger); }
.help-text {
    font-size: 0.75rem;
    color: var(--color-gray-medium);
    margin-top: 5px;
}
.flex-end-wrapper {
    display: flex;
    justify-content: flex-end;
    margin-top: 20px;
}
.error-text {
    color: var(--color-danger);
    font-weight: 600;
}
</style>
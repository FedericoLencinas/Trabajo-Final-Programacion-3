<template>
  <div class="simple-page">
    <div class="form-wrapper">

      <h1 class="title">💰 Registrar Transacción</h1>
      <form @submit.prevent="handleSubmit" class="simple-form">
        
        <p v-if="!authStore.isLoggedIn" class="error-text mb-4">
          Error: Debes iniciar sesión.
        </p>

        <div class="form-group">
          <label for="action" class="label">Tipo:</label>
          <select 
            id="action" 
            v-model="transaccion.action" 
            required 
            class="input-field"
            :class="{'input-compra': transaccion.action === 'purchase', 'input-venta': transaccion.action === 'sale'}"
          >
            <option value="purchase">Compra</option>
            <option value="sale">Venta</option>
          </select>
        </div>
        
        <div class="form-group">
          <label for="crypto" class="label">Criptomoneda:</label>
          <select 
            id="crypto" 
            v-model="transaccion.crypto_code" 
            required 
            class="input-field"
          >
            <option disabled value="">Cargando criptomonedas...</option>
            <option 
              v-for="moneda in monedas" 
              :key="moneda.abreviatura" 
              :value="moneda.abreviatura.toLowerCase()"
            >
              {{ moneda.nombre }} ({{ moneda.abreviatura.toUpperCase() }})
            </option>
          </select>
        </div>

        <div class="form-group">
          <label for="cantidad" class="label">Cantidad:</label>
          <input 
            type="number" 
            v-model.number="transaccion.crypto_amount" 
            min="0.0001" 
            step="0.0001" 
            required
            class="input-field"
          >
        </div>

        <button 
          type="submit" 
          :disabled="loading"
          class="btn-primary"
        >
          {{ loading ? 'Registrando...' : 'Registrar' }}
        </button>

        <p v-if="successMsg" class="message-success mt-4">{{ successMsg }}</p>
        <p v-if="errorMsg" class="message-error mt-4">{{ errorMsg }}</p>

      </form>

      <h2 class="cotizaciones-title">Cotizaciones en Vivo (ARS)</h2>
      <div class="cotizaciones-grid">
        <div v-for="(price, code) in currentPrices" :key="code" class="cotizacion-card">
          <p class="crypto-code">{{ code.toUpperCase() }}</p>
          <p class="crypto-price">{{ formatMoney(price) }}</p>
        </div>
        <div v-if="Object.keys(currentPrices).length === 0" class="loading-cotizaciones">
          Cargando precios...
        </div>
      </div>
      <button @click="router.push({ name: 'home' })" class="btn-menu-principal-fixed">
          Volver al Menú Principal
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted } from 'vue';
import axios from 'axios';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

const router = useRouter();

const API_BASE = 'https://localhost:7273/api'; 
const API_TRANSACCION = `${API_BASE}/Transaccion`;
const API_MONEDA = `${API_BASE}/Moneda`; 
const CRYPTOYA_URL = 'https://criptoya.com/api/satoshitango'; 

const CRYPTO_LIST = ['btc', 'eth', 'usdc'];

const authStore = useAuthStore();

const monedas = ref([]);
const currentPrices = ref({});
let priceIntervalId = null; 

const loading = ref(false);
const loadingMonedas = ref(true);
const errorMsg = ref('');
const successMsg = ref('');

const transaccion = reactive({
  action: 'purchase', 
  crypto_code: '',
  crypto_amount: 0,
  ClienteId: authStore.clienteId || 0 
});

const formatMoney = (amount) => {
    return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(amount);
};

const fetchMonedas = async () => {
    try {
        const response = await axios.get(API_MONEDA);
        monedas.value = response.data;
        if (monedas.value.length > 0) {
            transaccion.crypto_code = monedas.value[0].abreviatura.toLowerCase();
        }
    } catch (error) {
        errorMsg.value = 'Error al cargar monedas. Verifique la URL de la API Moneda.';
    } finally {
        loadingMonedas.value = false;
    }
};

const fetchCurrentPrices = async () => {
    try {
        const newPrices = {};
        for (const code of CRYPTO_LIST) {
            const response = await axios.get(`${CRYPTOYA_URL}/${code}/ars`);
            
            const askPrice = response.data.ask; 
            if (askPrice) {
                newPrices[code] = askPrice;
            }
        }
        currentPrices.value = newPrices;

    } catch (error) {
        console.error('Fallo al obtener cotizaciones en vivo:', error);
    }
};

const handleSubmit = async () => {
  
  if (!authStore.isLoggedIn || !transaccion.ClienteId) {
      errorMsg.value = 'Debes iniciar sesión para registrar una transacción.';
      return;
  }

  errorMsg.value = '';
  successMsg.value = '';
  loading.value = true;

  if (transaccion.crypto_amount <= 0) {
    errorMsg.value = 'La cantidad debe ser mayor a cero.';
    loading.value = false;
    return;
  }
  
  try {
    const payload = {
      action: transaccion.action,
      crypto_code: transaccion.crypto_code,
      crypto_amount: transaccion.crypto_amount,
      ClienteId: transaccion.ClienteId,
    };

    const response = await axios.post(API_TRANSACCION, payload);
    
    successMsg.value = `Transacción realizada con éxito`;
    transaccion.crypto_amount = 0;

  } catch (error) {
    console.error('Error:', error.response);
    errorMsg.value = `Transacción fallida`;  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchMonedas();
  fetchCurrentPrices();
  priceIntervalId = setInterval(fetchCurrentPrices, 15000);
});
onUnmounted(() => {
    if (priceIntervalId) {
        clearInterval(priceIntervalId);
    }
});
</script>

<style scoped>
:root {
    --color-primary: #2563EB; 
    --color-success: #059669; 
    --color-danger: #DC2626;  
    --color-gray-dark: #374151;
}

.simple-page {
    min-height: 100vh;
    padding: 30px 20px;
    display: flex;
    justify-content: center;
    align-items: flex-start; 
}

.form-wrapper {
    max-width: 600px;
    width: 100%;
}
.btn-menu-principal-fixed {
    position: fixed; 
    bottom: 20px; 
    right: 20px; 
    z-index: 900;     
    background-color: #2563EB; 
    border: 1px solid var(--color-primary);
    color: white;
    padding: 10px 18px;
    border-radius: 6px; 
    font-size: 0.9rem;
    cursor: pointer;
    text-decoration: none;
    font-weight: 600;
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
    transition: background-color 0.2s, color 0.2s;
}
.btn-menu-principal-fixed:hover {
    background-color: #082470; 
}

.title {
    font-size: 24px;
    font-weight: 700;
    margin-bottom: 20px;
    text-align: center;
    color: #8b929c; 
}

.cotizaciones-title {
    font-size: 18px;
    font-weight: 600;
    margin-top: 5px;
    margin-bottom: 5px;
    color: var(--color-gray-dark);
}

.simple-form {
    background-color: #FFFFFF;
    padding: 25px;
    border-radius: 8px;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1); 
    border-top: 5px solid var(--color-primary); 
}

.form-group {
    margin-bottom: 15px;
}

.label {
    display: block;
    margin-bottom: 5px;
    font-weight: 500;
    color: black;
}

.input-field, .input-select {
    width: 100%;
    padding: 10px;
    border: 1px solid #000000;
    border-radius: 5px;
    box-sizing: border-box; 
}

.input-field:focus, .input-select:focus {
    border-color: var(--color-primary);
    box-shadow: 0 0 0 1px var(--color-primary);
    outline: none;
}

.btn-primary {
   width: 100%;
 margin-top: 10px;
 background-color: #2563EB; 
 color: #FFFFFF;
 font-weight: 700;
 padding: 0.75rem 1rem;
 border: none;
 border-radius: 0.5rem;
 box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
 transition: background-color 150ms ease-in-out;
letter-spacing: 0.025em;
cursor: pointer;
}

.btn-primary:hover {
  background-color: #082470; 
}

.btn-primary:focus {
outline: none; 
box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.5); 
}

.btn-primary:disabled {
 opacity: 0.5;
 cursor: not-allowed;
}

.input-compra { border-color: var(--color-success); }
.input-venta { border-color: var(--color-danger); }

.bg-success { background-color: var(--color-success); }
.hover-bg-success-dark:hover { background-color: #047857; }

.bg-danger { background-color: var(--color-danger); }
.hover-bg-danger-dark:hover { background-color: #B91C1C; }

.message-error {
    color: black;
    font-size: 0.9rem;
    padding: 10px;
    background-color: #FEF2F2;
    border-radius: 0.25rem;
    border: 1px solid #e72f2f;
    text-align: center;
}

.message-success {
    color: black;
    font-size: 0.9rem;
    padding: 10px;
    background-color: #D1FAE5;
    border-radius: 0.25rem;
    border: 1px solid #08b173;
    text-align: center;
}

.cotizaciones-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(130px, 1fr));
    gap: 15px;
    margin-top : 0px;
}

.cotizacion-card {
    background: linear-gradient(#ffffff, #aeffb683);
    padding: 15px;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    border-left: 4px solid var(--color-primary);
    text-align: center;
}

.crypto-code {
    font-size: 1rem;
    font-weight: 600;
    color: rgb(12, 19, 75);
    margin-bottom: 5px;
}

.crypto-price {
    font-size: 1.125rem;
    font-weight: 700;
    color: rgb(12, 19, 75);
}

.loading-cotizaciones {
    grid-column: 1 / -1;
    text-align: center;
    color: var(--color-gray-dark);
    opacity: 0.7;
    padding: 10px;
}

</style>
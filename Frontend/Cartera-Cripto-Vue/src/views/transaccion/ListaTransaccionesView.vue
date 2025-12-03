<template>
  <div class="historial-page">
    <div class="content-wrapper">
      <h1 class="title">📜 Historial de Movimientos</h1>
      
      <div v-if="loading" class="info-message">Cargando transacciones...</div>
      <div v-else-if="error" class="error-message">{{ error }}</div>
      <div v-else-if="transacciones.length === 0" class="empty-message">
        No hay transacciones registradas para este usuario.
        <router-link :to="{ name: 'nuevatransaccion' }" class="link-action">
          Registrar la primera.
        </router-link>
      </div>
      
      <div v-else class="table-container">
        <table class="data-table">
          <thead class="table-header">
            <tr>
              <th class="py-3 px-4 rounded-tl-lg">ID</th>
              <th class="py-3 px-4">Fecha</th>
              <th class="py-3 px-4">Acción</th>
              <th class="py-3 px-4">Criptomoneda</th>
              <th class="py-3 px-4">Cantidad</th>
              <th class="py-3 px-4">Monto (ARS)</th>
              <th class="py-3 px-4 rounded-tr-lg">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr 
              v-for="t in transacciones" 
              :key="t.id" 
              :class="{'row-purchase': t.action === 'purchase', 'row-sale': t.action === 'sale'}"
              class="table-row"
            >
              <td class="py-3 px-4 text-center">{{ t.id }}</td>
              <td class="py-3 px-4">{{ formatDate(t.datetime)}}</td>
              <td class="py-3 px-4 font-bold" :class="{'text-success-dark': t.action === 'purchase', 'text-danger-dark': t.action === 'sale'}">
                {{ t.action.toUpperCase() === 'PURCHASE' ? 'COMPRA' : 'VENTA' }}
              </td>
              <td class="py-3 px-4 uppercase">{{ t.crypto_code }}</td>
              <td class="py-3 px-4 font-mono">{{ formatAmount(t.crypto_amount) }}</td>
              <td class="py-3 px-4 font-mono">{{ formatMoney(t.money) }}</td>
              <td class="py-3 px-4 action-buttons">
                <button @click="viewTransaction(t.id)" class="btn-action text-blue-600">Ver</button>
                <button @click="editTransaction(t.id)" class="btn-action text-yellow-600">Editar</button>
                <button @click="deleteTransaction(t.id)" class="btn-action text-red-600">Borrar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <div class="flex-end-wrapper mt-6">
        <button @click="router.push({ name: 'home' })" class="btn-menu-principal">
            Volver al Menú Principal
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const API_BASE = 'https://localhost:7273/api'; 
const API_URL_BASE = `${API_BASE}/Transaccion`; 

const authStore = useAuthStore();
const router = useRouter();

const transacciones = ref([]);
const loading = ref(true);
const error = ref(null);

onMounted(() => {
    if (!authStore.isLoggedIn) {
        router.push({ name: 'login' });
    } else {
        fetchTransacciones();
    }
});

const formatMoney = (amount) => {
    return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(parseFloat(amount) || 0);
};

const formatAmount = (amount) => {
    // CORRECCIÓN: Usamos 18 decimales. Esto es el límite de precisión del tipo Number de JavaScript.
    // Si la cantidad es menor a 1e-18, JavaScript lo tratará como 0.
    const num = Number(amount);
    const amountStr = num.toFixed(18); 
    
    // Eliminar ceros a la derecha innecesarios
    return amountStr.replace(/\.?0+$/, '');
};


const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    
    const date = new Date(dateString);
        if (isNaN(date.getTime())) { 
        return 'Fecha Inválida'; 
    }
        const formattedString = date.toLocaleString('es-AR', { 
        year: '2-digit',     
        month: 'numeric',     
        day: 'numeric',       
        hour: '2-digit', 
        minute: '2-digit', 
        hour12: false         
    });
        return formattedString.replace(/(\s+hs)$/, '');
};

const fetchTransacciones = async () => {
    loading.value = true;
    error.value = null;
    
    const clienteId = authStore.clienteId;

    if (!clienteId) {
        error.value = "Error: No se pudo obtener el Id del cliente logueado.";
        loading.value = false;
        return;
    }
    
    const HISTORIAL_URL = `${API_URL_BASE}/historial/${clienteId}`; 

    try {
        const response = await axios.get(HISTORIAL_URL);
        
        transacciones.value = response.data;
        
    } catch (err) {
        if (err.response) {
            if (err.response.status === 404) {
                transacciones.value = []; 
            } else {
                const serverMessage = err.response.data?.detail || err.response.data?.message || 'Error desconocido.';
                error.value = `Error ${err.response.status} en el servidor: ${serverMessage}`;
            }
        } else {
            error.value = 'No se puede conectar al backend. Verifique la URL o si el servidor está caído.';
        }
        console.error('Error al obtener el historial:', err);
    } finally {
        loading.value = false;
    }
};

const deleteTransaction = async (id) => {
    if (window.confirm(`¿Estás seguro de eliminar la transacción de ID ${id}?`)) {
        try {
            await axios.delete(`${API_URL_BASE}/${id}`);
            window.alert(`Transacción ${id} eliminada con éxito`);
            fetchTransacciones(); 
        } catch (err) {
            window.alert(`No se pudo eliminar la transacción: ${err.response?.data?.message || err.message}`);
        }
    }
};

// Funciones de navegación (manteniendo tu intención)
const viewTransaction = (id) => {
    router.push({ name: 'detalle-transaccion', params: { id: id } });
};

const editTransaction = (id) => {
    router.push({ name: 'editar-transaccion', params: { id: id } });
};

</script>

<style scoped>
:root {
    --color-primary: #1E40AF;
    --color-primary-light: #3B82F6; 
    --color-success: #10B981; 
    --color-danger: #F87171; 
    --color-gray-dark: #1F2937; 
    --color-gray-medium: #4B5563; 
    --color-background: #F3F4F6; 
    --color-card-bg: #FFFFFF; 
}

.historial-page {
    min-height: 100vh;
    padding: 3rem 1rem;
    box-sizing: border-box;
}

.content-wrapper {
    max-width: 1100px; 
    margin: 0 auto;
    width: 100%;
}

.title {
    font-size: 2rem;
    font-weight: 800;
    margin-bottom: 2.5rem;
    text-align: center;
    color: var(--color-gray-dark);
}

.table-container {
    background-color: var(--color-card-bg);
    border-radius: 12px;
    box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
    overflow: hidden; 
    border: 1px solid #E5E7EB;
}

.data-table {
    width: 100%;
    border-collapse: collapse;
}

.table-header {
    background-color: var(--color-gray-dark); 
    color: white;
    font-weight: 700;
    text-align: left;
    font-size: 0.9rem;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    background-image: linear-gradient(135deg, var(--color-gray-dark) 0%, #374151 100%);
}

.data-table th, .data-table td {
    padding: 1rem 1.25rem;
    white-space: nowrap;
}

.data-table tbody tr {
    border-bottom: 1px solid #F3F4F6;
    transition: background-color 0.2s;
}

.data-table tbody tr:last-child {
    border-bottom: none;
}

.row-purchase { background-color: #F0FAF5; } 
.row-sale { background-color: #FEF6F7; }   

.table-row:hover { 
    background-color: #E5E7EB; 
    cursor: default;
}

.text-success-dark { color: var(--color-success); }
.text-danger-dark { color: var(--color-danger); }


.data-table td {
    color: black; 
    font-size: 0.95rem;
}

.font-mono {
    font-family: 'SF Mono', 'Courier New', monospace;
    font-weight: 700; 
    color: var(--color-gray-dark); 
}

.action-buttons button {
    margin-right: 0.5rem;
    font-size: 0.875rem;
    font-weight: 600;
    background: none;
    border: none;
    cursor: pointer;
    text-decoration: none;
    transition: opacity 0.15s;
    padding: 0;
}
.action-buttons button:hover {
    opacity: 0.7;
}

.flex-end-wrapper {
    display: flex;
    justify-content: flex-end; 
    width: 100%;
    padding-top: 25px;
}
.btn-menu-principal {
    background-color: var(--color-card-bg);
    border: 1px solid var(--color-primary-light);
    color: var(--color-primary);
    padding: 12px 20px;
    border-radius: 8px; 
    font-size: 1rem;
    font-weight: 700;
    cursor: pointer;
    transition: background-color 0.2s, color 0.2s, box-shadow 0.2s;
    box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}
.btn-menu-principal:hover {
    background-color: var(--color-primary-light);
    color: white;
    box-shadow: 0 4px 6px rgba(0,0,0,0.1);
}

.info-message, .error-message, .empty-message {
    padding: 1.5rem;
    border-radius: 0.75rem;
    text-align: center;
    margin: 2rem auto;
    max-width: 600px;
    font-weight: 600;
}

.info-message {
    background-color: #DBEAFE;
    color: var(--color-primary);
    border: 1px solid var(--color-primary-light);
}

.error-message {
    background-color: #FEF2F2;
    color: var(--color-danger);
    border: 1px solid var(--color-danger);
}

.empty-message {
    background-color: var(--color-card-bg);
    color: var(--color-gray-medium);
    border: 2px dashed #D1D5DB;
}
</style>
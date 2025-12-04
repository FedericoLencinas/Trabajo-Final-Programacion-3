<template>
  <div class="profile-page">
    <div class="content-wrapper">
      <h1 class="title">👤 Mi Perfil y Balance</h1>
      
      <div v-if="authStore.user" class="user-details-card">
        <h2>Detalles del Cliente</h2>
        <div class="detail-item">
          <span class="detail-label">Nombre: </span>
          <span class="detail-value">{{ authStore.user.name }}</span>
        </div>
        <div class="detail-item">
          <span class="detail-label">Email: </span>
          <span class="detail-value">{{ authStore.user.email }}</span>
        </div>
      </div>
      
      <h2 class="section-subtitle">Saldos de Criptomonedas</h2>

      <div v-if="loading" class="info-message">Calculando saldos...</div>
      <div v-else-if="error" class="error-message">{{ error }}</div>
      <div v-else-if="balances.length === 0" class="empty-message">
        No se encontró ninguna criptomoneda con saldo positivo.
      </div>
      
      <div v-else class="grid-container">
        <div v-for="item in balances" :key="item.crypto_code" class="balance-card">
          <div class="crypto-code">{{ item.crypto_code.toUpperCase() }}</div>
          <div class="balance-amount">{{ formatBalance(item.balance) }}</div>
          <div class="label">Cantidad disponible</div>
        </div>
      </div>

      <div class="flex-end-wrapper mt-6">

        <button @click="router.push({ name: 'editar-perfil' })" class="btn-edit-profile">
                Editar Perfil
        </button>

        <button @click="deleteAccount" class="btn-delete-account">
                Borrar Cuenta
        </button>
        
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
const API_CLIENTE_BASE = 'https://localhost:7273/api/Cliente'; 

const authStore = useAuthStore();
const router = useRouter();

const balances = ref([]);
const loading = ref(true);
const error = ref(null);

const formatBalance = (amount) => {
    const num = Number(amount);
    const amountStr = num.toFixed(18); 
    return amountStr.replace(/\.?0+$/, '');
};

const fetchBalance = async () => {
    loading.value = true;
    error.value = null;
    
    const clienteId = authStore.user?.id;

    if (!clienteId) {
        error.value = "Error de autenticación. Por favor, reinicie sesión.";
        loading.value = false;
        return;
    }
    
    const BALANCE_URL = `${API_URL_BASE}/balance/${clienteId}`; 

    try {
        const response = await axios.get(BALANCE_URL);
        balances.value = response.data;
        
    } catch (err) {
        if (err.response && err.response.status === 404) {
            balances.value = []; 
        } else {
            error.value = 'Error al calcular el balance. Verifique el servidor.';
        }
        console.error('Error al obtener el balance:', err);
    } finally {
        loading.value = false;
    }
};

const deleteAccount = async () => {
    const userId = authStore.user?.id;
    const userName = authStore.user?.name;

    if (!userId) {
        window.alert('Error: No se pudo identificar al usuario para la eliminación.');
        return;
    }

    if (!window.confirm(`ADVERTENCIA: ¿Está seguro de que desea ELIMINAR permanentemente su cuenta, ${userName}, y TODAS sus transacciones? Esta acción no se puede deshacer.`)) {
        return;
    }

    try {
        await axios.delete(`${API_CLIENTE_BASE}/${userId}`);

        authStore.logout();
        window.alert('Su cuenta y todos los datos asociados han sido eliminados con éxito.');
        router.push({ name: 'login' });

    } catch (err) {
        let message = 'Error al intentar eliminar la cuenta.';
        
        if (err.response) {
            const status = err.response.status;
            const serverMessage = err.response.data?.message || err.response.data?.detail || 'Sin mensaje del servidor.';
            message = `Error ${status}: ${serverMessage}`;
        }
        
        window.alert(`Error: ${message}`);
        console.error('Error al eliminar cuenta:', err);
    }
};

onMounted(() => {
    if (!authStore.isLoggedIn) {
        router.push({ name: 'login' });
    } else {
        fetchBalance();
    }
});
</script>

<style scoped>
:root {
    --color-primary: #3B82F6; 
    --color-accent: #059669; 
    --color-danger: #EF4444;
    --color-background: #1A1A2E; 
    --color-card-bg: #2C394B; 
    --color-text-light: #F3F4F6; 
    --color-text-dim: #9CA3AF; 
    --color-border: #4B5563;
}

.profile-page {
    min-height: 100vh;
    padding: 3rem 1rem;
}

.content-wrapper {
    max-width: 900px; 
    margin: 0 auto;
}

.title {
    font-size: 2.5rem;
    font-weight: 900;
    margin-bottom: 2.5rem;
    text-align: center;
    color: var(--color-primary);
}

.user-details-card {
    background-color: rgba(65, 82, 158, 0.973);
    padding: 30px;
    border-radius: 16px;
    box-shadow: 0 15px 30px rgba(0, 0, 0, 0.4); 
    border: 1px solid var(--color-border);
}

.profile-header-compact {
    text-align: center; 
    margin-bottom: 10px;
}
.user-name-title {
    color: var(--color-text-light);
    font-size: 2.2rem; 
    font-weight: 900;
    margin-bottom: 5px;
}
.user-email-text {
    color: var(--color-text-dim);
    font-size: 1rem;
}

.divider {
    border: none;
    height: 1px;
    background-color: var(--color-border);
    margin: 20px 0;
}

.section-subtitle {
    color: var(--color-primary);
    font-size: 1.3rem;
    font-weight: 700;
    margin-top: 2rem;
    margin-bottom: 1.5rem;
    border-bottom: 1px solid var(--color-border);
    padding-bottom: 8px;
}

.grid-container {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); 
    gap: 15px; 
    padding-top: 10px;
}

.balance-card {
    background-color: rgba(65, 82, 158, 0.973);
    padding: 14px; 
    border-radius: 10px;
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.3);
    border: 1px solid var(--color-border);
    border-left: 5px solid var(--color-primary); 
    transition: transform 0.2s;
}

.balance-card:hover {
    transform: translateY(-3px);
    box-shadow: 0 8px 16px rgba(0, 0, 0, 0.4);
}

.crypto-code {
    color: var(--color-text-light);
    font-size: 1.3rem; 
    font-weight: 800;
    margin-bottom: 8px; 
}

.balance-amount {
    font-family: 'SF Mono', 'Courier New', monospace;
    font-size: 1.8rem; 
    font-weight: 900;
    color: var(--color-accent); 
    line-height: 1;
}

.label {
    font-size: 0.85rem; 
    color: var(--color-text-dim);
    margin-top: 8px;
}

.flex-end-wrapper {
    display: flex;
    justify-content: flex-end; 
    width: 100%;
    padding-top: 30px;
}
.btn-menu-principal {
   width: 100%;
 margin-top: 10px;
 background-color: #2563EB; 
 color: #FFFFFF;
 font-weight: 700;
 padding: 0.75rem 1rem;
 border: none;
 border-radius: 0.5rem;
 margin-left: 400px;
 box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
 transition: background-color 150ms ease-in-out;
letter-spacing: 0.025em;
cursor: pointer;
}

.btn-menu-principal:hover {
  background-color: #082470; 
}

.btn-menu-principal:focus {
outline: none; 
box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.5); 
}

.btn-delete-account {
    background-color: #EF4444; 
    color: white;
    font-weight: 600;
    padding: 2px 6px;
    border-radius: 0.5rem;
    transition: background-color 0.15s;
    cursor: pointer;
}

.btn-delete-account:hover {
    background-color: #DC2626; 
}

.btn-delete-account:focus {
    outline: none;
    box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.5); 
}

.btn-edit-profile {
    background-color: #2563EB; 
    color: white; 
    border: 1px solid var(--color-primary);
    padding: 8px 16px;
    border-radius: 8px;
    font-size: 0.95rem;
    cursor: pointer;
    font-weight: 700;
    transition: background-color 0.2s, color 0.2s;
    margin-right: 20px;
}
.btn-edit-profile:hover {
     background-color: #082470; 

}

.info-message { 
    background-color: #3B82F61A; 
    color: var(--color-primary); 
    border: 1px solid var(--color-primary); 
    padding: 15px;
    border-radius: 8px;
}
.error-message { 
    background-color: #EF44441A; 
    color: var(--color-danger); 
    border: 1px solid var(--color-danger); 
    padding: 15px;
    border-radius: 8px;
}
.empty-message { 
    background-color: var(--color-card-bg); 
    color: var(--color-text-dim); 
    border: 2px dashed var(--color-border); 
    padding: 15px;
    border-radius: 8px;
}
</style>
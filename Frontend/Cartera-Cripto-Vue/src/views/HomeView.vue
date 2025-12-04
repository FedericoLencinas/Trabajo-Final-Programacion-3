<template>
  <div class="home-page">
    <div class="max-w-6xl w-full mx-auto">
      
      <header class="header-menu">
        <h1 class="header-title">
          CARTERA CRYPTO
        </h1>
        
        <div class="user-info-section">
          <span class="user-name-text">
            ¡Hola, {{ authStore.user?.name || 'Usuario' }}!
          </span>
          <router-link :to="{ name: 'perfil' }" class="btn-perfil">
            Mi Perfil
          </router-link>
          <button @click="handleLogout" class="btn-logout">
            Cerrar Sesión
          </button>
        </div>
      </header>

      <main class="main-content">
        <div class="menu-grid">
          
          <router-link :to="{ name: 'nuevatransaccion' }" class="card-item card-green">
            <div class="card-icon-wrapper">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="feather feather-plus-square"><rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect><line x1="12" y1="8" x2="12" y2="16"></line><line x1="8" y1="12" x2="16" y2="12"></line></svg>
            </div>
            <h3 class="card-heading">Registrar Transacción</h3>
            <p class="card-description">Registre una compra o venta de criptomonedas.</p>
          </router-link>

          <router-link :to="{ name: 'listatransacciones' }" class="card-item card-blue">
             <div class="card-icon-wrapper">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="feather feather-list"><line x1="8" y1="6" x2="21" y2="6"></line><line x1="8" y1="12" x2="21" y2="12"></line><line x1="8" y1="18" x2="21" y2="18"></line><line x1="3" y1="6" x2="3.01" y2="6"></line><line x1="3" y1="12" x2="3.01" y2="12"></line><line x1="3" y1="18" x2="3.01" y2="18"></line></svg>
            </div>
            <h3 class="card-heading">Historial de Transacciones</h3>
            <p class="card-description">Consulta todas tus operaciones.</p>
          </router-link>
          
        </div>
      </main>

    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

onMounted(() => {
    if (!authStore.isLoggedIn) {
        router.push({ name: 'login' });
    }
});

const handleLogout = () => {
    authStore.logout();
    router.push({ name: 'login' }); 
};
</script>

<style scoped>

.home-page {
    padding: 3rem 1.5rem; 
    min-height: 100vh;
    box-sizing: border-box;
}

.header-menu {
    margin-top : 10px;
    width: 100%;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 10px 15px;
    margin-bottom: 10px;
    background-color: #1F2937; 
    color: white;
    border-radius: 0.75rem;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.header-title {
    font-size: 1.5rem;
    font-weight: 700;
    letter-spacing: 0.05em;
}

.user-info-section {
    display: flex;
    align-items: center;
    gap: 10px; 
}

.user-name-text {
    font-size: 0.875rem;
    opacity: 0.9;
    font-weight: 600; 
    padding-right: 10px; 
}

.btn-perfil {
    background-color: #3B82F6; 
    color: white;
    font-weight: 600;
    padding: 0.5rem 1rem;
    border-radius: 0.5rem;
    text-decoration: none; 
    transition: background-color 0.15s;
}
.btn-perfil:hover {
    background-color: #2563EB;
}

.btn-logout {
    background-color: #EF4444; 
    color: white;
    font-weight: 600;
    padding: 0.5rem 1rem;
    border-radius: 0.5rem;
    transition: background-color 0.15s;
}
.btn-logout:hover {
    background-color: #DC2626; 
}

.main-content {
    padding-top: 1rem;
}

.menu-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); 
    gap: 1.5rem;
}

.card-item {
    display: flex;
    flex-direction: column;
    padding: 1.5rem;
    background-color: white;
    border-radius: 0.75rem;
    box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
    border-left: 6px solid; 
    text-decoration: none; 
    color: inherit; 
    transition: transform 0.2s, box-shadow 0.2s;
}
.card-item:hover {
    transform: translateY(-5px);
    box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
}

.card-icon-wrapper {
    width: 3rem;
    height: 3rem;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 9999px; 
    margin-bottom: 1rem;
}

.card-heading {
    font-size: 1.25rem;
    font-weight: 600;
    color: #1F2937;
    margin-bottom: 0.5rem;
}
.card-description {
    color: #4B5563;
    font-size: 0.875rem;
}

.card-green { 
    border-color: #059669; 
}
.card-green .card-icon-wrapper { 
    background-color: #D1FAE5; 
    color: #059669;
}

.card-blue { 
    border-color: #2563EB; 
}
.card-blue .card-icon-wrapper { 
    background-color: #DBEAFE; 
    color: #2563EB; 
}

.card-purple { 
    border-color: #7C3AED; 
}
.card-purple .card-icon-wrapper { 
    background-color: #EDE9FE; 
    color: #7C3AED; 
}

@media (max-width: 640px) {
    .header-menu {
        flex-direction: column;
        align-items: flex-start;
        gap: 10px;
    }
    .user-info-section {
        width: 100%;
        justify-content: space-between;
    }
}
</style>
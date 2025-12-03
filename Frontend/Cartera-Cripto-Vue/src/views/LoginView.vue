
<template>
  <div class="login-page">
    
    <div class="login-container">
      <div class="header-app">
        <h1 class="title-app">Cartera Crypto</h1>
      </div>
      
      <div class="login-card">
        <h2 class="card-title">Inicie Sesión</h2>
        
        <form @submit.prevent="handleLogin">
          
          <div class="mb-4">
            <label for="email" class="label-field">Email</label>
            <input 
              id="email" 
              type="email" 
              v-model="email" 
              required 
              autocomplete="email"
              class="input-field"
              placeholder="usuario@ejemplo.com"
            />
          </div>

          <div class="mb-6">
            <label for="password" class="label-field">Contraseña</label>
            <input 
              id="password" 
              type="password" 
              v-model="password" 
              required 
              autocomplete="current-password"
              class="input-field"
              placeholder="••••••••"
            />
          </div>
          
          <p v-if="authStore.error" class="error-message">{{ authStore.error }}</p>

          <button 
            type="submit" 
            :disabled="authStore.loading"
            class="btn-primary"
          >
            {{ authStore.loading ? 'Verificando...' : 'Iniciar Sesión' }}
          </button>

          <p class="register-text">
            ¿Necesita una cuenta? 
            <router-link :to="{ name: 'register' }" class="register-link">Regístrese aquí</router-link>
          </p>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

const email = ref('');
const password = ref('');

const handleLogin = async () => {
  try {
    const id = await authStore.login(email.value, password.value);
    
    if (id) {
      router.push({ name: 'home' }); 
    }

  } catch (e) {
  }
};
</script>

<style scoped>

.login-page {
 height: 100vh; 
 width: 100%;
 display: flex; 
 align-items: center; 
 justify-content: center;     
 padding: 2rem;
 box-sizing: border-box; 
}

.login-container {
 max-width: 28rem; 
 width: 100%;
}

.header-app {
 text-align: center;
 margin-bottom: 3rem;
}

.title-app {
 font-size: 3rem;
 font-weight: 800;
 color: #1F2937; 
 letter-spacing: -0.025em;
}

.login-card {
 background-color: #FFFFFF;
 padding: 2.5rem;
 border-radius: 0.75rem;
 box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05); 
 border-top-width: 8px;
 border-color: #2563EB; 
}

.card-title {
 font-size: 1.25rem;
 font-weight: 700;
 color: #1F2937;
 margin-bottom: 1.5rem;
 text-align: center;
}

.label-field {
 display: block;
 font-size: 0.875rem;
 font-weight: 500;
 color: #374151;
 margin-bottom: 0.25rem;
}

.input-field {
 width: 100%;
 padding: 0.625rem 1rem;
 border-width: 1px;
 border-color: #D1D5DB;
 border-radius: 0.5rem;
 color: #374151;
 transition: all 150ms ease-in-out;
}
.input-field:focus {
 outline: 2px solid transparent;
 outline-offset: 2px;
 border-color: #3B82F6; 
 box-shadow: 0 0 0 1px #3B82F6;
}

.btn-primary {
 width: 100%;
 margin-top: 1.75rem;
 background-color: #2563EB; 
 color: #FFFFFF;
 font-weight: 700;
 padding: 0.75rem 1rem;
 border: none;
 border-radius: 0.5rem;
 box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
 transition: background-color 150ms ease-in-out;
letter-spacing: 0.025em;
}
.btn-primary:focus {
outline: none; 
box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.5); 
}
.btn-primary:hover {
  background-color: #082470; 
}
.btn-primary:disabled {
 opacity: 0.5;
 cursor: not-allowed;
}

.error-message {
 color: #DC2626; 
 font-size: 0.875rem;
 margin-bottom: 1rem;
 padding: 0.5rem;
 background-color: #FEF2F2; 
 border-radius: 0.25rem;
}

.register-text {
  margin-top: 1.5rem;
  text-align: center;
  font-size: 0.875rem;
  color: #6B7280; 
}
.register-link {
  color: #2563EB;
  font-weight: 600;
}
.register-link:hover {
  text-decoration: underline;
}
</style>
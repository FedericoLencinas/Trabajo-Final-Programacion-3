<template>
  <!-- Utiliza el mismo fondo y centrado del Login -->
  <div class="register-page login-page">
    
    <div class="login-container">
      <div class="header-app">
        <h1 class="title-app">Cartera Crypto</h1>
      </div>
      
      <!-- RECUADRO BLANCO DEL FORMULARIO -->
      <div class="login-card">
        <h2 class="card-title">Crear Cuenta</h2>
        
        <form @submit.prevent="handleRegister">
          
          <!-- Campo Nombre -->
          <div class="mb-4">
            <label for="name" class="label-field">Nombre</label>
            <input 
              id="name" 
              type="text" 
              v-model="name" 
              required 
              autocomplete="name"
              class="input-field"
              placeholder="Ej: Federico Lencinas"
            />
          </div>

          <!-- Campo Email -->
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

          <!-- Campo Contraseña -->
          <div class="mb-6">
            <label for="password" class="label-field">Contraseña</label>
            <input 
              id="password" 
              type="password" 
              v-model="password" 
              required 
              autocomplete="new-password"
              minlength="6"
              class="input-field"
              placeholder="••••••••"
            />
          </div>
          
          <p v-if="authStore.error" class="error-message">{{ authStore.error }}</p>

          <button 
            type="submit" 
            :disabled="authStore.loading"
            class="btn-primary btn-register"
          >
            {{ authStore.loading ? 'Registrando...' : 'Registrarse' }}
          </button>

          <p class="register-text">
            ¿Ya tienes cuenta? 
            <router-link :to="{ name: 'login' }" class="register-link">Inicia Sesión aquí</router-link>
          </p>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth'; // Usar el store para la lógica POST /register
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

const name = ref('');
const email = ref('');
const password = ref('');

const handleRegister = async () => {
  try {
    // Llama a la acción de registro en Pinia, que hace el POST al backend
    const id = await authStore.register(name.value, email.value, password.value);
    
    if (id) {
      // Si el registro fue exitoso y el login automático funcionó
      alert(`¡Registro exitoso para ${name.value}! Redirigiendo al menú principal.`);
      router.push({ name: 'home' }); 
    }

  } catch (e) {
    // El error se maneja en el authStore
  }
};
</script>

<style scoped>
/* APLICACIÓN DE CENTRADO Y ALTURA (SIN GRADIENTE NI FONDO - LO MANEJA main.css) */
.login-page {
  /* El fondo lo aplica main.css. Aquí solo centramos el contenido */
  padding: 2rem;
  min-height: 100vh; 
  display: flex;
  align-items: center;
  justify-content: center;
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

/* Botón de REGISTRO: Color Verde */
.btn-primary {
  width: 100%;
  margin-top: 1.75rem;
  color: #FFFFFF;
  font-weight: 700;
  padding: 0.75rem 1rem;
  border: none;
  border-radius: 0.5rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  transition: background-color 150ms ease-in-out;
  letter-spacing: 0.025em;
}
.btn-register {
  background-color: #2563EB; 
}
.btn-register:focus {
   outline: none; 
    box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.5); 
}
.btn-register:hover {
  background-color: #1D4ED8; 
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
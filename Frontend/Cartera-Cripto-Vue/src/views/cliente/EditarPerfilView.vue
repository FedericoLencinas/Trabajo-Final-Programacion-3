<template>
  <div class="edit-profile-page">
    <div class="content-wrapper">
      
      <h1 class="title">✏️ Editar Perfil</h1>
      
      <div class="forms-grid-container">

        <div class="form-column">
          <h2 class="section-subtitle">Información Básica</h2>
          <div class="form-card">
            <form @submit.prevent="handleSubmit">
              
              <div class="input-group">
                <label class="label-field">ID de Usuario</label>
                <input type="text" :value="editData.id" disabled class="input-field disabled-field">
              </div>

              <div class="input-group">
                <label for="name" class="label-field required">Nombre Completo</label>
                <input 
                  id="name" 
                  v-model="editData.name" 
                  type="text" 
                  required 
                  class="input-field"
                  placeholder="Ej: Juan Pérez"
                >
              </div>

              <div class="input-group">
                <label for="email" class="label-field required">Email</label>
                <input 
                  id="email" 
                  v-model="editData.email" 
                  type="email" 
                  required 
                  class="input-field"
                  placeholder="usuario@ejemplo.com"
                >
                <p v-if="dataError" class="validation-error">{{ dataError }}</p>
              </div>
              
              <button type="submit" :disabled="isSubmittingData" class="btn-submit btn-primary-submit">
                {{ isSubmittingData ? 'Guardando...' : 'Guardar Datos' }}
              </button>
            </form>
          </div>
        </div> 

        <div class="form-column">
          <h2 class="section-subtitle">Cambiar Contraseña</h2>
          <div class="form-card">
            <form @submit.prevent="handlePasswordChange">
              
              <div class="input-group">
                <label for="currentPassword" class="label-field required">Contraseña Actual</label>
                <input 
                  id="currentPassword" 
                  v-model="passwordData.currentPassword" 
                  type="password" 
                  required 
                  class="input-field"
                >
              </div>

              <div class="input-group">
                <label for="newPassword" class="label-field required">Nueva Contraseña</label>
                <input 
                  id="newPassword" 
                  v-model="passwordData.newPassword" 
                  type="password" 
                  required 
                  class="input-field"
                >
                <p v-if="passwordError" class="validation-error">{{ passwordError }}</p>
              </div>

              <div class="input-group">
                <label for="confirmPassword" class="label-field required">Confirmar Nueva Contraseña</label>
                <input 
                  id="confirmPassword" 
                  v-model="passwordData.confirmPassword" 
                  type="password" 
                  required 
                  class="input-field"
                >
              </div>
              
              <button type="submit" :disabled="isSubmittingPassword" class="btn-submit btn-danger-submit">
                {{ isSubmittingPassword ? 'Cambiando...' : 'Cambiar Contraseña' }}
              </button>
            </form>
          </div>
        </div> 

      </div>
      
      <div class="flex-end-wrapper mt-6">
        <button @click="router.push({ name: 'perfil' })" class="btn-secondary">
          Volver al Perfil
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
const API_CLIENTE_BASE = `${API_BASE}/Cliente`; 

const authStore = useAuthStore();
const router = useRouter();

const isSubmittingData = ref(false);
const isSubmittingPassword = ref(false);
const dataError = ref(null);
const passwordError = ref(null);

const editData = ref({
    id: authStore.user?.id || 0,
    name: authStore.user?.name || '',
    email: authStore.user?.email || ''
});

const passwordData = ref({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
});


const validateEmail = (email) => {
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        dataError.value = "Por favor, introduce un email válido.";
        return false;
    }
    dataError.value = null;
    return true;
};


const handleSubmit = async () => {
    if (!validateEmail(editData.value.email)) {
        return;
    }
    if (!editData.value.name.trim()) {
        dataError.value = "El nombre no puede estar vacío.";
        return;
    }

    isSubmittingData.value = true;
    dataError.value = null;

    try {
        const payload = {
            id: editData.value.id, 
            name: editData.value.name,
            email: editData.value.email,
            password: "" 
        };

        const response = await axios.patch(`${API_CLIENTE_BASE}/${editData.value.id}`, payload);

        const updatedUser = response.data;
        authStore.user = updatedUser;
        localStorage.setItem('user', JSON.stringify(updatedUser)); 

        window.alert('Datos de perfil actualizados con éxito.');
        
        router.push({ name: 'perfil' });

    } catch (err) {
        const serverMessage = err.response?.data?.message || 'Error desconocido al actualizar el perfil.';
        window.alert(`Fallo en la actualización de datos: ${serverMessage}`);
        dataError.value = serverMessage; 
    } finally {
        isSubmittingData.value = false;
    }
};


const handlePasswordChange = async () => {
    passwordError.value = null;
    
    if (passwordData.value.newPassword.length < 4) {
        passwordError.value = "La nueva contraseña debe tener al menos 4 caracteres.";
        return;
    }
    if (passwordData.value.newPassword !== passwordData.value.confirmPassword) {
        passwordError.value = "La nueva contraseña y la confirmación no coinciden.";
        return;
    }
    if (passwordData.value.currentPassword === passwordData.value.newPassword) { 
        passwordError.value = "La nueva contraseña no puede ser igual a la actual.";
        return;
    }

    isSubmittingPassword.value = true;

    try {
        const response = await axios.post(`${API_CLIENTE_BASE}/changepassword`, {
            id: editData.value.id, 
            CurrentPassword: passwordData.value.currentPassword,
            NewPassword: passwordData.value.newPassword
        });

        passwordData.value = { currentPassword: '', newPassword: '', confirmPassword: '' };
        window.alert(response.data.message); 

    } catch (err) {
        const serverMessage = err.response?.data?.message || 'Error desconocido al cambiar la contraseña.';
        window.alert(`Fallo al cambiar contraseña: ${serverMessage}`);
        passwordError.value = serverMessage;
    } finally {
        isSubmittingPassword.value = false;
    }
};


onMounted(() => {
    if (!authStore.isLoggedIn || !editData.value.id) {
        router.push({ name: 'login' });
    }
});
</script>

<style scoped>
:root {
    --color-primary: #3B82F6;
    --color-primary-dark: #1E40AF;
    --color-success: #059669;
    --color-danger: #EF4444;
    --color-background: #1A1A2E; 
    --color-card-bg: #2C394B; 
    --color-text-light: #F3F4F6;
    --color-text-dim: #9CA3AF;
    --color-border: #4B5563;
}

.edit-profile-page {
    min-height: 100vh;
    padding: 3rem 1rem;
}
.content-wrapper {
    max-width: 900px; 
    margin: 0 auto;
}
.title {
    margin-top: 50px;
    font-size: 2rem;
    font-weight: 700;
    margin-bottom: 2rem;
    text-align: center;
    color: var(--color-primary);
}

.forms-grid-container {
    display: grid;
    grid-template-columns: 1fr 1fr; 
    gap: 30px; 
    margin-bottom: 2rem;
}
.form-column {
    display: flex;
    flex-direction: column;
}
@media (max-width: 768px) {
    .forms-grid-container {
        grid-template-columns: 1fr;
        gap: 0;
    }
    .form-column:nth-child(2) {
        margin-top: 2rem; 
    }
}

.section-subtitle {
    color: var(--color-text-light);
    font-size: 1.2rem;
    font-weight: 700;
    margin-top: 0; 
    margin-bottom: 1rem;
    border-bottom: 1px solid var(--color-border);
    padding-bottom: 5px;
}

.form-card {
    background-color: rgb(20, 20, 92);
    padding: 30px;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
    border: 1px solid var(--color-border);
    flex-grow: 1; 
}

.input-group {
    margin-bottom: 1.5rem;
}
.label-field {
    display: block;
    font-size: 0.9rem;
    font-weight: 600;
    color: var(--color-text-dim);
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
    background-color: #374151;
    color: var(--color-text-light);
    transition: border-color 0.2s;
}
.input-field:focus {
    outline: none;
    border-color: var(--color-primary);
    box-shadow: 0 0 0 1px var(--color-primary);
}
.disabled-field {
    background-color: #2C394B;
    cursor: not-allowed;
    color: var(--color-text-dim);
}
.validation-error {
    color: var(--color-danger);
    font-size: 0.85rem;
    margin-top: 5px;
}

.btn-submit {
    width: 100%;
    font-weight: 700;
    padding: 0.8rem;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: background-color 0.2s;
    margin-top: 0.5rem;
}
.btn-primary-submit {
    background-color: #2563EB; 
    color: white;
}
.btn-primary-submit:hover:not(:disabled) {
  background-color: #082470; 
}
.btn-danger-submit {
    background-color: #2563EB; 
    color: white;
}
.btn-danger-submit:hover:not(:disabled) {
  background-color: #082470; 
}

.btn-secondary {
    background-color: #2563EB; 
    color: white;
    border: 1px solid var(--color-primary);  
    padding: 10px 18px;
    border-radius: 8px; 
    font-size: 0.9rem;
    font-weight: 600;
    cursor: pointer;
    transition: background-color 0.2s, color 0.2s;
}
.btn-secondary:hover {
  background-color: #082470; 
}
.flex-end-wrapper {
    display: flex;
    justify-content: flex-end;
    margin-top: 20px;
}
</style>
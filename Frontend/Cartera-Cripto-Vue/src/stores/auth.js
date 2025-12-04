import { defineStore } from 'pinia';
import axios from 'axios';

const API_BASE = 'https://localhost:7273/api/Cliente'; 

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user')) || null, 
    loading: false,
    error: null,
  }),
  
  getters: {
    isLoggedIn: (state) => !!state.user, 
    clienteId: (state) => state.user ? state.user.id : null,
  },

  actions: {
    async login(email, password) {
      this.loading = true;
      this.error = null;
      try {
        const response = await axios.post(`${API_BASE}/login`, { email, password });
        
        const userData = response.data;
        this.user = userData; 

        localStorage.setItem('user', JSON.stringify(userData));
        
        return userData.id; 

      } catch (err) {
        this.error = err.response?.data?.message || 'Error de inicio de sesión.';
        throw err;
      } finally {
        this.loading = false;
      }
    },

    logout() {
      this.user = null;
      localStorage.removeItem('user');
    },
    
    async register(name, email, password) {
      this.loading = true;
      this.error = null;
      try {
        const response = await axios.post(`${API_BASE}/register`, { name, email, password });
        
        this.login(email, password);
        return response.data.id;
        
      } catch (err) {
        this.error = err.response?.data?.message || 'Error de registro.';
        throw err;
      } finally {
        this.loading = false;
      }
    }
  }
});
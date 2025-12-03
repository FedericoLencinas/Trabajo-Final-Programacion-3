import { createRouter, createWebHistory } from 'vue-router'

import HomeView from '../views/HomeView.vue'
import LoginView from '../views/LoginView.vue' 
import RegisterView from '../views/RegisterView.vue' 

import NuevoClienteView from '@/components/models/cliente/NuevoClienteView.vue'
import ListaClientesView from '@/components/models/cliente/ListaClientesView.vue'

import NuevaTransaccionView from '@/views/transaccion/NuevaTransaccionView.vue'
import ListaTransaccionesView from '@/views/transaccion/ListaTransaccionesView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL), 
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView,
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView,
    },
    
    {
      path: '/', 
      name: 'home',
      component: HomeView,
    },
    {
        path: '/transacciones/new', 
        name: 'nuevatransaccion',
        component: NuevaTransaccionView,
    },
    {
        path: '/transacciones/list', 
        name: 'listatransacciones',
        component: ListaTransaccionesView,
    },

    { path: '/clientes/new', name: 'nuevocliente', component: NuevoClienteView },
    { path: '/clientes/list', name: 'listaclientes', component: ListaClientesView },
  ],
})

export default router
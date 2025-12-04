import { createRouter, createWebHistory } from 'vue-router'

import HomeView from '../views/HomeView.vue'
import LoginView from '../views/LoginView.vue' 
import RegisterView from '../views/RegisterView.vue' 

import PerfilView from '@/views/cliente/PerfilView.vue'
import EditarPerfilView from '@/views/cliente/EditarPerfilView.vue'

import NuevaTransaccionView from '@/views/transaccion/NuevaTransaccionView.vue'
import ListaTransaccionesView from '@/views/transaccion/ListaTransaccionesView.vue'
import EditarTransaccionView from '@/views/transaccion/EditarTransaccionView.vue'

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
    {
      path: '/perfil', 
      name: 'perfil',
      component: PerfilView,
    },
    {
      path: '/transaccion/editar/:id', 
      name: 'editar-transaccion', 
      component: EditarTransaccionView 
    },
    {
      path : '/perfil/editar',
      name : 'editar-perfil',
      component : EditarPerfilView
    }

  ],
})

export default router
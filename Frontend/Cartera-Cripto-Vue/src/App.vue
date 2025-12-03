<template>
 
  <div class="app-layout">

      <div class="fixed-clock">
          {{ currentTime }}
      </div>
      
      <RouterView />
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';

const currentTime = ref('');
let intervalId = null;

const updateTime = () => {
    const now = new Date();
    
    const timeString = now.toLocaleTimeString('es-AR', {
        hour: '2-digit', 
        minute: '2-digit', 
        hour12: false 
    });

    const dateString = now.toLocaleDateString('es-AR', {
        dateStyle: 'short'
    });

    currentTime.value = `${dateString}, ${timeString} hs`;
};

onMounted(() => {
    updateTime(); 
    
    intervalId = setInterval(updateTime, 10000); 
});

onUnmounted(() => {
    if (intervalId) {
        clearInterval(intervalId);
    }
});
</script>

<style>
.app-layout {
    min-height: 100vh;
    width: 100%;
    position: relative; 
}
.fixed-clock {
    position: fixed; 
    top: 10px;
    left: 10px;
    z-index: 1000; 
    background-color: rgba(255, 255, 255, 0.9); 
    padding: 8px 12px;
    border-radius: 6px;
    font-size: 0.9rem;
    font-weight: 600;
    color: #1F2937; 
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}
</style>
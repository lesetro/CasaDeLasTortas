<template>
  <div class="perfil-comprador">
    <h2><i class="fas fa-user me-2 text-primary"></i>Mi Perfil</h2>
    
    <div class="row">
      <div class="col-md-4">
        <div class="card">
          <div class="card-body text-center">
            <img :src="usuario.avatar || '/images/default-avatar.png'" 
                 class="rounded-circle mb-3" style="width: 120px; height: 120px; object-fit: cover;">
            <h4>{{ usuario.nombre }}</h4>
            <p class="text-muted">{{ usuario.email }}</p>
            <button class="btn btn-outline-primary btn-sm">
              <i class="fas fa-camera me-1"></i>Cambiar Foto
            </button>
          </div>
        </div>

        <div class="card mt-3">
          <div class="card-body">
            <h6 class="card-title">Preferencias</h6>
            <div class="mb-2">
              <strong>Categorías favoritas:</strong>
              <div class="mt-1">
                <span v-for="pref in usuario.preferencias" :key="pref" 
                      class="badge bg-light text-dark me-1 mb-1">
                  {{ pref }}
                </span>
              </div>
            </div>
            <button class="btn btn-outline-secondary btn-sm w-100">
              <i class="fas fa-edit me-1"></i>Editar Preferencias
            </button>
          </div>
        </div>
      </div>

      <div class="col-md-8">
        <div class="card">
          <div class="card-header">
            <h5 class="mb-0">Información Personal</h5>
          </div>
          <div class="card-body">
            <form @submit.prevent="guardarPerfil">
              <div class="row g-3">
                <div class="col-md-6">
                  <label class="form-label">Nombre completo</label>
                  <input type="text" class="form-control" v-model="usuario.nombre" required>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Email</label>
                  <input type="email" class="form-control" v-model="usuario.email" required>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Teléfono</label>
                  <input type="tel" class="form-control" v-model="usuario.telefono">
                </div>
                <div class="col-md-6">
                  <label class="form-label">Fecha de Nacimiento</label>
                  <input type="date" class="form-control" v-model="usuario.fechaNacimiento">
                </div>
                <div class="col-12">
                  <label class="form-label">Dirección</label>
                  <textarea class="form-control" v-model="usuario.direccion" rows="3"></textarea>
                </div>
                <div class="col-md-4">
                  <label class="form-label">Ciudad</label>
                  <input type="text" class="form-control" v-model="usuario.ciudad">
                </div>
                <div class="col-md-4">
                  <label class="form-label">Provincia</label>
                  <input type="text" class="form-control" v-model="usuario.provincia">
                </div>
                <div class="col-md-4">
                  <label class="form-label">Código Postal</label>
                  <input type="text" class="form-control" v-model="usuario.codigoPostal">
                </div>
                <div class="col-12">
                  <button type="submit" class="btn btn-primary" :disabled="guardando">
                    <span v-if="guardando" class="spinner-border spinner-border-sm me-1"></span>
                    Guardar Cambios
                  </button>
                </div>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'PerfilComprador',
  data() {
    return {
      usuario: {
        id: 1,
        nombre: 'Juan Comprador',
        email: 'juan@email.com',
        telefono: '+54 11 1234-5678',
        fechaNacimiento: '1990-05-15',
        direccion: 'Av. Siempre Viva 123',
        ciudad: 'Springfield',
        provincia: 'Buenos Aires',
        codigoPostal: '1234',
        avatar: '/images/avatar.jpg',
        preferencias: ['Chocolate', 'Frutas', 'Vainilla']
      },
      guardando: false
    }
  },
  methods: {
    async guardarPerfil() {
      this.guardando = true
      try {
        // Simular guardado en API
        await new Promise(resolve => setTimeout(resolve, 1000))
        console.log('Perfil guardado:', this.usuario)
        alert('Perfil actualizado exitosamente!')
      } catch (error) {
        console.error('Error guardando perfil:', error)
        alert('Error al guardar el perfil')
      } finally {
        this.guardando = false
      }
    }
  }
}
</script>

<style scoped>
.perfil-comprador .card {
  border: none;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
</style>
// PerfilVendedor.vue - Perfil y configuración del vendedor
const PerfilVendedor = {
    template: `
    <div class="perfil-vendedor">
        <h2 class="mb-4"><i class="fas fa-user-circle text-success me-2"></i>Mi Perfil</h2>

        <div class="row">
            <div class="col-md-4 mb-4">
                <div class="card shadow">
                    <div class="card-body text-center">
                        <img :src="vendedor.avatar || '/images/default-avatar.png'" 
                             class="rounded-circle mb-3"
                             style="width: 150px; height: 150px; object-fit: cover;"
                             alt="Avatar">
                        <h5 class="mb-1">{{ vendedor.nombreComercial }}</h5>
                        <p class="text-muted mb-3">{{ vendedor.especialidad }}</p>
                        <div class="d-flex justify-content-center mb-3">
                            <div class="text-warning">
                                <i v-for="n in 5" :key="n" 
                                   :class="n <= vendedor.calificacion ? 'fas fa-star' : 'far fa-star'"></i>
                            </div>
                            <span class="ms-2 text-muted">({{ vendedor.calificacion }}/5)</span>
                        </div>
                        <button class="btn btn-outline-primary w-100" @click="modoEdicion = true">
                            <i class="fas fa-edit me-2"></i>Editar Perfil
                        </button>
                    </div>
                </div>
            </div>

            <div class="col-md-8">
                <div class="card shadow mb-4">
                    <div class="card-header bg-white">
                        <h6 class="mb-0"><i class="fas fa-info-circle me-2"></i>Información del Negocio</h6>
                    </div>
                    <div class="card-body">
                        <div v-if="!modoEdicion">
                            <div class="row">
                                <div class="col-md-6 mb-3">
                                    <small class="text-muted">Nombre Comercial</small>
                                    <p class="fw-bold">{{ vendedor.nombreComercial }}</p>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <small class="text-muted">Especialidad</small>
                                    <p class="fw-bold">{{ vendedor.especialidad }}</p>
                                </div>
                                <div class="col-12 mb-3">
                                    <small class="text-muted">Descripción</small>
                                    <p>{{ vendedor.descripcion || 'Sin descripción' }}</p>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <small class="text-muted">Horario</small>
                                    <p>{{ vendedor.horario || 'No especificado' }}</p>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <small class="text-muted">Miembro desde</small>
                                    <p>{{ formatearFecha(vendedor.fechaCreacion) }}</p>
                                </div>
                            </div>
                        </div>

                        <form v-else @submit.prevent="guardarCambios">
                            <div class="row">
                                <div class="col-md-6 mb-3">
                                    <label class="form-label">Nombre Comercial *</label>
                                    <input type="text" class="form-control" v-model="formEdicion.nombreComercial" required>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <label class="form-label">Especialidad *</label>
                                    <input type="text" class="form-control" v-model="formEdicion.especialidad" required>
                                </div>
                                <div class="col-12 mb-3">
                                    <label class="form-label">Descripción</label>
                                    <textarea class="form-control" rows="3" v-model="formEdicion.descripcion"></textarea>
                                </div>
                                <div class="col-12 mb-3">
                                    <label class="form-label">Horario de Atención</label>
                                    <input type="text" class="form-control" v-model="formEdicion.horario" 
                                           placeholder="Ej: Lunes a Viernes 9:00 - 18:00">
                                </div>
                                <div class="col-12">
                                    <button type="submit" class="btn btn-success me-2" :disabled="guardando">
                                        <span v-if="guardando">
                                            <span class="spinner-border spinner-border-sm me-2"></span>
                                            Guardando...
                                        </span>
                                        <span v-else>
                                            <i class="fas fa-save me-2"></i>Guardar Cambios
                                        </span>
                                    </button>
                                    <button type="button" class="btn btn-secondary" @click="cancelarEdicion">
                                        Cancelar
                                    </button>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>

                <div class="card shadow">
                    <div class="card-header bg-white">
                        <h6 class="mb-0"><i class="fas fa-lock me-2"></i>Seguridad</h6>
                    </div>
                    <div class="card-body">
                        <button class="btn btn-outline-warning">
                            <i class="fas fa-key me-2"></i>Cambiar Contraseña
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
    `,
    
    props: {
        vendedorId: {
            type: Number,
            required: true
        }
    },
    
    data() {
        return {
            vendedor: {
                nombreComercial: '',
                especialidad: '',
                descripcion: '',
                horario: '',
                avatar: null,
                calificacion: 0,
                fechaCreacion: new Date()
            },
            modoEdicion: false,
            formEdicion: {},
            guardando: false
        }
    },
    
    async mounted() {
        await this.cargarPerfil();
    },
    
    methods: {
        async cargarPerfil() {
            try {
                const response = await fetch(`/api/vendedor/${this.vendedorId}`, {
                    headers: {
                        'Authorization': `Bearer ${window.authToken}`,
                        'Content-Type': 'application/json'
                    }
                });
                
                if (!response.ok) throw new Error('Error al cargar perfil');
                
                this.vendedor = await response.json();
            } catch (error) {
                console.error('Error:', error);
                toastr.error('Error al cargar el perfil');
            }
        },
        
        editarPerfil() {
            this.modoEdicion = true;
            this.formEdicion = { ...this.vendedor };
        },
        
        cancelarEdicion() {
            this.modoEdicion = false;
            this.formEdicion = {};
        },
        
        async guardarCambios() {
            this.guardando = true;
            
            try {
                const response = await fetch(`/api/vendedor/${this.vendedorId}`, {
                    method: 'PUT',
                    headers: {
                        'Authorization': `Bearer ${window.authToken}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(this.formEdicion)
                });
                
                if (!response.ok) throw new Error('Error al actualizar');
                
                this.vendedor = await response.json();
                this.modoEdicion = false;
                toastr.success('Perfil actualizado exitosamente');
            } catch (error) {
                console.error('Error:', error);
                toastr.error('Error al actualizar el perfil');
            } finally {
                this.guardando = false;
            }
        },
        
        formatearFecha(fecha) {
            return new Date(fecha).toLocaleDateString('es-AR', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });
        }
    }
};

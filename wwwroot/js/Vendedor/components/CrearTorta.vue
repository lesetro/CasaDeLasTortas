// CrearTorta.vue - Modal para crear nueva torta
const CrearTorta = {
    template: `
    <div class="modal fade show d-block" tabindex="-1" style="background-color: rgba(0,0,0,0.5);">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header bg-success text-white">
                    <h5 class="modal-title">
                        <i class="fas fa-plus-circle me-2"></i>
                        Nueva Torta
                    </h5>
                    <button type="button" class="btn-close btn-close-white" @click="$emit('cerrar')"></button>
                </div>
                
                <form @submit.prevent="guardarTorta">
                    <div class="modal-body">
                        <div class="row">
                            <!-- Nombre -->
                            <div class="col-md-6 mb-3">
                                <label class="form-label">Nombre <span class="text-danger">*</span></label>
                                <input type="text" 
                                       class="form-control" 
                                       v-model="form.nombre"
                                       :class="{ 'is-invalid': errores.nombre }"
                                       required>
                                <div v-if="errores.nombre" class="invalid-feedback">{{ errores.nombre }}</div>
                            </div>

                            <!-- Categoría -->
                            <div class="col-md-6 mb-3">
                                <label class="form-label">Categoría</label>
                                <select class="form-select" v-model="form.categoria">
                                    <option value="">Seleccionar...</option>
                                    <option value="Cumpleaños">Cumpleaños</option>
                                    <option value="Bodas">Bodas</option>
                                    <option value="Infantiles">Infantiles</option>
                                    <option value="Temáticas">Temáticas</option>
                                    <option value="Clásicas">Clásicas</option>
                                    <option value="Personalizadas">Personalizadas</option>
                                </select>
                            </div>

                            <!-- Descripción -->
                            <div class="col-12 mb-3">
                                <label class="form-label">Descripción <span class="text-danger">*</span></label>
                                <textarea class="form-control" 
                                          rows="3"
                                          v-model="form.descripcion"
                                          :class="{ 'is-invalid': errores.descripcion }"
                                          required></textarea>
                                <div v-if="errores.descripcion" class="invalid-feedback">{{ errores.descripcion }}</div>
                            </div>

                            <!-- Precio -->
                            <div class="col-md-4 mb-3">
                                <label class="form-label">Precio ($) <span class="text-danger">*</span></label>
                                <input type="number" 
                                       step="0.01"
                                       min="0"
                                       class="form-control" 
                                       v-model.number="form.precio"
                                       :class="{ 'is-invalid': errores.precio }"
                                       required>
                                <div v-if="errores.precio" class="invalid-feedback">{{ errores.precio }}</div>
                            </div>

                            <!-- Stock -->
                            <div class="col-md-4 mb-3">
                                <label class="form-label">Stock Inicial <span class="text-danger">*</span></label>
                                <input type="number" 
                                       min="0"
                                       class="form-control" 
                                       v-model.number="form.stock"
                                       :class="{ 'is-invalid': errores.stock }"
                                       required>
                                <div v-if="errores.stock" class="invalid-feedback">{{ errores.stock }}</div>
                            </div>

                            <!-- Peso (kg) -->
                            <div class="col-md-4 mb-3">
                                <label class="form-label">Peso (kg)</label>
                                <input type="number" 
                                       step="0.1"
                                       min="0"
                                       class="form-control" 
                                       v-model.number="form.peso">
                            </div>

                            <!-- Disponible -->
                            <div class="col-12 mb-3">
                                <div class="form-check form-switch">
                                    <input class="form-check-input" 
                                           type="checkbox" 
                                           id="disponible"
                                           v-model="form.disponible">
                                    <label class="form-check-label" for="disponible">
                                        Torta disponible para la venta
                                    </label>
                                </div>
                            </div>

                            <!-- Imágenes -->
                            <div class="col-12 mb-3">
                                <label class="form-label">Imágenes</label>
                                <input type="file" 
                                       class="form-control" 
                                       accept="image/*"
                                       multiple
                                       @change="onFileChange"
                                       ref="fileInput">
                                <small class="text-muted">Puedes seleccionar múltiples imágenes</small>
                                
                                <!-- Preview de imágenes -->
                                <div v-if="imagenesPrevisualizadas.length > 0" class="mt-3">
                                    <div class="row g-2">
                                        <div v-for="(img, index) in imagenesPrevisualizadas" 
                                             :key="index"
                                             class="col-md-3">
                                            <div class="position-relative">
                                                <img :src="img" class="img-thumbnail" style="width: 100%; height: 150px; object-fit: cover;">
                                                <button type="button" 
                                                        class="btn btn-sm btn-danger position-absolute top-0 end-0 m-1"
                                                        @click="eliminarImagen(index)">
                                                    <i class="fas fa-times"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" @click="$emit('cerrar')">
                            <i class="fas fa-times me-2"></i>Cancelar
                        </button>
                        <button type="submit" class="btn btn-success" :disabled="guardando">
                            <span v-if="guardando">
                                <span class="spinner-border spinner-border-sm me-2"></span>
                                Guardando...
                            </span>
                            <span v-else>
                                <i class="fas fa-save me-2"></i>Crear Torta
                            </span>
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
    `,
    
    data() {
        return {
            form: {
                nombre: '',
                categoria: '',
                descripcion: '',
                precio: 0,
                stock: 0,
                peso: 0,
                disponible: true
            },
            archivos: [],
            imagenesPrevisualizadas: [],
            errores: {},
            guardando: false
        }
    },
    
    methods: {
        onFileChange(event) {
            this.archivos = Array.from(event.target.files);
            this.imagenesPrevisualizadas = [];
            
            this.archivos.forEach(file => {
                const reader = new FileReader();
                reader.onload = (e) => {
                    this.imagenesPrevisualizadas.push(e.target.result);
                };
                reader.readAsDataURL(file);
            });
        },
        
        eliminarImagen(index) {
            this.archivos.splice(index, 1);
            this.imagenesPrevisualizadas.splice(index, 1);
        },
        
        validarFormulario() {
            this.errores = {};
            
            if (!this.form.nombre || this.form.nombre.trim() === '') {
                this.errores.nombre = 'El nombre es requerido';
            }
            
            if (!this.form.descripcion || this.form.descripcion.trim() === '') {
                this.errores.descripcion = 'La descripción es requerida';
            }
            
            if (this.form.precio <= 0) {
                this.errores.precio = 'El precio debe ser mayor a 0';
            }
            
            if (this.form.stock < 0) {
                this.errores.stock = 'El stock no puede ser negativo';
            }
            
            return Object.keys(this.errores).length === 0;
        },
        
        async guardarTorta() {
            if (!this.validarFormulario()) {
                return;
            }
            
            this.guardando = true;
            
            try {
                // Crear FormData para enviar con imágenes
                const formData = new FormData();
                formData.append('nombre', this.form.nombre);
                formData.append('categoria', this.form.categoria);
                formData.append('descripcion', this.form.descripcion);
                formData.append('precio', this.form.precio);
                formData.append('stock', this.form.stock);
                formData.append('peso', this.form.peso);
                formData.append('disponible', this.form.disponible);
                
                // Agregar imágenes
                this.archivos.forEach((file, index) => {
                    formData.append(`imagenes`, file);
                });
                
                const response = await fetch('/api/torta', {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${window.authToken}`
                    },
                    body: formData
                });
                
                if (!response.ok) {
                    const error = await response.json();
                    throw new Error(error.message || 'Error al crear la torta');
                }
                
                toastr.success('Torta creada exitosamente');
                this.$emit('torta-creada');
            } catch (error) {
                console.error('Error creando torta:', error);
                toastr.error(error.message || 'Error al crear la torta');
            } finally {
                this.guardando = false;
            }
        }
    }
};

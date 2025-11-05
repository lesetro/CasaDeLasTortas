// EditarTorta.vue - Modal para editar torta existente
const EditarTorta = {
    template: `
    <div class="modal fade show d-block" tabindex="-1" style="background-color: rgba(0,0,0,0.5);">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title">
                        <i class="fas fa-edit me-2"></i>
                        Editar Torta
                    </h5>
                    <button type="button" class="btn-close btn-close-white" @click="$emit('cerrar')"></button>
                </div>
                
                <form @submit.prevent="guardarCambios">
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label class="form-label">Nombre *</label>
                                <input type="text" class="form-control" v-model="form.nombre" required>
                            </div>

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

                            <div class="col-12 mb-3">
                                <label class="form-label">Descripción *</label>
                                <textarea class="form-control" rows="3" v-model="form.descripcion" required></textarea>
                            </div>

                            <div class="col-md-4 mb-3">
                                <label class="form-label">Precio ($) *</label>
                                <input type="number" step="0.01" min="0" class="form-control" v-model.number="form.precio" required>
                            </div>

                            <div class="col-md-4 mb-3">
                                <label class="form-label">Stock *</label>
                                <input type="number" min="0" class="form-control" v-model.number="form.stock" required>
                            </div>

                            <div class="col-md-4 mb-3">
                                <label class="form-label">Peso (kg)</label>
                                <input type="number" step="0.1" min="0" class="form-control" v-model.number="form.peso">
                            </div>

                            <div class="col-12 mb-3">
                                <div class="form-check form-switch">
                                    <input class="form-check-input" type="checkbox" id="disponibleEdit" v-model="form.disponible">
                                    <label class="form-check-label" for="disponibleEdit">
                                        Torta disponible para la venta
                                    </label>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" @click="$emit('cerrar')">
                            <i class="fas fa-times me-2"></i>Cancelar
                        </button>
                        <button type="submit" class="btn btn-primary" :disabled="guardando">
                            <span v-if="guardando">
                                <span class="spinner-border spinner-border-sm me-2"></span>
                                Guardando...
                            </span>
                            <span v-else">
                                <i class="fas fa-save me-2"></i>Guardar Cambios
                            </span>
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
    `,
    
    props: {
        torta: {
            type: Object,
            required: true
        }
    },
    
    data() {
        return {
            form: {
                nombre: this.torta.nombre,
                categoria: this.torta.categoria,
                descripcion: this.torta.descripcion,
                precio: this.torta.precio,
                stock: this.torta.stock,
                peso: this.torta.peso,
                disponible: this.torta.disponible
            },
            guardando: false
        }
    },
    
    methods: {
        async guardarCambios() {
            this.guardando = true;
            
            try {
                const response = await fetch(`/api/torta/${this.torta.id}`, {
                    method: 'PUT',
                    headers: {
                        'Authorization': `Bearer ${window.authToken}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(this.form)
                });
                
                if (!response.ok) throw new Error('Error al actualizar');
                
                this.$emit('torta-editada');
            } catch (error) {
                console.error('Error:', error);
                toastr.error('Error al actualizar la torta');
            } finally {
                this.guardando = false;
            }
        }
    }
};

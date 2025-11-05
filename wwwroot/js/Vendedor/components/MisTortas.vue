// MisTortas.vue - Listado paginado de tortas del vendedor con CRUD
const MisTortas = {
    template: `
    <div class="mis-tortas">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2><i class="fas fa-birthday-cake text-primary me-2"></i>Mis Tortas</h2>
            <button class="btn btn-success" @click="abrirModalCrear">
                <i class="fas fa-plus me-2"></i>Nueva Torta
            </button>
        </div>

        <!-- Filtros y Búsqueda -->
        <div class="card shadow-sm mb-4">
            <div class="card-body">
                <div class="row g-3">
                    <div class="col-md-4">
                        <div class="input-group">
                            <span class="input-group-text"><i class="fas fa-search"></i></span>
                            <input type="text" 
                                   class="form-control" 
                                   placeholder="Buscar por nombre..."
                                   v-model="filtros.busqueda"
                                   @input="debounceCargarTortas">
                        </div>
                    </div>
                    <div class="col-md-3">
                        <select class="form-select" v-model="filtros.disponible" @change="cargarTortas(1)">
                            <option value="">Todas</option>
                            <option value="true">Solo disponibles</option>
                            <option value="false">No disponibles</option>
                        </select>
                    </div>
                    <div class="col-md-3">
                        <select class="form-select" v-model="filtros.ordenarPor" @change="cargarTortas(1)">
                            <option value="nombre">Nombre A-Z</option>
                            <option value="nombre_desc">Nombre Z-A</option>
                            <option value="precio">Precio menor a mayor</option>
                            <option value="precio_desc">Precio mayor a menor</option>
                            <option value="stock">Stock menor a mayor</option>
                            <option value="fecha_desc">Más recientes</option>
                        </select>
                    </div>
                    <div class="col-md-2">
                        <select class="form-select" v-model="registrosPorPagina" @change="cargarTortas(1)">
                            <option :value="10">10 por página</option>
                            <option :value="20">20 por página</option>
                            <option :value="50">50 por página</option>
                        </select>
                    </div>
                </div>
            </div>
        </div>

        <!-- Tabla de Tortas -->
        <div class="card shadow">
            <div class="card-body">
                <div v-if="cargando" class="text-center py-5">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <p class="mt-3 text-muted">Cargando tortas...</p>
                </div>

                <div v-else-if="tortas.length === 0" class="text-center py-5">
                    <i class="fas fa-birthday-cake fa-4x text-muted mb-3"></i>
                    <h5 class="text-muted">No tienes tortas registradas</h5>
                    <p class="text-muted">Crea tu primera torta para comenzar a vender</p>
                    <button class="btn btn-success mt-3" @click="abrirModalCrear">
                        <i class="fas fa-plus me-2"></i>Crear Primera Torta
                    </button>
                </div>

                <div v-else class="table-responsive">
                    <table class="table table-hover align-middle">
                        <thead class="table-light">
                            <tr>
                                <th>Imagen</th>
                                <th>Nombre</th>
                                <th>Precio</th>
                                <th>Stock</th>
                                <th>Categoría</th>
                                <th>Estado</th>
                                <th class="text-center">Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="torta in tortas" :key="torta.id">
                                <td>
                                    <img :src="torta.imagenPrincipal || '/images/no-image.png'" 
                                         class="rounded"
                                         style="width: 60px; height: 60px; object-fit: cover;"
                                         :alt="torta.nombre">
                                </td>
                                <td>
                                    <strong>{{ torta.nombre }}</strong>
                                    <br>
                                    <small class="text-muted">{{ torta.descripcion?.substring(0, 50) }}...</small>
                                </td>
                                <td>
                                    <strong class="text-success">\${{ torta.precio.toFixed(2) }}</strong>
                                </td>
                                <td>
                                    <span :class="getStockClass(torta.stock)">
                                        <i class="fas fa-box me-1"></i>
                                        {{ torta.stock }}
                                    </span>
                                </td>
                                <td>
                                    <span class="badge bg-info">{{ torta.categoria || 'Sin categoría' }}</span>
                                </td>
                                <td>
                                    <span class="badge" :class="torta.disponible ? 'bg-success' : 'bg-secondary'">
                                        {{ torta.disponible ? 'Disponible' : 'No disponible' }}
                                    </span>
                                </td>
                                <td class="text-center">
                                    <div class="btn-group" role="group">
                                        <button class="btn btn-sm btn-outline-info" 
                                                @click="verDetalle(torta)"
                                                title="Ver detalle">
                                            <i class="fas fa-eye"></i>
                                        </button>
                                        <button class="btn btn-sm btn-outline-primary" 
                                                @click="editarTorta(torta)"
                                                title="Editar">
                                            <i class="fas fa-edit"></i>
                                        </button>
                                        <button class="btn btn-sm btn-outline-danger" 
                                                @click="confirmarEliminar(torta)"
                                                title="Eliminar">
                                            <i class="fas fa-trash"></i>
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <!-- Paginación -->
                <div v-if="totalPaginas > 1" class="d-flex justify-content-between align-items-center mt-4">
                    <div class="text-muted">
                        Mostrando {{ (paginaActual - 1) * registrosPorPagina + 1 }} a 
                        {{ Math.min(paginaActual * registrosPorPagina, totalRegistros) }} 
                        de {{ totalRegistros }} tortas
                    </div>
                    <nav>
                        <ul class="pagination mb-0">
                            <li class="page-item" :class="{ disabled: paginaActual === 1 }">
                                <a class="page-link" href="#" @click.prevent="cargarTortas(paginaActual - 1)">
                                    <i class="fas fa-chevron-left"></i>
                                </a>
                            </li>
                            <li v-for="pagina in paginasVisibles" 
                                :key="pagina"
                                class="page-item" 
                                :class="{ active: pagina === paginaActual }">
                                <a class="page-link" href="#" @click.prevent="cargarTortas(pagina)">
                                    {{ pagina }}
                                </a>
                            </li>
                            <li class="page-item" :class="{ disabled: paginaActual === totalPaginas }">
                                <a class="page-link" href="#" @click.prevent="cargarTortas(paginaActual + 1)">
                                    <i class="fas fa-chevron-right"></i>
                                </a>
                            </li>
                        </ul>
                    </nav>
                </div>
            </div>
        </div>

        <!-- Modales -->
        <CrearTorta 
            v-if="mostrarModalCrear"
            @cerrar="cerrarModalCrear"
            @torta-creada="onTortaCreada">
        </CrearTorta>

        <EditarTorta 
            v-if="tortaEditando"
            :torta="tortaEditando"
            @cerrar="cerrarModalEditar"
            @torta-editada="onTortaEditada">
        </EditarTorta>
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
            tortas: [],
            cargando: false,
            paginaActual: 1,
            totalPaginas: 0,
            totalRegistros: 0,
            registrosPorPagina: 10,
            filtros: {
                busqueda: '',
                disponible: '',
                ordenarPor: 'nombre'
            },
            mostrarModalCrear: false,
            tortaEditando: null,
            debounceTimer: null
        }
    },
    
    computed: {
        paginasVisibles() {
            const paginas = [];
            const maxPaginas = 5;
            let inicio = Math.max(1, this.paginaActual - 2);
            let fin = Math.min(this.totalPaginas, inicio + maxPaginas - 1);
            
            if (fin - inicio < maxPaginas - 1) {
                inicio = Math.max(1, fin - maxPaginas + 1);
            }
            
            for (let i = inicio; i <= fin; i++) {
                paginas.push(i);
            }
            
            return paginas;
        }
    },
    
    async mounted() {
        await this.cargarTortas(1);
    },
    
    methods: {
        async cargarTortas(pagina) {
            this.cargando = true;
            this.paginaActual = pagina;
            
            try {
                const params = new URLSearchParams({
                    pagina: pagina,
                    registrosPorPagina: this.registrosPorPagina,
                    busqueda: this.filtros.busqueda,
                    disponible: this.filtros.disponible,
                    ordenarPor: this.filtros.ordenarPor
                });
                
                const response = await fetch(
                    `/api/torta/vendedor/${this.vendedorId}?${params}`,
                    {
                        headers: {
                            'Authorization': `Bearer ${window.authToken}`,
                            'Content-Type': 'application/json'
                        }
                    }
                );
                
                if (!response.ok) throw new Error('Error al cargar tortas');
                
                const data = await response.json();
                this.tortas = data.data || [];
                this.totalPaginas = data.totalPaginas || 0;
                this.totalRegistros = data.totalRegistros || 0;
            } catch (error) {
                console.error('Error cargando tortas:', error);
                toastr.error('Error al cargar las tortas');
            } finally {
                this.cargando = false;
            }
        },
        
        debounceCargarTortas() {
            clearTimeout(this.debounceTimer);
            this.debounceTimer = setTimeout(() => {
                this.cargarTortas(1);
            }, 500);
        },
        
        getStockClass(stock) {
            if (stock === 0) return 'badge bg-danger';
            if (stock <= 5) return 'badge bg-warning text-dark';
            return 'badge bg-success';
        },
        
        abrirModalCrear() {
            this.mostrarModalCrear = true;
        },
        
        cerrarModalCrear() {
            this.mostrarModalCrear = false;
        },
        
        onTortaCreada() {
            this.cerrarModalCrear();
            this.cargarTortas(this.paginaActual);
            this.$emit('torta-creada');
        },
        
        editarTorta(torta) {
            this.tortaEditando = { ...torta };
        },
        
        cerrarModalEditar() {
            this.tortaEditando = null;
        },
        
        onTortaEditada() {
            this.cerrarModalEditar();
            this.cargarTortas(this.paginaActual);
            this.$emit('torta-editada');
        },
        
        verDetalle(torta) {
            // Abrir modal o navegar a página de detalle
            toastr.info('Funcionalidad de detalle en desarrollo');
        },
        
        async confirmarEliminar(torta) {
            if (!confirm(`¿Estás seguro de eliminar "${torta.nombre}"? Esta acción es una baja lógica.`)) {
                return;
            }
            
            try {
                const response = await fetch(`/api/torta/${torta.id}`, {
                    method: 'DELETE',
                    headers: {
                        'Authorization': `Bearer ${window.authToken}`,
                        'Content-Type': 'application/json'
                    }
                });
                
                if (!response.ok) throw new Error('Error al eliminar');
                
                toastr.success('Torta eliminada exitosamente');
                this.cargarTortas(this.paginaActual);
                this.$emit('torta-eliminada');
            } catch (error) {
                console.error('Error eliminando torta:', error);
                toastr.error('Error al eliminar la torta');
            }
        }
    }
};

using CasaDeLasTortas.Data;
using CasaDeLasTortas.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasaDeLasTortas.Data
{
    /// <summary>
    /// Inicializador de base de datos con datos de prueba
    /// Incluye configuración de plataforma, vendedores con datos de pago, 
    /// compradores, tortas, ventas y pagos de ejemplo
    /// </summary>
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            // Asegura que la base de datos existe
            await context.Database.EnsureCreatedAsync();

            // Si ya hay datos, no hacer nada
            if (await context.Personas.AnyAsync())
            {
                return;
            }

            // ==================== 1. CONFIGURACIÓN DE LA PLATAFORMA ====================
            var configuracion = ConfiguracionPlataforma.CrearDefault();
            configuracion.AliasCBU = "casadelastortas.pagos";
            configuracion.CBU = "0110012345678901234567";
            configuracion.Banco = "Banco Nación";
            configuracion.TitularCuenta = "Casa de las Tortas S.A.";
            configuracion.CUIT = "30-71234567-9";
            configuracion.ComisionPorcentaje = 10.00m;
            configuracion.InstruccionesPago = @"Para completar tu compra:

1. Realizá una transferencia o pago por MercadoPago al alias o CBU indicado
2. Ingresá el monto exacto de tu pedido
3. Subí el comprobante de pago (captura de pantalla o PDF)
4. Esperá la confirmación del administrador (máximo 24hs hábiles)
5. ¡Listo! Los vendedores comenzarán a preparar tu pedido

IMPORTANTE: El comprobante debe mostrar claramente:
- Monto transferido
- Fecha de la operación
- CBU o alias de destino";
            
            context.ConfiguracionPlataforma.Add(configuracion);
            await context.SaveChangesAsync();

            // ==================== 2. PERSONAS ====================
            var personas = new List<Persona>
            {
                // Admin
                new Persona
                {
                    Nombre = "Admin Sistema",
                    Email = "admin@casadetortas.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Rol = "Admin",
                    Activo = true,
                    FechaRegistro = DateTime.Now.AddMonths(-6),
                    Avatar = "https://ui-avatars.com/api/?name=Admin+Sistema&background=dc3545&color=fff"
                },
                // Vendedor 1 - Con datos de pago completos
                new Persona
                {
                    Nombre = "Carlos González",
                    Email = "carlos@pasteleria.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Vendedor123!"),
                    Rol = "Vendedor",
                    Telefono = "3541234567",
                    Activo = true,
                    FechaRegistro = DateTime.Now.AddMonths(-4),
                    Avatar = "https://ui-avatars.com/api/?name=Carlos+Gonzalez&background=28a745&color=fff"
                },
                // Vendedor 2 - Con datos de pago completos
                new Persona
                {
                    Nombre = "María López",
                    Email = "maria@reposteria.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Vendedor123!"),
                    Rol = "Vendedor",
                    Telefono = "3549876543",
                    Activo = true,
                    FechaRegistro = DateTime.Now.AddMonths(-3),
                    Avatar = "https://ui-avatars.com/api/?name=Maria+Lopez&background=17a2b8&color=fff"
                },
                // Comprador 1
                new Persona
                {
                    Nombre = "Juan Pérez",
                    Email = "juan@cliente.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Comprador123!"),
                    Rol = "Comprador",
                    Telefono = "3541112233",
                    Activo = true,
                    FechaRegistro = DateTime.Now.AddMonths(-2),
                    Avatar = "https://ui-avatars.com/api/?name=Juan+Perez&background=ffc107&color=000"
                },
                // Comprador 2
                new Persona
                {
                    Nombre = "Ana Martínez",
                    Email = "ana@cliente.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Comprador123!"),
                    Rol = "Comprador",
                    Telefono = "3544445566",
                    Activo = true,
                    FechaRegistro = DateTime.Now.AddMonths(-1),
                    Avatar = "https://ui-avatars.com/api/?name=Ana+Martinez&background=6f42c1&color=fff"
                }
            };

            context.Personas.AddRange(personas);
            await context.SaveChangesAsync();

            // ==================== 3. VENDEDORES (con datos de pago) ====================
            var vendedores = new List<Vendedor>
            {
                new Vendedor
                {
                    PersonaId = personas[1].Id, // Carlos
                    NombreComercial = "Pastelería Don Carlos",
                    Especialidad = "Tortas de Chocolate y Cheesecakes",
                    Descripcion = "Pastelería artesanal con más de 10 años de experiencia. Especialistas en tortas de chocolate belga y cheesecakes cremosos.",
                    Horario = "Lunes a Viernes 9:00 - 18:00, Sábados 9:00 - 14:00",
                    Calificacion = 4.8m,
                    TotalVentas = 150,
                    Verificado = true,
                    Activo = true,
                    // Datos de pago completos
                    AliasCBU = "pasteleria.doncarlos.mp",
                    CBU = "0140012301234567890123",
                    Banco = "Banco Galicia",
                    TitularCuenta = "Carlos Alberto González",
                    CUIT = "20-25678901-3",
                    DatosPagoCompletos = true,
                    FechaDatosPago = DateTime.Now.AddMonths(-4),
                    TotalCobrado = 1500000m,
                    TotalComisiones = 150000m,
                    PendienteCobro = 0
                },
                new Vendedor
                {
                    PersonaId = personas[2].Id, // María
                    NombreComercial = "Dulces Sueños Repostería",
                    Especialidad = "Tortas Frutales y Decoradas",
                    Descripcion = "Repostería creativa especializada en tortas decoradas para eventos especiales. Trabajamos con frutas de estación.",
                    Horario = "Lunes a Sábado 10:00 - 20:00",
                    Calificacion = 4.9m,
                    TotalVentas = 89,
                    Verificado = true,
                    Activo = true,
                    // Datos de pago completos
                    AliasCBU = "dulces.suenos.maria",
                    CBU = "0170012345678901234567",
                    Banco = "Banco Santander",
                    TitularCuenta = "María Elena López",
                    CUIT = "27-30456789-1",
                    DatosPagoCompletos = true,
                    FechaDatosPago = DateTime.Now.AddMonths(-3),
                    TotalCobrado = 890000m,
                    TotalComisiones = 89000m,
                    PendienteCobro = 0
                }
            };

            context.Vendedores.AddRange(vendedores);
            await context.SaveChangesAsync();

            // ==================== 4. COMPRADORES ====================
            var compradores = new List<Comprador>
            {
                new Comprador
                {
                    PersonaId = personas[3].Id, // Juan
                    Direccion = "Av. San Martín 1234",
                    Telefono = "3541112233",
                    Ciudad = "Villa Mercedes",
                    Provincia = "San Luis",
                    CodigoPostal = "5730",
                    TotalCompras = 5,
                    Preferencias = "Chocolate, Sin TACC"
                },
                new Comprador
                {
                    PersonaId = personas[4].Id, // Ana
                    Direccion = "Calle Belgrano 567",
                    Telefono = "3544445566",
                    Ciudad = "Villa Mercedes",
                    Provincia = "San Luis",
                    CodigoPostal = "5730",
                    TotalCompras = 3,
                    Preferencias = "Frutas, Vegano"
                }
            };

            context.Compradores.AddRange(compradores);
            await context.SaveChangesAsync();

            // ==================== 5. TORTAS ====================
            var tortas = new List<Torta>
            {
                // Tortas de Carlos (Vendedor 1)
                new Torta
                {
                    VendedorId = vendedores[0].Id,
                    Nombre = "Torta de Chocolate Belga",
                    Descripcion = "Exquisita torta de chocolate belga con ganache y decoración artesanal. Tres capas de bizcochuelo húmedo bañadas en chocolate.",
                    Precio = 45000m,
                    Stock = 5,
                    Categoria = "Chocolate",
                    Tamanio = "Grande",
                    TiempoPreparacion = 48,
                    Ingredientes = "Chocolate belga, harina, huevos, manteca, azúcar, crema",
                    Personalizable = true,
                    VecesVendida = 78,
                    Calificacion = 4.9m,
                    Disponible = true
                },
                new Torta
                {
                    VendedorId = vendedores[0].Id,
                    Nombre = "Cheesecake New York",
                    Descripcion = "Auténtico cheesecake estilo New York con base de galletas y cobertura de frutos rojos.",
                    Precio = 38000m,
                    Stock = 3,
                    Categoria = "Cheesecake",
                    Tamanio = "Mediana",
                    TiempoPreparacion = 24,
                    Ingredientes = "Queso crema, huevos, azúcar, vainilla, galletas, manteca, frutos rojos",
                    Personalizable = true,
                    VecesVendida = 45,
                    Calificacion = 4.8m,
                    Disponible = true
                },
                new Torta
                {
                    VendedorId = vendedores[0].Id,
                    Nombre = "Brownie Tower",
                    Descripcion = "Torre de brownies con nueces y chocolate, perfecta para compartir.",
                    Precio = 28000m,
                    Stock = 8,
                    Categoria = "Chocolate",
                    Tamanio = "Mediana",
                    TiempoPreparacion = 12,
                    Ingredientes = "Chocolate, nueces, harina, huevos, manteca, azúcar",
                    Personalizable = false,
                    VecesVendida = 32,
                    Calificacion = 4.7m,
                    Disponible = true
                },
                // Tortas de María (Vendedor 2)
                new Torta
                {
                    VendedorId = vendedores[1].Id,
                    Nombre = "Torta de Frutillas con Crema",
                    Descripcion = "Delicada torta de vainilla con capas de crema chantilly y frutillas frescas de estación.",
                    Precio = 42000m,
                    Stock = 4,
                    Categoria = "Frutas",
                    Tamanio = "Grande",
                    TiempoPreparacion = 24,
                    Ingredientes = "Frutillas, crema, bizcochuelo de vainilla, azúcar, gelatina",
                    Personalizable = true,
                    VecesVendida = 56,
                    Calificacion = 4.9m,
                    Disponible = true
                },
                new Torta
                {
                    VendedorId = vendedores[1].Id,
                    Nombre = "Selva Negra",
                    Descripcion = "Clásica torta alemana con chocolate, cerezas y crema. Una combinación perfecta.",
                    Precio = 48000m,
                    Stock = 2,
                    Categoria = "Chocolate",
                    Tamanio = "Grande",
                    TiempoPreparacion = 36,
                    Ingredientes = "Chocolate, cerezas, crema, kirsch, bizcochuelo",
                    Personalizable = true,
                    VecesVendida = 28,
                    Calificacion = 4.8m,
                    Disponible = true
                },
                new Torta
                {
                    VendedorId = vendedores[1].Id,
                    Nombre = "Lemon Pie",
                    Descripcion = "Tarta de limón con merengue italiano tostado. Refrescante y deliciosa.",
                    Precio = 32000m,
                    Stock = 6,
                    Categoria = "Cítricos",
                    Tamanio = "Mediana",
                    TiempoPreparacion = 12,
                    Ingredientes = "Limones, huevos, azúcar, manteca, masa quebrada",
                    Personalizable = false,
                    VecesVendida = 19,
                    Calificacion = 4.6m,
                    Disponible = true
                }
            };

            context.Tortas.AddRange(tortas);
            await context.SaveChangesAsync();

            // ==================== 6. IMÁGENES DE TORTAS ====================
            var imagenes = new List<ImagenTorta>
            {
                // Torta Chocolate Belga
                new ImagenTorta { TortaId = tortas[0].Id, UrlImagen = "https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=600", NombreArchivo = "chocolate_belga.jpg", EsPrincipal = true, Orden = 1 },
                // Cheesecake
                new ImagenTorta { TortaId = tortas[1].Id, UrlImagen = "https://images.unsplash.com/photo-1533134242443-d4fd215305ad?w=600", NombreArchivo = "cheesecake.jpg", EsPrincipal = true, Orden = 1 },
                // Brownie
                new ImagenTorta { TortaId = tortas[2].Id, UrlImagen = "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=600", NombreArchivo = "brownie.jpg", EsPrincipal = true, Orden = 1 },
                // Frutillas
                new ImagenTorta { TortaId = tortas[3].Id, UrlImagen = "https://images.unsplash.com/photo-1565958011703-44f9829ba187?w=600", NombreArchivo = "frutillas.jpg", EsPrincipal = true, Orden = 1 },
                // Selva Negra
                new ImagenTorta { TortaId = tortas[4].Id, UrlImagen = "https://images.unsplash.com/photo-1602351447937-745cb720612f?w=600", NombreArchivo = "selva_negra.jpg", EsPrincipal = true, Orden = 1 },
                // Lemon Pie
                new ImagenTorta { TortaId = tortas[5].Id, UrlImagen = "https://images.unsplash.com/photo-1519915028121-7d3463d20b13?w=600", NombreArchivo = "lemon_pie.jpg", EsPrincipal = true, Orden = 1 }
            };

            context.ImagenesTorta.AddRange(imagenes);
            await context.SaveChangesAsync();

            // ==================== 7. VENTAS DE EJEMPLO ====================
            var ventas = new List<Venta>
            {
                // Venta completada (con pago verificado)
                new Venta
                {
                    CompradorId = compradores[0].Id,
                    NumeroOrden = $"ORD-{DateTime.Now.AddDays(-15):yyyyMMdd}-001",
                    FechaVenta = DateTime.Now.AddDays(-15),
                    Estado = EstadoVenta.Entregada,
                    Subtotal = 83000m,
                    DescuentoTotal = 0,
                    Total = 83000m,
                    PorcentajeComision = 10.00m,
                    ComisionPlataforma = 8300m,
                    MontoVendedores = 74700m,
                    FondosLiberados = true,
                    FechaLiberacion = DateTime.Now.AddDays(-10),
                    DireccionEntrega = "Av. San Martín 1234, Villa Mercedes, San Luis",
                    Ciudad = "Villa Mercedes",
                    Provincia = "San Luis",
                    CodigoPostal = "5730",
                    FechaEntregaEstimada = DateTime.Now.AddDays(-12),
                    FechaEntregaReal = DateTime.Now.AddDays(-12)
                },
                // Venta pendiente de pago
                new Venta
                {
                    CompradorId = compradores[1].Id,
                    NumeroOrden = $"ORD-{DateTime.Now:yyyyMMdd}-002",
                    FechaVenta = DateTime.Now.AddHours(-2),
                    Estado = EstadoVenta.Pendiente,
                    Subtotal = 90000m,
                    DescuentoTotal = 0,
                    Total = 90000m,
                    PorcentajeComision = 10.00m,
                    ComisionPlataforma = 9000m,
                    MontoVendedores = 81000m,
                    FondosLiberados = false,
                    DireccionEntrega = "Calle Belgrano 567, Villa Mercedes, San Luis",
                    Ciudad = "Villa Mercedes",
                    Provincia = "San Luis",
                    CodigoPostal = "5730",
                    FechaEntregaEstimada = DateTime.Now.AddDays(3)
                },
                // Venta con pago en revisión
                new Venta
                {
                    CompradorId = compradores[0].Id,
                    NumeroOrden = $"ORD-{DateTime.Now.AddDays(-1):yyyyMMdd}-003",
                    FechaVenta = DateTime.Now.AddDays(-1),
                    Estado = EstadoVenta.PagoEnRevision,
                    Subtotal = 45000m,
                    DescuentoTotal = 0,
                    Total = 45000m,
                    PorcentajeComision = 10.00m,
                    ComisionPlataforma = 4500m,
                    MontoVendedores = 40500m,
                    FondosLiberados = false,
                    DireccionEntrega = "Av. San Martín 1234, Villa Mercedes, San Luis",
                    Ciudad = "Villa Mercedes",
                    Provincia = "San Luis",
                    CodigoPostal = "5730",
                    FechaEntregaEstimada = DateTime.Now.AddDays(2)
                }
            };

            context.Ventas.AddRange(ventas);
            await context.SaveChangesAsync();

            // ==================== 8. DETALLES DE VENTA ====================
            var detalles = new List<DetalleVenta>
            {
                // Detalles de Venta 1 (completada)
                new DetalleVenta
                {
                    VentaId = ventas[0].Id,
                    TortaId = tortas[0].Id, // Chocolate Belga de Carlos
                    VendedorId = vendedores[0].Id,
                    Cantidad = 1,
                    PrecioUnitario = 45000m,
                    Subtotal = 45000m,
                    Estado = EstadoDetalleVenta.Entregado
                },
                new DetalleVenta
                {
                    VentaId = ventas[0].Id,
                    TortaId = tortas[1].Id, // Cheesecake de Carlos
                    VendedorId = vendedores[0].Id,
                    Cantidad = 1,
                    PrecioUnitario = 38000m,
                    Subtotal = 38000m,
                    Estado = EstadoDetalleVenta.Entregado
                },
                // Detalles de Venta 2 (pendiente) - Multi-vendedor
                new DetalleVenta
                {
                    VentaId = ventas[1].Id,
                    TortaId = tortas[3].Id, // Frutillas de María
                    VendedorId = vendedores[1].Id,
                    Cantidad = 1,
                    PrecioUnitario = 42000m,
                    Subtotal = 42000m,
                    Estado = EstadoDetalleVenta.Pendiente
                },
                new DetalleVenta
                {
                    VentaId = ventas[1].Id,
                    TortaId = tortas[4].Id, // Selva Negra de María
                    VendedorId = vendedores[1].Id,
                    Cantidad = 1,
                    PrecioUnitario = 48000m,
                    Subtotal = 48000m,
                    Estado = EstadoDetalleVenta.Pendiente
                },
                // Detalles de Venta 3 (pago en revisión)
                new DetalleVenta
                {
                    VentaId = ventas[2].Id,
                    TortaId = tortas[0].Id, // Chocolate Belga de Carlos
                    VendedorId = vendedores[0].Id,
                    Cantidad = 1,
                    PrecioUnitario = 45000m,
                    Subtotal = 45000m,
                    Estado = EstadoDetalleVenta.Pendiente
                }
            };

            context.DetallesVenta.AddRange(detalles);
            await context.SaveChangesAsync();

            // ==================== 9. PAGOS DE EJEMPLO ====================
            var pagos = new List<Pago>
            {
                // Pago completado (Venta 1)
                new Pago
                {
                    VentaId = ventas[0].Id,
                    CompradorId = compradores[0].Id,
                    Monto = 83000m,
                    ComisionPlataforma = 8300m,
                    MontoVendedores = 74700m,
                    FechaPago = DateTime.Now.AddDays(-15),
                    Estado = EstadoPago.Completado,
                    MetodoPago = MetodoPago.MercadoPago,
                    ArchivoComprobante = "/uploads/comprobantes/pago_001.jpg",
                    NumeroTransaccion = "MP-2025-001234",
                    FechaComprobante = DateTime.Now.AddDays(-15),
                    FechaVerificacion = DateTime.Now.AddDays(-15),
                    VerificadoPorId = personas[0].Id, // Admin
                    ObservacionesAdmin = "Pago verificado correctamente"
                },
                // Pago en revisión (Venta 3)
                new Pago
                {
                    VentaId = ventas[2].Id,
                    CompradorId = compradores[0].Id,
                    Monto = 45000m,
                    ComisionPlataforma = 4500m,
                    MontoVendedores = 40500m,
                    FechaPago = DateTime.Now.AddDays(-1),
                    Estado = EstadoPago.EnRevision,
                    MetodoPago = MetodoPago.Transferencia,
                    ArchivoComprobante = "/uploads/comprobantes/pago_003.jpg",
                    NumeroTransaccion = "TRF-2025-005678",
                    FechaComprobante = DateTime.Now.AddDays(-1)
                }
            };

            context.Pagos.AddRange(pagos);
            await context.SaveChangesAsync();

            // ==================== 10. LIBERACIONES DE EJEMPLO ====================
            var liberaciones = new List<LiberacionFondos>
            {
                // Liberación completada (de Venta 1)
                new LiberacionFondos
                {
                    VentaId = ventas[0].Id,
                    VendedorId = vendedores[0].Id, // Carlos
                    MontoBruto = 83000m,
                    Comision = 8300m,
                    MontoNeto = 74700m,
                    Estado = EstadoLiberacion.Confirmado,
                    FechaCreacion = DateTime.Now.AddDays(-15),
                    FechaListoParaLiberar = DateTime.Now.AddDays(-12),
                    FechaTransferencia = DateTime.Now.AddDays(-10),
                    FechaConfirmacion = DateTime.Now.AddDays(-10),
                    NumeroOperacion = "LIB-2025-0001",
                    ArchivoComprobante = "/uploads/liberaciones/lib_001.jpg",
                    AliasDestino = "pasteleria.doncarlos.mp",
                    CBUDestino = "0140012301234567890123",
                    TitularDestino = "Carlos Alberto González",
                    ProcesadoPorId = personas[0].Id
                }
            };

            context.LiberacionesFondos.AddRange(liberaciones);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Base de datos inicializada con datos de prueba");
            Console.WriteLine($"   - 1 Configuración de plataforma");
            Console.WriteLine($"   - {personas.Count} Personas");
            Console.WriteLine($"   - {vendedores.Count} Vendedores (con datos de pago)");
            Console.WriteLine($"   - {compradores.Count} Compradores");
            Console.WriteLine($"   - {tortas.Count} Tortas");
            Console.WriteLine($"   - {imagenes.Count} Imágenes");
            Console.WriteLine($"   - {ventas.Count} Ventas");
            Console.WriteLine($"   - {detalles.Count} Detalles de venta");
            Console.WriteLine($"   - {pagos.Count} Pagos");
            Console.WriteLine($"   - {liberaciones.Count} Liberaciones");
        }
    }
}
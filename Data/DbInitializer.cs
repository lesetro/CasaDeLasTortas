using CasaDeLasTortas.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasaDeLasTortas.Data
{
    /// <summary>
    /// Inicializador de base de datos
    /// Maneja datos semilla según el entorno (Desarrollo/Producción)
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Inicializa la base de datos según el entorno
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        /// <param name="isDevelopment">True para desarrollo, False para producción</param>
        public static async Task InitializeAsync(ApplicationDbContext context, bool isDevelopment = false)
        {
            // 1. Aplicar migraciones (siempre necesario)
            await context.Database.MigrateAsync();

            // 2. En producción, solo datos esenciales
            if (!isDevelopment)
            {
                await SeedProductionDataAsync(context);
                Console.WriteLine("✅ Base de datos de PRODUCCIÓN inicializada (solo datos esenciales)");
                return;
            }

            // 3. En desarrollo, datos completos de prueba
            await SeedDevelopmentDataAsync(context);
            Console.WriteLine("✅ Base de datos de DESARROLLO inicializada con datos semilla completos");
        }

        /// <summary>
        /// Datos mínimos esenciales para producción
        /// </summary>
        private static async Task SeedProductionDataAsync(ApplicationDbContext context)
        {
            // Solo crear usuario administrador si no existe
            if (!await context.Personas.AnyAsync(p => p.Rol == "Admin"))
            {
                var admin = new Persona
                {
                    Nombre = "Administrador Sistema",
                    Apellido = "Casa de las Tortas",
                    Email = "admin@casadelastortas.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("CambiarPassword123!"),
                    Rol = "Admin",
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };

                await context.Personas.AddAsync(admin);
                await context.SaveChangesAsync();

                Console.WriteLine("⚠️  USUARIO ADMINISTRADOR CREADO PARA PRODUCCIÓN");
                Console.WriteLine("⚠️  EMAIL: admin@casadelastortas.com");
                Console.WriteLine("⚠️  CONTRASEÑA TEMPORAL: CambiarPassword123!");
                Console.WriteLine("⚠️  CAMBIA LA CONTRASEÑA INMEDIATAMENTE!");
            }
        }

        /// <summary>
        /// Datos completos de prueba para desarrollo
        /// </summary>
        private static async Task SeedDevelopmentDataAsync(ApplicationDbContext context)
        {
            // Si ya hay datos, no hacer nada
            if (await context.Personas.AnyAsync())
            {
                Console.WriteLine("ℹ️  Base de datos ya contiene datos, omitiendo inicialización");
                return;
            }

            // ==================== PERSONAS ====================
            var personas = new List<Persona>
            {
                // Administrador
                new Persona
                {
                    Nombre = "Admin",
                    Apellido = "Sistema",
                    Email = "admin@casadelastortas.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Telefono = "2664123456",
                    Rol = "Admin",
                    FechaRegistro = DateTime.Now,
                    Activo = true
                },
                // Vendedores
                new Persona
                {
                    Nombre = "María",
                    Apellido = "González",
                    Email = "maria.gonzalez@torteria.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Vendedor123!"),
                    Telefono = "2664234567",
                    Rol = "Vendedor",
                    FechaRegistro = DateTime.Now.AddMonths(-6),
                    Activo = true
                },
                new Persona
                {
                    Nombre = "Carlos",
                    Apellido = "Rodríguez",
                    Email = "carlos.rodriguez@reposteria.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Vendedor123!"),
                    Telefono = "2664345678",
                    Rol = "Vendedor",
                    FechaRegistro = DateTime.Now.AddMonths(-4),
                    Activo = true
                },
                new Persona
                {
                    Nombre = "Ana",
                    Apellido = "Martínez",
                    Email = "ana.martinez@pasteleria.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Vendedor123!"),
                    Telefono = "2664456789",
                    Rol = "Vendedor",
                    FechaRegistro = DateTime.Now.AddMonths(-3),
                    Activo = true
                },
                // Compradores
                new Persona
                {
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    Email = "juan.perez@email.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Comprador123!"),
                    Telefono = "2664567890",
                    Rol = "Comprador",
                    FechaRegistro = DateTime.Now.AddMonths(-2),
                    Activo = true
                },
                new Persona
                {
                    Nombre = "Laura",
                    Apellido = "Fernández",
                    Email = "laura.fernandez@email.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Comprador123!"),
                    Telefono = "2664678901",
                    Rol = "Comprador",
                    FechaRegistro = DateTime.Now.AddMonths(-1),
                    Activo = true
                },
                new Persona
                {
                    Nombre = "Diego",
                    Apellido = "López",
                    Email = "diego.lopez@email.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Comprador123!"),
                    Telefono = "2664789012",
                    Rol = "Comprador",
                    FechaRegistro = DateTime.Now.AddDays(-15),
                    Activo = true
                }
            };

            await context.Personas.AddRangeAsync(personas);
            await context.SaveChangesAsync();

            // ==================== VENDEDORES ====================
            var vendedores = new List<Vendedor>
            {
                new Vendedor
                {
                    PersonaId = personas[1].Id, // María González
                    NombreComercial = "Tortería María",
                    Especialidad = "Tortas de Chocolate y Crema",
                    Descripcion = "Especialistas en tortas artesanales con más de 10 años de experiencia. Usamos ingredientes premium y recetas tradicionales.",
                    Calificacion = 4.8m,
                    TotalVentas = 150,
                    Horario = "Lunes a Sábado: 9:00 - 20:00",
                    FechaCreacion = DateTime.Now.AddMonths(-6),
                    Activo = true,
                    Verificado = true
                },
                new Vendedor
                {
                    PersonaId = personas[2].Id, // Carlos Rodríguez
                    NombreComercial = "Repostería Don Carlos",
                    Especialidad = "Tortas de Frutas y Cheesecakes",
                    Descripcion = "Creaciones únicas con frutas frescas de estación. Cada torta es una obra de arte.",
                    Calificacion = 4.5m,
                    TotalVentas = 95,
                    Horario = "Martes a Domingo: 10:00 - 19:00",
                    FechaCreacion = DateTime.Now.AddMonths(-4),
                    Activo = true,
                    Verificado = true
                },
                new Vendedor
                {
                    PersonaId = personas[3].Id, // Ana Martínez
                    NombreComercial = "Pastelería Anita",
                    Especialidad = "Tortas Temáticas Infantiles",
                    Descripcion = "Diseños personalizados para cumpleaños infantiles. Trabajamos con fondant y decoraciones 3D.",
                    Calificacion = 4.9m,
                    TotalVentas = 120,
                    Horario = "Lunes a Viernes: 8:00 - 18:00",
                    FechaCreacion = DateTime.Now.AddMonths(-3),
                    Activo = true,
                    Verificado = true
                }
            };

            await context.Vendedores.AddRangeAsync(vendedores);
            await context.SaveChangesAsync();

            // ==================== COMPRADORES ====================
            var compradores = new List<Comprador>
            {
                new Comprador
                {
                    PersonaId = personas[4].Id, // Juan Pérez
                    Direccion = "Barrio Centro 111, Villa Mercedes",
                    Telefono = "2664567890",
                    Ciudad = "Villa Mercedes",
                    Provincia = "San Luis",
                    CodigoPostal = "5730",
                    TotalCompras = 5,
                    FechaCreacion = DateTime.Now.AddMonths(-2),
                    Activo = true
                },
                new Comprador
                {
                    PersonaId = personas[5].Id, // Laura Fernández
                    Direccion = "Barrio Norte 222, Villa Mercedes",
                    Telefono = "2664678901",
                    Ciudad = "Villa Mercedes",
                    Provincia = "San Luis",
                    CodigoPostal = "5730",
                    TotalCompras = 3,
                    FechaCreacion = DateTime.Now.AddMonths(-1),
                    Activo = true
                },
                new Comprador
                {
                    PersonaId = personas[6].Id, // Diego López
                    Direccion = "Barrio Sur 333, Villa Mercedes",
                    Telefono = "2664789012",
                    Ciudad = "Villa Mercedes",
                    Provincia = "San Luis",
                    CodigoPostal = "5730",
                    TotalCompras = 2,
                    FechaCreacion = DateTime.Now.AddDays(-15),
                    Activo = true
                }
            };

            await context.Compradores.AddRangeAsync(compradores);
            await context.SaveChangesAsync();

            // ==================== TORTAS ====================
            var tortas = new List<Torta>
            {
                // Tortas de Tortería María
                new Torta
                {
                    VendedorId = vendedores[0].Id,
                    Nombre = "Torta de Chocolate con Dulce de Leche",
                    Descripcion = "Deliciosa torta de chocolate con capas de dulce de leche artesanal y cobertura de ganache.",
                    Categoria = "Chocolate",
                    Precio = 3500.00m,
                    Stock = 5,
                    Tamanio = "Grande",
                    Ingredientes = "Chocolate belga, dulce de leche, harina, huevos, manteca",
                    TiempoPreparacion = 2,
                    Personalizable = true,
                    Disponible = true,
                    FechaCreacion = DateTime.Now.AddDays(-30)
                },
                new Torta
                {
                    VendedorId = vendedores[0].Id,
                    Nombre = "Torta Chocotorta Clásica",
                    Descripcion = "La tradicional chocotorta con chocolinas, dulce de leche y crema. Un clásico argentino.",
                    Categoria = "Chocolate",
                    Precio = 2800.00m,
                    Stock = 8,
                    Tamanio = "Mediana",
                    Ingredientes = "Chocolinas, dulce de leche, queso crema, crema de leche",
                    TiempoPreparacion = 1,
                    Personalizable = false,
                    Disponible = true,
                    FechaCreacion = DateTime.Now.AddDays(-25)
                },
                new Torta
                {
                    VendedorId = vendedores[0].Id,
                    Nombre = "Torta Red Velvet",
                    Descripcion = "Esponjosa torta red velvet con frosting de queso crema. Perfecta para ocasiones especiales.",
                    Categoria = "Especiales",
                    Precio = 4200.00m,
                    Stock = 3,
                    Tamanio = "Grande",
                    Ingredientes = "Harina, cacao, buttermilk, queso crema, azúcar",
                    TiempoPreparacion = 2,
                    Personalizable = true,
                    Disponible = true,
                    FechaCreacion = DateTime.Now.AddDays(-20)
                },
                // Tortas de Repostería Don Carlos
                new Torta
                {
                    VendedorId = vendedores[1].Id,
                    Nombre = "Cheesecake de Frutos Rojos",
                    Descripcion = "Cremoso cheesecake horneado con coulis de frutos rojos frescos.",
                    Categoria = "Cheesecakes",
                    Precio = 3800.00m,
                    Stock = 4,
                    Tamanio = "Mediana",
                    Ingredientes = "Queso crema Philadelphia, frutos rojos, galletas, azúcar",
                    TiempoPreparacion = 3,
                    Personalizable = false,
                    Disponible = true,
                    FechaCreacion = DateTime.Now.AddDays(-18)
                },
                new Torta
                {
                    VendedorId = vendedores[1].Id,
                    Nombre = "Torta de Limón y Merengue",
                    Descripcion = "Torta de limón con relleno cítrico y merengue italiano flameado.",
                    Categoria = "Frutas",
                    Precio = 3200.00m,
                    Stock = 6,
                    Tamanio = "Mediana",
                    Ingredientes = "Limones, huevos, azúcar, harina, manteca",
                    TiempoPreparacion = 2,
                    Personalizable = false,
                    Disponible = true,
                    FechaCreacion = DateTime.Now.AddDays(-15)
                },
                // Tortas de Pastelería Anita
                new Torta
                {
                    VendedorId = vendedores[2].Id,
                    Nombre = "Torta Unicornio",
                    Descripcion = "Torta decorada con fondant en colores pastel, cuerno dorado y detalles mágicos. Ideal para cumpleaños infantiles.",
                    Categoria = "Infantiles",
                    Precio = 4500.00m,
                    Stock = 2,
                    Tamanio = "Grande",
                    Ingredientes = "Bizcochuelo de vainilla, dulce de leche, fondant, decoraciones comestibles",
                    TiempoPreparacion = 3,
                    Personalizable = true,
                    Disponible = true,
                    FechaCreacion = DateTime.Now.AddDays(-10)
                },
                new Torta
                {
                    VendedorId = vendedores[2].Id,
                    Nombre = "Torta Paw Patrol",
                    Descripcion = "Torta temática de Paw Patrol con personajes en 3D de fondant.",
                    Categoria = "Infantiles",
                    Precio = 5000.00m,
                    Stock = 1,
                    Tamanio = "Extra Grande",
                    Ingredientes = "Bizcochuelo de chocolate, relleno de dulce de leche, fondant",
                    TiempoPreparacion = 4,
                    Personalizable = true,
                    Disponible = true,
                    FechaCreacion = DateTime.Now.AddDays(-7)
                }
            };

            await context.Tortas.AddRangeAsync(tortas);
            await context.SaveChangesAsync();

            // ==================== IMÁGENES ====================
            var imagenes = new List<ImagenTorta>();
            foreach (var torta in tortas)
            {
                imagenes.Add(new ImagenTorta
                {
                    TortaId = torta.Id,
                    UrlImagen = $"/images/tortas/torta_{torta.Id}_1.jpg",
                    NombreArchivo = $"torta_{torta.Id}_1.jpg",
                    EsPrincipal = true,
                    Orden = 1,
                    FechaSubida = DateTime.Now.AddDays(-5)
                });

                // Agregar imagen secundaria para algunas tortas
                if (torta.Id % 2 == 0)
                {
                    imagenes.Add(new ImagenTorta
                    {
                        TortaId = torta.Id,
                        UrlImagen = $"/images/tortas/torta_{torta.Id}_2.jpg",
                        NombreArchivo = $"torta_{torta.Id}_2.jpg",
                        EsPrincipal = false,
                        Orden = 2,
                        FechaSubida = DateTime.Now.AddDays(-5)
                    });
                }
            }

            await context.ImagenesTorta.AddRangeAsync(imagenes);
            await context.SaveChangesAsync();

            // ==================== PAGOS (solo en desarrollo) ====================
            var pagos = new List<Pago>
            {
                new Pago
                {
                    TortaId = tortas[0].Id,
                    CompradorId = compradores[0].Id,
                    VendedorId = vendedores[0].Id,
                    Monto = 3500.00m,
                    Cantidad = 1,
                    PrecioUnitario = 3500.00m,
                    Subtotal = 3500.00m,
                    MetodoPago = MetodoPago.Transferencia,
                    Estado = EstadoPago.Completado,
                    FechaPago = DateTime.Now.AddDays(-20),
                    NumeroTransaccion = "TRF-001-2024"
                },
                new Pago
                {
                    TortaId = tortas[3].Id,
                    CompradorId = compradores[1].Id,
                    VendedorId = vendedores[1].Id,
                    Monto = 3800.00m,
                    Cantidad = 1,
                    PrecioUnitario = 3800.00m,
                    Subtotal = 3800.00m,
                    MetodoPago = MetodoPago.MercadoPago,
                    Estado = EstadoPago.Completado,
                    FechaPago = DateTime.Now.AddDays(-15),
                    NumeroTransaccion = "MP-002-2024"
                },
                new Pago
                {
                    TortaId = tortas[5].Id,
                    CompradorId = compradores[2].Id,
                    VendedorId = vendedores[2].Id,
                    Monto = 4500.00m,
                    Cantidad = 1,
                    PrecioUnitario = 4500.00m,
                    Subtotal = 4500.00m,
                    MetodoPago = MetodoPago.Efectivo,
                    Estado = EstadoPago.Pendiente,
                    FechaPago = DateTime.Now.AddDays(-2)
                }
            };

            await context.Pagos.AddRangeAsync(pagos);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ DESARROLLO: {personas.Count} personas, {vendedores.Count} vendedores, {compradores.Count} compradores, {tortas.Count} tortas, {pagos.Count} pagos creados");
        }
    }
}
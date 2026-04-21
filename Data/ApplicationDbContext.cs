using Microsoft.EntityFrameworkCore;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Data
{
    /// <summary>
    /// Contexto de base de datos principal de la aplicación
    /// Configurado para usar MariaDB con Pomelo.EntityFrameworkCore.MySql
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ==================== DBSETS ====================
        
        public DbSet<Persona> Personas { get; set; } = null!;
        public DbSet<Vendedor> Vendedores { get; set; } = null!;
        public DbSet<Comprador> Compradores { get; set; } = null!;
        public DbSet<Torta> Tortas { get; set; } = null!;
        public DbSet<ImagenTorta> ImagenesTorta { get; set; } = null!;
        public DbSet<Venta> Ventas { get; set; } = null!;
        public DbSet<DetalleVenta> DetallesVenta { get; set; } = null!;
        public DbSet<Pago> Pagos { get; set; } = null!;
        
        
        public DbSet<LiberacionFondos> LiberacionesFondos { get; set; } = null!;
        public DbSet<Disputa> Disputas { get; set; } = null!;
        public DbSet<MensajeDisputa> MensajesDisputa { get; set; } = null!;
        public DbSet<ConfiguracionPlataforma> ConfiguracionPlataforma { get; set; } = null!;

        // ==================== CONFIGURACIÓN DEL MODELO ====================
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================== PERSONA ====================
            modelBuilder.Entity<Persona>(entity =>
            {
                entity.ToTable("Personas");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.Rol)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("varchar(20)")
                    .HasDefaultValue("Comprador");

                entity.Property(e => e.Telefono)
                    .HasMaxLength(20)
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.FechaRegistro)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UltimoAcceso)
                    .HasColumnType("datetime");

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Personas_Email");

                entity.HasIndex(e => e.Rol)
                    .HasDatabaseName("IX_Personas_Rol");
            });

            // ==================== VENDEDOR (MODIFICADO) ====================
            modelBuilder.Entity<Vendedor>(entity =>
            {
                entity.ToTable("Vendedores");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.NombreComercial)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.Especialidad)
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.Calificacion)
                    .HasPrecision(3, 2)
                    .HasDefaultValue(0.00m);

                entity.Property(e => e.TotalVentas)
                    .HasDefaultValue(0);

                entity.Property(e => e.Horario)
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                //  CAMPOS DE PAGO
                entity.Property(e => e.AliasCBU)
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.CBU)
                    .HasMaxLength(22)
                    .HasColumnType("varchar(22)");

                entity.Property(e => e.Banco)
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.TitularCuenta)
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.CUIT)
                    .HasMaxLength(13)
                    .HasColumnType("varchar(13)");

                entity.Property(e => e.ImagenQR)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.DatosPagoCompletos)
                    .HasDefaultValue(false);

                entity.Property(e => e.FechaDatosPago)
                    .HasColumnType("datetime");

                //  CAMPOS DE ESTADÍSTICAS
                entity.Property(e => e.TotalCobrado)
                    .HasPrecision(12, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.TotalComisiones)
                    .HasPrecision(12, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.PendienteCobro)
                    .HasPrecision(12, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Verificado)
                    .HasDefaultValue(false);

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.HasOne(e => e.Persona)
                    .WithOne(p => p.Vendedor)
                    .HasForeignKey<Vendedor>(e => e.PersonaId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Vendedores_Personas");

                entity.HasIndex(e => e.PersonaId)
                    .IsUnique()
                    .HasDatabaseName("IX_Vendedores_PersonaId");

                entity.HasIndex(e => e.NombreComercial)
                    .HasDatabaseName("IX_Vendedores_NombreComercial");

                entity.HasIndex(e => e.DatosPagoCompletos)
                    .HasDatabaseName("IX_Vendedores_DatosPagoCompletos");
            });

            // ==================== COMPRADOR ====================
            modelBuilder.Entity<Comprador>(entity =>
            {
                entity.ToTable("Compradores");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Direccion)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.Telefono)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Ciudad)
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Provincia)
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.CodigoPostal)
                    .HasMaxLength(10)
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.TotalCompras)
                    .HasDefaultValue(0);

                entity.Property(e => e.FechaNacimiento)
                    .HasColumnType("date");

                entity.Property(e => e.Preferencias)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.HasOne(e => e.Persona)
                    .WithOne(p => p.Comprador)
                    .HasForeignKey<Comprador>(e => e.PersonaId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Compradores_Personas");

                entity.HasIndex(e => e.PersonaId)
                    .IsUnique()
                    .HasDatabaseName("IX_Compradores_PersonaId");

                entity.HasIndex(e => e.Ciudad)
                    .HasDatabaseName("IX_Compradores_Ciudad");
            });

            // ==================== TORTA ====================
            modelBuilder.Entity<Torta>(entity =>
            {
                entity.ToTable("Tortas");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.Descripcion)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.Precio)
                    .IsRequired()
                    .HasPrecision(10, 2);

                entity.Property(e => e.Stock)
                    .HasDefaultValue(0);

                entity.Property(e => e.Categoria)
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Tamanio)
                    .HasMaxLength(20)
                    .HasColumnType("varchar(20)")
                    .HasDefaultValue("Mediana");

                entity.Property(e => e.Ingredientes)
                    .HasColumnType("text");

                entity.Property(e => e.Personalizable)
                    .HasDefaultValue(false);

                entity.Property(e => e.VecesVendida)
                    .HasDefaultValue(0);

                entity.Property(e => e.Calificacion)
                    .HasPrecision(3, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.Disponible)
                    .HasDefaultValue(true);

                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.FechaActualizacion)
                    .HasColumnType("datetime");

                entity.HasOne(e => e.Vendedor)
                    .WithMany(v => v.Tortas)
                    .HasForeignKey(e => e.VendedorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Tortas_Vendedores");

                entity.HasIndex(e => e.VendedorId)
                    .HasDatabaseName("IX_Tortas_VendedorId");

                entity.HasIndex(e => e.Categoria)
                    .HasDatabaseName("IX_Tortas_Categoria");

                entity.HasIndex(e => e.Precio)
                    .HasDatabaseName("IX_Tortas_Precio");

                entity.HasIndex(e => e.Disponible)
                    .HasDatabaseName("IX_Tortas_Disponible");
            });

            // ==================== IMAGEN TORTA ====================
            modelBuilder.Entity<ImagenTorta>(entity =>
            {
                entity.ToTable("ImagenesTorta");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.UrlImagen)
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.NombreArchivo)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.TipoArchivo)
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.EsPrincipal)
                    .HasDefaultValue(false);

                entity.Property(e => e.Orden)
                    .HasDefaultValue(0);

                entity.Property(e => e.FechaSubida)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Torta)
                    .WithMany(t => t.Imagenes)
                    .HasForeignKey(e => e.TortaId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ImagenesTorta_Tortas");

                entity.HasIndex(e => e.TortaId)
                    .HasDatabaseName("IX_ImagenesTorta_TortaId");

                entity.HasIndex(e => new { e.TortaId, e.EsPrincipal })
                    .HasDatabaseName("IX_ImagenesTorta_TortaId_EsPrincipal");
            });

            // ==================== VENTA (MODIFICADO) ====================
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.ToTable("Ventas");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.NumeroOrden)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.FechaVenta)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(EstadoVenta.Pendiente);

                entity.Property(e => e.Subtotal)
                    .IsRequired()
                    .HasPrecision(12, 2);

                entity.Property(e => e.DescuentoTotal)
                    .HasPrecision(10, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.Total)
                    .IsRequired()
                    .HasPrecision(12, 2);

                //  CAMPOS DE COMISIÓN
                entity.Property(e => e.PorcentajeComision)
                    .HasPrecision(5, 2)
                    .HasDefaultValue(10.00m);

                entity.Property(e => e.ComisionPlataforma)
                    .HasPrecision(10, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.MontoVendedores)
                    .HasPrecision(12, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.FondosLiberados)
                    .HasDefaultValue(false);

                entity.Property(e => e.FechaLiberacion)
                    .HasColumnType("datetime");

                entity.Property(e => e.DireccionEntrega)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.Ciudad)
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Provincia)
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.CodigoPostal)
                    .HasMaxLength(10)
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.FechaEntregaEstimada)
                    .HasColumnType("datetime");

                entity.Property(e => e.FechaEntregaReal)
                    .HasColumnType("datetime");

                entity.Property(e => e.NotasCliente)
                    .HasColumnType("text");

                entity.Property(e => e.NotasInternas)
                    .HasColumnType("text");

                entity.Property(e => e.FechaActualizacion)
                    .HasColumnType("datetime");

                entity.Property(e => e.RequiereFactura)
                    .HasDefaultValue(false);

                entity.Property(e => e.CUITCliente)
                    .HasMaxLength(13)
                    .HasColumnType("varchar(13)");

                entity.Property(e => e.RazonSocial)
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.HasOne(e => e.Comprador)
                    .WithMany(c => c.Ventas)
                    .HasForeignKey(e => e.CompradorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Ventas_Compradores");

                entity.HasIndex(e => e.NumeroOrden)
                    .IsUnique()
                    .HasDatabaseName("IX_Ventas_NumeroOrden");

                entity.HasIndex(e => e.CompradorId)
                    .HasDatabaseName("IX_Ventas_CompradorId");

                entity.HasIndex(e => e.Estado)
                    .HasDatabaseName("IX_Ventas_Estado");

                entity.HasIndex(e => e.FechaVenta)
                    .HasDatabaseName("IX_Ventas_FechaVenta");

                entity.HasIndex(e => e.FondosLiberados)
                    .HasDatabaseName("IX_Ventas_FondosLiberados");
            });

            // ==================== DETALLE VENTA ====================
            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.ToTable("DetallesVenta");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Cantidad)
                    .IsRequired();

                entity.Property(e => e.PrecioUnitario)
                    .IsRequired()
                    .HasPrecision(10, 2);

                entity.Property(e => e.Descuento)
                    .HasPrecision(10, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.Subtotal)
                    .IsRequired()
                    .HasPrecision(10, 2);

                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(EstadoDetalleVenta.Pendiente);

                entity.Property(e => e.NotasPersonalizacion)
                    .HasColumnType("text");

                entity.Property(e => e.FechaEstimadaPreparacion)
                    .HasColumnType("datetime");

                entity.Property(e => e.FechaRealPreparacion)
                    .HasColumnType("datetime");

                entity.HasOne(e => e.Venta)
                    .WithMany(v => v.Detalles)
                    .HasForeignKey(e => e.VentaId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DetallesVenta_Ventas");

                entity.HasOne(e => e.Torta)
                    .WithMany(t => t.DetallesVenta)
                    .HasForeignKey(e => e.TortaId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_DetallesVenta_Tortas");

                entity.HasOne(e => e.Vendedor)
                    .WithMany(v => v.DetallesVenta)
                    .HasForeignKey(e => e.VendedorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_DetallesVenta_Vendedores");

                entity.HasIndex(e => e.VentaId)
                    .HasDatabaseName("IX_DetallesVenta_VentaId");

                entity.HasIndex(e => e.TortaId)
                    .HasDatabaseName("IX_DetallesVenta_TortaId");

                entity.HasIndex(e => e.VendedorId)
                    .HasDatabaseName("IX_DetallesVenta_VendedorId");

                entity.HasIndex(e => e.Estado)
                    .HasDatabaseName("IX_DetallesVenta_Estado");
            });

            // ==================== PAGO (MODIFICADO) ====================
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.ToTable("Pagos");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.VentaId)
                    .IsRequired();

                entity.Property(e => e.Monto)
                    .IsRequired()
                    .HasPrecision(12, 2);

                //  CAMPOS DE MONTOS
                entity.Property(e => e.ComisionPlataforma)
                    .HasPrecision(10, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.MontoVendedores)
                    .HasPrecision(12, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.FechaPago)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(EstadoPago.Pendiente);

                entity.Property(e => e.MetodoPago)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.Property(e => e.FechaVerificacion)
                    .HasColumnType("datetime");

                entity.Property(e => e.FechaActualizacion)
                    .HasColumnType("datetime");

                // Comprobante
                entity.Property(e => e.ArchivoComprobante)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.NumeroTransaccion)
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.FechaComprobante)
                    .HasColumnType("datetime");

                // Verificación
                entity.Property(e => e.ObservacionesAdmin)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                //  CAMPOS DE RECHAZO
                entity.Property(e => e.MotivoRechazo)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.FechaRechazo)
                    .HasColumnType("datetime");

                entity.Property(e => e.IntentosRechazados)
                    .HasDefaultValue(0);

                //  CAMPOS DE REEMBOLSO
                entity.Property(e => e.ArchivoReembolso)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.NumeroTransaccionReembolso)
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.FechaReembolso)
                    .HasColumnType("datetime");

                entity.Property(e => e.MotivoReembolso)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.Observaciones)
                    .HasColumnType("text");

                entity.Property(e => e.NotificacionEnviada)
                    .HasDefaultValue(false);

                // Relaciones
                entity.HasOne(e => e.Venta)
                    .WithMany(v => v.Pagos)
                    .HasForeignKey(e => e.VentaId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Pagos_Ventas");

                entity.HasOne(e => e.Comprador)
                    .WithMany(c => c.Pagos)
                    .HasForeignKey(e => e.CompradorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Pagos_Compradores");

                entity.HasOne(e => e.VerificadoPor)
                    .WithMany()
                    .HasForeignKey(e => e.VerificadoPorId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Pagos_Personas_Verificador");

                entity.HasIndex(e => e.VentaId)
                    .HasDatabaseName("IX_Pagos_VentaId");

                entity.HasIndex(e => e.CompradorId)
                    .HasDatabaseName("IX_Pagos_CompradorId");

                entity.HasIndex(e => e.Estado)
                    .HasDatabaseName("IX_Pagos_Estado");

                entity.HasIndex(e => e.FechaPago)
                    .HasDatabaseName("IX_Pagos_FechaPago");

                entity.HasIndex(e => e.NumeroTransaccion)
                    .HasDatabaseName("IX_Pagos_NumeroTransaccion");
            });

            // ==================== LIBERACIÓN DE FONDOS (NUEVO) ====================
            modelBuilder.Entity<LiberacionFondos>(entity =>
            {
                entity.ToTable("LiberacionesFondos");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.MontoBruto)
                    .IsRequired()
                    .HasPrecision(12, 2);

                entity.Property(e => e.Comision)
                    .IsRequired()
                    .HasPrecision(10, 2);

                entity.Property(e => e.MontoNeto)
                    .IsRequired()
                    .HasPrecision(12, 2);

                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(EstadoLiberacion.Pendiente);

                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.FechaListoParaLiberar)
                    .HasColumnType("datetime");

                entity.Property(e => e.FechaTransferencia)
                    .HasColumnType("datetime");

                entity.Property(e => e.FechaConfirmacion)
                    .HasColumnType("datetime");

                entity.Property(e => e.NumeroOperacion)
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.ArchivoComprobante)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.AliasDestino)
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.CBUDestino)
                    .HasMaxLength(22)
                    .HasColumnType("varchar(22)");

                entity.Property(e => e.TitularDestino)
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.Observaciones)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                // Relaciones
                entity.HasOne(e => e.Venta)
                    .WithMany()
                    .HasForeignKey(e => e.VentaId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LiberacionesFondos_Ventas");

                entity.HasOne(e => e.Vendedor)
                    .WithMany(v => v.Liberaciones)
                    .HasForeignKey(e => e.VendedorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LiberacionesFondos_Vendedores");

                entity.HasOne(e => e.ProcesadoPor)
                    .WithMany()
                    .HasForeignKey(e => e.ProcesadoPorId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_LiberacionesFondos_Personas");

                // Índices
                entity.HasIndex(e => e.VentaId)
                    .HasDatabaseName("IX_LiberacionesFondos_VentaId");

                entity.HasIndex(e => e.VendedorId)
                    .HasDatabaseName("IX_LiberacionesFondos_VendedorId");

                entity.HasIndex(e => e.Estado)
                    .HasDatabaseName("IX_LiberacionesFondos_Estado");

                entity.HasIndex(e => e.FechaCreacion)
                    .HasDatabaseName("IX_LiberacionesFondos_FechaCreacion");
            });

            // ==================== DISPUTA (NUEVO) ====================
            modelBuilder.Entity<Disputa>(entity =>
            {
                entity.ToTable("Disputas");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.NumeroDisputa)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Tipo)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(30);

                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(30)
                    .HasDefaultValue(EstadoDisputa.Abierta);

                entity.Property(e => e.Resolucion)
                    .HasConversion<string>()
                    .HasMaxLength(30)
                    .HasDefaultValue(ResolucionDisputa.SinResolucion);

                entity.Property(e => e.Titulo)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.Descripcion)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.Evidencia)
                    .HasColumnType("text");

                entity.Property(e => e.MontoInvolucrado)
                    .HasPrecision(12, 2);

                entity.Property(e => e.MontoResolucion)
                    .HasPrecision(12, 2);

                entity.Property(e => e.DetalleResolucion)
                    .HasColumnType("text");

                entity.Property(e => e.Prioridad)
                    .HasDefaultValue(3);

                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.FechaActualizacion)
                    .HasColumnType("datetime");

                entity.Property(e => e.FechaResolucion)
                    .HasColumnType("datetime");

                entity.Property(e => e.FechaLimite)
                    .HasColumnType("datetime");

                // Relaciones
                entity.HasOne(e => e.Venta)
                    .WithMany(v => v.Disputas)
                    .HasForeignKey(e => e.VentaId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Disputas_Ventas");

                entity.HasOne(e => e.Iniciador)
                    .WithMany()
                    .HasForeignKey(e => e.IniciadorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Disputas_Personas_Iniciador");

                entity.HasOne(e => e.AdminAsignado)
                    .WithMany()
                    .HasForeignKey(e => e.AdminAsignadoId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Disputas_Personas_Admin");

                // Índices
                entity.HasIndex(e => e.NumeroDisputa)
                    .IsUnique()
                    .HasDatabaseName("IX_Disputas_NumeroDisputa");

                entity.HasIndex(e => e.VentaId)
                    .HasDatabaseName("IX_Disputas_VentaId");

                entity.HasIndex(e => e.Estado)
                    .HasDatabaseName("IX_Disputas_Estado");

                entity.HasIndex(e => e.Prioridad)
                    .HasDatabaseName("IX_Disputas_Prioridad");

                entity.HasIndex(e => e.FechaCreacion)
                    .HasDatabaseName("IX_Disputas_FechaCreacion");
            });

            // ==================== MENSAJE DISPUTA (NUEVO) ====================
            modelBuilder.Entity<MensajeDisputa>(entity =>
            {
                entity.ToTable("MensajesDisputa");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Contenido)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.Adjuntos)
                    .HasColumnType("text");

                entity.Property(e => e.Fecha)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.EsInterno)
                    .HasDefaultValue(false);

                // Relaciones
                entity.HasOne(e => e.Disputa)
                    .WithMany(d => d.Mensajes)
                    .HasForeignKey(e => e.DisputaId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_MensajesDisputa_Disputas");

                entity.HasOne(e => e.Autor)
                    .WithMany()
                    .HasForeignKey(e => e.AutorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MensajesDisputa_Personas");

                // Índices
                entity.HasIndex(e => e.DisputaId)
                    .HasDatabaseName("IX_MensajesDisputa_DisputaId");

                entity.HasIndex(e => e.Fecha)
                    .HasDatabaseName("IX_MensajesDisputa_Fecha");
            });

            // ==================== CONFIGURACIÓN PLATAFORMA (NUEVO) ====================
            modelBuilder.Entity<ConfiguracionPlataforma>(entity =>
            {
                entity.ToTable("ConfiguracionPlataforma");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.NombrePlataforma)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.LogoUrl)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.AliasCBU)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.CBU)
                    .IsRequired()
                    .HasMaxLength(22)
                    .HasColumnType("varchar(22)");

                entity.Property(e => e.Banco)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.TitularCuenta)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.CUIT)
                    .IsRequired()
                    .HasMaxLength(13)
                    .HasColumnType("varchar(13)");

                entity.Property(e => e.ImagenQR)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.ComisionPorcentaje)
                    .HasPrecision(5, 2)
                    .HasDefaultValue(10.00m);

                entity.Property(e => e.ComisionMinima)
                    .HasPrecision(10, 2)
                    .HasDefaultValue(0);

                entity.Property(e => e.ComisionMaxima)
                    .HasPrecision(10, 2);

                entity.Property(e => e.DiasParaLiberarFondos)
                    .HasDefaultValue(3);

                entity.Property(e => e.DiasLimitePago)
                    .HasDefaultValue(3);

                entity.Property(e => e.MaxIntentosRechazados)
                    .HasDefaultValue(3);

                entity.Property(e => e.DiasLimiteDisputa)
                    .HasDefaultValue(15);

                entity.Property(e => e.EmailNotificaciones)
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.TelefonoContacto)
                    .HasMaxLength(20)
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.InstruccionesPago)
                    .HasColumnType("text");

                entity.Property(e => e.TerminosCondiciones)
                    .HasColumnType("text");

                entity.Property(e => e.PlataformaActiva)
                    .HasDefaultValue(true);

                entity.Property(e => e.MensajeMantenimiento)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.FechaActualizacion)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }

        // ==================== MÉTODOS AUXILIARES ====================

        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void AddTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Torta torta && entry.State == EntityState.Modified)
                {
                    torta.FechaActualizacion = DateTime.Now;
                }

                if (entry.Entity is Venta venta && entry.State == EntityState.Modified)
                {
                    venta.FechaActualizacion = DateTime.Now;
                }

                if (entry.Entity is Pago pago && entry.State == EntityState.Modified)
                {
                    pago.FechaActualizacion = DateTime.Now;
                }

                if (entry.Entity is Disputa disputa && entry.State == EntityState.Modified)
                {
                    disputa.FechaActualizacion = DateTime.Now;
                }

                if (entry.Entity is ConfiguracionPlataforma config && entry.State == EntityState.Modified)
                {
                    config.FechaActualizacion = DateTime.Now;
                }
            }
        }
    }
}
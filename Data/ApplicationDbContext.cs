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
        public DbSet<Pago> Pagos { get; set; } = null!;

        // ==================== CONFIGURACIÓN DEL MODELO ====================
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================== PERSONA ====================
            modelBuilder.Entity<Persona>(entity =>
            {
                entity.ToTable("Personas");
                entity.HasKey(e => e.Id);
                
                // Propiedades
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

                // Índices
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Personas_Email");

                entity.HasIndex(e => e.Rol)
                    .HasDatabaseName("IX_Personas_Rol");
            });

            // ==================== VENDEDOR ====================
            modelBuilder.Entity<Vendedor>(entity =>
            {
                entity.ToTable("Vendedores");
                entity.HasKey(e => e.Id);

                // Propiedades
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

                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Verificado)
                    .HasDefaultValue(false);

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                // Relaciones
                entity.HasOne(e => e.Persona)
                    .WithOne(p => p.Vendedor)
                    .HasForeignKey<Vendedor>(e => e.PersonaId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Vendedores_Personas");

                // Índices
                entity.HasIndex(e => e.PersonaId)
                    .IsUnique()
                    .HasDatabaseName("IX_Vendedores_PersonaId");

                entity.HasIndex(e => e.NombreComercial)
                    .HasDatabaseName("IX_Vendedores_NombreComercial");
            });

            // ==================== COMPRADOR ====================
            modelBuilder.Entity<Comprador>(entity =>
            {
                entity.ToTable("Compradores");
                entity.HasKey(e => e.Id);

                // Propiedades
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

                // Relaciones
                entity.HasOne(e => e.Persona)
                    .WithOne(p => p.Comprador)
                    .HasForeignKey<Comprador>(e => e.PersonaId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Compradores_Personas");

                // Índices
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

                // Propiedades
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

                entity.Property(e => e.TiempoPreparacion);

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

                // Relaciones
                entity.HasOne(e => e.Vendedor)
                    .WithMany(v => v.Tortas)
                    .HasForeignKey(e => e.VendedorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Tortas_Vendedores");

                // Índices
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

                // Propiedades
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

                entity.Property(e => e.TamanioArchivo);

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

                // Relaciones
                entity.HasOne(e => e.Torta)
                    .WithMany(t => t.Imagenes)
                    .HasForeignKey(e => e.TortaId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ImagenesTorta_Tortas");

                // Índices
                entity.HasIndex(e => e.TortaId)
                    .HasDatabaseName("IX_ImagenesTorta_TortaId");

                entity.HasIndex(e => new { e.TortaId, e.EsPrincipal })
                    .HasDatabaseName("IX_ImagenesTorta_TortaId_EsPrincipal");
            });

            // ==================== PAGO ====================
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.ToTable("Pagos");
                entity.HasKey(e => e.Id);

                // Propiedades
                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Monto)
                    .IsRequired()
                    .HasPrecision(10, 2);

                entity.Property(e => e.PrecioUnitario)
                    .IsRequired()
                    .HasPrecision(10, 2);

                entity.Property(e => e.Cantidad)
                    .HasDefaultValue(1);

                entity.Property(e => e.Subtotal)
                    .HasPrecision(10, 2);

                entity.Property(e => e.Descuento)
                    .HasPrecision(10, 2)
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

                entity.Property(e => e.ArchivoComprobante)
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.NumeroTransaccion)
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.Observaciones)
                    .HasColumnType("text");

                entity.Property(e => e.DireccionEntrega)
                    .HasMaxLength(200)
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.FechaEntrega)
                    .HasColumnType("datetime");

                entity.Property(e => e.FechaActualizacion)
                    .HasColumnType("datetime");

                entity.Property(e => e.NotificacionEnviada)
                    .HasDefaultValue(false);

                // Relaciones
                entity.HasOne(e => e.Torta)
                    .WithMany(t => t.Pagos)
                    .HasForeignKey(e => e.TortaId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Pagos_Tortas");

                entity.HasOne(e => e.Comprador)
                    .WithMany(c => c.Pagos)
                    .HasForeignKey(e => e.CompradorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Pagos_Compradores");

                entity.HasOne(e => e.Vendedor)
                    .WithMany(v => v.PagosRecibidos)
                    .HasForeignKey(e => e.VendedorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Pagos_Vendedores");

                // Índices
                entity.HasIndex(e => e.TortaId)
                    .HasDatabaseName("IX_Pagos_TortaId");

                entity.HasIndex(e => e.CompradorId)
                    .HasDatabaseName("IX_Pagos_CompradorId");

                entity.HasIndex(e => e.VendedorId)
                    .HasDatabaseName("IX_Pagos_VendedorId");

                entity.HasIndex(e => e.Estado)
                    .HasDatabaseName("IX_Pagos_Estado");

                entity.HasIndex(e => e.FechaPago)
                    .HasDatabaseName("IX_Pagos_FechaPago");

                entity.HasIndex(e => e.NumeroTransaccion)
                    .HasDatabaseName("IX_Pagos_NumeroTransaccion");
            });
        }

        // ==================== MÉTODOS AUXILIARES ====================

        /// <summary>
        /// Configura el timestamp automático para auditoría
        /// </summary>
        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Configura el timestamp automático para auditoría (async)
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Agrega timestamps automáticos a las entidades
        /// </summary>
        private void AddTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // Para Tortas, actualizar FechaActualizacion
                if (entry.Entity is Torta torta && entry.State == EntityState.Modified)
                {
                    torta.FechaActualizacion = DateTime.Now;
                }

                // Para Pagos, actualizar FechaActualizacion
                if (entry.Entity is Pago pago && entry.State == EntityState.Modified)
                {
                    pago.FechaActualizacion = DateTime.Now;
                }
            }
        }
    }
}
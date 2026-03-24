using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasaDeLasTortas.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Apellido = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dni = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Direccion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Avatar = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rol = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Comprador")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UltimoAcceso = table.Column<DateTime>(type: "datetime", nullable: true),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Compradores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    Direccion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ciudad = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Provincia = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodigoPostal = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalCompras = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FechaNacimiento = table.Column<DateTime>(type: "date", nullable: true),
                    Preferencias = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compradores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compradores_Personas",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Vendedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    NombreComercial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Especialidad = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Calificacion = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false, defaultValue: 0.00m),
                    TotalVentas = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Horario = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Verificado = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendedores_Personas",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompradorId = table.Column<int>(type: "int", nullable: false),
                    NumeroOrden = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaVenta = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DescuentoTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    Total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DireccionEntrega = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ciudad = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Provincia = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodigoPostal = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaEntregaEstimada = table.Column<DateTime>(type: "datetime", nullable: true),
                    FechaEntregaReal = table.Column<DateTime>(type: "datetime", nullable: true),
                    NotasCliente = table.Column<string>(type: "text", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NotasInternas = table.Column<string>(type: "text", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    RequiereFactura = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    RucOCedula = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RazonSocial = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_Compradores",
                        column: x => x.CompradorId,
                        principalTable: "Compradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tortas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Categoria = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tamanio = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Mediana")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TiempoPreparacion = table.Column<int>(type: "int", nullable: true),
                    Ingredientes = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Personalizable = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    VecesVendida = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Calificacion = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false, defaultValue: 0m),
                    Disponible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tortas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tortas_Vendedores",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    CompradorId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MetodoPago = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ArchivoComprobante = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroTransaccion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    ConfirmadoPorVendedorId = table.Column<int>(type: "int", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    NotificacionEnviada = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Compradores",
                        column: x => x.CompradorId,
                        principalTable: "Compradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_Vendedores_Confirmacion",
                        column: x => x.ConfirmadoPorVendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Pagos_Ventas",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DetallesVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    TortaId = table.Column<int>(type: "int", nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NotasPersonalizacion = table.Column<string>(type: "text", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaEstimadaPreparacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    FechaRealPreparacion = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesVenta_Tortas",
                        column: x => x.TortaId,
                        principalTable: "Tortas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesVenta_Vendedores",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesVenta_Ventas",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ImagenesTorta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TortaId = table.Column<int>(type: "int", nullable: false),
                    UrlImagen = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NombreArchivo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TamanioArchivo = table.Column<long>(type: "bigint", nullable: true),
                    TipoArchivo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EsPrincipal = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Orden = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FechaSubida = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagenesTorta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImagenesTorta_Tortas",
                        column: x => x.TortaId,
                        principalTable: "Tortas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Compradores_Ciudad",
                table: "Compradores",
                column: "Ciudad");

            migrationBuilder.CreateIndex(
                name: "IX_Compradores_PersonaId",
                table: "Compradores",
                column: "PersonaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVenta_Estado",
                table: "DetallesVenta",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVenta_TortaId",
                table: "DetallesVenta",
                column: "TortaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVenta_VendedorId",
                table: "DetallesVenta",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVenta_VentaId",
                table: "DetallesVenta",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesTorta_TortaId",
                table: "ImagenesTorta",
                column: "TortaId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesTorta_TortaId_EsPrincipal",
                table: "ImagenesTorta",
                columns: new[] { "TortaId", "EsPrincipal" });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_CompradorId",
                table: "Pagos",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ConfirmadoPorVendedorId",
                table: "Pagos",
                column: "ConfirmadoPorVendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Estado",
                table: "Pagos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_FechaPago",
                table: "Pagos",
                column: "FechaPago");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_NumeroTransaccion",
                table: "Pagos",
                column: "NumeroTransaccion");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_VentaId",
                table: "Pagos",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Email",
                table: "Personas",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Rol",
                table: "Personas",
                column: "Rol");

            migrationBuilder.CreateIndex(
                name: "IX_Tortas_Categoria",
                table: "Tortas",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Tortas_Disponible",
                table: "Tortas",
                column: "Disponible");

            migrationBuilder.CreateIndex(
                name: "IX_Tortas_Precio",
                table: "Tortas",
                column: "Precio");

            migrationBuilder.CreateIndex(
                name: "IX_Tortas_VendedorId",
                table: "Tortas",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_NombreComercial",
                table: "Vendedores",
                column: "NombreComercial");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_PersonaId",
                table: "Vendedores",
                column: "PersonaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CompradorId",
                table: "Ventas",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Estado",
                table: "Ventas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_FechaVenta",
                table: "Ventas",
                column: "FechaVenta");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_NumeroOrden",
                table: "Ventas",
                column: "NumeroOrden",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesVenta");

            migrationBuilder.DropTable(
                name: "ImagenesTorta");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Tortas");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "Vendedores");

            migrationBuilder.DropTable(
                name: "Compradores");

            migrationBuilder.DropTable(
                name: "Personas");
        }
    }
}

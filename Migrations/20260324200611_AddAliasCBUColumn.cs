using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasaDeLasTortas.Migrations
{
    /// <inheritdoc />
    public partial class AddAliasCBUColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Vendedores_Confirmacion",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "RucOCedula",
                table: "Ventas");

            migrationBuilder.RenameColumn(
                name: "FechaConfirmacion",
                table: "Pagos",
                newName: "FechaVerificacion");

            migrationBuilder.RenameColumn(
                name: "ConfirmadoPorVendedorId",
                table: "Pagos",
                newName: "VerificadoPorId");

            migrationBuilder.RenameIndex(
                name: "IX_Pagos_ConfirmadoPorVendedorId",
                table: "Pagos",
                newName: "IX_Pagos_VerificadoPorId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "Ventas",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Subtotal",
                table: "Ventas",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "CUITCliente",
                table: "Ventas",
                type: "varchar(13)",
                maxLength: 13,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "ComisionPlataforma",
                table: "Ventas",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLiberacion",
                table: "Ventas",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FondosLiberados",
                table: "Ventas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoVendedores",
                table: "Ventas",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeComision",
                table: "Ventas",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 10.00m);

            migrationBuilder.AddColumn<string>(
                name: "AliasCBU",
                table: "Vendedores",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Banco",
                table: "Vendedores",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CBU",
                table: "Vendedores",
                type: "varchar(22)",
                maxLength: 22,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CUIT",
                table: "Vendedores",
                type: "varchar(13)",
                maxLength: 13,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "DatosPagoCompletos",
                table: "Vendedores",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDatosPago",
                table: "Vendedores",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagenQR",
                table: "Vendedores",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "PendienteCobro",
                table: "Vendedores",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TitularCuenta",
                table: "Vendedores",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCobrado",
                table: "Vendedores",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalComisiones",
                table: "Vendedores",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Monto",
                table: "Pagos",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "ArchivoReembolso",
                table: "Pagos",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "ComisionPlataforma",
                table: "Pagos",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaComprobante",
                table: "Pagos",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRechazo",
                table: "Pagos",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaReembolso",
                table: "Pagos",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntentosRechazados",
                table: "Pagos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoVendedores",
                table: "Pagos",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                table: "Pagos",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MotivoReembolso",
                table: "Pagos",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NumeroTransaccionReembolso",
                table: "Pagos",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ObservacionesAdmin",
                table: "Pagos",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "VendedorId",
                table: "Pagos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConfiguracionPlataforma",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NombrePlataforma = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LogoUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AliasCBU = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CBU = table.Column<string>(type: "varchar(22)", maxLength: 22, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Banco = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TitularCuenta = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CUIT = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImagenQR = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ComisionPorcentaje = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 10.00m),
                    ComisionMinima = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    ComisionMaxima = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    DiasParaLiberarFondos = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    DiasLimitePago = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    MaxIntentosRechazados = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    DiasLimiteDisputa = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    EmailNotificaciones = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TelefonoContacto = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstruccionesPago = table.Column<string>(type: "text", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TerminosCondiciones = table.Column<string>(type: "text", maxLength: 5000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlataformaActiva = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    MensajeMantenimiento = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ActualizadoPorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionPlataforma", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Disputas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    IniciadorId = table.Column<int>(type: "int", nullable: false),
                    AdminAsignadoId = table.Column<int>(type: "int", nullable: true),
                    NumeroDisputa = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "Abierta")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Resolucion = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "SinResolucion")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Titulo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "text", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Evidencia = table.Column<string>(type: "text", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MontoInvolucrado = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    MontoResolucion = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    FechaResolucion = table.Column<DateTime>(type: "datetime", nullable: true),
                    FechaLimite = table.Column<DateTime>(type: "datetime", nullable: true),
                    DetalleResolucion = table.Column<string>(type: "text", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Prioridad = table.Column<int>(type: "int", nullable: false, defaultValue: 3)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disputas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disputas_Personas_Admin",
                        column: x => x.AdminAsignadoId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Disputas_Personas_Iniciador",
                        column: x => x.IniciadorId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Disputas_Ventas",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LiberacionesFondos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    ProcesadoPorId = table.Column<int>(type: "int", nullable: true),
                    MontoBruto = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Comision = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MontoNeto = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    FechaListoParaLiberar = table.Column<DateTime>(type: "datetime", nullable: true),
                    FechaTransferencia = table.Column<DateTime>(type: "datetime", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    NumeroOperacion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ArchivoComprobante = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AliasDestino = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CBUDestino = table.Column<string>(type: "varchar(22)", maxLength: 22, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TitularDestino = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiberacionesFondos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiberacionesFondos_Personas",
                        column: x => x.ProcesadoPorId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LiberacionesFondos_Vendedores",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LiberacionesFondos_Ventas",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MensajesDisputa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DisputaId = table.Column<int>(type: "int", nullable: false),
                    AutorId = table.Column<int>(type: "int", nullable: false),
                    Contenido = table.Column<string>(type: "text", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Adjuntos = table.Column<string>(type: "text", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fecha = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EsInterno = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensajesDisputa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MensajesDisputa_Disputas",
                        column: x => x.DisputaId,
                        principalTable: "Disputas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MensajesDisputa_Personas",
                        column: x => x.AutorId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_FondosLiberados",
                table: "Ventas",
                column: "FondosLiberados");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_DatosPagoCompletos",
                table: "Vendedores",
                column: "DatosPagoCompletos");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_VendedorId",
                table: "Pagos",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputas_AdminAsignadoId",
                table: "Disputas",
                column: "AdminAsignadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputas_Estado",
                table: "Disputas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Disputas_FechaCreacion",
                table: "Disputas",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Disputas_IniciadorId",
                table: "Disputas",
                column: "IniciadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputas_NumeroDisputa",
                table: "Disputas",
                column: "NumeroDisputa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Disputas_Prioridad",
                table: "Disputas",
                column: "Prioridad");

            migrationBuilder.CreateIndex(
                name: "IX_Disputas_VentaId",
                table: "Disputas",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_LiberacionesFondos_Estado",
                table: "LiberacionesFondos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_LiberacionesFondos_FechaCreacion",
                table: "LiberacionesFondos",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_LiberacionesFondos_ProcesadoPorId",
                table: "LiberacionesFondos",
                column: "ProcesadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_LiberacionesFondos_VendedorId",
                table: "LiberacionesFondos",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_LiberacionesFondos_VentaId",
                table: "LiberacionesFondos",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesDisputa_AutorId",
                table: "MensajesDisputa",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesDisputa_DisputaId",
                table: "MensajesDisputa",
                column: "DisputaId");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesDisputa_Fecha",
                table: "MensajesDisputa",
                column: "Fecha");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Personas_Verificador",
                table: "Pagos",
                column: "VerificadoPorId",
                principalTable: "Personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Vendedores_VendedorId",
                table: "Pagos",
                column: "VendedorId",
                principalTable: "Vendedores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Personas_Verificador",
                table: "Pagos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Vendedores_VendedorId",
                table: "Pagos");

            migrationBuilder.DropTable(
                name: "ConfiguracionPlataforma");

            migrationBuilder.DropTable(
                name: "LiberacionesFondos");

            migrationBuilder.DropTable(
                name: "MensajesDisputa");

            migrationBuilder.DropTable(
                name: "Disputas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_FondosLiberados",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Vendedores_DatosPagoCompletos",
                table: "Vendedores");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_VendedorId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "CUITCliente",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "ComisionPlataforma",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "FechaLiberacion",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "FondosLiberados",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "MontoVendedores",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "PorcentajeComision",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "AliasCBU",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "Banco",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "CBU",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "CUIT",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "DatosPagoCompletos",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "FechaDatosPago",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "ImagenQR",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "PendienteCobro",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "TitularCuenta",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "TotalCobrado",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "TotalComisiones",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "ArchivoReembolso",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "ComisionPlataforma",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "FechaComprobante",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "FechaRechazo",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "FechaReembolso",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "IntentosRechazados",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "MontoVendedores",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "MotivoRechazo",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "MotivoReembolso",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "NumeroTransaccionReembolso",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "ObservacionesAdmin",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "VendedorId",
                table: "Pagos");

            migrationBuilder.RenameColumn(
                name: "VerificadoPorId",
                table: "Pagos",
                newName: "ConfirmadoPorVendedorId");

            migrationBuilder.RenameColumn(
                name: "FechaVerificacion",
                table: "Pagos",
                newName: "FechaConfirmacion");

            migrationBuilder.RenameIndex(
                name: "IX_Pagos_VerificadoPorId",
                table: "Pagos",
                newName: "IX_Pagos_ConfirmadoPorVendedorId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "Ventas",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Subtotal",
                table: "Ventas",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "RucOCedula",
                table: "Ventas",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "Monto",
                table: "Pagos",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Vendedores_Confirmacion",
                table: "Pagos",
                column: "ConfirmadoPorVendedorId",
                principalTable: "Vendedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

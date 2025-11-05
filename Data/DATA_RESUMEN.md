# 📂 CARPETA DATA - RESUMEN Y USO

## 📋 Archivos Incluidos

### 1️⃣ ApplicationDbContext.cs
**Propósito**: Contexto principal de Entity Framework Core para acceso a datos

### 2️⃣ DbInitializer.cs  
**Propósito**: Inicializador de base de datos con datos semilla (seed data)

---

## 🗄️ ApplicationDbContext.cs

### ¿Qué hace?
Es el **punto central** para todas las operaciones de base de datos. Define:
- Los `DbSet<T>` para cada entidad
- Las configuraciones de tablas, columnas, relaciones
- Los índices para optimización
- Las restricciones de integridad referencial

### Características Implementadas:

#### ✅ **6 DbSets Configurados**:
```csharp
DbSet<Persona> Personas
DbSet<Vendedor> Vendedores
DbSet<Comprador> Compradores
DbSet<Torta> Tortas
DbSet<ImagenTorta> ImagenesTorta
DbSet<Pago> Pagos
```

#### ✅ **Configuración Detallada de Cada Entidad**:

**PERSONA**:
- Email único con índice
- Índice en Rol
- Validación de longitudes
- Timestamps automáticos
- Soporte para Avatar

**VENDEDOR**:
- Relación 1:1 con Persona (Cascade Delete)
- NombreComercial indexado
- Calificación con precisión decimal (3,2)
- Campos de verificación y activación

**COMPRADOR**:
- Relación 1:1 con Persona (Cascade Delete)
- Índice en Ciudad
- Historial de compras en TEXT
- Totales calculados

**TORTA**:
- Relación N:1 con Vendedor (Restrict Delete)
- Múltiples índices (Categoría, Precio, Disponible)
- Precisión decimal en Precio (10,2)
- Timestamps de creación y actualización

**IMAGEN_TORTA**:
- Relación N:1 con Torta (Cascade Delete)
- Índice compuesto (IdTorta + EsPrincipal)
- Soporte para múltiples imágenes ordenadas

**PAGO**:
- Relaciones con Torta y Comprador (Restrict Delete)
- Múltiples índices (Estado, Fecha, NumeroTransaccion)
- Precisión decimal en Monto (10,2)

#### ✅ **Optimizaciones para MariaDB**:
- Tipos de columna explícitos (`varchar`, `datetime`, `text`)
- Índices estratégicos para búsquedas comunes
- Foreign Keys con nombres descriptivos
- Valores por defecto en nivel de base de datos
- Charset UTF-8 compatible

#### ✅ **Métodos Auxiliares**:
```csharp
SaveChanges() // Con timestamps automáticos
SaveChangesAsync() // Versión asíncrona
AddTimestamps() // Actualiza FechaActualizacion
```

---

## 🌱 DbInitializer.cs

### ¿Qué hace?
Crea **datos de prueba** automáticamente para desarrollo y testing.

### Métodos Principales:

#### 1️⃣ **InitializeAsync()**
Inicializa la base de datos con datos semilla si está vacía.

```csharp
await DbInitializer.InitializeAsync(context, isDevelopment: true);
```

#### 2️⃣ **MigrateAsync()**
Aplica migraciones pendientes automáticamente.

```csharp
await DbInitializer.MigrateAsync(context);
```

### Datos Semilla Incluidos:

#### 👥 **7 Personas**:
1. **Admin** (`admin@casadelastortas.com` / `Admin123!`)
2. **María González** - Vendedor (Tortería María)
3. **Carlos Rodríguez** - Vendedor (Repostería Don Carlos)
4. **Ana Martínez** - Vendedor (Pastelería Anita)
5. **Juan Pérez** - Comprador
6. **Laura Fernández** - Comprador
7. **Diego López** - Comprador

#### 👨‍🍳 **3 Vendedores**:
- Tortería María (4.8⭐, 150 ventas)
- Repostería Don Carlos (4.5⭐, 95 ventas)
- Pastelería Anita (4.9⭐, 120 ventas)

#### 🛍️ **3 Compradores**:
- Juan: $12,500 gastados, 5 compras
- Laura: $8,900 gastados, 3 compras
- Diego: $6,000 gastados, 2 compras

#### 🍰 **7 Tortas**:
1. Torta de Chocolate con Dulce de Leche ($3,500)
2. Chocotorta Clásica ($2,800)
3. Red Velvet ($4,200)
4. Cheesecake de Frutos Rojos ($3,800)
5. Torta de Limón y Merengue ($3,200)
6. Torta Unicornio ($4,500)
7. Torta Paw Patrol ($5,000)

#### 🖼️ **Imágenes**: 
- 1-2 imágenes por torta
- Una marcada como principal
- Rutas simuladas en `/uploads/tortas/`

#### 💰 **3 Pagos** (solo en desarrollo):
- 2 completados
- 1 pendiente

---

## 🚀 USO EN PROGRAM.CS

### Configuración Completa:

```csharp
using Microsoft.EntityFrameworkCore;
using CasaDeLasTortas.Data;

var builder = WebApplication.CreateBuilder(args);

// ==================== CONFIGURAR BASE DE DATOS ====================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            );
        }
    )
);

// ... otros servicios ...

var app = builder.Build();

// ==================== INICIALIZAR BASE DE DATOS ====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Aplicar migraciones
        await DbInitializer.MigrateAsync(context);
        
        // Inicializar datos semilla (solo en desarrollo)
        if (app.Environment.IsDevelopment())
        {
            await DbInitializer.InitializeAsync(context, isDevelopment: true);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar la base de datos");
    }
}

app.Run();
```

---

## 📊 ESTRUCTURA DE TABLAS CREADAS

```
MariaDB [CasaDeLasTortas]
├── Personas
│   ├── IdPersona (PK, AUTO_INCREMENT)
│   ├── Email (UNIQUE INDEX)
│   ├── Rol (INDEX)
│   └── ... otros campos
├── Vendedores
│   ├── IdVendedor (PK, AUTO_INCREMENT)
│   ├── IdPersona (FK → Personas, UNIQUE INDEX)
│   ├── NombreComercial (INDEX)
│   └── ... otros campos
├── Compradores
│   ├── IdComprador (PK, AUTO_INCREMENT)
│   ├── IdPersona (FK → Personas, UNIQUE INDEX)
│   ├── Ciudad (INDEX)
│   └── ... otros campos
├── Tortas
│   ├── IdTorta (PK, AUTO_INCREMENT)
│   ├── IdVendedor (FK → Vendedores, INDEX)
│   ├── Categoria (INDEX)
│   ├── Precio (INDEX)
│   ├── Disponible (INDEX)
│   └── ... otros campos
├── ImagenesTorta
│   ├── IdImagen (PK, AUTO_INCREMENT)
│   ├── IdTorta (FK → Tortas, INDEX)
│   ├── (IdTorta, EsPrincipal) (COMPOSITE INDEX)
│   └── ... otros campos
└── Pagos
    ├── IdPago (PK, AUTO_INCREMENT)
    ├── IdTorta (FK → Tortas, INDEX)
    ├── IdComprador (FK → Compradores, INDEX)
    ├── Estado (INDEX)
    ├── FechaPago (INDEX)
    ├── NumeroTransaccion (INDEX)
    └── ... otros campos
```

---

## 🔑 CREDENCIALES DE PRUEBA

### Administrador:
- Email: `admin@casadelastortas.com`
- Password: `Admin123!`

### Vendedores:
- María: `maria.gonzalez@torteria.com` / `Vendedor123!`
- Carlos: `carlos.rodriguez@reposteria.com` / `Vendedor123!`
- Ana: `ana.martinez@pasteleria.com` / `Vendedor123!`

### Compradores:
- Juan: `juan.perez@email.com` / `Comprador123!`
- Laura: `laura.fernandez@email.com` / `Comprador123!`
- Diego: `diego.lopez@email.com` / `Comprador123!`

---

## ⚙️ COMANDOS DE MIGRACIÓN

```bash
# Crear migración inicial
dotnet ef migrations add InitialCreate

# Aplicar migraciones
dotnet ef database update

# Ver migraciones pendientes
dotnet ef migrations list

# Revertir última migración
dotnet ef database update PreviousMigrationName

# Eliminar migración (si no se aplicó)
dotnet ef migrations remove

# Generar script SQL
dotnet ef migrations script
```

---

## 🎯 BUENAS PRÁCTICAS

### ✅ Hacer:
- Usar `async/await` siempre
- Aplicar migraciones en producción con scripts SQL
- Usar transacciones para operaciones múltiples
- Mantener índices en columnas de búsqueda frecuente

### ❌ Evitar:
- No usar datos semilla en producción
- No hacer cambios directos en la base de datos
- No ignorar las migraciones pendientes
- No exponer contraseñas en el código

---

## 📈 RENDIMIENTO

### Índices Implementados:
- **Personas**: Email, Rol
- **Vendedores**: IdPersona, NombreComercial
- **Compradores**: IdPersona, Ciudad
- **Tortas**: IdVendedor, Categoria, Precio, Disponible
- **ImagenesTorta**: IdTorta, (IdTorta + EsPrincipal)
- **Pagos**: IdTorta, IdComprador, Estado, FechaPago, NumeroTransaccion

### Optimizaciones:
- Connection pooling automático
- Retry policy configurado (5 intentos)
- Tipos de datos específicos de MariaDB
- Foreign Keys con nombres descriptivos

---

## 🔧 TROUBLESHOOTING

### Error: "Table doesn't exist"
```bash
dotnet ef database update
```

### Error: "Duplicate entry for key 'Email'"
El email ya existe, usar otro email o eliminar el registro duplicado.

### Error: "Connection refused"
Verificar que MariaDB esté corriendo:
```bash
sudo systemctl status mariadb
```

### Limpiar y reiniciar:
```bash
dotnet ef database drop --force
dotnet ef database update
```

---

**✅ Archivos listos para usar con MariaDB**  
**📅 Octubre 2025**
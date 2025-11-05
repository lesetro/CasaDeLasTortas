# 📡 HUBS (SignalR) - GUÍA DE ESTUDIO COMPLETA

## 🎯 ¿Qué es SignalR y para qué sirve?

### El Problema: Comunicación en Tiempo Real

Imagina estas situaciones comunes:
1. Un comprador realiza un pago → El vendedor debe saberlo **inmediatamente**
2. Un vendedor publica una nueva torta → Los compradores deben verla sin refrescar
3. El stock de una torta llega a cero → El vendedor necesita una **alerta instantánea**

**Solución tradicional (HTTP)**: El cliente pregunta cada X segundos "¿hay algo nuevo?"
```
Cliente → Servidor: ¿Hay notificaciones?
Servidor → Cliente: No
[espera 5 segundos]
Cliente → Servidor: ¿Hay notificaciones?
Servidor → Cliente: No
[espera 5 segundos]
Cliente → Servidor: ¿Hay notificaciones?
Servidor → Cliente: Sí, hay 1 pago nuevo
```

**Problemas**:
- ❌ Desperdicia ancho de banda (muchas peticiones vacías)
- ❌ Retraso de hasta X segundos
- ❌ Carga innecesaria en el servidor

**Solución SignalR (WebSockets)**: Conexión persistente bidireccional
```
Cliente ←→ Servidor [CONEXIÓN ABIERTA PERMANENTE]

Cuando pasa algo:
Servidor → Cliente: ¡Pago nuevo! (INSTANTÁNEO)
```

**Ventajas**:
- ✅ Notificaciones instantáneas
- ✅ Sin polling innecesario
- ✅ Menor latencia
- ✅ Comunicación bidireccional

---

## 🌐 ¿Qué es un Hub?

Un **Hub** es como una "sala de chat" en el servidor donde:
- Los clientes se conectan
- El servidor puede enviar mensajes
- Los clientes pueden invocar métodos del servidor
- Los clientes pueden agruparse (grupos)

**Analogía de Radio**:
```
Hub = Estación de radio
├── Clientes conectados = Radios sintonizadas
├── Grupos = Diferentes frecuencias (FM 100.1, FM 102.5)
└── Mensajes = Transmisiones
```

---

## 📄 ARCHIVO: NotificationHub.cs

### 🏗️ Estructura General

```
NotificationHub : Hub
│
├── 🔌 CICLO DE VIDA
│   ├── OnConnectedAsync()     → Cuando un cliente se conecta
│   └── OnDisconnectedAsync()  → Cuando un cliente se desconecta
│
├── 👥 GESTIÓN DE GRUPOS
│   ├── JoinGroupAsync()       → Unirse a un grupo
│   └── LeaveGroupAsync()      → Salir de un grupo
│
├── 💬 MENSAJERÍA
│   ├── SendPrivateMessageAsync() → Mensaje directo usuario a usuario
│   └── PingAsync()               → Mantener conexión viva
│
├── 🔔 NOTIFICACIONES ESPECÍFICAS
│   ├── NotificarNuevoPago()
│   ├── NotificarCambioEstadoPago()
│   ├── NotificarNuevaTorta()
│   ├── NotificarStockBajo()
│   ├── NotificarATodos()
│   └── NotificarAGrupo()
│
└── 🛠️ MÉTODOS AUXILIARES
    ├── GetUserId()
    ├── GetUserName()
    ├── GetUserRole()
    ├── IsUserConnected()
    └── GetConnectionStats()
```

---

## 📖 SECCIÓN 1: CICLO DE VIDA

### OnConnectedAsync()

**¿Qué hace?**
Se ejecuta automáticamente cuando un cliente abre una conexión con el hub.

**Proceso paso a paso**:

```csharp
public override async Task OnConnectedAsync()
{
    // PASO 1: Obtener información del usuario
    var userId = GetUserId();         // "123"
    var connectionId = Context.ConnectionId;  // "abc-xyz-789"
    
    // PASO 2: Registrar la conexión
    _userConnections[userId].Add(connectionId);
    
    // PASO 3: Notificar al cliente
    await Clients.Caller.SendAsync("OnConnected", { ... });
    
    // PASO 4: Agregar a su grupo por rol
    var userRole = GetUserRole();  // "Comprador"
    await Groups.AddToGroupAsync(connectionId, userRole);
}
```

**¿Por qué registrar conexiones?**
Un usuario puede tener **múltiples conexiones** simultáneas:
```
Usuario Juan (ID: 123)
├── Conexión 1: Navegador Chrome (connectionId: abc-123)
├── Conexión 2: App móvil Android (connectionId: def-456)
└── Conexión 3: Tablet iPad (connectionId: ghi-789)
```

Para enviarle notificaciones, debemos conocer todas sus conexiones activas.

**Ejemplo visual**:
```
Cliente conecta:
┌─────────────┐      OnConnectedAsync()      ┌──────────────┐
│   Cliente   │ ────────────────────────────> │   Servidor   │
│  (Browser)  │                               │     Hub      │
└─────────────┘                               └──────────────┘
                                                      │
                                                      ├─ Registra conexión
                                                      ├─ Agrega a grupo "Comprador"
                                                      └─ Envía confirmación
```

---

### OnDisconnectedAsync()

**¿Qué hace?**
Se ejecuta cuando un cliente se desconecta (cierra la ventana, pierde internet, etc.)

**¿Por qué es importante?**
Limpiar las conexiones muertas para no intentar enviar notificaciones a clientes inexistentes.

```csharp
public override async Task OnDisconnectedAsync(Exception? exception)
{
    // Eliminar del diccionario
    _userConnections[userId].Remove(connectionId);
    
    // Si no tiene más conexiones, eliminar usuario
    if (_userConnections[userId].Count == 0)
    {
        _userConnections.Remove(userId);
    }
}
```

**Escenario real**:
```
Usuario cierra el navegador:
┌─────────────┐                              ┌──────────────┐
│   Cliente   │  X  [Conexión cerrada]       │   Servidor   │
│  (Browser)  │                               │     Hub      │
└─────────────┘                               └──────────────┘
                                                      │
                     OnDisconnectedAsync()            │
                     <───────────────────────────────┘
                                                      │
                                                      └─ Limpia conexión del diccionario
```

---

## 📖 SECCIÓN 2: CONTEXTO (Context)

El objeto `Context` contiene información sobre la conexión actual:

```csharp
Context.ConnectionId    // ID único de esta conexión: "abc-123-xyz"
Context.User           // ClaimsPrincipal del usuario autenticado
Context.User.Identity  // Identidad del usuario
```

**Ejemplo de uso**:
```csharp
// Obtener el ID del usuario desde el token JWT
var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
// userId = "123"

// Obtener el nombre
var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
// userName = "Juan Pérez"

// Obtener el rol
var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
// userRole = "Comprador"
```

---

## 📖 SECCIÓN 3: CLIENTES (Clients)

`Clients` es el objeto que usamos para **enviar mensajes** a los clientes conectados.

### Tipos de destinatarios:

**1. Clients.Caller** - Solo al cliente que llamó el método
```csharp
await Clients.Caller.SendAsync("Pong", { message: "Respuesta solo para ti" });
```
**Uso**: Confirmaciones, respuestas personales

**2. Clients.All** - A TODOS los clientes conectados
```csharp
await Clients.All.SendAsync("NotificacionGeneral", {
    titulo: "Mantenimiento programado",
    mensaje: "El sistema estará en mantenimiento mañana"
});
```
**Uso**: Anuncios globales

**3. Clients.User(userId)** - A un usuario específico (todas sus conexiones)
```csharp
await Clients.User("123").SendAsync("NuevoPago", {
    pagoId: 456,
    monto: 3500.00
});
```
**Uso**: Notificaciones personales

**4. Clients.Group(groupName)** - A todos los miembros de un grupo
```csharp
await Clients.Group("Vendedor").SendAsync("NuevaTorta", {
    tortaId: 789,
    nombre: "Torta de Chocolate"
});
```
**Uso**: Notificaciones por rol

**5. Clients.Others** - A todos EXCEPTO el caller
```csharp
await Clients.Others.SendAsync("UsuarioConectado", {
    userId: "123",
    userName: "Juan Pérez"
});
```
**Uso**: Notificar a otros que alguien se conectó

---

## 📖 SECCIÓN 4: GRUPOS (Groups)

Los grupos permiten **organizar** a los clientes conectados.

**¿Por qué usar grupos?**
En lugar de:
```csharp
// ❌ MALO: Iterar y filtrar manualmente
foreach (var userId in _userConnections.Keys)
{
    if (GetUserRole(userId) == "Vendedor")
    {
        await Clients.User(userId).SendAsync(...);
    }
}
```

Puedes:
```csharp
// ✅ BUENO: Enviar al grupo directamente
await Clients.Group("Vendedor").SendAsync(...);
```

### Operaciones con grupos:

**Agregar a un grupo**:
```csharp
await Groups.AddToGroupAsync(Context.ConnectionId, "Vendedor");
```

**Quitar de un grupo**:
```csharp
await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Vendedor");
```

**Enviar a un grupo**:
```csharp
await Clients.Group("Vendedor").SendAsync("Mensaje", datos);
```

### Casos de uso reales:

**1. Grupos por rol**:
```csharp
// En OnConnectedAsync
var role = GetUserRole(); // "Comprador", "Vendedor", "Admin"
await Groups.AddToGroupAsync(Context.ConnectionId, role);
```

**2. Grupos por categoría de interés**:
```csharp
// Cliente se suscribe a categoría
public async Task SuscribirseACategoria(string categoria)
{
    await Groups.AddToGroupAsync(Context.ConnectionId, $"Categoria_{categoria}");
}

// Notificar nueva torta de esa categoría
await Clients.Group("Categoria_Chocolate").SendAsync("NuevaTortaChocolate", ...);
```

**3. Grupos por ubicación**:
```csharp
await Groups.AddToGroupAsync(Context.ConnectionId, $"Ciudad_{ciudad}");
```

---

## 📖 SECCIÓN 5: MÉTODOS PÚBLICOS

Estos métodos pueden ser **llamados por los clientes**.

### JoinGroupAsync()

```csharp
[HubMethodName("JoinGroup")]
public async Task JoinGroupAsync(string groupName)
{
    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    await Clients.Caller.SendAsync("GroupJoined", { group: groupName });
}
```

**Llamada desde JavaScript**:
```javascript
// Cliente se une a un grupo
await connection.invoke("JoinGroup", "Vendedores_VIP");

// Escucha confirmación
connection.on("GroupJoined", (data) => {
    console.log(`Unido al grupo: ${data.group}`);
});
```

---

### SendPrivateMessageAsync()

```csharp
[HubMethodName("SendPrivateMessage")]
public async Task SendPrivateMessageAsync(string toUserId, string message)
{
    var fromUserId = GetUserId();
    
    // Enviar al destinatario
    await Clients.User(toUserId).SendAsync("ReceivePrivateMessage", {
        fromUserId,
        message
    });
    
    // Confirmar al remitente
    await Clients.Caller.SendAsync("MessageSent");
}
```

**Flujo completo**:
```
Usuario A (ID: 123)               Usuario B (ID: 456)
       │                                  │
       ├─ invoke("SendPrivateMessage",   │
       │         "456", "Hola")           │
       │                                  │
       │         Servidor Hub             │
       │              │                   │
       │              ├──────────────────>│
       │              │  "ReceivePrivateMessage"
       │              │                   │
       │<─────────────┤                   │
       │  "MessageSent"                   │
       │                                  │
```

---

## 📖 SECCIÓN 6: NOTIFICACIONES ESPECÍFICAS

Estos métodos son llamados desde el **servidor** (no desde clientes).

### NotificarNuevoPago()

**¿Cuándo se llama?**
Desde el controlador de Pagos cuando se crea un nuevo pago.

```csharp
// En PagoController.cs
[HttpPost]
public async Task<IActionResult> CrearPago(PagoViewModel model)
{
    // 1. Guardar el pago en BD
    var pago = await _unitOfWork.PagoRepository.AddAsync(nuevoPago);
    await _unitOfWork.SaveAsync();
    
    // 2. Notificar en tiempo real
    await _hubContext.Clients
        .User(pago.VendedorId.ToString())
        .SendAsync("NuevoPago", new {
            pagoId = pago.Id,
            monto = pago.Monto
        });
    
    return Ok();
}
```

**Flujo completo**:
```
1. Comprador hace POST /api/pagos
         ↓
2. PagoController crea el pago en BD
         ↓
3. PagoController llama al Hub
         ↓
4. Hub envía notificación al Vendedor
         ↓
5. Navegador del Vendedor recibe notificación
         ↓
6. Se muestra alerta: "¡Nuevo pago de $3,500!"
```

---

### NotificarNuevaTorta()

```csharp
public async Task NotificarNuevaTorta(int tortaId, string nombreTorta, decimal precio)
{
    // Notificar a TODOS los compradores
    await Clients.Group("Comprador").SendAsync("NuevaTorta", new {
        tortaId,
        nombreTorta,
        precio,
        message = $"Nueva torta disponible: {nombreTorta}"
    });
}
```

**Llamada desde TortaController**:
```csharp
[HttpPost]
public async Task<IActionResult> CrearTorta(TortaViewModel model)
{
    var torta = await _unitOfWork.TortaRepository.AddAsync(nuevaTorta);
    await _unitOfWork.SaveAsync();
    
    // Notificar a todos los compradores
    await _hubContext.Clients
        .Group("Comprador")
        .SendAsync("NuevaTorta", new {
            tortaId = torta.Id,
            nombreTorta = torta.Nombre,
            precio = torta.Precio
        });
    
    return Ok();
}
```

---

### NotificarStockBajo()

```csharp
public async Task NotificarStockBajo(int tortaId, string vendedorId, int stockActual)
{
    await Clients.User(vendedorId).SendAsync("StockBajo", new {
        tortaId,
        stockActual,
        nivel = stockActual == 0 ? "critico" : "advertencia"
    });
}
```

**Uso en TortaController cuando se vende**:
```csharp
// Después de confirmar un pago
var torta = await _unitOfWork.TortaRepository.GetByIdAsync(pagoModel.TortaId);
torta.Stock -= pagoModel.Cantidad;
await _unitOfWork.TortaRepository.UpdateAsync(torta);

// Si el stock es bajo, notificar
if (torta.Stock <= 3)
{
    await _hubContext.Clients
        .User(torta.VendedorId.ToString())
        .SendAsync("StockBajo", new {
            tortaId = torta.Id,
            nombreTorta = torta.Nombre,
            stockActual = torta.Stock
        });
}
```

---

## 📖 SECCIÓN 7: DICCIONARIO DE CONEXIONES

```csharp
private static readonly Dictionary<string, HashSet<string>> _userConnections = new();
```

**¿Por qué un Dictionary<string, HashSet<string>>?**

```
_userConnections = {
    "123": ["conn-abc", "conn-def"],  ← Usuario 123 tiene 2 conexiones
    "456": ["conn-ghi"],              ← Usuario 456 tiene 1 conexión
    "789": ["conn-jkl", "conn-mno", "conn-pqr"]  ← Usuario 789 tiene 3 conexiones
}
```

**Estructura de datos**:
- **Key** (string): User ID
- **Value** (HashSet<string>): Conjunto de Connection IDs

**¿Por qué HashSet y no List?**
```csharp
// HashSet: O(1) para Add, Remove, Contains
_userConnections["123"].Add("conn-abc");     // Rápido
_userConnections["123"].Remove("conn-abc");  // Rápido
_userConnections["123"].Contains("conn-abc"); // Rápido

// List: O(n) para Remove y Contains
```

---

## 📖 SECCIÓN 8: THREAD SAFETY

**Problema**: Múltiples clientes pueden conectarse/desconectarse **simultáneamente**.

```csharp
private static readonly object _lock = new();

// ❌ MALO: No thread-safe
_userConnections[userId].Add(connectionId);

// ✅ BUENO: Thread-safe con lock
lock (_lock)
{
    if (!_userConnections.ContainsKey(userId))
    {
        _userConnections[userId] = new HashSet<string>();
    }
    _userConnections[userId].Add(connectionId);
}
```

**¿Qué hace `lock`?**
Asegura que solo un hilo puede ejecutar ese código a la vez.

```
Thread 1: lock(_lock) { ... }  ← Ejecutando
Thread 2: lock(_lock) { ... }  ← Esperando
Thread 3: lock(_lock) { ... }  ← Esperando
```

---

## ⚙️ CONFIGURACIÓN EN PROGRAM.CS

```csharp
// 1. Agregar servicios de SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Solo en desarrollo
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// 2. Agregar contexto del hub como servicio (para usarlo en controladores)
builder.Services.AddSingleton<IHubContext<NotificationHub>>();

var app = builder.Build();

// 3. Mapear el endpoint del hub
app.MapHub<NotificationHub>("/hubs/notifications");
```

**Opciones importantes**:
- `KeepAliveInterval`: Cada cuánto enviar ping (15 segundos)
- `ClientTimeoutInterval`: Cuánto esperar sin respuesta (30 segundos)
- `EnableDetailedErrors`: Errores detallados (solo desarrollo)

---

## 💻 CLIENTE JAVASCRIPT

### Conexión básica:

```javascript
// 1. Importar librería SignalR
import * as signalR from '@microsoft/signalr';

// 2. Crear conexión
const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/notifications', {
        accessTokenFactory: () => localStorage.getItem('authToken')
    })
    .withAutomaticReconnect() // Reconectar automáticamente
    .configureLogging(signalR.LogLevel.Information)
    .build();

// 3. Definir manejadores de eventos
connection.on('NuevoPago', (data) => {
    console.log('Nuevo pago recibido:', data);
    mostrarNotificacion(`Nuevo pago de $${data.monto}`);
});

connection.on('StockBajo', (data) => {
    mostrarAlerta(`Stock bajo: ${data.nombreTorta} (${data.stockActual} unidades)`);
});

// 4. Iniciar conexión
try {
    await connection.start();
    console.log('Conectado a SignalR');
} catch (err) {
    console.error('Error al conectar:', err);
}

// 5. Enviar mensajes al servidor
await connection.invoke('JoinGroup', 'Vendedores_VIP');
await connection.invoke('SendPrivateMessage', '123', 'Hola');
```

### Reconexión automática:

```javascript
connection.onreconnecting((error) => {
    console.log('Reconectando...', error);
    mostrarBannerReconectando();
});

connection.onreconnected((connectionId) => {
    console.log('Reconectado con ID:', connectionId);
    ocultarBannerReconectando();
});

connection.onclose((error) => {
    console.log('Conexión cerrada:', error);
    mostrarBannerDesconectado();
});
```

---

## 🎓 CASOS DE USO COMPLETOS

### Caso 1: Notificación de Nuevo Pago

**Backend (PagoController.cs)**:
```csharp
[HttpPost]
public async Task<IActionResult> CrearPago(PagoViewModel model)
{
    // 1. Crear pago
    var pago = new Pago { ... };
    await _unitOfWork.PagoRepository.AddAsync(pago);
    await _unitOfWork.SaveAsync();
    
    // 2. Notificar al vendedor via SignalR
    await _hubContext.Clients
        .User(pago.VendedorId.ToString())
        .SendAsync("NuevoPago", new {
            pagoId = pago.Id,
            compradorNombre = pago.Comprador.Persona.Nombre,
            monto = pago.Monto,
            tortaNombre = pago.Torta.Nombre
        });
    
    return Ok(new { success = true });
}
```

**Frontend (vendedor-dashboard.js)**:
```javascript
// Escuchar notificaciones de nuevos pagos
connection.on('NuevoPago', (data) => {
    // Mostrar notificación visual
    toastr.success(
        `${data.compradorNombre} ha pagado $${data.monto} por ${data.tortaNombre}`,
        'Nuevo Pago',
        {
            timeOut: 10000,
            onclick: () => window.location.href = `/pagos/details/${data.pagoId}`
        }
    );
    
    // Reproducir sonido
    new Audio('/sounds/notification.mp3').play();
    
    // Actualizar contador en el navbar
    actualizarContadorPagos();
    
    // Agregar a la lista en tiempo real
    agregarPagoALista(data);
});
```

---

### Caso 2: Chat Privado entre Comprador y Vendedor

**Hub**:
```csharp
[HubMethodName("SendChatMessage")]
public async Task SendChatMessageAsync(string toUserId, string message)
{
    var fromUserId = GetUserId();
    var fromUserName = GetUserName();
    
    // Guardar en BD
    var chatMessage = new ChatMessage {
        FromUserId = int.Parse(fromUserId),
        ToUserId = int.Parse(toUserId),
        Message = message,
        Timestamp = DateTime.UtcNow,
        IsRead = false
    };
    await _context.ChatMessages.AddAsync(chatMessage);
    await _context.SaveChangesAsync();
    
    // Enviar en tiempo real
    await Clients.User(toUserId).SendAsync("ReceiveChatMessage", new {
        messageId = chatMessage.Id,
        fromUserId,
        fromUserName,
        message,
        timestamp = chatMessage.Timestamp
    });
}
```

**Frontend (chat.js)**:
```javascript
// Enviar mensaje
async function enviarMensaje() {
    const message = document.getElementById('messageInput').value;
    const toUserId = document.getElementById('toUserId').value;
    
    await connection.invoke('SendChatMessage', toUserId, message);
    
    // Agregar a mi chat
    agregarMensajePropio(message);
    document.getElementById('messageInput').value = '';
}

// Recibir mensajes
connection.on('ReceiveChatMessage', (data) => {
    // Verificar si la ventana de chat está abierta
    if (isChatWindowOpen(data.fromUserId)) {
        agregarMensajeRecibido(data.fromUserName, data.message, data.timestamp);
    } else {
        // Mostrar badge de mensaje nuevo
        mostrarBadgeNuevoMensaje(data.fromUserId);
    }
    
    // Reproducir sonido
    new Audio('/sounds/message.mp3').play();
});
```

---

## 🔒 SEGURIDAD

### 1. Autenticación requerida:
```csharp
[Authorize] // Solo usuarios autenticados
public class NotificationHub : Hub
{
    // ...
}
```

### 2. Validar permisos en métodos:
```csharp
public async Task EliminarTorta(int tortaId)
{
    var userId = GetUserId();
    var torta = await _context.Tortas.FindAsync(tortaId);
    
    // Verificar que el usuario sea el dueño
    if (torta.VendedorId.ToString() != userId)
    {
        throw new HubException("No tienes permiso para eliminar esta torta");
    }
    
    // Proceder con eliminación...
}
```

### 3. Sanitizar input:
```csharp
public async Task SendMessage(string message)
{
    // Prevenir XSS
    message = HtmlEncoder.Default.Encode(message);
    
    // Limitar longitud
    if (message.Length > 500)
    {
        message = message.Substring(0, 500);
    }
    
    // Enviar mensaje sanitizado
    await Clients.All.SendAsync("ReceiveMessage", message);
}
```

### 4. Rate limiting:
```csharp
private static readonly Dictionary<string, DateTime> _lastMessageTime = new();

public async Task SendMessage(string message)
{
    var userId = GetUserId();
    
    // Verificar rate limit (máximo 1 mensaje por segundo)
    if (_lastMessageTime.ContainsKey(userId))
    {
        var elapsed = DateTime.UtcNow - _lastMessageTime[userId];
        if (elapsed.TotalSeconds < 1)
        {
            throw new HubException("Estás enviando mensajes muy rápido. Espera 1 segundo.");
        }
    }
    
    _lastMessageTime[userId] = DateTime.UtcNow;
    
    // Proceder...
}
```

---

## 🚀 ESCALABILIDAD

**Problema**: En producción con múltiples servidores, las conexiones pueden estar en diferentes instancias.

```
Usuario A → Servidor 1 (conexión activa)
Usuario B → Servidor 2 (conexión activa)

Usuario A envía mensaje a Usuario B
→ El mensaje no llega porque están en servidores diferentes
```

**Solución**: Usar un **backplane** (Redis, Azure SignalR Service)

```csharp
// Program.cs
builder.Services.AddSignalR()
    .AddStackExchangeRedis("localhost:6379", options =>
    {
        options.Configuration.ChannelPrefix = "CasaDeLasTortas";
    });
```

Con Redis:
```
Usuario A → Servidor 1 ─┐
                        ├──> Redis (backplane)
Usuario B → Servidor 2 ─┘

Todos los servidores comparten el estado via Redis
```

---

## 📚 CONCEPTOS CLAVE PARA RECORDAR

1. **Hub**: Punto central de comunicación en el servidor
2. **Connection**: Cada cliente tiene un ConnectionId único
3. **Context**: Información de la conexión actual (usuario, ID, etc.)
4. **Clients**: Objeto para enviar mensajes a clientes
5. **Groups**: Organizar clientes en categorías
6. **WebSockets**: Protocolo de comunicación bidireccional
7. **SignalR**: Librería que abstrae WebSockets y fallbacks
8. **Backplane**: Sistema para sincronizar múltiples servidores

---

**✅ Archivo NotificationHub.cs listo para usar con autenticación JWT y grupos por rol**
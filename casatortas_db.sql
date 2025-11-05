-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 05-11-2025 a las 02:19:12
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `casatortas_db`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `compradores`
--

CREATE TABLE `compradores` (
  `Id` int(11) NOT NULL,
  `PersonaId` int(11) NOT NULL,
  `Direccion` varchar(200) NOT NULL,
  `Telefono` varchar(20) NOT NULL,
  `Ciudad` varchar(50) DEFAULT NULL,
  `Provincia` varchar(50) DEFAULT NULL,
  `CodigoPostal` varchar(10) DEFAULT NULL,
  `TotalCompras` int(11) NOT NULL DEFAULT 0,
  `FechaNacimiento` date DEFAULT NULL,
  `Preferencias` varchar(500) DEFAULT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp(),
  `Activo` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `compradores`
--

INSERT INTO `compradores` (`Id`, `PersonaId`, `Direccion`, `Telefono`, `Ciudad`, `Provincia`, `CodigoPostal`, `TotalCompras`, `FechaNacimiento`, `Preferencias`, `FechaCreacion`, `Activo`) VALUES
(1, 5, 'Barrio Centro 111, Villa Mercedes', '2664567890', 'Villa Mercedes', 'San Luis', '5730', 5, NULL, NULL, '2025-09-01 17:09:30', 1),
(2, 6, 'Barrio Norte 222, Villa Mercedes', '2664678901', 'Villa Mercedes', 'San Luis', '5730', 3, NULL, NULL, '2025-10-01 17:09:30', 1),
(3, 7, 'Barrio Sur 333, Villa Mercedes', '2664789012', 'Villa Mercedes', 'San Luis', '5730', 2, NULL, NULL, '2025-10-17 17:09:30', 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `imagenestorta`
--

CREATE TABLE `imagenestorta` (
  `Id` int(11) NOT NULL,
  `TortaId` int(11) NOT NULL,
  `UrlImagen` varchar(500) NOT NULL,
  `NombreArchivo` varchar(255) NOT NULL,
  `TamanioArchivo` bigint(20) DEFAULT NULL,
  `TipoArchivo` varchar(50) DEFAULT NULL,
  `EsPrincipal` tinyint(1) NOT NULL DEFAULT 0,
  `Orden` int(11) NOT NULL DEFAULT 0,
  `FechaSubida` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `imagenestorta`
--

INSERT INTO `imagenestorta` (`Id`, `TortaId`, `UrlImagen`, `NombreArchivo`, `TamanioArchivo`, `TipoArchivo`, `EsPrincipal`, `Orden`, `FechaSubida`) VALUES
(1, 1, '/images/tortas/torta_1_1.jpg', 'torta_1_1.jpg', NULL, NULL, 1, 1, '2025-10-27 17:09:31'),
(2, 2, '/images/tortas/torta_2_1.jpg', 'torta_2_1.jpg', NULL, NULL, 1, 1, '2025-10-27 17:09:31'),
(3, 2, '/images/tortas/torta_2_2.jpg', 'torta_2_2.jpg', NULL, NULL, 0, 2, '2025-10-27 17:09:31'),
(4, 3, '/images/tortas/torta_3_1.jpg', 'torta_3_1.jpg', NULL, NULL, 1, 1, '2025-10-27 17:09:31'),
(5, 4, '/images/tortas/torta_4_1.jpg', 'torta_4_1.jpg', NULL, NULL, 1, 1, '2025-10-27 17:09:31'),
(6, 4, '/images/tortas/torta_4_2.jpg', 'torta_4_2.jpg', NULL, NULL, 0, 2, '2025-10-27 17:09:31'),
(7, 5, '/images/tortas/torta_5_1.jpg', 'torta_5_1.jpg', NULL, NULL, 1, 1, '2025-10-27 17:09:31'),
(8, 6, '/images/tortas/torta_6_1.jpg', 'torta_6_1.jpg', NULL, NULL, 1, 1, '2025-10-27 17:09:31'),
(9, 6, '/images/tortas/torta_6_2.jpg', 'torta_6_2.jpg', NULL, NULL, 0, 2, '2025-10-27 17:09:31'),
(10, 7, '/images/tortas/torta_7_1.jpg', 'torta_7_1.jpg', NULL, NULL, 1, 1, '2025-10-27 17:09:31');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `pagos`
--

CREATE TABLE `pagos` (
  `Id` int(11) NOT NULL,
  `TortaId` int(11) NOT NULL,
  `CompradorId` int(11) NOT NULL,
  `VendedorId` int(11) NOT NULL,
  `Monto` decimal(10,2) NOT NULL,
  `PrecioUnitario` decimal(10,2) NOT NULL,
  `Cantidad` int(11) NOT NULL DEFAULT 1,
  `Subtotal` decimal(10,2) NOT NULL,
  `Descuento` decimal(10,2) NOT NULL DEFAULT 0.00,
  `FechaPago` datetime NOT NULL DEFAULT current_timestamp(),
  `Estado` varchar(20) NOT NULL DEFAULT 'Pendiente',
  `MetodoPago` varchar(50) DEFAULT NULL,
  `ArchivoComprobante` varchar(500) DEFAULT NULL,
  `NumeroTransaccion` varchar(100) DEFAULT NULL,
  `Observaciones` text DEFAULT NULL,
  `DireccionEntrega` varchar(200) DEFAULT NULL,
  `FechaEntrega` datetime DEFAULT NULL,
  `FechaActualizacion` datetime DEFAULT NULL,
  `NotificacionEnviada` tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `pagos`
--

INSERT INTO `pagos` (`Id`, `TortaId`, `CompradorId`, `VendedorId`, `Monto`, `PrecioUnitario`, `Cantidad`, `Subtotal`, `Descuento`, `FechaPago`, `Estado`, `MetodoPago`, `ArchivoComprobante`, `NumeroTransaccion`, `Observaciones`, `DireccionEntrega`, `FechaEntrega`, `FechaActualizacion`, `NotificacionEnviada`) VALUES
(1, 1, 1, 1, 3500.00, 3500.00, 1, 3500.00, 0.00, '2025-10-12 17:09:31', 'Completado', 'Transferencia', NULL, 'TRF-001-2024', NULL, NULL, NULL, NULL, 0),
(2, 4, 2, 2, 3800.00, 3800.00, 1, 3800.00, 0.00, '2025-10-17 17:09:31', 'Completado', 'MercadoPago', NULL, 'MP-002-2024', NULL, NULL, NULL, NULL, 0),
(3, 6, 3, 3, 4500.00, 4500.00, 1, 4500.00, 0.00, '2025-10-30 17:09:31', 'Pendiente', 'Efectivo', NULL, NULL, NULL, NULL, NULL, NULL, 0);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `personas`
--

CREATE TABLE `personas` (
  `Id` int(11) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(50) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `Dni` varchar(20) DEFAULT NULL,
  `Telefono` varchar(20) DEFAULT NULL,
  `Direccion` varchar(200) DEFAULT NULL,
  `FechaNacimiento` datetime(6) DEFAULT NULL,
  `Avatar` varchar(500) DEFAULT NULL,
  `Rol` varchar(20) NOT NULL DEFAULT 'Comprador',
  `FechaRegistro` datetime NOT NULL DEFAULT current_timestamp(),
  `UltimoAcceso` datetime DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `personas`
--

INSERT INTO `personas` (`Id`, `Nombre`, `Apellido`, `Email`, `PasswordHash`, `Dni`, `Telefono`, `Direccion`, `FechaNacimiento`, `Avatar`, `Rol`, `FechaRegistro`, `UltimoAcceso`, `Activo`) VALUES
(1, 'Admin', 'Sistema', 'admin@casadelastortas.com', '$2a$11$20COjJ1/p2xDEYJOo5b4lee9WWqInw.S.wU2dwiKR57vBzeSyYboS', NULL, '2664123456', NULL, NULL, NULL, 'Admin', '2025-11-01 17:09:25', NULL, 1),
(2, 'María', 'González', 'maria.gonzalez@torteria.com', '$2a$11$6.kf0zw.Ly4Lea.1cVBSW.JBZABs7rMrZHozkwyiXmQNd4gkbFAiW', NULL, '2664234567', NULL, NULL, NULL, 'Vendedor', '2025-05-01 17:09:26', '2025-11-02 10:02:45', 1),
(3, 'Carlos', 'Rodríguez', 'carlos.rodriguez@reposteria.com', '$2a$11$PHM.3ayVKmIKYK8DE18IMeqdEafj8cu4T.rC0M7Vg0P6vUxwXnd6i', NULL, '2664345678', NULL, NULL, NULL, 'Vendedor', '2025-07-01 17:09:26', NULL, 1),
(4, 'Ana', 'Martínez', 'ana.martinez@pasteleria.com', '$2a$11$Z6mX8ynBeNuRoFsRytbcNeKnDBm8dYeP78RCcaIfXZnY2RvRp0AFO', NULL, '2664456789', NULL, NULL, NULL, 'Vendedor', '2025-08-01 17:09:27', NULL, 1),
(5, 'Juan', 'Pérez', 'juan.perez@email.com', '$2a$11$buuqaJ9Dh0pzLsNNE.5vHOH4J7gfP0KpHDHJPaLpXdAbpeLWF3u3G', NULL, '2664567890', NULL, NULL, NULL, 'Comprador', '2025-09-01 17:09:27', NULL, 1),
(6, 'Laura', 'Fernández', 'laura.fernandez@email.com', '$2a$11$oj8xowVcgQZXPus9R0HM7.gqN43eBi0je4ksDAKzBHBoneZZ0Al3m', NULL, '2664678901', NULL, NULL, NULL, 'Comprador', '2025-10-01 17:09:28', NULL, 1),
(7, 'Diego', 'López', 'diego.lopez@email.com', '$2a$11$SBFOBwCMdLTC1WHTY4cJ9OdC8SGyMzvk.k7b0pDyEkvTFsbYppse6', NULL, '2664789012', NULL, NULL, NULL, 'Comprador', '2025-10-17 17:09:28', NULL, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `tortas`
--

CREATE TABLE `tortas` (
  `Id` int(11) NOT NULL,
  `VendedorId` int(11) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Descripcion` text NOT NULL,
  `Precio` decimal(10,2) NOT NULL,
  `Stock` int(11) NOT NULL DEFAULT 0,
  `Categoria` varchar(50) DEFAULT NULL,
  `Tamanio` varchar(20) NOT NULL DEFAULT 'Mediana',
  `TiempoPreparacion` int(11) DEFAULT NULL,
  `Ingredientes` text DEFAULT NULL,
  `Personalizable` tinyint(1) NOT NULL DEFAULT 0,
  `VecesVendida` int(11) NOT NULL DEFAULT 0,
  `Calificacion` decimal(3,2) NOT NULL DEFAULT 0.00,
  `Disponible` tinyint(1) NOT NULL DEFAULT 1,
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp(),
  `FechaActualizacion` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `tortas`
--

INSERT INTO `tortas` (`Id`, `VendedorId`, `Nombre`, `Descripcion`, `Precio`, `Stock`, `Categoria`, `Tamanio`, `TiempoPreparacion`, `Ingredientes`, `Personalizable`, `VecesVendida`, `Calificacion`, `Disponible`, `FechaCreacion`, `FechaActualizacion`) VALUES
(1, 1, 'Torta de Chocolate con Dulce de Leche', 'Deliciosa torta de chocolate con capas de dulce de leche artesanal y cobertura de ganache.', 3500.00, 5, 'Chocolate', 'Grande', 2, 'Chocolate belga, dulce de leche, harina, huevos, manteca', 1, 0, 0.00, 1, '2025-10-02 17:09:30', NULL),
(2, 1, 'Torta Chocotorta Clásica', 'La tradicional chocotorta con chocolinas, dulce de leche y crema. Un clásico argentino.', 2800.00, 8, 'Chocolate', 'Mediana', 1, 'Chocolinas, dulce de leche, queso crema, crema de leche', 0, 0, 0.00, 1, '2025-10-07 17:09:30', NULL),
(3, 1, 'Torta Red Velvet', 'Esponjosa torta red velvet con frosting de queso crema. Perfecta para ocasiones especiales.', 4200.00, 3, 'Especiales', 'Grande', 2, 'Harina, cacao, buttermilk, queso crema, azúcar', 1, 0, 0.00, 1, '2025-10-12 17:09:30', NULL),
(4, 2, 'Cheesecake de Frutos Rojos', 'Cremoso cheesecake horneado con coulis de frutos rojos frescos.', 3800.00, 4, 'Cheesecakes', 'Mediana', 3, 'Queso crema Philadelphia, frutos rojos, galletas, azúcar', 0, 0, 0.00, 1, '2025-10-14 17:09:30', NULL),
(5, 2, 'Torta de Limón y Merengue', 'Torta de limón con relleno cítrico y merengue italiano flameado.', 3200.00, 6, 'Frutas', 'Mediana', 2, 'Limones, huevos, azúcar, harina, manteca', 0, 0, 0.00, 1, '2025-10-17 17:09:30', NULL),
(6, 3, 'Torta Unicornio', 'Torta decorada con fondant en colores pastel, cuerno dorado y detalles mágicos. Ideal para cumpleaños infantiles.', 4500.00, 2, 'Infantiles', 'Grande', 3, 'Bizcochuelo de vainilla, dulce de leche, fondant, decoraciones comestibles', 1, 0, 0.00, 1, '2025-10-22 17:09:30', NULL),
(7, 3, 'Torta Paw Patrol', 'Torta temática de Paw Patrol con personajes en 3D de fondant.', 5000.00, 1, 'Infantiles', 'Extra Grande', 4, 'Bizcochuelo de chocolate, relleno de dulce de leche, fondant', 1, 0, 0.00, 1, '2025-10-25 17:09:30', NULL);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `vendedores`
--

CREATE TABLE `vendedores` (
  `Id` int(11) NOT NULL,
  `PersonaId` int(11) NOT NULL,
  `NombreComercial` varchar(100) NOT NULL,
  `Especialidad` varchar(100) DEFAULT NULL,
  `Descripcion` varchar(500) DEFAULT NULL,
  `Calificacion` decimal(3,2) NOT NULL DEFAULT 0.00,
  `TotalVentas` int(11) NOT NULL DEFAULT 0,
  `Horario` varchar(100) DEFAULT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT current_timestamp(),
  `Verificado` tinyint(1) NOT NULL DEFAULT 0,
  `Activo` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `vendedores`
--

INSERT INTO `vendedores` (`Id`, `PersonaId`, `NombreComercial`, `Especialidad`, `Descripcion`, `Calificacion`, `TotalVentas`, `Horario`, `FechaCreacion`, `Verificado`, `Activo`) VALUES
(1, 2, 'Tortería María', 'Tortas de Chocolate y Crema', 'Especialistas en tortas artesanales con más de 10 años de experiencia. Usamos ingredientes premium y recetas tradicionales.', 4.80, 150, 'Lunes a Sábado: 9:00 - 20:00', '2025-05-01 17:09:29', 1, 1),
(2, 3, 'Repostería Don Carlos', 'Tortas de Frutas y Cheesecakes', 'Creaciones únicas con frutas frescas de estación. Cada torta es una obra de arte.', 4.50, 95, 'Martes a Domingo: 10:00 - 19:00', '2025-07-01 17:09:29', 1, 1),
(3, 4, 'Pastelería Anita', 'Tortas Temáticas Infantiles', 'Diseños personalizados para cumpleaños infantiles. Trabajamos con fondant y decoraciones 3D.', 4.90, 120, 'Lunes a Viernes: 8:00 - 18:00', '2025-08-01 17:09:29', 1, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `__efmigrationshistory`
--

CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `__efmigrationshistory`
--

INSERT INTO `__efmigrationshistory` (`MigrationId`, `ProductVersion`) VALUES
('20251101200749_InitialCreate', '9.0.10');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `compradores`
--
ALTER TABLE `compradores`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `IX_Compradores_PersonaId` (`PersonaId`),
  ADD KEY `IX_Compradores_Ciudad` (`Ciudad`);

--
-- Indices de la tabla `imagenestorta`
--
ALTER TABLE `imagenestorta`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_ImagenesTorta_TortaId` (`TortaId`),
  ADD KEY `IX_ImagenesTorta_TortaId_EsPrincipal` (`TortaId`,`EsPrincipal`);

--
-- Indices de la tabla `pagos`
--
ALTER TABLE `pagos`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_Pagos_CompradorId` (`CompradorId`),
  ADD KEY `IX_Pagos_Estado` (`Estado`),
  ADD KEY `IX_Pagos_FechaPago` (`FechaPago`),
  ADD KEY `IX_Pagos_NumeroTransaccion` (`NumeroTransaccion`),
  ADD KEY `IX_Pagos_TortaId` (`TortaId`),
  ADD KEY `IX_Pagos_VendedorId` (`VendedorId`);

--
-- Indices de la tabla `personas`
--
ALTER TABLE `personas`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `IX_Personas_Email` (`Email`),
  ADD KEY `IX_Personas_Rol` (`Rol`);

--
-- Indices de la tabla `tortas`
--
ALTER TABLE `tortas`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_Tortas_Categoria` (`Categoria`),
  ADD KEY `IX_Tortas_Disponible` (`Disponible`),
  ADD KEY `IX_Tortas_Precio` (`Precio`),
  ADD KEY `IX_Tortas_VendedorId` (`VendedorId`);

--
-- Indices de la tabla `vendedores`
--
ALTER TABLE `vendedores`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `IX_Vendedores_PersonaId` (`PersonaId`),
  ADD KEY `IX_Vendedores_NombreComercial` (`NombreComercial`);

--
-- Indices de la tabla `__efmigrationshistory`
--
ALTER TABLE `__efmigrationshistory`
  ADD PRIMARY KEY (`MigrationId`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `compradores`
--
ALTER TABLE `compradores`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `imagenestorta`
--
ALTER TABLE `imagenestorta`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT de la tabla `pagos`
--
ALTER TABLE `pagos`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `personas`
--
ALTER TABLE `personas`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `tortas`
--
ALTER TABLE `tortas`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `vendedores`
--
ALTER TABLE `vendedores`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `compradores`
--
ALTER TABLE `compradores`
  ADD CONSTRAINT `FK_Compradores_Personas` FOREIGN KEY (`PersonaId`) REFERENCES `personas` (`Id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `imagenestorta`
--
ALTER TABLE `imagenestorta`
  ADD CONSTRAINT `FK_ImagenesTorta_Tortas` FOREIGN KEY (`TortaId`) REFERENCES `tortas` (`Id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `pagos`
--
ALTER TABLE `pagos`
  ADD CONSTRAINT `FK_Pagos_Compradores` FOREIGN KEY (`CompradorId`) REFERENCES `compradores` (`Id`),
  ADD CONSTRAINT `FK_Pagos_Tortas` FOREIGN KEY (`TortaId`) REFERENCES `tortas` (`Id`),
  ADD CONSTRAINT `FK_Pagos_Vendedores` FOREIGN KEY (`VendedorId`) REFERENCES `vendedores` (`Id`);

--
-- Filtros para la tabla `tortas`
--
ALTER TABLE `tortas`
  ADD CONSTRAINT `FK_Tortas_Vendedores` FOREIGN KEY (`VendedorId`) REFERENCES `vendedores` (`Id`);

--
-- Filtros para la tabla `vendedores`
--
ALTER TABLE `vendedores`
  ADD CONSTRAINT `FK_Vendedores_Personas` FOREIGN KEY (`PersonaId`) REFERENCES `personas` (`Id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

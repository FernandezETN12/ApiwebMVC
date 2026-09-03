-- Crear base de datos y usuario (ajusta la contraseña y el host según sea necesario)
CREATE DATABASE IF NOT EXISTS `pizzeria_db`
  CHARACTER SET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- Usuario opcional para la aplicación
CREATE USER IF NOT EXISTS 'pizzeria_user'@'localhost' IDENTIFIED BY 'SUSTITUYE_POR_UNA_CONTRASEÑA_SEGURA';
GRANT ALL PRIVILEGES ON `pizzeria_db`.* TO 'pizzeria_user'@'localhost';
FLUSH PRIVILEGES;

USE `pizzeria_db`;

-- Tabla Clientes
CREATE TABLE IF NOT EXISTS `Clientes` (
  `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `Nombre` VARCHAR(200) NOT NULL,
  `Telefono` VARCHAR(50) NOT NULL,
  `Direccion` VARCHAR(300) NOT NULL,
  `FechaRegistro` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla Pizzas
CREATE TABLE IF NOT EXISTS `Pizzas` (
  `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `Nombre` VARCHAR(200) NOT NULL,
  `Descripcion` TEXT NOT NULL,
  `Precio` DECIMAL(10,2) NOT NULL,
  `Ingredientes` JSON NOT NULL,
  `FechaCreacion` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla Pedidos
CREATE TABLE IF NOT EXISTS `Pedidos` (
  `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `ClienteId` INT NOT NULL,
  `Estado` TINYINT NOT NULL DEFAULT 0,
  `Total` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
  `FechaPedido` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `FechaConfirmacion` DATETIME NULL,
  `FechaPreparacion` DATETIME NULL,
  `FechaEnViaje` DATETIME NULL,
  `FechaEntrega` DATETIME NULL,
  `NotasDelivery` TEXT NULL,
  CONSTRAINT `FK_Pedidos_Clientes` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes`(`Id`) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla PedidoDetalles
CREATE TABLE IF NOT EXISTS `PedidoDetalles` (
  `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `PedidoId` INT NOT NULL,
  `PizzaId` INT NOT NULL,
  `Cantidad` INT NOT NULL DEFAULT 1,
  `PrecioUnitario` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
  CONSTRAINT `FK_Detalles_Pedidos` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos`(`Id`) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT `FK_Detalles_Pizzas` FOREIGN KEY (`PizzaId`) REFERENCES `Pizzas`(`Id`) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Índices para optimizar búsquedas por FK
CREATE INDEX `IX_Pedidos_ClienteId` ON `Pedidos` (`ClienteId`);
CREATE INDEX `IX_PedidoDetalles_PedidoId` ON `PedidoDetalles` (`PedidoId`);
CREATE INDEX `IX_PedidoDetalles_PizzaId` ON `PedidoDetalles` (`PizzaId`);
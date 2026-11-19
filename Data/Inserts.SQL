USE `5to_Pizzeria`;

INSERT INTO Pizzas
    (Nombre, Descripcion, Precio, Ingredientes, FechaCreacion, ImagenUrl)
VALUES
(
    'Muzzarella',
    'La clásica pizza argentina con salsa de tomate, muzzarella y orégano.',
    8500.00,
    '["Salsa de tomate", "Muzzarella", "Orégano"]',
    UTC_TIMESTAMP(),
    '/img/productos/muzzarella.webp'
),
(
    'Napolitana',
    'Pizza con muzzarella, rodajas de tomate y orégano.',
    9500.00,
    '["Salsa de tomate", "Muzzarella", "Tomate", "Orégano"]',
    UTC_TIMESTAMP(),
    '/img/productos/napolitana.jpg'
),
(
    'Jamón y Morrones',
    'Pizza con muzzarella, jamón y morrones.',
    10500.00,
    '["Salsa de tomate", "Muzzarella", "Jamón", "Morrones"]',
    UTC_TIMESTAMP(),
    '/img/productos/jamon-y-morrones.jpg'
),
(
    'Fugazzeta',
    'Pizza con abundante muzzarella, cebolla y orégano.',
    11000.00,
    '["Muzzarella", "Cebolla", "Orégano"]',
    UTC_TIMESTAMP(),
    '/img/productos/fugazzeta.jpg'
),
(
    'Calabresa',
    'Pizza con muzzarella, longaniza calabresa y orégano.',
    11500.00,
    '["Salsa de tomate", "Muzzarella", "Longaniza calabresa", "Orégano"]',
    UTC_TIMESTAMP(),
    '/img/productos/calabresa.jpg'
);

UPDATE Pizzas
SET ImagenURL = CASE Nombre
    WHEN 'Muzzarella' THEN '/img/productos/muzzarella.webp'
    WHEN 'Napolitana' THEN '/img/productos/napolitana.jpg'
    WHEN 'Jamón y Morrones' THEN '/img/productos/jamon-y-morrones.jpg'
    WHEN 'Fugazzeta' THEN '/img/productos/fugazzeta.jpg'
    WHEN 'Calabresa' THEN '/img/productos/calabresa.jpg'
    WHEN 'Margherita' THEN '/img/productos/margherita.jpg'
    WHEN 'Rúcula & Parmesano' THEN '/img/productos/rucula-parmesano.jpg'
    WHEN 'Diavola' THEN '/img/productos/diavola.jpg'
    ELSE ImagenURL
END
WHERE Nombre IN (
    'Muzzarella', 'Napolitana', 'Jamón y Morrones', 'Fugazzeta', 'Calabresa',
    'Margherita', 'Rúcula & Parmesano', 'Diavola'
);
# ApiwebMVC

## Requisitos

- .NET SDK 10
- MySQL 8.x en ejecución
- Un usuario MySQL con permiso para crear/modificar el esquema de la base

## Ejecutar en desarrollo

Configura la conexión fuera de los archivos versionados. User Secrets se conserva en el perfil local del usuario y no se publica en Git:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=5to_pizzeria_db;User=TU_USUARIO;Password=TU_PASSWORD;"
dotnet user-secrets set "Authentication:Google:ClientId" "TU_CLIENT_ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "TU_CLIENT_SECRET"
dotnet restore
dotnet run
```

Al iniciar, la aplicación aplica las migraciones de EF Core y siembra los productos que falten en el catálogo. El servidor MySQL debe estar activo y el usuario configurado debe tener permisos de creación y modificación de tablas.

En servidores, configura las variables de entorno `ConnectionStrings__DefaultConnection`, `Authentication__Google__ClientId` y `Authentication__Google__ClientSecret` en lugar de User Secrets.


## Usuarios y roles

Para **confirmar un pedido hay que iniciar sesión** (con Google). El carrito se puede armar sin sesión y se conserva al iniciarla.

| Acción | Invitado | Cliente | Admin |
|---|:-:|:-:|:-:|
| Ver menú y armar carrito | ✅ | ✅ | ✅ |
| Confirmar pedido | ❌ (pide login) | ✅ | ✅ |
| Ver "Mis pedidos" y cancelar los que están en espera | ❌ | ✅ (solo los suyos) | ✅ |
| Editar pizzas del menú | ❌ | ❌ | ✅ |
| Panel admin (todos los pedidos, cambiar estado) | ❌ | ❌ | ✅ |
| API: `GET /api/pizzas` | ✅ | ✅ | ✅ |
| API: crear/editar/borrar pizzas, listar todos los pedidos, cambiar estados, `/api/clientes` | 401 | 403 | ✅ |
| API: `POST /api/pedidos`, ver/cancelar pedidos propios | 401 | ✅ (solo a su nombre) | ✅ |

### ¿Quién es admin?

Los emails listados en `Admin:Emails`. Se configuran con User Secrets (no se suben a Git):

```bash
dotnet user-secrets set "Admin:Emails:0" "tu-email@gmail.com"
dotnet user-secrets set "Admin:Emails:1" "otro-admin@gmail.com"
```

El rol se calcula al iniciar sesión: si agregás un email, esa persona tiene que cerrar sesión y volver a entrar.

### Login con Google

Las dos credenciales se configuran con los comandos anteriores. Si no se proporcionan, el login con Google queda desactivado.

En Google Cloud Console, la URI de redirección autorizada es `http://localhost:5046/signin-google` (y la versión https si usás ese perfil).

### Login de prueba (solo Development)

Al correr en modo Development, la página de login muestra **"Entrar como prueba"**: se entra con cualquier email, sin Google. Sirve para probar los roles (usá un email de `Admin:Emails` para entrar como admin). En producción no aparece y la ruta devuelve 404.

## Tests

```bash
dotnet test ApiwebMVC.Tests
```

## Fotos de los productos

El menú muestra cada foto **completa**, sin recortes. Si un producto no tiene foto, o la foto no carga, aparece un reemplazo con su ícono y su nombre.

Para usar fotos propias en buena calidad, que además funcionan sin internet:

**Opción A · Desde la app (la más fácil):** iniciá sesión como admin, entrá a **Imágenes** (en el menú o en el Panel admin), elegí la foto de cada producto y tocá **Subir**. La app la valida (JPG, PNG o WEBP, hasta 5 MB), la guarda en `wwwroot/img/productos/` con el nombre correcto y la asigna al producto al instante.

**Opción B · Copiando los archivos a mano:**

1. Guardá la foto en `wwwroot/img/productos/` con el nombre de la tabla (también sirven `.jpeg`, `.png` o `.webp`).
2. Reiniciá la app (`dotnet run`): al arrancar asigna sola la foto a su producto. La foto local tiene prioridad sobre una dirección web.

Recomendado: fotos cuadradas o 4:3, de 800 a 1200 px de ancho y menos de 300 KB.

| Producto | Categoría | Nombre del archivo |
|---|---|---|
| Muzzarella | Pizzas | `muzzarella.jpg` |
| Napolitana | Pizzas | `napolitana.jpg` |
| Jamón y Morrones | Pizzas | `jamon-y-morrones.jpg` |
| Fugazzeta | Pizzas | `fugazzeta.jpg` |
| Calabresa | Pizzas | `calabresa.jpg` |
| Tomate con Albahaca | Pizzas | `tomate-con-albahaca.jpg` |
| Margherita | Pizzas | `margherita.jpg` |
| Rúcula & Parmesano | Pizzas | `rucula-parmesano.jpg` |
| Diavola | Pizzas | `diavola.jpg` |
| Empanada de Carne | Empanadas | `empanada-de-carne.jpg` |
| Empanada de Jamón y Queso | Empanadas | `empanada-de-jamon-y-queso.jpg` |
| Empanada de Pollo | Empanadas | `empanada-de-pollo.jpg` |
| Empanada de Humita | Empanadas | `empanada-de-humita.jpg` |
| Empanada de Papa | Empanadas | `empanada-de-papa.jpg` |
| Empanada de Verdura | Empanadas | `empanada-de-verdura.jpg` |
| Coca-Cola | Bebidas | `coca-cola.jpg` |
| Sprite | Bebidas | `sprite.jpg` |
| Pepsi | Bebidas | `pepsi.jpg` |
| Agua | Bebidas | `agua.jpg` |
| Paso de los Toros | Bebidas | `paso-de-los-toros.jpg` |
| Agua con gas | Bebidas | `agua-con-gas.jpg` |


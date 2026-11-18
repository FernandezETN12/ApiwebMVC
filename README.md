# ApiwebMVC

## Requisitos

- .NET SDK 10
- MySQL 8.x en ejecución
- Un usuario MySQL con permiso para crear/modificar el esquema de la base

## Ejecutar en desarrollo

Configura la conexión fuera de los archivos versionados. User Secrets se conserva en el perfil local del usuario y no se publica en Git:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=5to_pizzeria_db;User=TU_USUARIO;Password=TU_PASSWORD;"
dotnet restore
dotnet run
```

Al iniciar, la aplicación aplica las migraciones de EF Core y siembra los productos que falten en el catálogo. El servidor MySQL debe estar activo y el usuario configurado debe tener permisos de creación y modificación de tablas.

En servidores, configura la variable de entorno `ConnectionStrings__DefaultConnection` en lugar de User Secrets.


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

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "TU_CLIENT_ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "TU_CLIENT_SECRET"
```

En Google Cloud Console, la URI de redirección autorizada es `http://localhost:5046/signin-google` (y la versión https si usás ese perfil).

### Login de prueba (solo Development)

Al correr en modo Development, la página de login muestra **"Entrar como prueba"**: se entra con cualquier email, sin Google. Sirve para probar los roles (usá un email de `Admin:Emails` para entrar como admin). En producción no aparece y la ruta devuelve 404.

## Tests

```bash
dotnet test ApiwebMVC.Tests
```

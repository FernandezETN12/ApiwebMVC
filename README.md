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


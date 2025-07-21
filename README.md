# UserManagementAPI


## 🚀 Backend API (.NET 6)
- **Ubicación**: `/backend-api`
- **Framework**: .NET 6
- **Base de datos**: SQL Server (LocalDB)
- **ORM**: Entity Framework Core
- **Autenticación**: JWT
- **Caché**: Memory Cache
- **Logs**: Serilog
- **Documentación**: Swagger UI

- ### ✅ Manejo de Caché
- Cache de respuestas del API
- Cache de datos de países (restcountries.com)

- 
### ✅ Sistema de Logs
- Logs estructurados con Serilog
- Logs de autenticación, operaciones CRUD y errores

- 
### ✅ CRUD de Usuarios y Permisos
- Administrador: CRUD completo
- Operador: Leer y editar usuarios cliente
- Cliente: Solo leer su información


### ✅ Integración con API Externa
- Consumo de restcountries.com
- Cache de respuestas



## 🔐 Usuario por Defecto
- **Email**: admin@system.com
- **Password**: Admin123!
- **Rol**: Administrador

## 📊 Estructura de Permisos
- **Administrador**: Acceso completo al sistema
- **Operador**: Gestión de usuarios cliente
- **Cliente**: Solo lectura de su información

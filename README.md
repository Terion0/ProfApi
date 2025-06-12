# 👤 API de Perfiles ASP.NET Core con JWT

Esta API permite gestionar perfiles de usuario, consultar información pública y privada, y gestionar relaciones de seguimiento entre usuarios (seguir / dejar de seguir). La autenticación se realiza con JWT para proteger los endpoints.

---

## ⚙️ Configuración de entorno (`appsettings.json`)

```jsonc
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",               // Nivel general de logs (información y superior)
      "Microsoft.AspNetCore": "Warning"      // Nivel específico para logs de ASP.NET Core (advertencias y errores)
    }
  },
  "AllowedHosts": "*",                      // Hosts permitidos para acceder a la API, "*" permite todos

  "DbSettings": {
    "Host": "postgres",                     // Dirección del servidor PostgreSQL (puede ser localhost o nombre del contenedor en Docker)
    "Port": 5434,                           // Puerto donde escucha PostgreSQL
    "Username": "postgres",                 // Usuario de la base de datos
    "Password": "postgres",                 // Contraseña del usuario de la base de datos
    "Database": "postgres"                  // Nombre de la base de datos utilizada por la API
  },

  "JwtSettings": {
    "SecretKey": "llavesecreta"             // Clave secreta para firmar y validar tokens JWT (mantener privada)
  }
}

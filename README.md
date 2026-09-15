# COTELNET-NEXT

Base independiente para modernizar COTELNET con el mismo stack tecnológico de SITRAP.

## Tecnología

- React 19, Vite 7, React Router 7 y Material UI 7.
- ASP.NET Core Web API en .NET 10 y C#.
- EF Core 10 con Microsoft SQL Server 2022.
- JWT Bearer HS256.
- Contraseñas PBKDF2-SHA256 con sal aleatoria y 100 000 iteraciones.
- Swagger/OpenAPI.
- Nginx como servidor del SPA y proxy hacia la API.
- Docker Compose, preparado para una red externa `coolify`.

## Estado de esta primera entrega

Incluye la arquitectura base, autenticación, usuario administrador opcional, entidad inicial de estafetas, pantalla de acceso, panel de módulos, documentación Swagger, prueba del hash de contraseñas y contenedores. Todavía no incluye conexión ni migración de información del COTELNET actual.

## Inicio con Docker

1. Copiar `.env.example` como `.env`.
2. Cambiar todas las contraseñas y el secreto JWT.
3. Crear la red local si no existe: `docker network create coolify`.
4. Ejecutar `docker compose up --build`.
5. Abrir `http://localhost:8088`.

La documentación de la API queda disponible a través del contenedor en `/swagger`. Para producción se debe decidir si Swagger será interno o estará deshabilitado.

## Estructura

```text
backend/
  src/CotelNet.Domain          Entidades y reglas puras
  src/CotelNet.Application     Casos de uso y contratos
  src/CotelNet.Infrastructure  EF Core, seguridad e integraciones
  src/CotelNet.Api             Controladores y configuración HTTP
  tests/CotelNet.Tests         Pruebas automatizadas
frontend/                      SPA React
nginx/                         Proxy inverso y fallback del SPA
```

## Reglas de migración

- El COTELNET actual continúa funcionando sin cambios.
- No usar la base productiva durante el desarrollo.
- Migrar un módulo a la vez y comparar sus resultados con el sistema actual.
- Evitar escritura simultánea de ambos sistemas sobre el mismo módulo.
- Los comandos seriales de impresoras se trasladarán a un agente local de Windows.
- Las contraseñas MD5 del sistema anterior no se copiarán como credenciales válidas; se definirá un proceso de actualización segura.

Consulte `docs/architecture.md` y `docs/migration-map.md` antes de implementar nuevos módulos.

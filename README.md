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

## Estado actual

Incluye autenticación, caja, flujo de admisión postal, cálculo de tarifas por destino y peso, servicios suplementarios, códigos S10 UPU, etiqueta, recibo térmico y mantenimiento de servicios. También incorpora una importación controlada y de solo lectura de los catálogos operativos del COTELNET actual.

## Inicio con Docker

1. Copiar `.env.example` como `.env`.
2. Cambiar todas las contraseñas y el secreto JWT.
3. Crear la red local si no existe: `docker network create coolify`.
4. Ejecutar `docker compose up --build`.
5. Abrir `http://localhost:8088`.

### Importar los catálogos reales de COTELNET

La importación lee servicios, tipos, vías de encaminamiento, países, provincias, grupos, tarifas, ajustes, suplementarios y formatos S10. No lee ni modifica clientes, usuarios, ventas o historiales del sistema anterior.

1. Habilitar TCP/IP en la instancia SQL Server donde se encuentra la base anterior y comprobar que sea accesible desde Docker.
2. Agregar en `.env` una conexión con un usuario de solo lectura, por ejemplo:

   ```env
   LEGACY_DB_CONNECTION_STRING=Server=host.docker.internal,1433;Database=DBCOTELNET;User Id=cotelnet_lector;Password=CAMBIAR;TrustServerCertificate=True
   ```

3. Recrear la API: `docker compose up -d --build api frontend`.
4. Ingresar como administrador, abrir **Mantenimiento de servicios** y seleccionar **Importar desde COTELNET**.

La operación reemplaza los catálogos y rangos de tarifa de COTELNET-NEXT, pero conserva las ventas y envíos ya registrados. La base anterior siempre se abre con intención de solo lectura.

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

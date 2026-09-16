# Guía para agentes de COTELNET-NEXT

## Propósito del proyecto

COTELNET-NEXT es la reconstrucción moderna e independiente del COTELNET legado. Usa el mismo enfoque tecnológico y arquitectónico de SITRAP, pero no forma parte de SITRAP ni debe acoplarse a ese sistema.

El COTELNET legado continúa operando por separado. La migración se realiza gradualmente, módulo por módulo, comparando los resultados del sistema nuevo con los del sistema actual antes de cualquier transición productiva.

## Arquitectura y responsabilidades

El backend es un monolito modular organizado en capas:

- `backend/src/CotelNet.Domain`: entidades y reglas de negocio puras. No debe depender de HTTP, EF Core ni de la interfaz gráfica.
- `backend/src/CotelNet.Application`: casos de uso, contratos, DTO, permisos y validaciones de aplicación.
- `backend/src/CotelNet.Infrastructure`: EF Core, SQL Server, repositorios, seguridad e integraciones, incluida la importación del sistema legado.
- `backend/src/CotelNet.Api`: controladores, autenticación/autorización, configuración HTTP, Swagger y arranque de la aplicación.
- `backend/tests/CotelNet.Tests`: pruebas automatizadas de reglas críticas.
- `frontend`: SPA React. Presenta los flujos y permisos, pero no debe contener cálculos financieros ni reglas críticas que deban protegerse en la API.
- `nginx`: servidor del SPA, fallback de rutas y proxy inverso hacia la API.

Mantén las dependencias orientadas hacia el dominio. Las nuevas reglas de negocio deben quedar en Domain o Application y no en controladores ni componentes React.

## Stack tecnológico

- Frontend: React 19, JavaScript/JSX, Vite 7, React Router 7, Material UI 7 y Emotion.
- Backend: C#, ASP.NET Core Web API y .NET 10 (`net10.0`).
- Persistencia: EF Core 10 y Microsoft SQL Server 2022.
- Seguridad: JWT Bearer HS256, autorización mediante roles/permisos y contraseñas PBKDF2-SHA256 con sal aleatoria y 100 000 iteraciones.
- Identificación postal: códigos S10 UPU, código de barras en SVG y QRCoder cuando corresponda.
- Infraestructura: Nginx, Docker, Docker Compose y red externa `coolify`, compatible con despliegues mediante Coolify/Traefik.
- Documentación y calidad: Swagger/OpenAPI, xUnit, ESLint y GitHub Actions.

## Estado funcional actual

El sistema ya incluye:

- Inicio de sesión, usuarios, roles y permisos.
- Estafetas y terminales.
- Apertura y operación de caja.
- Admisión postal con datos de remitente y destinatario.
- Selección de servicio y destino, cálculo de tarifas por peso y servicios suplementarios.
- Cobro, ventas, factura/recibo térmico y etiqueta postal.
- Generación de códigos S10 UPU y código de barras.
- Mantenimiento de servicios, destinos, tarifas y suplementarios.
- Importación controlada de catálogos operativos desde el COTELNET legado.

La última importación validada produjo 12 servicios postales, 255 destinos, 59 076 rangos de tarifa, 8 servicios suplementarios y 48 asociaciones. El flujo completo de venta fue validado con tarifa base, suplementario, cobro, S10, factura y etiqueta coherentes.

Estos conteos describen la validación conocida, no constantes del sistema. Pueden cambiar cuando cambien los catálogos de origen.

## Reglas de negocio y seguridad

- El peso se captura en kilogramos con tres decimales, por ejemplo `0.500`, y se convierte a gramos para las reglas internas cuando corresponda.
- La cotización depende de la cadena activa de servicio, destino específico o grupo, vía de encaminamiento, rango de peso y ajustes tarifarios.
- Una tarifa específica del destino tiene prioridad sobre la tarifa de grupo aplicable.
- Deben respetarse el peso máximo, los intervalos, ajustes y reglas de redondeo provenientes de COTELNET.
- El S10 debe seguir el estándar UPU: prefijo de dos letras resuelto desde el servicio o suplementario, serie de ocho dígitos, dígito verificador y sufijo `PA`.
- No fijes un prefijo S10 universal: puede depender del servicio suplementario o de la configuración importada vigente.
- Las operaciones de venta, pago, caja y generación de consecutivos deben ser atómicas e idempotentes cuando exista riesgo de duplicación.
- Almacena fechas en UTC y preséntalas en la zona horaria de Panamá.
- La importación reemplaza catálogos y rangos operativos de COTELNET-NEXT, pero debe conservar ventas y envíos ya registrados.
- La base del COTELNET legado se usa solamente como fuente de lectura. Utiliza una cuenta SQL exclusiva de solo lectura y los permisos mínimos necesarios para consultar catálogos y resolver formatos S10.
- Nunca escribas en la base legada desde COTELNET-NEXT.
- Nunca incluyas contraseñas, tokens, cadenas de conexión reales, IP internas ni secretos en código, documentación, pruebas, commits o mensajes de diagnóstico. Usa variables de entorno y valores ficticios.
- No ejecutes `docker compose down -v` salvo que el usuario solicite explícitamente eliminar la base local. El modificador `-v` destruye el volumen `sqlserver-data`.
- La comunicación COM/USB y los comandos ESC/POS de impresoras Epson o NCR pertenecen a un futuro agente local de Windows, no al navegador ni a un contenedor Linux.

## Prioridad inmediata

El flujo funcional está validado. La siguiente prioridad es mejorar la presentación de impresión para acercarla al formato real de COTELNET:

- Etiqueta postal de 4 × 6 pulgadas: aumentar la presencia del S10 y del código de barras, aprovechar mejor el espacio, incorporar el logo oficial y destacar destinatario, país, peso y servicio.
- Incluir claramente el código de estafeta y el formato de etiqueta, por ejemplo `CN-04`.
- Recibo térmico de 80 mm: reducir espacios vacíos y mejorar encabezado, jerarquía visual y legibilidad sin perder la información de la transacción.

Antes de incorporar el logo u otros activos oficiales, confirma que el recurso correcto esté disponible en el repositorio o haya sido suministrado por el usuario.

## Flujo de trabajo

Antes de modificar un módulo:

1. Lee `README.md`, `docs/architecture.md` y `docs/migration-map.md`.
2. Revisa el estado de Git y conserva cambios ajenos o no relacionados.
3. Localiza las reglas equivalentes en Domain, Application e Infrastructure antes de agregar lógica al frontend.
4. Mantén compatibles los contratos API existentes, o documenta expresamente cualquier cambio de interfaz.
5. Agrega o actualiza pruebas para reglas de tarifas, S10, seguridad y transacciones.
6. Verifica backend y frontend antes de entregar.

No edites directamente catálogos reales en código para corregir datos importados. Ajusta el mapeo o la regla de importación y conserva trazabilidad hacia el identificador legado cuando exista.

## Comandos oficiales

### Preparación y Docker

```powershell
Copy-Item .env.example .env
docker network create coolify
docker compose up -d --build
docker compose ps
```

La aplicación queda disponible en `http://localhost:8088` y Swagger en `http://localhost:8088/swagger/index.html` mientras esa ruta esté habilitada.

Para reconstruir solamente la API y el frontend después de un cambio:

```powershell
docker compose up -d --build api frontend
```

Para detener los servicios sin borrar la base local:

```powershell
docker compose down
```

### Diagnóstico

```powershell
docker compose ps
docker compose logs api --tail 200
docker compose logs sqlserver --tail 100
docker compose logs frontend --tail 100
```

Después de cambiar variables de entorno de la API, recréala para que lea la configuración nueva:

```powershell
docker compose up -d --force-recreate api
```

### Backend

Desde la raíz del repositorio:

```powershell
dotnet restore backend/CotelNet.slnx
dotnet test backend/CotelNet.slnx --configuration Release --no-restore
dotnet publish backend/src/CotelNet.Api/CotelNet.Api.csproj --configuration Release --no-restore
```

### Frontend

```powershell
Set-Location frontend
npm ci
npm run lint
npm run build
```

Para desarrollo interactivo del frontend:

```powershell
Set-Location frontend
npm install
npm run dev
```

## Criterios de entrega

Antes de dar un cambio por terminado:

- Ejecuta las pruebas del backend relacionadas y, cuando sea posible, la solución completa en configuración Release.
- Ejecuta `npm run lint` y `npm run build` para cualquier cambio del frontend.
- Comprueba que no haya secretos ni datos personales en el diff.
- Confirma que ventas, envíos y catálogos existentes se preservan cuando el cambio afecta persistencia o importación.
- Para impresión, revisa visualmente la etiqueta 4 × 6 y el recibo de 80 mm, además de confirmar que ambos muestran el mismo S10.
- Resume qué se validó y cualquier comprobación que no haya podido ejecutarse.

La automatización de `.github/workflows/ci.yml` reproduce las verificaciones principales: restaura, prueba y publica el backend; instala con `npm ci`, ejecuta ESLint y compila el frontend.

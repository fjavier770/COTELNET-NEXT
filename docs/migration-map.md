# Mapa de migración funcional

El repositorio anterior usa .NET Framework 4.8, VB.NET, WinForms, Web Forms, WCF, DataSets tipados, TableAdapters, SQL Server Compact y Crystal Reports. La nueva solución reimplementará las reglas en C# sin compartir binarios con el sistema anterior.

| Etapa | Módulos | Dependencias críticas | Validación requerida |
|---|---|---|---|
| 0 | Infraestructura, seguridad y auditoría | JWT, PBKDF2, permisos, Docker | Inicio de sesión, expiración y trazabilidad |
| 1 | Usuarios, roles, estafetas y terminales | Catálogos y permisos | Equivalencia de accesos |
| 2 | Países, destinos, servicios y tarifas | Reglas tarifarias | Comparación de precios por servicio y destino |
| 3 | Clientes, empresas y perfiles | Identificación y duplicados | Búsquedas y registros equivalentes |
| 4 | Inventario y franqueo | Existencias por estafeta/usuario | Saldos antes y después de movimientos |
| 5 | Aperturas, recaudación y cierre | Transacciones y arqueos | Totales diarios exactos |
| 6 | Ventas y métodos de pago | Inventario, tarifas, caja | Comparación de facturas y totales |
| 7 | Envíos, EMS, etiquetas y mercadería | Numeración, IPS, QR/barra | Códigos únicos y mensajes IPS |
| 8 | Apartados postales | Clientes, períodos y morosidad | Estados, deuda y descuentos |
| 9 | Giros, transferencias y remesas | Caja, aprobaciones y archivos | Estados y conciliación |
| 10 | Informes y estadísticas | Todos los módulos | Comparación contra cierres históricos |
| 11 | Impresión y transición operativa | Agente local Windows | Epson L90/T88V y NCR 7167 |

## Reglas que requieren caracterización antes de reescribir

1. Registro atómico de venta, detalles, inventario, caja e IPS.
2. Anulación y devolución de ventas y transferencias.
3. Cálculo del saldo de recaudador y estados de movimientos.
4. Apertura y cierre diario con inventario postal.
5. Asignación y disminución de inventario por usuario y estafeta.
6. Selección de tarifas por servicio, destino, provincia, peso y ajustes.
7. Creación y vigencia de códigos de envío.
8. Morosidad, descuentos, períodos y adjuntos de apartados.
9. Flujo de solicitud, aprobación, tesorería, contabilidad y recepción de remesas.
10. Generación de informes de conciliación y estadísticas.

Cada regla tendrá casos de prueba construidos con resultados conocidos del COTELNET actual antes de activarse en la nueva plataforma.

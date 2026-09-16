/*
  Exportación de catálogos parametrales desde DBCOTELNET.

  Ejecutar en SQL Server Management Studio contra la base del COTELNET actual.
  No exporta clientes, ventas, contraseñas ni información personal.
  Guardar cada resultado como CSV conservando el nombre indicado en el comentario.
*/

-- 01_servicios.csv
SELECT servicioID, nombreServicio, descripcion, activo
FROM dbo.Servicios
ORDER BY servicioID;

-- 02_servicios_categorias.csv
SELECT servicioCategoriaID, servicioID, nombreCategoria, descripcion, ventaTipoID, activo
FROM dbo.[Servicios.Categorias]
ORDER BY servicioID, servicioCategoriaID;

-- 03_servicios_tipos.csv
SELECT servicioTipoID, servicioCategoriaID, codigoTipoServicio, nombreTipo, descripcion, activo
FROM dbo.[Servicios.Tipos]
ORDER BY servicioCategoriaID, servicioTipoID;

-- 04_grupos.csv
SELECT grupoID, tarifaAmbitoID, nombreGrupo, descripcion, tipo
FROM dbo.Grupos
ORDER BY grupoID;

-- 05_paises.csv
SELECT paisID, paisNombre, numeroISO1, numeroISO2, numeroISO3, continente, idioma
FROM dbo.Paises
ORDER BY paisNombre;

-- 06_grupos_paises.csv
SELECT grupoID, paisID
FROM dbo.[Grupos.Paises]
ORDER BY grupoID, paisID;

-- 07_vias_encaminamiento.csv
SELECT viaEncaminamientoID, nombreViaEncaminamiento, descripcion
FROM dbo.[Servicios.ViasEncaminamiento]
ORDER BY viaEncaminamientoID;

-- 08_tarifas.csv
SELECT tarifaID, servicioTipoID, grupoID, paisID, viaEncaminamientoID,
       rangoUnidad, rangoInicio, rangoFin, rangoIntervalo, cantidadTasas,
       tasaInicio, tasaFin, tasaIntervalo, tarifaAmbitoID, activo, aplicarRedondeo
FROM dbo.Tarifas
ORDER BY servicioTipoID, tarifaID;

-- 09_tarifas_ajustes.csv
SELECT tarifaAjusteID, tarifaID, rangoInicio, rangoFin,
       rangoInicioAjuste, rangoFinAjuste, tasaAjuste, ajusteAcumulativo
FROM dbo.[Tarifas.Ajustes]
ORDER BY tarifaID, rangoInicio;

-- 10_servicios_suplementarios.csv
SELECT servicioSuplementarioID, nombreServicioSuplementario, precio,
       tarifaAmbitoID, requiereCodigoEnvio, servicioSuplementarioPadreID,
       excluirDeDistribucionPostal, requerido, productoID
FROM dbo.[Servicios.Suplementarios]
ORDER BY servicioSuplementarioID;

-- 11_tarifas_servicios_suplementarios.csv
SELECT tarifaID, servicioSuplementarioID, precioAjuste
FROM dbo.[Tarifas.ServiciosSuplementarios]
ORDER BY tarifaID, servicioSuplementarioID;

-- 12_codigos_envio_tipos.csv
SELECT codigoEnvioTipoID, nombreCodigoEnvio, nombreEtiqueta,
       aplicaServicioSuplementarioID, aplicaServicioTipoID
FROM dbo.[CodigosEnvios.Tipos]
ORDER BY codigoEnvioTipoID;

-- 13_codigos_envio_formatos.csv
SELECT codigoEnvioTipoID, letraPosicionUno, letraPosicionDos, letrasEtiquetas,
       anio, codigosEtiquetas, cantidadImpresion, autoImprimirEtiquetas
FROM dbo.[CodigosEnvios.TiposFormatoVigentes]
ORDER BY anio DESC, codigoEnvioTipoID;

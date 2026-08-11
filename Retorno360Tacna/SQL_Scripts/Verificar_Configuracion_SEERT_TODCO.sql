-- ================================================
-- Script de Verificación y Corrección de Configuración
-- Base de Datos: SEERT_TODCO
-- Razón Social: RASMUSSEN DE TECATE SA DE CV
-- ================================================

-- PASO 1: Verificar configuración actual
PRINT '================================================'
PRINT 'VERIFICACIÓN DE CONFIGURACIÓN ACTUAL'
PRINT '================================================'
PRINT ''

-- 1.1 Verificar NOM_TABLARAZON
PRINT '1. Configuración en NOM_TABLARAZON:'
PRINT '-----------------------------------'
SELECT 
    IdTabla,
    NOMBRE_TABLA,
    IdRazon,
    ConnExterna,
    IdConexion,
    CASE 
        WHEN ConnExterna = 'S' AND IdConexion IS NOT NULL THEN '✅ Configurado correctamente'
        WHEN ConnExterna IS NULL OR ConnExterna <> 'S' THEN '❌ ConnExterna debe ser "S"'
        WHEN IdConexion IS NULL THEN '❌ IdConexion no está configurado'
        ELSE '⚠️ Revisar configuración'
    END AS Estado
FROM NOM_TABLARAZON
WHERE NOMBRE_TABLA = 'SEERT_TODCO' AND IdRazon = 17
PRINT ''

-- 1.2 Verificar RAZONXTABLA
PRINT '2. Configuración en RAZONXTABLA:'
PRINT '--------------------------------'
SELECT 
    IdRazon,
    NOMBRE_RAZON,
    DB,
    ConnExterna,
    IdConexion,
    CASE 
        WHEN ConnExterna = 'S' AND IdConexion IS NOT NULL THEN '✅ Configurado correctamente'
        ELSE '⚠️ Revisar configuración'
    END AS Estado
FROM RAZONXTABLA
WHERE IdRazon = 17
PRINT ''

-- 1.3 Verificar Conexiones
PRINT '3. Configuración en Conexiones:'
PRINT '-------------------------------'
SELECT 
    IdConexion,
    NombreConexion,
    Servidor,
    UsuarioSQL,
    CASE 
        WHEN PasswordSQL IS NOT NULL THEN '****** (Configurado)'
        ELSE 'NULL (Sin configurar)'
    END AS PasswordSQL,
    TipoMotor,
    Activo,
    CASE 
        WHEN Servidor IS NOT NULL AND UsuarioSQL IS NOT NULL AND PasswordSQL IS NOT NULL THEN '✅ Configurado correctamente'
        ELSE '❌ Faltan credenciales'
    END AS Estado
FROM Conexiones
WHERE IdConexion = 2
PRINT ''

-- ================================================
-- PASO 2: Script de corrección (COMENTADO)
-- Descomenta y ejecuta SOLO si la verificación muestra errores
-- ================================================

/*
PRINT '================================================'
PRINT 'APLICANDO CORRECCIONES'
PRINT '================================================'
PRINT ''

-- 2.1 Actualizar NOM_TABLARAZON
PRINT '1. Actualizando NOM_TABLARAZON...'
UPDATE NOM_TABLARAZON
SET 
    ConnExterna = 'S',
    IdConexion = 2
WHERE NOMBRE_TABLA = 'SEERT_TODCO' 
  AND IdRazon = 17
PRINT '   ✅ SEERT_TODCO actualizado'
PRINT ''

-- 2.2 Verificar que la conexión externa existe
IF NOT EXISTS (SELECT 1 FROM Conexiones WHERE IdConexion = 2)
BEGIN
    PRINT '2. Creando conexión externa en tabla Conexiones...'
    INSERT INTO Conexiones (IdConexion, NombreConexion, Servidor, UsuarioSQL, PasswordSQL, TipoMotor, Activo)
    VALUES (
        2,
        'TJ-SQL2019-03',
        '172.20.21.36',
        'jnieto',
        'admin1234',
        'SQLServer',
        1
    )
    PRINT '   ✅ Conexión creada'
END
ELSE
BEGIN
    PRINT '2. Actualizando conexión externa en tabla Conexiones...'
    UPDATE Conexiones
    SET 
        Servidor = '172.20.21.36',
        UsuarioSQL = 'jnieto',
        PasswordSQL = 'admin1234',
        Activo = 1
    WHERE IdConexion = 2
    PRINT '   ✅ Conexión actualizada'
END
PRINT ''

-- 2.3 Verificar que RAZONXTABLA está correcto
PRINT '3. Verificando RAZONXTABLA...'
UPDATE RAZONXTABLA
SET 
    ConnExterna = 'S',
    IdConexion = 2
WHERE IdRazon = 17
  AND DB = 'SEERT_RASMUSSEN'
PRINT '   ✅ RAZONXTABLA verificado'
PRINT ''

PRINT '================================================'
PRINT '✅ CORRECCIONES APLICADAS'
PRINT '================================================'
PRINT ''
PRINT 'RESULTADO ESPERADO:'
PRINT '  • SEERT_TODCO → Servidor 172.20.21.36 (IdConexion=2)'
PRINT '  • SEERT_RASMUSSEN → Servidor 172.20.21.36 (IdConexion=2)'
PRINT '  • Ambas bases en el MISMO servidor'
PRINT '  • Cálculo de retorno debe funcionar correctamente'
PRINT ''
*/

-- ================================================
-- PASO 3: Verificación final (después de aplicar correcciones)
-- ================================================

PRINT '================================================'
PRINT 'VERIFICACIÓN FINAL'
PRINT '================================================'
PRINT ''

PRINT 'Configuración completa:'
PRINT '----------------------'
SELECT 
    'NOM_TABLARAZON' AS Tabla,
    N.NOMBRE_TABLA AS BaseDatos,
    N.IdRazon,
    N.ConnExterna,
    N.IdConexion,
    C.Servidor,
    C.UsuarioSQL
FROM NOM_TABLARAZON N
LEFT JOIN Conexiones C ON N.IdConexion = C.IdConexion
WHERE N.NOMBRE_TABLA = 'SEERT_TODCO' AND N.IdRazon = 17

UNION ALL

SELECT 
    'RAZONXTABLA' AS Tabla,
    R.DB AS BaseDatos,
    R.IdRazon,
    R.ConnExterna,
    R.IdConexion,
    C.Servidor,
    C.UsuarioSQL
FROM RAZONXTABLA R
LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
WHERE R.IdRazon = 17

PRINT ''
PRINT '================================================'
PRINT 'DIAGNÓSTICO COMPLETO'
PRINT '================================================'
PRINT ''
PRINT 'SI TODO ESTÁ CORRECTO:'
PRINT '  ✅ SEERT_TODCO debe tener IdConexion = 2'
PRINT '  ✅ SEERT_RASMUSSEN debe tener IdConexion = 2'
PRINT '  ✅ IdConexion 2 debe apuntar a 172.20.21.36'
PRINT '  ✅ Usuario debe ser "jnieto" (no "MedTiempos")'
PRINT ''
PRINT 'SI HAY ERRORES:'
PRINT '  1. Descomenta la sección "APLICANDO CORRECCIONES"'
PRINT '  2. Ejecuta nuevamente este script'
PRINT '  3. Reinicia la aplicación'
PRINT ''

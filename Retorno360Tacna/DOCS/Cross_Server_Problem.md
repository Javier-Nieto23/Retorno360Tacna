# ⚠️ Problema: Cross-Server Queries en SQL Server

## 📋 Descripción del Problema

El error "Cannot open database 'SEERT_VIDRIOS' requested by the login" ocurre porque el cálculo de retorno intenta hacer un **JOIN entre bases de datos que están en servidores SQL diferentes**.

### 🔍 Ejemplo del Problema

```sql
-- Esta query FALLA si las bases están en servidores diferentes:
SELECT *
FROM [BaseSeleccionada].dbo.Di_Pedimento DI
WHERE EXISTS (
    SELECT 1 
    FROM [BaseOrigen].dbo.TR_Glosa G  -- ❌ Si está en otro servidor, SQL no puede hacer esto
    WHERE G.Gl_Aduana = DI.Adu_AduanaSecc
)
```

### 📊 Escenario Actual

```
Servidor Principal (172.20.20.26)
├─ RetornoMaster
├─ BASE_A
└─ BASE_B

Servidor Secundario (172.20.21.33)
├─ SEERT_VIDRIOS
└─ OTRA_BASE

❌ PROBLEMA:
Si seleccionas:
  - Base Seleccionada: BASE_A (en servidor principal)
  - Base Origen: SEERT_VIDRIOS (en servidor secundario)

El query intenta hacer:
  SELECT * FROM [BASE_A].dbo.Di_Pedimento
  WHERE EXISTS (SELECT 1 FROM [SEERT_VIDRIOS].dbo.TR_Glosa ...)

Pero SEERT_VIDRIOS no existe en el servidor donde está BASE_A.
```

## ✅ Soluciones

### Solución 1: Ambas Bases en el Mismo Servidor (RECOMENDADO)

La forma más simple es asegurar que **la base seleccionada y la base origen estén en el mismo servidor**.

**¿Cómo funciona?**

En `NOM_TABLARAZON`, cada razón social debe tener:
- Una entrada para cada base de datos
- Todas las bases de esa razón social deben estar en el mismo servidor

**Ejemplo de configuración correcta**:

```sql
-- Razón Social: SEERT
-- TODAS las bases de SEERT deben estar en el servidor secundario
INSERT INTO NOM_TABLARAZON (IdRazon, NOMBRE_TABLA)
VALUES 
    (1, 'SEERT_VIDRIOS'),    -- En servidor 172.20.21.33
    (1, 'SEERT_EXPORTA'),    -- En servidor 172.20.21.33
    (1, 'SEERT_IMPORTA');    -- En servidor 172.20.21.33

-- Razón Social: EMPRESA_A
-- TODAS las bases de EMPRESA_A deben estar en el servidor principal
INSERT INTO NOM_TABLARAZON (IdRazon, NOMBRE_TABLA)
VALUES 
    (2, 'EMPRESA_A_DB1'),    -- En servidor 172.20.20.26
    (2, 'EMPRESA_A_DB2');    -- En servidor 172.20.20.26
```

**✅ Con esta configuración**:
- Cuando seleccionas una base de SEERT → El sistema se conecta al servidor secundario
- Todas las bases de SEERT están ahí → El JOIN funciona perfectamente
- No se necesitan cambios de código

---

### Solución 2: Configurar Linked Server en SQL Server

Si necesitas que las bases estén en servidores diferentes, debes configurar **Linked Server**.

#### Paso 1: Crear Linked Server en el Servidor Principal

```sql
-- Ejecutar en el servidor 172.20.20.26
EXEC sp_addlinkedserver   
    @server='ServidorSecundario',     -- Nombre del linked server
    @srvproduct='',
    @provider='SQLNCLI',              -- O 'MSOLEDBSQL'
    @datasrc='172.20.21.33';          -- IP del servidor secundario

-- Configurar credenciales
EXEC sp_addlinkedsrvlogin
    @rmtsrvname='ServidorSecundario',
    @useself='FALSE',
    @locallogin=NULL,
    @rmtuser='MedTiempos',
    @rmtpassword='T3ch4dm1n';
```

#### Paso 2: Probar Linked Server

```sql
-- Probar acceso
SELECT * FROM [ServidorSecundario].[SEERT_VIDRIOS].dbo.TR_Glosa
```

#### Paso 3: Modificar Código para Usar Linked Server

Si usas Linked Server, debes cambiar el código para usar el nombre del linked server:

```csharp
// En ValidarPedimentosCruzados, cambiar:
string baseDatosOrigenConServidor = baseDatosOrigen;

// Por:
string baseDatosOrigenConServidor = gestorConexiones.EsConexionSecundaria(baseDatosOrigen)
    ? $"[ServidorSecundario].[{baseDatosOrigen}]"
    : $"[{baseDatosOrigen}]";

// Luego usar en el query:
string sql = $@"
SELECT ...
FROM [{baseDatosSeleccionada}].dbo.Di_Pedimento DI
WHERE EXISTS (
    SELECT 1 
    FROM {baseDatosOrigenConServidor}.dbo.TR_Glosa G
    ...
)";
```

**⚠️ Desventajas**:
- Más complejo de configurar
- Performance puede ser más lento
- Requiere permisos adicionales en SQL Server

---

### Solución 3: Queries Separadas (Opción de Respaldo)

Si no puedes usar las soluciones anteriores, se pueden hacer dos queries separadas:

1. Query 1: Obtener pedimentos de `baseDatosSeleccionada`
2. Query 2: Obtener glosas de `baseDatosOrigen`
3. Cruzar los datos en memoria con C#

**⚠️ Desventajas**:
- Más tráfico de red
- Mayor uso de memoria
- Código más complejo
- Performance más lento

---

## 🎯 Recomendación

**Usa la Solución 1: Mismo Servidor**

Es la más simple, rápida y confiable. Asegura que todas las bases de datos de una razón social estén en el mismo servidor SQL.

### ✅ Pasos a Seguir

1. **Revisar `NOM_TABLARAZON`**: Agrupa bases de datos por servidor
2. **Migrar bases si es necesario**: Si una razón tiene bases en ambos servidores, muévelas todas a uno solo
3. **Actualizar configuración**: El código actual ya maneja múltiples servidores automáticamente

### 📝 Script para Revisar Configuración Actual

```sql
-- Ver qué bases están configuradas para cada razón social
SELECT 
    R.IdRazon,
    R.NOMBRE_RAZON,
    R.DB as BaseDatosOrigen,
    N.NOMBRE_TABLA as BasesDisponibles
FROM RAZONXTABLA R
LEFT JOIN NOM_TABLARAZON N ON R.IdRazon = N.IdRazon
ORDER BY R.IdRazon, N.NOMBRE_TABLA
```

### 🔍 Verificar si Hay Problemas

```sql
-- Encontrar razones sociales que tienen bases en múltiples servidores
-- (Esto requeriría crear una tabla de mapeo servidor-base primero)
```

---

## 🛠️ Cambios Realizados en el Código

El código ahora:

1. ✅ **Detecta cuando las bases están en servidores diferentes**
2. ✅ **Muestra un mensaje de error claro** explicando el problema
3. ✅ **Registra información de diagnóstico** en el Debug Output
4. ✅ **Usa automáticamente el servidor correcto** cuando ambas bases están en el mismo lugar

### Mensaje de Error Mejorado

Cuando el problema ocurre, ahora ves:

```
❌ ERROR DE CONFIGURACIÓN MULTI-SERVIDOR

Las bases de datos están en servidores diferentes:
  • Base seleccionada: BASE_A → 172.20.20.26
  • Base origen: SEERT_VIDRIOS → 172.20.21.33

Para que el cálculo de retorno funcione, ambas bases de datos deben estar:
1. En el MISMO servidor, O
2. Configurar Linked Server en SQL Server

Por favor, verifica la configuración de las bases de datos en NOM_TABLARAZON.
```

---

## 📞 Preguntas Frecuentes

### P: ¿Por qué funcionaba antes?

R: Probablemente todas las bases estaban en el mismo servidor antes. Al agregar el servidor secundario y mover bases de datos allá, se creó este problema.

### P: ¿No puede la aplicación manejar esto automáticamente?

R: No de forma nativa. SQL Server no permite JOINs entre servidores sin configuración adicional (Linked Server). Es una limitación del motor de SQL Server, no de la aplicación.

### P: ¿Cuál es la mejor solución a largo plazo?

R: Mantener todas las bases de datos de una razón social en el mismo servidor. Es más rápido, simple y confiable.

### P: ¿Qué pasa si NECESITO tener bases en servidores diferentes?

R: Entonces debes configurar Linked Server (Solución 2) o modificar el código para hacer queries separadas (Solución 3).

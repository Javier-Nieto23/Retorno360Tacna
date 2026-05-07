# 🔍 Explicación: ¿Qué TR_GLOSA se usa en cada paso?

## 📋 Pregunta

> "y al hacer la conexion secundaria a que TR_GLOSA se tomando para comparar con Di_Pedimento y De_Pedimentos?"

## ✅ Respuesta Directa

**Se usa la tabla `TR_Glosa` de la base de datos `baseDatosOrigen`**, que es el campo `DB` de la tabla `RAZONXTABLA` para la razón social seleccionada.

---

## 🔍 Flujo Detallado

### 1️⃣ Obtener Base de Datos Origen

```csharp
// En CalcularRetorno(), línea 316
RazonSocial razonInfo = ObtenerRazonSocial(idRazon);
```

**Query ejecutado**:
```sql
-- Conexión: Servidor Principal (172.20.20.26)
-- Base de datos: RetornoMaster
SELECT IdRazon, NOMBRE_RAZON, DB 
FROM RAZONXTABLA 
WHERE IdRazon = @IdRazon
```

**Resultado**:
```
baseDatosOrigen = razonInfo.BaseDatosOrigen  
                  ↑
                  └── Campo 'DB' de RAZONXTABLA
```

### Ejemplo:
```
IdRazon = 1 (SEERT)
NOMBRE_RAZON = "SEERT Vidrios"
DB = "SEERT_VIDRIOS"  ← Este es baseDatosOrigen
```

---

### 2️⃣ Validar Pedimentos Cruzados

```csharp
// En CalcularRetorno(), línea 332
var pedimentosValidos = ValidarPedimentosCruzados(
    baseDatosSeleccionada,  // Ej: "SEERT_VIDRIOS"
    razonInfo.BaseDatosOrigen,  // Ej: "SEERT_VIDRIOS"
    fechaInicio,
    fechaFin
);
```

**Conexión**:
- Servidor: El que corresponda a `baseDatosSeleccionada` (determinado por `gestorConexiones`)
- Base de datos contexto: `baseDatosSeleccionada`

**Query ejecutado**:
```sql
-- Si baseDatosSeleccionada = "SEERT_VIDRIOS" en servidor 172.20.21.33
-- La conexión se abre a: Server=172.20.21.33;Database=SEERT_VIDRIOS

SELECT
    X.Tipo, X.Aduana, X.Patente, X.Pedimento, X.FechaPago
FROM
(
    -- IMPORTACIONES
    SELECT DISTINCT
        'IMPORTACION' AS Tipo,
        DI.Adu_AduanaSecc AS Aduana,
        DI.AgP_Patente AS Patente,
        DI.Pim_Folio AS Pedimento,
        DI.Pim_FechaPago AS FechaPago
    FROM Di_Pedimento DI
         ↑
         └── Tabla en la base de datos LOCAL (SEERT_VIDRIOS)
             Sin prefijo = base de datos del contexto de conexión

    WHERE DI.Pim_FechaPago >= @FechaInicio
      AND DI.Pim_FechaPago <= @FechaFin
      AND EXISTS (
          SELECT 1 
          FROM [SEERT_VIDRIOS].dbo.TR_Glosa G
                ↑
                └── Base de datos EXPLÍCITA (baseDatosOrigen)
                    Con prefijo [baseDatos] = puede ser diferente

          WHERE G.Gl_Aduana = DI.Adu_AduanaSecc
            AND G.Gl_Patente = DI.AgP_Patente
            AND G.Gl_Pedimento = DI.Pim_Folio
            AND G.Gl_TOper = 1  -- Importación
      )

    UNION ALL

    -- EXPORTACIONES
    SELECT DISTINCT
        'EXPORTACION' AS Tipo,
        DE.Adu_AduanaSecc AS Aduana,
        DE.AgP_Patente AS Patente,
        DE.Pex_Folio AS Pedimento,
        DE.Pex_FechaPago AS FechaPago
    FROM De_Pedimento DE
         ↑
         └── Tabla en la base de datos LOCAL (SEERT_VIDRIOS)

    WHERE DE.Pex_FechaPago >= @FechaInicio
      AND DE.Pex_FechaPago <= @FechaFin
      AND EXISTS (
          SELECT 1 
          FROM [SEERT_VIDRIOS].dbo.TR_Glosa G
                ↑
                └── Base de datos EXPLÍCITA (baseDatosOrigen)

          WHERE G.Gl_Aduana = DE.Adu_AduanaSecc
            AND G.Gl_Patente = DE.AgP_Patente
            AND G.Gl_Pedimento = DE.Pex_Folio
            AND G.Gl_TOper = 2  -- Exportación
      )
) X
ORDER BY X.Tipo, X.Aduana, X.Patente, X.Pedimento
```

**Tablas usadas**:
- `Di_Pedimento` → Base de datos: **baseDatosSeleccionada** (contexto de conexión)
- `De_Pedimento` → Base de datos: **baseDatosSeleccionada** (contexto de conexión)
- `TR_Glosa` → Base de datos: **baseDatosOrigen** (especificada explícitamente)

---

### 3️⃣ Obtener Importaciones Validadas

```csharp
// En CalcularRetorno(), línea 345
decimal importado = ObtenerImportacionesValidadas(
    razonInfo.BaseDatosOrigen,  // Ej: "SEERT_VIDRIOS"
    pedimentosValidos.Where(p => p.Tipo == "IMPORTACION").ToList(),
    fechaInicio,
    fechaFin
);
```

**Conexión**:
- Servidor: El que corresponda a `baseDatosOrigen` (determinado por `gestorConexiones`)
- Base de datos: `baseDatosOrigen`

**Query ejecutado**:
```sql
-- Si baseDatosOrigen = "SEERT_VIDRIOS" en servidor 172.20.21.33
-- La conexión se abre a: Server=172.20.21.33;Database=SEERT_VIDRIOS

SELECT ISNULL(SUM(CAST(Gl_ValorComercial AS DECIMAL(28,2))), 0)
FROM [SEERT_VIDRIOS].dbo.TR_Glosa par
      ↑
      └── Base de datos EXPLÍCITA (baseDatosOrigen)

LEFT JOIN [SEERT_VIDRIOS].dbo.TR_GlosaIdentifica ide
          ↑
          └── Misma base de datos (baseDatosOrigen)

LEFT JOIN [SEERT_VIDRIOS].dbo.TR_GlosaR1 r1
          ↑
          └── Misma base de datos (baseDatosOrigen)

WHERE (Pedimento_Ant IS NULL)
  AND Gl_CveDocto IN ('IN', 'V1')
  AND Gl_TOper = 1
  AND ide.Identificador IS NULL
  AND Gl_Aduana + '-' + Gl_Patente + '-' + par.Gl_Pedimento IN (...)
  AND CONVERT(DATE, Gl_FecPagoReal, 101) BETWEEN @ini AND @fin
  AND Gl_OrigenZipGlosa = 'S'
```

**Tabla usada**:
- `TR_Glosa` → Base de datos: **baseDatosOrigen**
- `TR_GlosaIdentifica` → Base de datos: **baseDatosOrigen**
- `TR_GlosaR1` → Base de datos: **baseDatosOrigen**

---

### 4️⃣ Obtener Exportaciones Validadas

```csharp
// En CalcularRetorno(), línea 352
decimal exportado = ObtenerExportacionesValidadas(
    razonInfo.BaseDatosOrigen,  // Ej: "SEERT_VIDRIOS"
    pedimentosValidos.Where(p => p.Tipo == "EXPORTACION").ToList(),
    fechaInicio,
    fechaFin,
    incluirMateriaPrima
);
```

**Similar al paso anterior, pero para exportaciones** (`Gl_TOper = 2`).

**Tabla usada**:
- `TR_Glosa` → Base de datos: **baseDatosOrigen**

---

## 📊 Resumen Visual

```
┌────────────────────────────────────────────────────────────────┐
│ TABLAS USADAS EN CADA PASO                                     │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│ PASO 1: ObtenerRazonSocial()                                   │
│   └─ RetornoMaster.dbo.RAZONXTABLA                             │
│      (Servidor Principal siempre)                              │
│                                                                │
│ PASO 2: ValidarPedimentosCruzados()                            │
│   ├─ [baseDatosSeleccionada].dbo.Di_Pedimento                  │
│   ├─ [baseDatosSeleccionada].dbo.De_Pedimento                  │
│   └─ [baseDatosOrigen].dbo.TR_Glosa  ← AQUÍ                    │
│                                                                │
│ PASO 3: ObtenerImportacionesValidadas()                        │
│   ├─ [baseDatosOrigen].dbo.TR_Glosa  ← AQUÍ                    │
│   ├─ [baseDatosOrigen].dbo.TR_GlosaIdentifica                  │
│   └─ [baseDatosOrigen].dbo.TR_GlosaR1                          │
│                                                                │
│ PASO 4: ObtenerExportacionesValidadas()                        │
│   ├─ [baseDatosOrigen].dbo.TR_Glosa  ← AQUÍ                    │
│   ├─ [baseDatosOrigen].dbo.TR_GlosaIdentifica                  │
│   ├─ [baseDatosOrigen].dbo.TR_GlosaPartidaIdentifica           │
│   └─ [baseDatosOrigen].dbo.TR_GlosaR1                          │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

---

## ⚠️ Por Qué Ambas Bases Deben Estar en el Mismo Servidor

### Escenario Problemático

```
Servidor 172.20.20.26 (Principal)
├─ RetornoMaster
└─ EMPRESA_A

Servidor 172.20.21.33 (Secundario)
└─ SEERT_VIDRIOS

Usuario selecciona:
  • Razón Social: SEERT
  • Base Seleccionada: EMPRESA_A (servidor principal)

Sistema obtiene:
  • baseDatosOrigen = "SEERT_VIDRIOS" (de RAZONXTABLA.DB)

En ValidarPedimentosCruzados():
  • Se conecta a servidor 172.20.20.26 (servidor de EMPRESA_A)
  • Query intenta acceder a:
    - Di_Pedimento  ✅ Existe en EMPRESA_A (servidor principal)
    - TR_Glosa de SEERT_VIDRIOS  ❌ NO existe en servidor principal

ERROR: "Cannot open database 'SEERT_VIDRIOS' requested by the login"
```

---

## ✅ Configuración Correcta

Para que funcione, **baseDatosSeleccionada y baseDatosOrigen deben estar en el mismo servidor**.

### Opción 1: Ambas en Servidor Principal

```
Servidor 172.20.20.26
├─ RetornoMaster
├─ EMPRESA_A
└─ EMPRESA_A_ORIGEN
```

**RAZONXTABLA**:
```sql
IdRazon = 1
NOMBRE_RAZON = "Empresa A"
DB = "EMPRESA_A_ORIGEN"  ← Base origen en servidor principal
```

**NOM_TABLARAZON**:
```sql
IdRazon = 1, NOMBRE_TABLA = "EMPRESA_A"         ← En servidor principal
IdRazon = 1, NOMBRE_TABLA = "EMPRESA_A_ORIGEN"  ← En servidor principal
```

### Opción 2: Ambas en Servidor Secundario

```
Servidor 172.20.21.33
├─ SEERT_VIDRIOS
└─ SEERT_VIDRIOS_ORIGEN
```

**RAZONXTABLA**:
```sql
IdRazon = 2
NOMBRE_RAZON = "SEERT"
DB = "SEERT_VIDRIOS"  ← Base origen en servidor secundario
```

**NOM_TABLARAZON**:
```sql
IdRazon = 2, NOMBRE_TABLA = "SEERT_VIDRIOS"  ← En servidor secundario
```

---

## 🔍 Cómo Verificar el Logging

Con los cambios implementados, ahora verás en **Debug Output**:

```
🔍 INICIO CÁLCULO DE RETORNO
   Base de datos seleccionada: SEERT_VIDRIOS
   ¿Es conexión secundaria?: True
   Servidor a usar: 172.20.21.33
   Base de datos origen: SEERT_VIDRIOS
   ¿Origen es secundaria?: True
   Servidor origen: 172.20.21.33

🔍 VALIDAR PEDIMENTOS CRUZADOS
   Base seleccionada: SEERT_VIDRIOS (Servidor: 172.20.21.33)
   Base origen: SEERT_VIDRIOS (Servidor: 172.20.21.33)
   ✅ Ambas bases en el mismo servidor: 172.20.21.33

   📋 Query a ejecutar:
   Conexión: 172.20.21.33
   Di_Pedimento y De_Pedimento de: [SEERT_VIDRIOS]
   TR_Glosa de: [SEERT_VIDRIOS]
   (Ambas deben estar en el mismo servidor: 172.20.21.33)

📊 OBTENER IMPORTACIONES VALIDADAS
   Base de datos: SEERT_VIDRIOS
   Servidor: 172.20.21.33
   Tabla TR_Glosa: [SEERT_VIDRIOS].dbo.TR_Glosa
   Pedimentos a validar: 150

   🔌 Conexión abierta:
      Server: 172.20.21.33
      Database: SEERT_VIDRIOS
   ✅ Query ejecutado exitosamente
      Resultado: 1234567.89

📊 OBTENER EXPORTACIONES VALIDADAS
   Base de datos: SEERT_VIDRIOS
   Servidor: 172.20.21.33
   Tabla TR_Glosa: [SEERT_VIDRIOS].dbo.TR_Glosa
   Pedimentos a validar: 200
   Incluir materia prima: True

   🔌 Conexión abierta:
      Server: 172.20.21.33
      Database: SEERT_VIDRIOS
   ✅ Query ejecutado exitosamente
      Resultado: 987654.32
```

---

## 📝 Conclusión

**¿A qué TR_GLOSA se accede?**

- **Siempre** a `TR_Glosa` de la base de datos **`baseDatosOrigen`**
- **`baseDatosOrigen`** viene del campo `DB` de `RAZONXTABLA`
- **Para que funcione**: `baseDatosSeleccionada` y `baseDatosOrigen` deben estar en el **mismo servidor SQL**

El sistema ya enruta correctamente las conexiones, pero **no puede hacer JOINs cross-server sin Linked Server**.

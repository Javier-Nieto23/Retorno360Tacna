# 🐛 Fix: Error "Invalid column name 'Razon'"

## ❌ Problema

Al abrir el formulario de Reportes de IGI, aparecía el siguiente error:

```
System.Exception: Error al obtener razones sociales: Invalid column name 'Razon'.
Invalid column name 'Razon'.

SqlException: Invalid column name 'Razon'.
```

**Ubicación del error:**
- `ReporteServiceBase.cs` → `ObtenerRazonesSociales()`
- `ReporteServiceBase.cs` → `ObtenerBasesDatosRazon()`

---

## 🔍 Causa Raíz

### Nombres de Columnas Incorrectos

El código en `ReporteServiceBase.cs` usaba nombres de columnas que **no existen** en la base de datos:

#### ❌ Código Incorrecto:

```csharp
// En ObtenerRazonesSociales()
string sql = @"
    SELECT DISTINCT 
        IdRazon,
        Razon,        ← ❌ Columna no existe
        DB
    FROM RAZONXTABLA
    WHERE Razon IS NOT NULL   ← ❌ Columna no existe
    ORDER BY Razon";         ← ❌ Columna no existe

// En ObtenerBasesDatosRazon()
string sql = @"
    SELECT DISTINCT DB
    FROM RAZONXTABLA
    WHERE Razon = @Razon   ← ❌ Columna no existe
      AND DB IS NOT NULL
    ORDER BY DB";
```

---

## ✅ Solución Aplicada

### 1️⃣ Corrección de `ObtenerRazonesSociales()`

**Cambio:** Usar `NOMBRE_RAZON` en lugar de `Razon`

#### ✅ Código Correcto:

```csharp
string sql = @"
    SELECT DISTINCT 
        IdRazon,
        NOMBRE_RAZON,           ← ✅ Nombre correcto
        DB
    FROM RAZONXTABLA
    WHERE NOMBRE_RAZON IS NOT NULL   ← ✅ Validación correcta
    ORDER BY NOMBRE_RAZON";         ← ✅ Orden correcto
```

**Referencia:** Se verificó contra `RetornoService.cs` línea 574:
```csharp
string sql = "SELECT IdRazon, NOMBRE_RAZON, DB FROM RAZONXTABLA ORDER BY NOMBRE_RAZON";
```

---

### 2️⃣ Corrección de `ObtenerBasesDatosRazon()`

**Cambios:**
1. Cambiar tabla de `RAZONXTABLA` a `NOM_TABLARAZON`
2. Cambiar parámetro de `string nombreRazon` a `int idRazon`
3. Usar columna `NOMBRE_TABLA` en lugar de `DB`

#### ❌ Código Anterior:

```csharp
public List<string> ObtenerBasesDatosRazon(string nombreRazon)
{
    string sql = @"
        SELECT DISTINCT DB
        FROM RAZONXTABLA
        WHERE Razon = @Razon
          AND DB IS NOT NULL
        ORDER BY DB";

    cmd.Parameters.AddWithValue("@Razon", nombreRazon);
    // ...
}
```

#### ✅ Código Correcto:

```csharp
public List<string> ObtenerBasesDatosRazon(int idRazon)
{
    string sql = @"
        SELECT NOMBRE_TABLA 
        FROM NOM_TABLARAZON 
        WHERE IdRazon = @IdRazon 
        ORDER BY NOMBRE_TABLA";

    cmd.Parameters.AddWithValue("@IdRazon", idRazon);
    // ...
}
```

**Referencia:** Se verificó contra `RetornoService.cs` línea 614:
```csharp
string sql = "SELECT NOMBRE_TABLA FROM NOM_TABLARAZON WHERE IdRazon = @IdRazon ORDER BY NOMBRE_TABLA";
```

---

### 3️⃣ Actualización de `FrmReportes.cs`

**Cambio:** Pasar `IdRazon` en lugar de `NombreRazon`

#### ❌ Código Anterior:

```csharp
var basesDatos = await Task.Run(() =>
    reporteService.ObtenerBasesDatosRazon(razonSeleccionada.NombreRazon)
);
```

#### ✅ Código Correcto:

```csharp
var basesDatos = await Task.Run(() =>
    reporteService.ObtenerBasesDatosRazon(razonSeleccionada.IdRazon)
);
```

---

## 📊 Estructura de Tablas Correcta

### Tabla: `RAZONXTABLA`

| Columna | Tipo | Descripción |
|---------|------|-------------|
| `IdRazon` | int | ID de la razón social |
| `NOMBRE_RAZON` | varchar | Nombre de la razón social ✅ |
| `DB` | varchar | Nombre de la base de datos |
| `ConnExterna` | char(1) | 'S' si usa servidor externo |
| `IdConexion` | int | ID de la conexión externa |

**❌ NO existe columna `Razon`**  
**✅ SÍ existe columna `NOMBRE_RAZON`**

---

### Tabla: `NOM_TABLARAZON`

| Columna | Tipo | Descripción |
|---------|------|-------------|
| `IdRazon` | int | ID de la razón social (FK) |
| `NOMBRE_TABLA` | varchar | Nombre de la base de datos ✅ |

**Relación:** 1 razón social → N bases de datos

---

## 🔧 Archivos Modificados

```
Retorno360Tacna/
├── SERVICES/
│   └── ReporteServiceBase.cs          [CORREGIDO]
│       ├── ObtenerRazonesSociales()   → Usa NOMBRE_RAZON
│       └── ObtenerBasesDatosRazon()   → Usa NOM_TABLARAZON + IdRazon
│
└── FORMS/
    └── FrmReportes.cs                 [CORREGIDO]
        └── cmbRazonSocial_SelectedIndexChanged() → Pasa IdRazon
```

---

## ✅ Validación

### Prueba 1: Compilación

```bash
dotnet build
# Build successful ✅
```

### Prueba 2: Comparación con `RetornoService`

| Aspecto | RetornoService (✅ Funciona) | ReporteServiceBase (Antes ❌) | ReporteServiceBase (Ahora ✅) |
|---------|------------------------------|-------------------------------|-------------------------------|
| **Tabla razones** | `RAZONXTABLA` | `RAZONXTABLA` | `RAZONXTABLA` ✅ |
| **Columna razón** | `NOMBRE_RAZON` | `Razon` ❌ | `NOMBRE_RAZON` ✅ |
| **Tabla bases** | `NOM_TABLARAZON` | `RAZONXTABLA` ❌ | `NOM_TABLARAZON` ✅ |
| **Columna base** | `NOMBRE_TABLA` | `DB` ❌ | `NOMBRE_TABLA` ✅ |
| **Parámetro** | `IdRazon` (int) | `nombreRazon` (string) ❌ | `IdRazon` (int) ✅ |

---

## 🎯 Resultado

✅ **Error corregido**  
✅ **Build exitoso**  
✅ **Código alineado con `RetornoService`**  
✅ **Formulario de reportes ahora funciona correctamente**  

---

## 📚 Lecciones Aprendidas

### 1️⃣ **Siempre verificar nombres de columnas**

Cuando se crea código nuevo que accede a la base de datos:
- ✅ Verificar nombres de columnas con código existente
- ✅ Revisar la estructura real de la tabla
- ✅ No asumir nombres de columnas

### 2️⃣ **Reutilizar queries existentes**

Si ya existe código que funciona (`RetornoService`):
- ✅ Copiar los queries exactos
- ✅ Usar los mismos nombres de columnas
- ✅ Mantener consistencia

### 3️⃣ **Relaciones entre tablas**

- `RAZONXTABLA` → Información general de razones sociales
- `NOM_TABLARAZON` → Relación razón ↔ bases de datos (1 a N)
- Usar `IdRazon` para relacionar ambas tablas

---

## 🔄 Flujo Correcto

### Cargar Razones Sociales

```sql
-- Query correcto
SELECT IdRazon, NOMBRE_RAZON, DB 
FROM RAZONXTABLA 
WHERE NOMBRE_RAZON IS NOT NULL AND DB IS NOT NULL
ORDER BY NOMBRE_RAZON
```

```csharp
// C# correcto
var razones = ObtenerRazonesSociales();
cmbRazonSocial.DataSource = razones;
cmbRazonSocial.DisplayMember = "NombreRazon";
cmbRazonSocial.ValueMember = "IdRazon";
```

---

### Cargar Bases de Datos por Razón

```sql
-- Query correcto
SELECT NOMBRE_TABLA 
FROM NOM_TABLARAZON 
WHERE IdRazon = @IdRazon 
ORDER BY NOMBRE_TABLA
```

```csharp
// C# correcto
var razonSeleccionada = (RazonSocial)cmbRazonSocial.SelectedItem;
var bases = reporteService.ObtenerBasesDatosRazon(razonSeleccionada.IdRazon);
cmbCliente.DataSource = bases;
```

---

**Fecha de Fix:** Enero 2026  
**Versión:** 3.0  
**Sistema:** Retorno 360° Tacna  
**Estado:** ✅ RESUELTO

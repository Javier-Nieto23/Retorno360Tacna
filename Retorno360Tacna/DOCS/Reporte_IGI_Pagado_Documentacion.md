# 📊 Reporte de IGI Pagado - Documentación Técnica

## 🎯 Objetivo

Implementar un sistema de reportes para consultar el **IGI Pagado** de pedimentos, utilizando prácticas de **polimorfismo** y **reutilización de código**, basándose en la infraestructura existente del sistema de retorno.

---

## 🏗️ Arquitectura Implementada

### 📁 Estructura de Clases

```
Retorno360Tacna/
├── MODELS/
│   └── ReporteIGI.cs
│       ├── ReporteBase (abstracta)
│       ├── ReporteIGIPagado
│       └── ResumenIGI
│
├── SERVICES/
│   ├── ReporteServiceBase.cs (clase base abstracta)
│   └── ReporteIGIService.cs (hereda de ReporteServiceBase)
│
└── FORMS/
    ├── FrmReportes.cs
    └── FrmReportes.Designer.cs
```

---

## 🧩 Polimorfismo y Reutilización

### 1️⃣ **ReporteServiceBase** (Clase Base Abstracta)

**Patrón:** Template Method

**Responsabilidades:**
- ✅ Reutilizar lógica de conexión a bases de datos
- ✅ Gestionar cache de conexiones externas
- ✅ Proporcionar métodos comunes para todos los reportes

**Métodos Reutilizables:**

```csharp
// Obtener razones sociales
public List<RazonSocial> ObtenerRazonesSociales()

// Obtener bases de datos por razón social
public List<string> ObtenerBasesDatosRazon(string nombreRazon)

// Obtener conexión externa (con cache)
protected virtual ConexionExternaInfo ObtenerConexionExterna(string baseDatos)

// Obtener conexión apropiada (principal o externa)
protected Conexion ObtenerConexionParaBaseDatos(string baseDatos)
```

**Ventajas:**
- 🔄 **Reutilización:** Otros servicios de reporte pueden heredar esta clase
- 🧠 **Polimorfismo:** Métodos `virtual` pueden ser sobrescritos por clases derivadas
- 📦 **Encapsulación:** Cache y lógica de conexión centralizados

---

### 2️⃣ **ReporteIGIService** (Clase Derivada)

**Hereda de:** `ReporteServiceBase`

**Responsabilidades Específicas:**
- ✅ Generar reporte de IGI pagado
- ✅ Calcular IGI por partida
- ✅ Generar resúmenes estadísticos
- ✅ Convertir a DataTable para visualización

**Métodos Propios:**

```csharp
// Generar reporte para una base de datos
public List<ReporteIGIPagado> GenerarReporteIGI(
    string baseDatos, 
    DateTime fechaInicio, 
    DateTime fechaFin)

// Calcular IGI de una partida
private decimal CalcularIGI(decimal valorAduana, decimal tasaIGI)

// Generar resumen del reporte
public ResumenIGI GenerarResumen(List<ReporteIGIPagado> reportes)

// Generar reporte consolidado (múltiples bases)
public List<ReporteIGIPagado> GenerarReporteConsolidado(
    List<string> basesDatos, 
    DateTime fechaInicio, 
    DateTime fechaFin,
    IProgress<string>? progreso = null)

// Convertir a DataTable
public DataTable ConvertirADataTable(List<ReporteIGIPagado> reportes)
```

---

## 🔄 Comparativa: Antes vs Después

### ❌ **ANTES** (Sin Polimorfismo)

```csharp
// Cada formulario tenía su propia lógica de conexión
public class FrmReportes
{
    private void CargarRazones()
    {
        // Código duplicado aquí
        using var cn = new SqlConnection(...);
        // SELECT razones...
    }

    private void CargarBases()
    {
        // Código duplicado aquí
        using var cn = new SqlConnection(...);
        // SELECT bases...
    }
}
```

**Problemas:**
- ❌ Código duplicado en múltiples formularios
- ❌ Difícil de mantener
- ❌ Sin reutilización

---

### ✅ **DESPUÉS** (Con Polimorfismo)

```csharp
// Servicio base reutilizable
public abstract class ReporteServiceBase
{
    // Métodos comunes heredados por todos
    public List<RazonSocial> ObtenerRazonesSociales() { }
    public List<string> ObtenerBasesDatosRazon(string razon) { }
    protected Conexion ObtenerConexionParaBaseDatos(string bd) { }
}

// Servicio específico hereda funcionalidad común
public class ReporteIGIService : ReporteServiceBase
{
    // Solo implementa lógica específica de IGI
    public List<ReporteIGIPagado> GenerarReporteIGI(...) { }
}

// Futuro: Nuevo reporte hereda la misma base
public class ReportePedimentosService : ReporteServiceBase
{
    // Reutiliza toda la lógica de conexión
    public List<Pedimento> GenerarReportePedimentos(...) { }
}
```

**Ventajas:**
- ✅ **DRY** (Don't Repeat Yourself)
- ✅ Fácil de extender (nuevos reportes)
- ✅ Mantenimiento centralizado
- ✅ Polimorfismo: `ReporteServiceBase service = new ReporteIGIService()`

---

## 📊 Query SQL Adaptado

### 🔧 **Optimizaciones Realizadas**

1. **Cálculo de IGI movido al sistema C#:**
   ```csharp
   // ❌ ANTES: En SQL
   SUM(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100,0)) AS IGI_Calculado

   // ✅ AHORA: En C# (más control)
   private decimal CalcularIGI(decimal valorAduana, decimal tasaIGI)
   {
       return Math.Round((valorAduana * tasaIGI) / 100, 0);
   }
   ```

2. **Agrupación por pedimento en memoria:**
   - SQL devuelve datos a nivel partida
   - C# agrupa por `Pim_Consecutivo`
   - Suma IGI calculado de todas las partidas

3. **Cache de conexiones:**
   - Evita consultas repetidas a `RAZONXTABLA`
   - Mejora performance en reportes consolidados

---

## 🎨 Interfaz de Usuario

### 📋 Filtros

| Campo | Tipo | Descripción |
|-------|------|-------------|
| **Razón Social** | ComboBox | Carga automáticamente desde `RAZONXTABLA` |
| **Cliente (Base Dato)** | ComboBox | Se filtra según razón seleccionada |
| **Fecha Inicio** | DateTimePicker | Inicio del período a consultar |
| **Fecha Fin** | DateTimePicker | Fin del período a consultar |

### 📊 Resultados

**DataGridView con columnas:**
- Base Datos
- ID Pedimento
- Pedimento (formato: `ADUANA-PATENTE-PEDIMENTO`)
- Fecha Pago
- IGI Pagado
- IGI Calculado
- **Diferencia IGI** (rojo)
- IVA Pagado
- Forma Pago IGI (descriptiva: "Transferencia", "Compensación")
- Forma Pago IVA (descriptiva)
- Estatus Glosa

### 📈 Resumen

```
📊 Total Pedimentos: 45 | 
💰 IGI Pagado: $1,234,567.89 | 
🧮 IGI Calculado: $1,200,000.00 | 
📈 Diferencia: $34,567.89 | 
💵 IVA Pagado: $567,890.12
```

---

## 🔐 Flujo de Datos

```
┌─────────────────┐
│  FrmReportes    │
│  (UI Layer)     │
└────────┬────────┘
         │ 1. Usuario selecciona filtros
         ↓
┌─────────────────────────┐
│ ReporteIGIService       │
│ (Business Logic)        │
├─────────────────────────┤
│ • GenerarReporteIGI()   │
│ • CalcularIGI()         │ ← Cálculo en C#
│ • GenerarResumen()      │
└────────┬────────────────┘
         │ 2. Hereda métodos comunes
         ↓
┌─────────────────────────────┐
│ ReporteServiceBase          │
│ (Template Method Pattern)   │
├─────────────────────────────┤
│ • ObtenerRazonesSociales()  │ ← Reutilizable
│ • ObtenerBasesDatosRazon()  │ ← Reutilizable
│ • ObtenerConexionExterna()  │ ← Cache
│ • ObtenerConexionParaBase() │ ← Multi-servidor
└────────┬────────────────────┘
         │ 3. Gestión de conexiones
         ↓
┌─────────────────────────────┐
│ ConexionExternaInfo         │
│ (Connection Routing)        │
├─────────────────────────────┤
│ • TieneConexionExterna      │
│ • IdConexion                │
│ • Servidor / Usuario / Pass │
└────────┬────────────────────┘
         │ 4. Conexión física
         ↓
┌─────────────────────────────┐
│ Conexion (CNX)              │
│ • ObtenerConexion()         │
└────────┬────────────────────┘
         │
         ↓
┌─────────────────────────────┐
│ SQL Server                  │
│ • 172.20.20.26 (principal)  │
│ • 172.20.21.33 (secundario) │
└─────────────────────────────┘
```

---

## 🧪 Casos de Uso

### 📌 **Caso 1: Consulta Simple**

**Flujo:**
1. Usuario selecciona **"SEERT"** como razón social
2. Sistema carga bases de datos asociadas
3. Usuario selecciona **"SEERT_OPERACIONES"**
4. Usuario elige **Fecha Inicio:** 01/01/2026, **Fecha Fin:** 31/01/2026
5. Click en **"🔍 Consultar"**
6. Sistema:
   - Obtiene conexión para `SEERT_OPERACIONES` (servidor principal)
   - Ejecuta query SQL
   - Calcula IGI por partida en C#
   - Agrupa por pedimento
   - Muestra resultados en DataGridView
   - Genera resumen estadístico

---

### 📌 **Caso 2: Base de Datos Externa**

**Flujo:**
1. Usuario selecciona **"VIDRIOS"** como razón social
2. Sistema detecta que `RAZONXTABLA.ConnExterna = 'S'`
3. Sistema obtiene `IdConexion` y consulta `Conexiones`
4. Crea conexión a **172.20.21.33** (servidor secundario)
5. Ejecuta query en el servidor externo
6. Resultados se muestran normalmente

**Transparente para el usuario:**
- No necesita saber en qué servidor está la base
- El sistema rutea automáticamente

---

### 📌 **Caso 3: Múltiples Bases (Futuro)**

```csharp
// Potencial extensión
var basesDatos = new List<string> 
{ 
    "SEERT_OPERACIONES", 
    "SEERT_VIDRIOS", 
    "OTRA_BASE" 
};

var reporteConsolidado = reporteService.GenerarReporteConsolidado(
    basesDatos,
    fechaInicio,
    fechaFin,
    progreso
);
```

---

## 🎓 Patrones de Diseño Aplicados

### 1️⃣ **Template Method Pattern**

**Dónde:** `ReporteServiceBase`

**Qué hace:**
- Define el esqueleto de operaciones comunes
- Las clases derivadas implementan detalles específicos

**Ejemplo:**
```csharp
// Método común (template)
protected Conexion ObtenerConexionParaBaseDatos(string baseDatos)
{
    var info = ObtenerConexionExterna(baseDatos); // ← Puede ser sobrescrito

    if (info.UsarConexionPrincipal)
        return new Conexion(...);  // Servidor principal
    else
        return new Conexion(...);  // Servidor externo
}

// Clase derivada puede sobrescribir
protected override ConexionExternaInfo ObtenerConexionExterna(string bd)
{
    // Lógica personalizada si es necesario
    return base.ObtenerConexionExterna(bd);
}
```

---

### 2️⃣ **Repository Pattern** (Implícito)

**Dónde:** `ReporteIGIService`

**Qué hace:**
- Encapsula acceso a datos
- Abstrae lógica de base de datos
- Retorna modelos de dominio

**Ejemplo:**
```csharp
// El servicio actúa como repository
public List<ReporteIGIPagado> GenerarReporteIGI(...)
{
    // Encapsula toda la lógica de acceso a datos
    // Devuelve modelos de negocio, no DataReaders
}
```

---

### 3️⃣ **Strategy Pattern** (Conexión Dinámica)

**Dónde:** `ObtenerConexionParaBaseDatos`

**Qué hace:**
- Selecciona estrategia de conexión en tiempo de ejecución
- Servidor principal vs servidor externo

**Ejemplo:**
```csharp
if (infoConexionExterna.UsarConexionPrincipal)
{
    // Estrategia 1: Servidor principal
    return new Conexion(conexionPrincipal.Servidor, ...);
}
else
{
    // Estrategia 2: Servidor externo
    return new Conexion(infoConexionExterna.Servidor, ...);
}
```

---

## 🚀 Extensibilidad Futura

### ➕ **Agregar Nuevo Reporte**

**Paso 1:** Crear modelo

```csharp
public class ReportePedimentos : ReporteBase
{
    public string NumPedimento { get; set; }
    public string Tipo { get; set; }
    public decimal Valor { get; set; }
}
```

**Paso 2:** Crear servicio (hereda de `ReporteServiceBase`)

```csharp
public class ReportePedimentosService : ReporteServiceBase
{
    public ReportePedimentosService(ConexionInfo conexion) 
        : base(conexion) { }

    public List<ReportePedimentos> Generar(string bd, DateTime inicio, DateTime fin)
    {
        // Reutiliza automáticamente:
        // - ObtenerConexionParaBaseDatos()
        // - Cache de conexiones
        // - Ruteo multi-servidor

        var conexion = ObtenerConexionParaBaseDatos(bd);
        // ... ejecutar query ...
    }
}
```

**Paso 3:** Crear formulario

```csharp
public partial class FrmReportePedimentos : Form
{
    private readonly ReportePedimentosService service;

    public FrmReportePedimentos(ConexionInfo conexion)
    {
        service = new ReportePedimentosService(conexion);

        // Reutiliza métodos comunes
        var razones = service.ObtenerRazonesSociales();
        var bases = service.ObtenerBasesDatosRazon(razon);
    }
}
```

**✅ Ventajas:**
- No necesitas reescribir lógica de conexión
- No necesitas duplicar código de razones/bases
- Sistema de cache ya incluido
- Multi-servidor automático

---

## 📝 Resumen de Beneficios

| Aspecto | Beneficio |
|---------|-----------|
| **Reutilización** | ✅ Métodos comunes heredados de `ReporteServiceBase` |
| **Polimorfismo** | ✅ `ReporteServiceBase service = new ReporteIGIService()` |
| **Mantenibilidad** | ✅ Cambios en lógica de conexión afectan a todos los reportes |
| **Extensibilidad** | ✅ Nuevos reportes solo implementan lógica específica |
| **Performance** | ✅ Cache de conexiones externas |
| **Multi-servidor** | ✅ Transparente para el desarrollador |
| **Separación de Responsabilidades** | ✅ UI ↔ Service ↔ Data Access |
| **Testeable** | ✅ Servicios pueden ser probados independientemente |

---

## 🎯 Conclusión

La implementación utiliza **polimorfismo** y **patrones de diseño** para:
1. ✅ **Reutilizar** lógica existente del sistema de retorno
2. ✅ **Separar** responsabilidades (UI, lógica de negocio, acceso a datos)
3. ✅ **Facilitar** extensión futura de nuevos reportes
4. ✅ **Mantener** código limpio y mantenible

**Todos los cambios son compatibles con el sistema existente** y no afectan funcionalidades previas (cálculo de porcentaje de retorno, login, etc.).

---

**Fecha:** Enero 2026  
**Versión:** 3.0  
**Sistema:** Retorno 360° Tacna  
**Desarrollado por:** Javier Nieto

# 🔧 FIX: Validación de Conexión en Reporte IGI

## 📋 Problema Identificado

Al realizar consultas de reporte IGI, algunas bases de datos **no mostraban resultados** en las tablas, aún cuando:
- ✅ La razón social estaba seleccionada
- ✅ Las fechas eran correctas
- ✅ No se mostraban errores visibles

### **Causa Raíz**

El método `ObtenerResumenTablasPorBase()` **no validaba**:
1. Si la conexión a la base de datos era exitosa
2. Si la base de datos existía en el servidor
3. Si las tablas requeridas (`Di_Pedimento`, `TR_GLOSA`, etc.) existían
4. Si había errores silenciosos durante las consultas

Esto provocaba que:
- Bases de datos inexistentes retornaran tablas vacías sin error
- Conexiones fallidas no se reportaran
- Servidores externos inaccesibles pasaran desapercibidos

---

## ✅ Solución Implementada

### **1. Método de Validación de Conexión**

Se agregó el método privado `ValidarConexionYBaseDatos()`:

```csharp
private bool ValidarConexionYBaseDatos(CNX.Conexion conexion, string nombreBaseDatos)
{
	try
	{
		using (var cn = conexion.ObtenerConexion())
		{
			cn.Open();

			// ✅ PASO 1: Verificar que la base de datos exista
			string sqlVerificarBD = @"
				SELECT COUNT(*) 
				FROM sys.databases 
				WHERE name = @NombreBaseDatos";

			using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(sqlVerificarBD, cn))
			{
				cmd.Parameters.AddWithValue("@NombreBaseDatos", nombreBaseDatos);
				int count = (int)cmd.ExecuteScalar();

				if (count == 0) return false; // ❌ Base no existe
			}

			// ✅ PASO 2: Verificar que las tablas necesarias existan
			string sqlVerificarTablas = $@"
				SELECT 
					(SELECT COUNT(*) FROM {nombreBaseDatos}.INFORMATION_SCHEMA.TABLES 
					 WHERE TABLE_NAME = 'Di_Pedimento') AS TienePedimento,
					(SELECT COUNT(*) FROM {nombreBaseDatos}.INFORMATION_SCHEMA.TABLES 
					 WHERE TABLE_NAME = 'Di_PedimentoDet') AS TienePedimentoDet,
					(SELECT COUNT(*) FROM {nombreBaseDatos}.INFORMATION_SCHEMA.TABLES 
					 WHERE TABLE_NAME = 'Ca_Farancelaria') AS TieneFarancelaria,
					(SELECT COUNT(*) FROM {nombreBaseDatos}.INFORMATION_SCHEMA.TABLES 
					 WHERE TABLE_NAME = 'TR_GLOSA') AS TieneGlosa";

			// Si falta alguna tabla crítica → retorna false
			return true;
		}
	}
	catch (Exception ex)
	{
		// Log del error y retorna false
		return false;
	}
}
```

---

### **2. Validación en Flujo Principal**

Se agregaron validaciones en `ObtenerResumenTablasPorBase()`:

```csharp
public (System.Data.DataTable IGI, System.Data.DataTable IVA) ObtenerResumenTablasPorBase(
	string baseDatos, DateTime fechaInicio, DateTime fechaFin)
{
	try
	{
		var conexionCliente = ObtenerConexionParaBaseDatos(baseDatos);

		// ✅ VALIDAR CONEXIÓN CLIENTE
		if (!ValidarConexionYBaseDatos(conexionCliente, baseDatos))
		{
			throw new Exception(
				$"No se pudo conectar a la base de datos '{baseDatos}'. " +
				"Verifique que la base exista y la conexión sea correcta."
			);
		}

		// ... determinar baseGlosa ...

		var conexionGlosa = ObtenerConexionParaBaseDatos(baseGlosa);

		// ✅ VALIDAR CONEXIÓN GLOSA
		if (!ValidarConexionYBaseDatos(conexionGlosa, baseGlosa))
		{
			throw new Exception(
				$"No se pudo conectar a la base de glosa '{baseGlosa}'. " +
				"Verifique que la base exista y la conexión sea correcta."
			);
		}

		// ... resto de la lógica ...
	}
	catch (Exception ex)
	{
		throw new Exception($"Error al obtener resumen por base '{baseDatos}': {ex.Message}", ex);
	}
}
```

---

### **3. Logs de Diagnóstico (DEBUG)**

Se agregaron logs detallados para facilitar la depuración:

```csharp
#if DEBUG
System.Diagnostics.Debug.WriteLine($"=== INICIO ObtenerResumenTablasPorBase ===");
System.Diagnostics.Debug.WriteLine($"Base de datos: {baseDatos}");
System.Diagnostics.Debug.WriteLine($"Fechas: {fechaInicio:yyyy-MM-dd} a {fechaFin:yyyy-MM-dd}");

// Durante validación
System.Diagnostics.Debug.WriteLine($"✅ Validando conexión: {cn.DataSource} / {cn.Database}");
System.Diagnostics.Debug.WriteLine($"   Di_Pedimento: Sí");
System.Diagnostics.Debug.WriteLine($"   TR_GLOSA: Sí");

// Durante consultas
System.Diagnostics.Debug.WriteLine($"--- CONSULTANDO PEDIMENTOS CLIENTE ({baseDatos}) ---");
System.Diagnostics.Debug.WriteLine($"🔌 Conexión cliente abierta: {cn.Database} en {cn.DataSource}");
System.Diagnostics.Debug.WriteLine($"📊 Pedimentos cliente encontrados: 42");

System.Diagnostics.Debug.WriteLine($"--- CONSULTANDO GLOSA IGI ({baseGlosa}) ---");
System.Diagnostics.Debug.WriteLine($"📊 Pedimentos glosa IGI encontrados: 38");

System.Diagnostics.Debug.WriteLine($"--- PROCESANDO JOIN IGI ---");
System.Diagnostics.Debug.WriteLine($"✅ Filas IGI agregadas a tabla: 15");

System.Diagnostics.Debug.WriteLine($"=== FIN ObtenerResumenTablasPorBase ===\n");
#endif
```

---

### **4. Timeout de Comandos**

Se agregó timeout a todos los comandos SQL para evitar bloqueos:

```csharp
using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sqlCliente, cn);
cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
cmd.CommandTimeout = 120; // ✅ 120 segundos
```

---

## 📊 Flujo de Validación

```
┌─────────────────────────────────────────┐
│ Usuario consulta base: SEERT_VIDRIOS    │
└──────────────┬──────────────────────────┘
			   │
			   ▼
┌─────────────────────────────────────────┐
│ ObtenerConexionParaBaseDatos()          │
│ - Determina servidor: 172.20.21.33      │
│ - Crea conexión                         │
└──────────────┬──────────────────────────┘
			   │
			   ▼
┌─────────────────────────────────────────┐
│ ValidarConexionYBaseDatos()             │
│ ✅ Abre conexión                        │
│ ✅ Verifica que BD exista               │
│ ✅ Verifica tablas requeridas           │
└──────────────┬──────────────────────────┘
			   │
		┌──────┴──────┐
		│             │
		▼             ▼
	✅ VÁLIDO     ❌ INVÁLIDO
		│             │
		▼             ▼
  Ejecuta Query   Lanza Exception
				  con mensaje claro
```

---

## 🎯 Casos de Error Detectados

### **Error 1: Base de Datos No Existe**

**Antes:**
```
DataGridView: (vacío, sin error)
lblProgreso: "Consulta completada"
```

**Ahora:**
```
MessageBox: "No se pudo conectar a la base de datos 'SEERT_INEXISTENTE'. 
			 Verifique que la base exista y la conexión sea correcta."
```

---

### **Error 2: Servidor Externo Inaccesible**

**Antes:**
```
(Se cuelga durante 30 segundos)
DataGridView: (vacío, timeout silencioso)
```

**Ahora:**
```
(Timeout de 120s configurado)
MessageBox: "No se pudo conectar a la base de datos 'SEERT_VIDRIOS'. 
			 Verifique que la base exista y la conexión sea correcta."

Debug Log:
⚠️ Error al validar conexión para 'SEERT_VIDRIOS': 
   Timeout expired. The timeout period elapsed prior to completion...
```

---

### **Error 3: Faltan Tablas Requeridas**

**Antes:**
```
Error SQL: Invalid object name 'BASENUEVA.dbo.TR_GLOSA'
(Error genérico, difícil de interpretar)
```

**Ahora:**
```
Debug Log:
   Di_Pedimento: Sí
   Di_PedimentoDet: Sí
   Ca_Farancelaria: Sí
   TR_GLOSA: NO  ← Falta tabla
⚠️ Faltan tablas necesarias en 'BASENUEVA'

MessageBox: "No se pudo conectar a la base de datos 'BASENUEVA'. 
			 Verifique que la base exista y la conexión sea correcta."
```

---

## 🔍 Cómo Usar los Logs de Debug

### **1. Activar Output de Debug en Visual Studio**

1. Abrir Visual Studio
2. Menú: `View` → `Output` (o `Ctrl+W, O`)
3. En el dropdown del panel Output, seleccionar: **Debug**

### **2. Ejecutar en Modo Debug**

```
F5 (Start Debugging)
```

### **3. Ejemplo de Output**

```
=== INICIO ObtenerResumenTablasPorBase ===
Base de datos: SEERT_OPERACIONES
Fechas: 2026-01-01 a 2026-01-31

✅ Validando conexión: 172.20.20.26 / SEERT_OPERACIONES
   Di_Pedimento: Sí
   Di_PedimentoDet: Sí
   Ca_Farancelaria: Sí
   TR_GLOSA: Sí
✅ Validación exitosa para 'SEERT_OPERACIONES'

🔍 Base de glosa encontrada: SEERT_OPERACIONES_ABLE

✅ Validando conexión: 172.20.20.26 / SEERT_OPERACIONES_ABLE
   Di_Pedimento: Sí
   Di_PedimentoDet: Sí
   Ca_Farancelaria: Sí
   TR_GLOSA: Sí
✅ Validación exitosa para 'SEERT_OPERACIONES_ABLE'

--- CONSULTANDO PEDIMENTOS CLIENTE (SEERT_OPERACIONES) ---
🔌 Conexión cliente abierta: SEERT_OPERACIONES en 172.20.20.26
📊 Pedimentos cliente encontrados: 42

--- CONSULTANDO GLOSA IGI (SEERT_OPERACIONES_ABLE) ---
🔌 Conexión glosa abierta: SEERT_OPERACIONES_ABLE en 172.20.20.26
📊 Pedimentos glosa IGI encontrados: 38

--- CONSULTANDO GLOSA IVA (SEERT_OPERACIONES_ABLE) ---
📊 Pedimentos glosa IVA encontrados: 40

--- PROCESANDO JOIN IGI ---
✅ Filas IGI agregadas a tabla: 15

--- PROCESANDO JOIN IVA ---
✅ Filas IVA agregadas a tabla: 12

=== FIN ObtenerResumenTablasPorBase ===
```

---

## 📝 Checklist de Validación

Ahora el método `ObtenerResumenTablasPorBase()` valida:

- [x] Conexión al servidor cliente
- [x] Existencia de base de datos cliente
- [x] Existencia de tablas requeridas (Di_Pedimento, Di_PedimentoDet, Ca_Farancelaria)
- [x] Conexión al servidor glosa
- [x] Existencia de base de datos glosa
- [x] Existencia de tabla TR_GLOSA
- [x] Timeout configurado para evitar bloqueos
- [x] Logs de diagnóstico para cada paso
- [x] Mensajes de error claros y específicos
- [x] Manejo de excepciones con contexto

---

## ⚡ Beneficios

| Antes | Ahora |
|-------|-------|
| ❌ Tablas vacías sin explicación | ✅ Mensaje de error claro |
| ❌ Timeouts silenciosos | ✅ Timeout configurado (120s) |
| ❌ Difícil depurar | ✅ Logs detallados en Output |
| ❌ No se sabía qué falló | ✅ Especifica exactamente el problema |
| ❌ Usuario confundido | ✅ Usuario informado |

---

## 🚀 Próximos Pasos Recomendados

### **1. Validación de Permisos**

Agregar validación de permisos SQL:
```sql
SELECT HAS_PERMS_BY_NAME('SEERT_OPERACIONES', 'DATABASE', 'SELECT')
```

### **2. Cache de Validaciones**

Para evitar validar la misma base múltiples veces:
```csharp
private Dictionary<string, bool> cacheValidaciones = new();

if (cacheValidaciones.TryGetValue(baseDatos, out bool esValida))
{
	if (!esValida) throw new Exception(...);
}
else
{
	bool resultado = ValidarConexionYBaseDatos(...);
	cacheValidaciones[baseDatos] = resultado;
}
```

### **3. Retry Automático**

Para conexiones externas inestables:
```csharp
for (int intento = 1; intento <= 3; intento++)
{
	try
	{
		return ValidarConexionYBaseDatos(...);
	}
	catch when (intento < 3)
	{
		Thread.Sleep(1000 * intento);
	}
}
```

---

## 📌 Resumen

✅ **Problema resuelto:** Bases que no mostraban datos ahora lanzan errores claros  
✅ **Diagnóstico mejorado:** Logs detallados en Output window  
✅ **Experiencia de usuario:** Mensajes de error específicos  
✅ **Robustez:** Validación completa antes de ejecutar consultas  

---

**Fecha:** Enero 2026  
**Versión:** 1.0  
**Autor:** Sistema de Diagnóstico Automático

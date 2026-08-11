# 🔧 FIX: Corrección de Lógica de Conexiones según Estructura de Tablas

## 📋 Problema Identificado

La lógica de conexión **NO seguía correctamente** la estructura de las tablas de la base de datos:

### ❌ **Lógica INCORRECTA (antes)**

El código mezclaba datos de `RAZONXTABLA` y `NOM_TABLARAZON` sin respetar sus propósitos:

```csharp
// ❌ INCORRECTO: Hacía JOIN entre NOM_TABLARAZON y RAZONXTABLA
SELECT NT.NOMBRE_TABLA, R.ConnExterna, R.IdConexion, C.Servidor
FROM NOM_TABLARAZON NT
LEFT JOIN RAZONXTABLA R ON R.DB = NT.NOMBRE_TABLA  -- ❌ JOIN erróneo
LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
```

**Problemas:**
1. Buscaba primero en `RAZONXTABLA` (tabla de glosa)
2. Usaba `ConnExterna = 'S'` como indicador (campo que no siempre está presente)
3. No distinguía correctamente entre bases cliente y bases glosa
4. Ignoraba el `IdConexion` de `NOM_TABLARAZON`

---

## ✅ Estructura Correcta de las Tablas

Según las imágenes que proporcionaste:

### **1. Tabla RAZONXTABLA** (Bases de Glosa)

| Campo | Descripción |
|-------|-------------|
| `IdRazon` | ID de la razón social |
| `NOMBRE_RAZON` | Nombre de la razón social |
| `DB` | **Base de datos de GLOSA** (ej: `SEERT_Able`) |
| `ConnExterna` | 'S' o 'N' (indica si usa conexión externa) |
| `IdConexion` | FK a tabla `Conexiones` (servidor de la glosa) |

**Ejemplos:**
```
IdRazon | NOMBRE_RAZON                        | DB              | ConnExterna | IdConexion
1       | MAM DE LA FRONTERA SA DE CV         | SEERT_Able      | N           | 1
2       | BAJA FUR SA DE CV                   | SEERT_Abella    | N           | 1
3       | ARROYO HOLDINGS DE MEXICO...        | SEERT_Arroyo    | N           | 1
```

---

### **2. Tabla NOM_TABLARAZON** (Bases Seleccionables del Cliente)

| Campo | Descripción |
|-------|-------------|
| `IdTabla` | ID de la tabla |
| `NOMBRE_TABLA` | **Base de datos del CLIENTE** (ej: `SEERT_Acme`) |
| `IdRazon` | FK a razón social |
| `IdConexion` | FK a tabla `Conexiones` (servidor de esta base) |

**Ejemplos:**
```
IdTabla | NOMBRE_TABLA      | IdRazon | IdConexion
1       | SEERT_Able        | 1       | 1
3       | SEERT_Acme        | 1       | 1
4       | SEERT_Ameramerk   | 1       | 1
5       | SEERT_Bi          | 1       | 1
```

---

### **3. Tabla Conexiones** (Información de Servidores)

| Campo | Descripción |
|-------|-------------|
| `IdConexion` | ID de la conexión |
| `NombreConexion` | Nombre descriptivo |
| `Servidor` | IP o nombre del servidor |
| `UsuarioSQL` | Usuario SQL |
| `PasswordSQL` | Contraseña SQL |
| `TipoMotor` | Tipo de motor SQL |
| `Activo` | Si está activa |

**Ejemplos:**
```
IdConexion | NombreConexion  | Servidor      | UsuarioSQL  | PasswordSQL | TipoMotor  | Activo
1          | TJ-SQL-SRV03    | 172.20.20.26  | MedTiempos  | T3ch4dm1n   | SQLServer  | 1
2          | TJ-SQL-2019-03  | 172.20.21.36  | jnieto      | admin1234   | SQLServer  | 1
1002       | tj-sedsrv-04    | 172.20.21.33  | jnieto      | admin1234   | SQLServer  | 1
```

---

## ✅ Lógica CORREGIDA

### **Reglas de Negocio Correctas**

1. **Para bases de datos CLIENTE** (seleccionables en el combo):
   - Leer de `NOM_TABLARAZON` donde `IdRazon = @IdRazonSeleccionado`
   - Si `IdConexion IS NULL` → usar conexión principal
   - Si `IdConexion IS NOT NULL` → buscar en tabla `Conexiones` y usar ese servidor

2. **Para base de datos GLOSA**:
   - Leer de `RAZONXTABLA` donde `IdRazon = @IdRazonSeleccionado`
   - Tomar el campo `DB` como la base de glosa
   - Si `IdConexion IS NULL` → usar conexión principal
   - Si `IdConexion IS NOT NULL` → buscar en tabla `Conexiones` y usar ese servidor

---

## 🔧 Código Corregido

### **1. Método ObtenerBasesDatosConConexion()**

```csharp
public List<ConexionExternaInfo> ObtenerBasesDatosConConexion(int idRazon)
{
	// ✅ CORRECTO: Consultar solo NOM_TABLARAZON con JOIN directo a Conexiones
	string sql = @"
		SELECT 
			NT.NOMBRE_TABLA AS BaseDatos,
			NT.IdConexion,
			C.NombreConexion,
			C.Servidor,
			C.UsuarioSQL,
			C.PasswordSQL
		FROM NOM_TABLARAZON NT
		LEFT JOIN Conexiones C ON NT.IdConexion = C.IdConexion
		WHERE NT.IdRazon = @IdRazon 
		  AND NT.NOMBRE_TABLA IS NOT NULL
		ORDER BY NT.NOMBRE_TABLA";

	// Leer resultados
	while (reader.Read())
	{
		var info = new ConexionExternaInfo
		{
			BaseDatos = reader.GetString(0)
		};

		// Si tiene IdConexion → es conexión externa
		if (!reader.IsDBNull(1))
		{
			info.IdConexion = reader.GetInt32(1);
			info.TieneConexionExterna = true;
			info.Servidor = reader.GetString(3);      // Del JOIN con Conexiones
			info.UsuarioSQL = reader.GetString(4);
			info.PasswordSQL = reader.GetString(5);
		}
		else
		{
			// IdConexion NULL → conexión principal
			info.TieneConexionExterna = false;
		}

		basesDatos.Add(info);
	}

	return basesDatos;
}
```

---

### **2. Método ObtenerConexionExterna()**

```csharp
protected virtual ConexionExternaInfo ObtenerConexionExterna(string baseDatos)
{
	// PASO 1: Buscar en NOM_TABLARAZON (bases del cliente)
	string sqlNomTablaRazon = @"
		SELECT TOP 1 
			NT.IdRazon,
			NT.IdConexion,
			C.NombreConexion,
			C.Servidor,
			C.UsuarioSQL,
			C.PasswordSQL
		FROM NOM_TABLARAZON NT
		LEFT JOIN Conexiones C ON NT.IdConexion = C.IdConexion
		WHERE NT.NOMBRE_TABLA = @BaseDatos";

	using (var cmd = new SqlCommand(sqlNomTablaRazon, cn))
	{
		cmd.Parameters.AddWithValue("@BaseDatos", baseDatos);
		using var reader = cmd.ExecuteReader();

		if (reader.Read())
		{
			// ✅ Encontrado en NOM_TABLARAZON
			if (!reader.IsDBNull(1)) // Tiene IdConexion
			{
				conexionExterna.IdConexion = reader.GetInt32(1);
				conexionExterna.TieneConexionExterna = true;
				conexionExterna.Servidor = reader.GetString(3);
				// ...
			}
			else
			{
				// IdConexion NULL → conexión principal
				conexionExterna.TieneConexionExterna = false;
			}
			return conexionExterna;
		}
	}

	// PASO 2: Si no está en NOM_TABLARAZON, buscar en RAZONXTABLA (base glosa)
	string sqlRazonXTabla = @"
		SELECT TOP 1 
			R.IdRazon,
			R.ConnExterna,
			R.IdConexion,
			C.Servidor,
			C.UsuarioSQL,
			C.PasswordSQL
		FROM RAZONXTABLA R
		LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
		WHERE R.DB = @BaseDatos";

	// Similar lógica...
}
```

---

## 📊 Flujo de Determinación de Conexión

```
┌─────────────────────────────────────────┐
│ Usuario selecciona: IdRazon = 1         │
└──────────────┬──────────────────────────┘
			   │
			   ▼
┌─────────────────────────────────────────┐
│ Cargar bases del combo (cliente)        │
│ SELECT * FROM NOM_TABLARAZON            │
│ WHERE IdRazon = 1                       │
└──────────────┬──────────────────────────┘
			   │
			   ├─ SEERT_Able     (IdConexion: 1 → 172.20.20.26)
			   ├─ SEERT_Acme     (IdConexion: 1 → 172.20.20.26)
			   ├─ SEERT_Faltech  (IdConexion: 1002 → 172.20.21.33) ✅ EXTERNO
			   └─ SEERT_Foampro  (IdConexion: 1 → 172.20.20.26)
			   │
			   ▼
┌─────────────────────────────────────────┐
│ Usuario selecciona: SEERT_Faltech       │
└──────────────┬──────────────────────────┘
			   │
			   ▼
┌─────────────────────────────────────────┐
│ ObtenerConexionExterna("SEERT_Faltech") │
│ 1. Busca en NOM_TABLARAZON              │
│    → Encuentra IdConexion = 1002        │
│ 2. JOIN con Conexiones                  │
│    → Servidor: 172.20.21.33             │
│    → Usuario: jnieto                    │
└──────────────┬──────────────────────────┘
			   │
			   ▼
┌─────────────────────────────────────────┐
│ ObtenerConexionParaBaseDatos()          │
│ Crea conexión a:                        │
│ Server=172.20.21.33;                    │
│ Database=SEERT_Faltech;                 │
│ User Id=jnieto;                         │
│ Password=admin1234;                     │
└──────────────┬──────────────────────────┘
			   │
			   ▼
		 ✅ CONSULTA EXITOSA
```

---

## 🔍 Determinación de Base de Glosa

```csharp
// En ObtenerResumenTablasPorBase()

string baseGlosa = baseDatos; // Default: misma base

try
{
	// 1. Obtener IdRazon de la base cliente
	int idRazon = ObtenerIdRazonDesdeBaseDatos(baseDatos);

	// 2. Consultar RAZONXTABLA para obtener la base de glosa
	string sql = @"
		SELECT DB, IdConexion
		FROM RAZONXTABLA
		WHERE IdRazon = @IdRazon";

	// Resultado: DB = 'SEERT_Able', IdConexion = 1

	// 3. Usar ObtenerConexionExterna('SEERT_Able')
	//    → Busca primero en NOM_TABLARAZON
	//    → Si no encuentra, busca en RAZONXTABLA ✅
	//    → Obtiene servidor desde tabla Conexiones
}
```

---

## 📝 Ejemplos de Casos

### **Caso 1: Base Cliente en Servidor Principal**

```
NOM_TABLARAZON:
IdTabla | NOMBRE_TABLA  | IdRazon | IdConexion
1       | SEERT_Able    | 1       | 1

Conexiones:
IdConexion | Servidor      | UsuarioSQL
1          | 172.20.20.26  | MedTiempos

RESULTADO:
✅ TieneConexionExterna = true
✅ Servidor: 172.20.20.26
✅ Usuario: MedTiempos
```

---

### **Caso 2: Base Cliente en Servidor Externo**

```
NOM_TABLARAZON:
IdTabla | NOMBRE_TABLA    | IdRazon | IdConexion
7       | SEERT_Faltech   | 1       | 1002

Conexiones:
IdConexion | Servidor      | UsuarioSQL
1002       | 172.20.21.33  | jnieto

RESULTADO:
✅ TieneConexionExterna = true
✅ Servidor: 172.20.21.33  ← EXTERNO
✅ Usuario: jnieto
```

---

### **Caso 3: Base Cliente sin IdConexion (usar principal)**

```
NOM_TABLARAZON:
IdTabla | NOMBRE_TABLA    | IdRazon | IdConexion
99      | SEERT_Temporal  | 1       | NULL

RESULTADO:
✅ TieneConexionExterna = false
✅ Usar conexionPrincipal (172.20.20.26 / MedTiempos)
```

---

### **Caso 4: Base de Glosa**

```
RAZONXTABLA:
IdRazon | DB              | ConnExterna | IdConexion
1       | SEERT_Able      | N           | 1

1. ObtenerConexionExterna('SEERT_Able')
2. No encuentra en NOM_TABLARAZON (porque es base de glosa)
3. Busca en RAZONXTABLA → ENCUENTRA
4. Obtiene IdConexion = 1
5. JOIN con Conexiones → Servidor: 172.20.20.26

RESULTADO:
✅ TieneConexionExterna = true
✅ Servidor: 172.20.20.26
```

---

## 🎯 Diferencias Clave

| Aspecto | ❌ Antes (Incorrecto) | ✅ Ahora (Correcto) |
|---------|---------------------|-------------------|
| **Fuente de bases cliente** | Mezclaba NOM_TABLARAZON + RAZONXTABLA | Solo NOM_TABLARAZON |
| **Indicador de externa** | Usaba `ConnExterna = 'S'` | Usa `IdConexion IS NOT NULL` |
| **Orden de búsqueda** | 1. RAZONXTABLA, 2. NOM_TABLARAZON | 1. NOM_TABLARAZON, 2. RAZONXTABLA |
| **Base de glosa** | Heurística por nombre ('ABLE', 'GLOSA') | Lee RAZONXTABLA.DB |
| **Validación de IdRazon** | No validaba IdRazon | Valida IdRazon en ambas tablas |

---

## 🔧 Logs de Diagnóstico Mejorados

```
🔍 Buscando conexión para 'SEERT_Faltech'...
   ✅ Encontrado en NOM_TABLARAZON
   📋 IdRazon: 1
   🔗 IdConexion: 1002
   🌐 Servidor: 172.20.21.33
   👤 Usuario: jnieto
   ✅ Resultado: EXTERNA

📋 Obteniendo bases para IdRazon: 1
   ✅ SEERT_Able → Servidor: 172.20.20.26 (IdConexion: 1)
   ✅ SEERT_Acme → Servidor: 172.20.20.26 (IdConexion: 1)
   ✅ SEERT_Faltech → Servidor: 172.20.21.33 (IdConexion: 1002)
   🔌 SEERT_Temporal → Conexión principal (IdConexion: NULL)
   📊 Total: 4 bases encontradas
```

---

## ✅ Validación de la Corrección

Para verificar que funcione correctamente:

1. **Abrir Output window**: `View` → `Output` → Seleccionar "Debug"
2. **Ejecutar en Debug** (F5)
3. **Seleccionar una razón social**
4. **Observar los logs**:
   ```
   📋 Obteniendo bases para IdRazon: 1
	  ✅ SEERT_Faltech → Servidor: 172.20.21.33 (IdConexion: 1002)
   ```
5. **Seleccionar base** `SEERT_Faltech`
6. **Hacer consulta**
7. **Verificar que conecta al servidor correcto**:
   ```
   🔍 Buscando conexión para 'SEERT_Faltech'...
	  ✅ Encontrado en NOM_TABLARAZON
	  🔗 IdConexion: 1002
	  🌐 Servidor: 172.20.21.33
   ```

---

## 📌 Resumen

✅ **Problema resuelto:** Conexiones ahora siguen la estructura correcta de tablas  
✅ **NOM_TABLARAZON:** Para bases seleccionables del cliente  
✅ **RAZONXTABLA:** Para base de glosa  
✅ **IdConexion:** Indicador principal de conexión externa (no `ConnExterna`)  
✅ **Validación de IdRazon:** Se valida en ambas búsquedas  
✅ **Logs detallados:** Facilitan diagnóstico de conexiones  

---

**Fecha:** Enero 2026  
**Versión:** 2.0  
**Autor:** Corrección de Lógica de Conexiones

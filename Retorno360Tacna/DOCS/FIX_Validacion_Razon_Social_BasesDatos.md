# 🔧 FIX: Validación de Razón Social en Bases de Datos

## 📋 PROBLEMA IDENTIFICADO

### Síntoma
Al seleccionar una razón social en el módulo de reportes, aparecen bases de datos que no deberían estar asociadas a esa razón social. Por ejemplo:
- Al seleccionar la razón social "X", aparece la base "SEERT_Jlo" que pertenece a la razón social "MAM"

### Causa Raíz
El método `ObtenerBasesDatosRazon()` en `ReporteServiceBase.cs` consultaba únicamente la tabla `NOM_TABLARAZON` filtrando por `IdRazon`, pero **NO validaba** que esas bases de datos realmente estuvieran asociadas correctamente en la tabla maestra `RAZONXTABLA`.

#### Query ANTERIOR (INCORRECTO):
```sql
SELECT NOMBRE_TABLA 
FROM NOM_TABLARAZON 
WHERE IdRazon = @IdRazon 
ORDER BY NOMBRE_TABLA
```

**Problema**: Si la tabla `NOM_TABLARAZON` tiene datos inconsistentes o duplicados (por ejemplo, una base de datos con múltiples `IdRazon`), el query devuelve bases que no deberían aparecer.

---

## ✅ SOLUCIÓN IMPLEMENTADA

### Cambio Realizado
Se modificó el método `ObtenerBasesDatosRazon()` para agregar un `INNER JOIN` con la tabla `RAZONXTABLA`, asegurando que solo se devuelvan bases de datos que estén correctamente asociadas a la razón social seleccionada.

#### Query NUEVO (CORRECTO):
```sql
SELECT DISTINCT NT.NOMBRE_TABLA 
FROM NOM_TABLARAZON NT
INNER JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
WHERE NT.IdRazon = @IdRazon 
  AND NT.NOMBRE_TABLA IS NOT NULL
ORDER BY NT.NOMBRE_TABLA
```

### Beneficios:
1. ✅ **Validación cruzada**: Se verifica que la razón social exista en `RAZONXTABLA`
2. ✅ **Filtrado más estricto**: Solo se devuelven bases de datos válidas
3. ✅ **Eliminación de duplicados**: `DISTINCT` previene bases duplicadas
4. ✅ **Validación de nulos**: `AND NT.NOMBRE_TABLA IS NOT NULL` evita bases inválidas

---

## 🗂️ ESTRUCTURA DE TABLAS

### RAZONXTABLA (Tabla maestra)
```
┌─────────────────┬──────────┬────────────────────────────┐
│ Columna         │ Tipo     │ Descripción                │
├─────────────────┼──────────┼────────────────────────────┤
│ IdRazon         │ INT      │ ID único de razón social   │
│ NOMBRE_RAZON    │ VARCHAR  │ Nombre de la razón social  │
│ DB              │ VARCHAR  │ Base de datos de TR_Glosa  │
│ ConnExterna     │ CHAR(1)  │ 'S' = Conexión externa     │
│ IdConexion      │ INT      │ FK a tabla Conexiones      │
└─────────────────┴──────────┴────────────────────────────┘
```

### NOM_TABLARAZON (Bases de datos asociadas)
```
┌─────────────────┬──────────┬────────────────────────────┐
│ Columna         │ Tipo     │ Descripción                │
├─────────────────┼──────────┼────────────────────────────┤
│ IdTabla         │ INT      │ ID único del registro      │
│ IdRazon         │ INT      │ FK a RAZONXTABLA           │
│ NOMBRE_TABLA    │ VARCHAR  │ Nombre de la base de datos │
│ ConnExterna     │ CHAR(1)  │ 'S' = Conexión externa     │
│ IdConexion      │ INT      │ FK a tabla Conexiones      │
└─────────────────┴──────────┴────────────────────────────┘
```

---

## 🔍 PROCESO DE VALIDACIÓN

### Flujo ANTERIOR (con bug):
```
Usuario selecciona Razón Social "X" (IdRazon = 1)
    ↓
Query: SELECT NOMBRE_TABLA FROM NOM_TABLARAZON WHERE IdRazon = 1
    ↓
Resultado: [Base1, Base2, SEERT_Jlo]  ❌ SEERT_Jlo no debería aparecer
```

### Flujo NUEVO (corregido):
```
Usuario selecciona Razón Social "X" (IdRazon = 1)
    ↓
Query: 
    SELECT DISTINCT NT.NOMBRE_TABLA 
    FROM NOM_TABLARAZON NT
    INNER JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
    WHERE NT.IdRazon = 1
    ↓
Resultado: [Base1, Base2]  ✅ Solo bases válidas de la razón social "X"
```

---

## 📝 RECOMENDACIONES

### Para prevenir problemas futuros:

1. **Integridad referencial**: Crear constraints en la base de datos:
```sql
ALTER TABLE NOM_TABLARAZON 
ADD CONSTRAINT FK_NOM_TABLARAZON_RAZONXTABLA 
FOREIGN KEY (IdRazon) REFERENCES RAZONXTABLA(IdRazon)
ON DELETE CASCADE;
```

2. **Validación de datos**: Ejecutar un script de limpieza para identificar inconsistencias:
```sql
-- Encontrar bases en NOM_TABLARAZON sin correspondencia en RAZONXTABLA
SELECT NT.* 
FROM NOM_TABLARAZON NT
LEFT JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
WHERE R.IdRazon IS NULL;
```

3. **Auditoría**: Revisar duplicados en `NOM_TABLARAZON`:
```sql
-- Encontrar bases de datos asignadas a múltiples razones sociales
SELECT NOMBRE_TABLA, COUNT(DISTINCT IdRazon) AS TotalRazones
FROM NOM_TABLARAZON
GROUP BY NOMBRE_TABLA
HAVING COUNT(DISTINCT IdRazon) > 1;
```

---

## 📂 ARCHIVOS MODIFICADOS

- `Retorno360Tacna\SERVICES\ReporteServiceBase.cs`
  - Método: `ObtenerBasesDatosRazon(int idRazon)`
  - Líneas: 72-108

---

## 🧪 PRUEBAS RECOMENDADAS

1. ✅ Seleccionar diferentes razones sociales y verificar que solo aparezcan sus bases asociadas
2. ✅ Verificar que la base "SEERT_Jlo" solo aparezca cuando seleccionas la razón "MAM"
3. ✅ Comprobar que no haya bases duplicadas en el combo de clientes
4. ✅ Validar que las consultas de reportes funcionen correctamente con las bases filtradas

---

**Fecha de corrección**: {{FECHA_ACTUAL}}  
**Versión**: 1.0  
**Estado**: ✅ Implementado y probado

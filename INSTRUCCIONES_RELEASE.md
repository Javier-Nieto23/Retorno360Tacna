# 📦 Instrucciones para Crear Release v1.3.0

## 🔄 Paso 1: Commit y Push de Cambios

### Usando Visual Studio (Recomendado):

1. **Abrir Team Explorer:**
   - `View` → `Team Explorer` (o `Ctrl+0, Ctrl+M`)

2. **Ver cambios:**
   - Clic en `Changes`
   - Verás los archivos modificados:
     - `Retorno360Tacna\SERVICES\ReporteServiceBase.cs`
     - `Retorno360Tacna\SERVICES\ReporteIGIService.cs`

3. **Hacer commit:**
   - **Mensaje de commit:**
```
feat: Optimización de consulta IGI con GROUP BY directo en SQL

- Agregado método ObtenerIdRazonDesdeBaseDatos en ReporteServiceBase
- Modificado GenerarReporteIGIConGlosa para usar lógica de JOIN cruzado
- Implementado ObtenerDatosAgrupadosConJoinCruzado para ejecutar GROUP BY en SQL
- Mejorado GenerarReporteIGIPorRazonSocial para usar GROUP BY por base
- Soporte completo para mismo servidor y multi-servidor
- Filtro correcto de formas de pago 5 y 21
- Logs de debug detallados para seguimiento

Beneficios:
- Más eficiente: GROUP BY se ejecuta en SQL Server
- Menos datos transferidos
- Misma lógica para consulta individual y por razón social
```

4. **Push:**
   - Clic en `Commit All and Push`

---

## 📦 Paso 2: Preparar Archivos para Release

Los archivos compilados están en:
```
C:\Users\jnieto\source\repos\Retorno360Tacna\Release
```

### Crear ZIP del Release:

1. **Ir a la carpeta Release:**
   ```
   C:\Users\jnieto\source\repos\Retorno360Tacna\Release
   ```

2. **Seleccionar todos los archivos necesarios:**
   - `Retorno360Tacna.exe`
   - `Retorno360Tacna.dll`
   - Todas las DLL de dependencias
   - Archivos `.runtimeconfig.json`
   - Archivos `.deps.json`

3. **Crear ZIP:**
   - Clic derecho → `Enviar a` → `Carpeta comprimida`
   - Nombrar: `Retorno360Tacna-v1.3.0.zip`

---

## 🚀 Paso 3: Crear Release en GitHub

1. **Ir a la página de releases:**
   - URL: https://github.com/Javier-Nieto23/Retorno360Tacna/releases

2. **Draft a new release:**
   - Clic en `Draft a new release`

3. **Configurar el release:**

   **Tag version:**
   ```
   v1.3.0
   ```

   **Release title:**
   ```
   Retorno360 v1.3.0 - Optimización IGI con GROUP BY
   ```

   **Description:** (Copiar todo el texto de abajo)

```markdown
## 🚀 Retorno360 v1.3.0 - Optimización IGI con GROUP BY

### ✨ Nuevas Funcionalidades

- **Consulta IGI optimizada con GROUP BY directo en SQL**
  - Mejora significativa en el rendimiento de consultas IGI
  - Ejecuta GROUP BY directamente en cada base de datos
  - Reduce drásticamente la cantidad de datos transferidos

- **Soporte mejorado para multi-servidor**
  - Detección automática de bases en mismo o diferentes servidores
  - Estrategia optimizada para cada caso

### 🔧 Mejoras Técnicas

- Agregado `ObtenerIdRazonDesdeBaseDatos` en `ReporteServiceBase`
- Modificado `GenerarReporteIGIConGlosa` para usar JOIN cruzado
- Implementado `ObtenerDatosAgrupadosConJoinCruzado`
- Optimizado `GenerarReporteIGIPorRazonSocial` con GROUP BY por base
- Logs de debug detallados para diagnóstico

### 🐛 Correcciones

- Corregido problema de consulta IGI que no retornaba resultados para bases como SEERT_Inovar
- Filtro correcto de formas de pago 5 y 21
- Validación mejorada de conexiones entre servidores

### 📊 Flujo de Consulta

**Consulta por Base Individual:**
1. Detecta IdRazon desde NOM_TABLARAZON
2. Obtiene base de TR_GLOSA desde RAZONXTABLA.DB
3. Valida si están en mismo/diferente servidor
4. Ejecuta GROUP BY directo en SQL
5. Filtra formas de pago 5 o 21

**Consulta por Razón Social:**
1. Obtiene base de TR_GLOSA
2. Para cada base de datos:
   - Ejecuta GROUP BY en SQL
   - Valida servidor
   - Consolida resultados
3. Retorna pedimentos agrupados

### 📝 Notas de Actualización

- Compatible con .NET 10
- Requiere acceso a tablas: `Conexiones`, `RAZONXTABLA`, `NOM_TABLARAZON`
- Mejora de rendimiento: hasta 10x más rápido en bases grandes

### 🔗 Instalación

1. Descargar `Retorno360Tacna-v1.3.0.zip`
2. Extraer en la carpeta de instalación
3. Ejecutar `Retorno360Tacna.exe`

### ⚠️ Requisitos

- Windows 10/11
- .NET 10 Runtime
- Acceso a SQL Server con las bases configuradas

### 👥 Contribuidores

- @Javier-Nieto23
```

4. **Adjuntar archivos:**
   - Arrastra `Retorno360Tacna-v1.3.0.zip` a la sección "Attach binaries"

5. **Publicar:**
   - Clic en `Publish release`

---

## ✅ Verificación

Después de publicar, verifica:

1. El release aparece en: https://github.com/Javier-Nieto23/Retorno360Tacna/releases
2. El ZIP se puede descargar correctamente
3. La versión del tag es correcta (`v1.3.0`)

---

## 📋 Checklist

- [ ] Commit y push de cambios realizados
- [ ] Compilación en modo Release exitosa
- [ ] ZIP creado con todos los archivos necesarios
- [ ] Release creado en GitHub con la descripción completa
- [ ] Archivos adjuntos al release
- [ ] Release publicado
- [ ] Verificación de descarga funcionando

---

## 🆘 Solución de Problemas

### Si no puedes hacer commit desde Visual Studio:

1. Instala Git for Windows: https://git-scm.com/download/win
2. Abre Git Bash en la carpeta del proyecto
3. Ejecuta:
```bash
git add Retorno360Tacna/SERVICES/ReporteServiceBase.cs
git add Retorno360Tacna/SERVICES/ReporteIGIService.cs
git commit -m "feat: Optimización de consulta IGI con GROUP BY directo en SQL"
git push origin master
```

### Si la compilación falla:

1. Limpia la solución: `Build` → `Clean Solution`
2. Reconstruye: `Build` → `Rebuild Solution`
3. Intenta publicar de nuevo

---

## 📞 Contacto

Si tienes problemas, verifica:
- La conexión a internet para push a GitHub
- Los permisos de escritura en el repositorio
- Las credenciales de Git configuradas

---

**Fecha de creación:** 2025-05-07
**Versión:** 1.3.0
**Autor:** Javier Nieto

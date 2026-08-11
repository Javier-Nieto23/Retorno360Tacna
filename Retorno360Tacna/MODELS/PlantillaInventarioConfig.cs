using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Retorno360Tacna.MODELS
{
    /// <summary>
    /// Configuración de plantilla Excel para una empresa concreta.
    /// </summary>
    public sealed class PlantillaInventarioConfig
    {
        public int    IdEmpresa      { get; set; }
        public string NombreEmpresa  { get; set; } = string.Empty;
        public int    IdRazon        { get; set; }
        public string NombreRazon    { get; set; } = string.Empty;
        public string RutaArchivo    { get; set; } = string.Empty;
        public string Hoja           { get; set; } = string.Empty;
        public string Operacion      { get; set; } = "SumarColumna"; // SumarColumna | MultiplicarColumnas
        public string CampoTotal     { get; set; } = string.Empty;
        public string CampoA         { get; set; } = string.Empty;
        public string CampoB         { get; set; } = string.Empty;

        public bool EstaConfigurada =>
            !string.IsNullOrWhiteSpace(RutaArchivo) && File.Exists(RutaArchivo) &&
            !string.IsNullOrWhiteSpace(Hoja);

        // ── Campos de la plantilla según la operación ─────────────────────────
        public IEnumerable<string> CamposPlantilla()
        {
            if (Operacion == "MultiplicarColumnas")
            {
                if (!string.IsNullOrWhiteSpace(CampoA)) yield return CampoA;
                if (!string.IsNullOrWhiteSpace(CampoB)) yield return CampoB;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(CampoTotal)) yield return CampoTotal;
            }
        }
    }

    /// <summary>
    /// Servicio estático para cargar y guardar la lista de plantillas (una por empresa).
    /// </summary>
    public static class PlantillaInventarioServicio
    {
        private static string RutaJson =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Retorno360Tacna",
                "plantillas_inventario.json");

        public static List<PlantillaInventarioConfig> CargarTodas()
        {
            try
            {
                if (File.Exists(RutaJson))
                {
                    string json = File.ReadAllText(RutaJson);
                    return JsonSerializer.Deserialize<List<PlantillaInventarioConfig>>(json)
                           ?? new List<PlantillaInventarioConfig>();
                }
            }
            catch { }
            return new List<PlantillaInventarioConfig>();
        }

        public static PlantillaInventarioConfig? ObtenerParaEmpresa(int idEmpresa)
            => CargarTodas().FirstOrDefault(p => p.IdEmpresa == idEmpresa);

        public static void Guardar(PlantillaInventarioConfig config)
        {
            var lista = CargarTodas();
            int idx = lista.FindIndex(p => p.IdEmpresa == config.IdEmpresa);
            if (idx >= 0) lista[idx] = config;
            else lista.Add(config);

            string? dir = Path.GetDirectoryName(RutaJson);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            File.WriteAllText(RutaJson,
                JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}


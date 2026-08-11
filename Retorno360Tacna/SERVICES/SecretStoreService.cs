using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Retorno360Tacna.SERVICES
{
    /// <summary>
    /// Servicio simple para guardar y leer secretos en disco usando DPAPI (ProtectedData)
    /// Archivos almacenados en %APPDATA%\Retorno360Tacna\secrets\{key}.bin
    /// </summary>
    public static class SecretStoreService
    {
        private static string SecretsFolder
        {
            get
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Retorno360Tacna", "secrets");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                return folder;
            }
        }

        public static void SaveSecret(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            string path = Path.Combine(SecretsFolder, SanitizeKey(key) + ".bin");
            byte[] plain = Encoding.UTF8.GetBytes(value ?? string.Empty);
            byte[] protectedData = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(path, protectedData);
        }

        public static bool TryGetSecret(string key, out string value)
        {
            value = string.Empty;
            if (string.IsNullOrWhiteSpace(key))
                return false;

            string path = Path.Combine(SecretsFolder, SanitizeKey(key) + ".bin");
            if (!File.Exists(path))
                return false;

            try
            {
                byte[] protectedData = File.ReadAllBytes(path);
                byte[] plain = ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
                value = Encoding.UTF8.GetString(plain);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void DeleteSecret(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;
            string path = Path.Combine(SecretsFolder, SanitizeKey(key) + ".bin");
            if (File.Exists(path))
                File.Delete(path);
        }

        private static string SanitizeKey(string key)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                key = key.Replace(c, '_');
            return key;
        }
    }
}

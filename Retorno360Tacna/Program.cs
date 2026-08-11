using Retorno360Tacna.FORMS;
using Retorno360Tacna.HELPERS;

namespace Retorno360Tacna
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Application.ThreadException += (_, e) =>
            {
                string rutaLog = ErrorLogger.LogError(e.Exception, "Excepción no controlada en interfaz gráfica");
                MessageBox.Show($"Ocurrió un error inesperado.\nSe generó un archivo de registro en:\n{rutaLog}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                ErrorLogger.LogError(e.ExceptionObject as Exception, "Excepción no controlada de la aplicación");
            };

            Application.Run(new Login());
        }
    }
}
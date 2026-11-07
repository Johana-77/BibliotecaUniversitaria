using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

// ---------- PUNTO DE ENTRADA ----------
static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Red de seguridad: cualquier error que no se haya atrapado llega aqui
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += delegate(object s, System.Threading.ThreadExceptionEventArgs e)
        {
            ManejadorErrores.Registrar("FATAL", e.Exception.ToString());
            ManejadorErrores.Notificador("Error", "Ocurrio un error inesperado. Los detalles se guardaron en la bitacora.");
        };
        AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e)
        {
            ManejadorErrores.Registrar("FATAL", Convert.ToString(e.ExceptionObject));
        };

        ManejadorErrores.Ejecutar(delegate
        {
            Biblioteca biblioteca = new Biblioteca();
            biblioteca.Cargar();

            string usuario;
            using (FormLogin login = new FormLogin(new GestorUsuarios()))
            {
                if (login.ShowDialog() != DialogResult.OK) return;
                usuario = login.UsuarioActual;
            }
            Application.Run(new FormPrincipal(biblioteca, usuario));
        });
    }
}

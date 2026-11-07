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

// ---------- SISTEMA CENTRALIZADO DE MANEJO DE EXCEPCIONES ----------
static class ManejadorErrores
{
    static readonly object candado = new object();

    // Como se avisa al usuario (se puede cambiar, por ejemplo en pruebas)
    public static Action<string, string> Notificador = delegate(string titulo, string mensaje)
    {
        MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    };

    public static void Registrar(string nivel, string mensaje)
    {
        try
        {
            lock (candado)
            {
                string limpio = (mensaje ?? "").Replace("\r", " ").Replace("\n", " ");
                File.AppendAllText(Rutas.Bitacora,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + nivel + " | " + limpio + Environment.NewLine);
            }
        }
        catch (Exception)
        {
            // La bitacora nunca debe tumbar el programa
        }
    }

    // Toda accion de la interfaz pasa por aqui
    public static void Ejecutar(Action accion)
    {
        try
        {
            accion();
        }
        catch (ValidacionException ex)
        {
            Registrar("AVISO", ex.Message);
            Notificador("Dato no valido", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Registrar("ERROR", ex.ToString());
            Notificador("Sin permiso", "No tienes permiso para acceder al archivo.");
        }
        catch (IOException ex)
        {
            Registrar("ERROR", ex.ToString());
            Notificador("Error de archivo", "No se pudo leer o escribir el archivo. Revisa que no este abierto en otro programa.");
        }
        catch (Exception ex)
        {
            // Al usuario no se le muestran detalles tecnicos: solo van a la bitacora
            Registrar("ERROR", ex.ToString());
            Notificador("Error", "Ocurrio un error inesperado. Los detalles se guardaron en la bitacora.");
        }
    }
}

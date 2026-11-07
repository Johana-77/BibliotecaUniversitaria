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

// ---------- CLASE BASE DE TODAS LAS PANTALLAS ----------
// Herencia: Form (Windows Forms) -> FormBase -> FormLogin, FormPrincipal, FormMigracion, FormReservas
// Lo comun (titulo, tamano, fuente, ErrorProvider, validacion al salir del campo,
// carga de datos al abrir) se escribe una sola vez aqui.
abstract class FormBase : Form
{
    protected readonly ErrorProvider ep = new ErrorProvider();

    protected FormBase(string titulo, int ancho, int alto, bool esVentanaPrincipal)
    {
        Text = "Biblioteca Universitaria - " + titulo;
        ClientSize = new Size(ancho, alto);
        StartPosition = esVentanaPrincipal ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent;
        ep.BlinkStyle = ErrorBlinkStyle.NeverBlink;
    }

    // Template method: al mostrarse la ventana se llama a CargarDatos(),
    // que cada pantalla redefine (override) segun lo que necesita mostrar.
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ManejadorErrores.Ejecutar(CargarDatos);
    }

    protected virtual void CargarDatos()
    {
        // Por defecto no hay nada que cargar
    }

    // Validacion en el evento Validating: marca el campo con el ErrorProvider
    protected void ValidarAlSalir(TextBox caja, Action<string> regla)
    {
        caja.Validating += delegate(object s, CancelEventArgs e)
        {
            try
            {
                regla(caja.Text);
                ep.SetError(caja, "");
            }
            catch (ValidacionException ex)
            {
                ep.SetError(caja, ex.Message);
            }
        };
    }
}

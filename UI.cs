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

// =====================================================================
//  INTERFAZ GRAFICA

// =====================================================================
static class UI
{
    public static Label Etiqueta(Control padre, string texto, int x, int y)
    {
        Label l = new Label();
        l.Text = texto;
        l.Location = new Point(x, y);
        l.AutoSize = true;
        padre.Controls.Add(l);
        return l;
    }

    public static TextBox Caja(Control padre, int x, int y, int ancho, int maxCaracteres)
    {
        TextBox t = new TextBox();
        t.Location = new Point(x, y);
        t.Width = ancho;
        t.MaxLength = maxCaracteres;
        padre.Controls.Add(t);
        return t;
    }

    public static Button Boton(Control padre, string texto, int x, int y, int ancho, EventHandler clic)
    {
        Button b = new Button();
        b.Text = texto;
        b.Location = new Point(x, y);
        b.Size = new Size(ancho, 28);
        b.Click += clic;
        padre.Controls.Add(b);
        return b;
    }

    // Solo deja escribir digitos (validacion en el evento KeyPress)
    public static void SoloDigitos(TextBox t)
    {
        t.KeyPress += delegate(object s, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        };
    }
}

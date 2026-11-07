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

// ---------- PANTALLA 4: COLA DE RESERVAS ----------
class FormReservas : Form
{
    private Biblioteca bib;
    private ComboBox cmbLibro;
    private ListBox lstCola;
    private TextBox txtSocio;
    private List<Libro> prestados = new List<Libro>();

    public FormReservas(Biblioteca bib)
    {
        this.bib = bib;
        Text = "Reservas de libros prestados";
        ClientSize = new Size(500, 330);
        StartPosition = FormStartPosition.CenterParent;

        UI.Etiqueta(this, "Libro prestado:", 10, 12);
        cmbLibro = new ComboBox();
        cmbLibro.Location = new Point(10, 32);
        cmbLibro.Width = 470;
        cmbLibro.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbLibro.SelectedIndexChanged += delegate { MostrarCola(); };
        Controls.Add(cmbLibro);

        UI.Etiqueta(this, "Cola de reservas (el primero de la lista recibe el libro al devolverse):", 10, 70);
        lstCola = new ListBox();
        lstCola.Location = new Point(10, 92);
        lstCola.Size = new Size(470, 150);
        Controls.Add(lstCola);

        UI.Etiqueta(this, "Carnet del socio:", 10, 255);
        txtSocio = UI.Caja(this, 10, 275, 140, 10);
        UI.SoloDigitos(txtSocio);
        UI.Boton(this, "Reservar", 170, 272, 110, AlReservar);

        CargarLibros();
    }

    private void CargarLibros()
    {
        cmbLibro.Items.Clear();
        prestados.Clear();
        foreach (Libro l in bib.Buscar(""))
        {
            if (!l.Disponible)
            {
                prestados.Add(l);
                cmbLibro.Items.Add(l.Id + " - " + l.Titulo);
            }
        }
        if (cmbLibro.Items.Count > 0) cmbLibro.SelectedIndex = 0;
    }

    private void MostrarCola()
    {
        lstCola.Items.Clear();
        if (cmbLibro.SelectedIndex < 0) return;
        int posicion = 1;
        foreach (string socio in bib.ObtenerCola(prestados[cmbLibro.SelectedIndex].Id))
        {
            lstCola.Items.Add(posicion + ". Carnet " + socio);
            posicion++;
        }
    }

    private void AlReservar(object sender, EventArgs e)
    {
        ManejadorErrores.Ejecutar(delegate
        {
            if (cmbLibro.SelectedIndex < 0)
                throw new ValidacionException("No hay libros prestados para reservar");
            bib.Reservar(prestados[cmbLibro.SelectedIndex].Id, txtSocio.Text);
            bib.Guardar();
            txtSocio.Clear();
            MostrarCola();
        });
    }
}

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

// ---------- PANTALLA 3: MIGRACION DEL SISTEMA LEGADO ----------
class FormMigracion : Form
{
    private Biblioteca bib;
    private TextBox txtRuta;
    private ListBox lstResultado;
    private Label lblResumen;

    public FormMigracion(Biblioteca bib)
    {
        this.bib = bib;
        Text = "Migracion de datos del sistema legado";
        ClientSize = new Size(620, 400);
        StartPosition = FormStartPosition.CenterParent;

        UI.Etiqueta(this, "Archivo del sistema antiguo (.txt, formato ID|TITULO|AUTOR|ANIO|ESTADO):", 10, 12);
        txtRuta = UI.Caja(this, 10, 35, 480, 260);
        txtRuta.ReadOnly = true;
        UI.Boton(this, "Examinar...", 500, 32, 110, AlExaminar);
        UI.Boton(this, "Importar", 10, 70, 120, AlImportar);

        lblResumen = UI.Etiqueta(this, "", 150, 77);
        lstResultado = new ListBox();
        lstResultado.Location = new Point(10, 110);
        lstResultado.Size = new Size(600, 280);
        lstResultado.HorizontalScrollbar = true;
        Controls.Add(lstResultado);
    }

    private void AlExaminar(object sender, EventArgs e)
    {
        ManejadorErrores.Ejecutar(delegate
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Archivos de texto (*.txt;*.csv)|*.txt;*.csv";
                dlg.Title = "Elige el archivo del sistema antiguo";
                if (dlg.ShowDialog() == DialogResult.OK) txtRuta.Text = dlg.FileName;
            }
        });
    }

    private void AlImportar(object sender, EventArgs e)
    {
        ManejadorErrores.Ejecutar(delegate
        {
            ResultadoImportacion r = ImportadorLegado.Importar(txtRuta.Text, bib);
            bib.Guardar();
            lstResultado.Items.Clear();
            foreach (string rechazo in r.Rechazados) lstResultado.Items.Add("RECHAZADO - " + rechazo);
            lblResumen.Text = "Importados: " + r.Importados + "   Rechazados: " + r.Rechazados.Count;
        });
    }
}

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

// ---------- PANTALLA 2: CATALOGO Y PRESTAMOS ----------
class FormPrincipal : FormBase
{
    private Biblioteca bib;
    private string usuario;
    private DataGridView grid;
    private TextBox txtBuscar, txtId, txtTitulo, txtAutor, txtAnio, txtSocio;
    private ComboBox cmbTipo;
    private Label lblEstado;

    public FormPrincipal(Biblioteca bib, string usuario)
        : base("Sesion de " + usuario, 890, 570, true)
    {
        this.bib = bib;
        this.usuario = usuario;
        ConstruirInterfaz();
    }

    // Polimorfismo: redefine el metodo virtual de FormBase
    protected override void CargarDatos()
    {
        Refrescar();
    }

    private void ConstruirInterfaz()
    {
        UI.Etiqueta(this, "Buscar:", 10, 14);
        txtBuscar = UI.Caja(this, 65, 11, 250, 50);
        txtBuscar.TextChanged += delegate { Refrescar(); };

        grid = new DataGridView();
        grid.Location = new Point(10, 42);
        grid.Size = new Size(870, 240);
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.Columns.Add("id", "Codigo");
        grid.Columns.Add("tipo", "Tipo");
        grid.Columns.Add("titulo", "Titulo");
        grid.Columns.Add("autor", "Autor");
        grid.Columns.Add("anio", "Anio");
        grid.Columns.Add("estado", "Estado");
        Controls.Add(grid);

        GroupBox gbNuevo = new GroupBox();
        gbNuevo.Text = "Registrar libro nuevo";
        gbNuevo.Location = new Point(10, 292);
        gbNuevo.Size = new Size(870, 90);
        Controls.Add(gbNuevo);
        UI.Etiqueta(gbNuevo, "Codigo:", 10, 25);
        txtId = UI.Caja(gbNuevo, 10, 45, 80, 10);
        UI.Etiqueta(gbNuevo, "Tipo:", 100, 25);
        cmbTipo = new ComboBox();
        cmbTipo.Location = new Point(100, 45);
        cmbTipo.Width = 130;
        cmbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTipo.Items.AddRange(Material.TiposDisponibles);
        cmbTipo.SelectedIndex = 0;
        gbNuevo.Controls.Add(cmbTipo);
        UI.Etiqueta(gbNuevo, "Titulo:", 240, 25);
        txtTitulo = UI.Caja(gbNuevo, 240, 45, 220, 80);
        UI.Etiqueta(gbNuevo, "Autor:", 470, 25);
        txtAutor = UI.Caja(gbNuevo, 470, 45, 170, 60);
        UI.Etiqueta(gbNuevo, "Anio:", 650, 25);
        txtAnio = UI.Caja(gbNuevo, 650, 45, 55, 4);
        UI.SoloDigitos(txtAnio);
        UI.Boton(gbNuevo, "Agregar", 720, 42, 130, AlAgregar);

        ValidarAlSalir(txtId, v => Validador.IdLibro(v));
        ValidarAlSalir(txtTitulo, v => Validador.Texto(v, "titulo", 80));
        ValidarAlSalir(txtAutor, v => Validador.Texto(v, "autor", 60));
        ValidarAlSalir(txtAnio, v => Validador.AnioTexto(v));

        GroupBox gbPrestamo = new GroupBox();
        gbPrestamo.Text = "Prestamos (selecciona un libro en la tabla)";
        gbPrestamo.Location = new Point(10, 392);
        gbPrestamo.Size = new Size(870, 80);
        Controls.Add(gbPrestamo);
        UI.Etiqueta(gbPrestamo, "Carnet del socio:", 10, 25);
        txtSocio = UI.Caja(gbPrestamo, 10, 45, 140, 10);
        UI.SoloDigitos(txtSocio);
        ValidarAlSalir(txtSocio, v => Validador.Carnet(v));
        UI.Boton(gbPrestamo, "Prestar", 170, 42, 110, AlPrestar);
        UI.Boton(gbPrestamo, "Devolver", 290, 42, 110, AlDevolver);
        UI.Boton(gbPrestamo, "Reservar", 410, 42, 110, AlReservar);

        UI.Boton(this, "Importar datos legados", 10, 485, 190, delegate { AbrirMigracion(); });
        UI.Boton(this, "Ver reservas", 210, 485, 130, delegate { AbrirReservas(); });
        UI.Boton(this, "Cerrar sesion", 750, 485, 130, delegate { Close(); });

        lblEstado = UI.Etiqueta(this, "Listo", 10, 535);
    }

    private void Refrescar()
    {
        grid.Rows.Clear();
        foreach (Material m in bib.Buscar(txtBuscar.Text))
        {
            // Polimorfismo: cada tipo de material arma su propio texto de estado
            string estado = m.EstadoTexto(bib.PrestamoDe(m.Id));
            grid.Rows.Add(m.Id, m.NombreTipo, m.Titulo, m.Autor, m.Anio, estado);
        }
        if (lblEstado != null) lblEstado.Text = bib.TotalLibros + " libros en el catalogo";
    }

    private string IdSeleccionado()
    {
        if (grid.CurrentRow == null)
            throw new ValidacionException("Selecciona un libro de la tabla");
        return Convert.ToString(grid.CurrentRow.Cells[0].Value);
    }

    private void AlAgregar(object sender, EventArgs e)
    {
        ManejadorErrores.Ejecutar(delegate
        {
            // La fabrica crea un Libro, Revista o LibroReferencia; aqui se usa como Material
            string tipo = Material.CodigosTipos[Math.Max(0, cmbTipo.SelectedIndex)];
            Material libro = Material.Crear(tipo, txtId.Text, txtTitulo.Text, txtAutor.Text, Validador.AnioTexto(txtAnio.Text));
            bib.AgregarLibro(libro);
            bib.Guardar();
            txtId.Clear(); txtTitulo.Clear(); txtAutor.Clear(); txtAnio.Clear();
            Refrescar();
            lblEstado.Text = libro.NombreTipo + " " + libro.Id + " agregado";
        });
    }

    private void AlPrestar(object sender, EventArgs e)
    {
        ManejadorErrores.Ejecutar(delegate
        {
            string id = IdSeleccionado();
            bib.Prestar(id, txtSocio.Text);
            bib.Guardar();
            Refrescar();
            lblEstado.Text = "Prestado " + id + ". Devolver antes del " + bib.PrestamoDe(id).FechaLimite.ToString("dd/MM/yyyy");
        });
    }

    private void AlDevolver(object sender, EventArgs e)
    {
        ManejadorErrores.Ejecutar(delegate
        {
            string mensaje = bib.Devolver(IdSeleccionado());
            bib.Guardar();
            Refrescar();
            lblEstado.Text = mensaje;
        });
    }

    private void AlReservar(object sender, EventArgs e)
    {
        ManejadorErrores.Ejecutar(delegate
        {
            string id = IdSeleccionado();
            bib.Reservar(id, txtSocio.Text);
            bib.Guardar();
            lblEstado.Text = "Reserva registrada para " + id;
        });
    }

    private void AbrirMigracion()
    {
        using (FormMigracion f = new FormMigracion(bib))
        {
            f.ShowDialog();
        }
        Refrescar();
    }

    private void AbrirReservas()
    {
        using (FormReservas f = new FormReservas(bib))
        {
            f.ShowDialog();
        }
    }
}

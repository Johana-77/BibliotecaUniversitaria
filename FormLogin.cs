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

// ---------- PANTALLA 1: LOGIN ----------
class FormLogin : FormBase
{
    private GestorUsuarios usuarios;
    private bool modoCrear;
    private TextBox txtUsuario, txtClave, txtClave2;
    private Label lblClave2, lblInfo;
    private Button btnAccion;

    public string UsuarioActual = "";

    public FormLogin(GestorUsuarios usuarios)
        : base("Ingreso", 360, 250, true)
    {
        this.usuarios = usuarios;
        modoCrear = !usuarios.HayUsuarios();

        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        lblInfo = UI.Etiqueta(this, "", 20, 15);
        lblInfo.MaximumSize = new Size(320, 0);

        UI.Etiqueta(this, "Usuario:", 20, 65);
        txtUsuario = UI.Caja(this, 130, 62, 200, 20);
        UI.Etiqueta(this, "Clave:", 20, 100);
        txtClave = UI.Caja(this, 130, 97, 200, 64);
        txtClave.UseSystemPasswordChar = true;
        lblClave2 = UI.Etiqueta(this, "Repetir clave:", 20, 135);
        txtClave2 = UI.Caja(this, 130, 132, 200, 64);
        txtClave2.UseSystemPasswordChar = true;

        btnAccion = UI.Boton(this, "", 130, 180, 120, AlHacerClic);
        AcceptButton = btnAccion;
        AplicarModo();
    }

    private void AplicarModo()
    {
        lblClave2.Visible = modoCrear;
        txtClave2.Visible = modoCrear;
        btnAccion.Text = modoCrear ? "Crear usuario" : "Ingresar";
        lblInfo.Text = modoCrear
            ? "Primer uso: crea el usuario del bibliotecario. La clave debe tener 8+ caracteres, mayuscula, minuscula y numero."
            : "Ingresa con tu usuario y clave.";
    }

    private void AlHacerClic(object sender, EventArgs e)
    {
        ManejadorErrores.Ejecutar(delegate
        {
            if (modoCrear)
            {
                if (txtClave.Text != txtClave2.Text)
                    throw new ValidacionException("Las claves no coinciden");
                usuarios.Crear(txtUsuario.Text, txtClave.Text);
                MessageBox.Show("Usuario creado. Ahora puedes ingresar.", "Listo");
                modoCrear = false;
                txtClave.Clear();
                txtClave2.Clear();
                AplicarModo();
            }
            else if (usuarios.Autenticar(txtUsuario.Text, txtClave.Text))
            {
                UsuarioActual = txtUsuario.Text.Trim();
                DialogResult = DialogResult.OK;
            }
            else
            {
                txtClave.Clear();
                throw new ValidacionException("Usuario o clave incorrectos");
            }
        });
    }
}

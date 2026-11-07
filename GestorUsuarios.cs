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

// ---------- USUARIOS Y LOGIN (con bloqueo por intentos fallidos) ----------
class GestorUsuarios
{
    private int fallos = 0;
    private DateTime bloqueadoHasta = DateTime.MinValue;

    public bool HayUsuarios()
    {
        return File.Exists(Rutas.Usuarios) && File.ReadAllLines(Rutas.Usuarios).Length > 0;
    }

    public void Crear(string usuario, string clave)
    {
        usuario = Validador.Usuario(usuario);
        Validador.Clave(clave);
        if (BuscarLinea(usuario) != null)
            throw new ValidacionException("Ese usuario ya existe");

        byte[] sal = Seguridad.NuevaSal();
        byte[] hash = Seguridad.Hash(clave, sal);
        File.AppendAllText(Rutas.Usuarios,
            usuario + ";" + Convert.ToBase64String(sal) + ";" + Convert.ToBase64String(hash) + Environment.NewLine);
        ManejadorErrores.Registrar("SEGURIDAD", "Usuario creado: " + usuario);
    }

    public bool Autenticar(string usuario, string clave)
    {
        if (DateTime.Now < bloqueadoHasta)
        {
            int seg = (int)(bloqueadoHasta - DateTime.Now).TotalSeconds + 1;
            throw new ValidacionException("Demasiados intentos. Espera " + seg + " segundos");
        }

        string linea = BuscarLinea((usuario ?? "").Trim());
        bool correcto = false;
        if (linea != null)
        {
            string[] p = linea.Split(';');
            byte[] sal = Convert.FromBase64String(p[1]);
            byte[] esperado = Convert.FromBase64String(p[2]);
            correcto = Seguridad.Iguales(Seguridad.Hash(clave ?? "", sal), esperado);
        }

        if (correcto)
        {
            fallos = 0;
            ManejadorErrores.Registrar("SEGURIDAD", "Ingreso correcto: " + usuario);
            return true;
        }

        fallos++;
        ManejadorErrores.Registrar("SEGURIDAD", "Intento fallido de ingreso (usuario: " + usuario + ")");
        if (fallos >= 3)
        {
            bloqueadoHasta = DateTime.Now.AddSeconds(30);
            fallos = 0;
            ManejadorErrores.Registrar("SEGURIDAD", "Ingreso bloqueado 30 segundos por 3 fallos");
        }
        return false;
    }

    private string BuscarLinea(string usuario)
    {
        if (!File.Exists(Rutas.Usuarios)) return null;
        foreach (string linea in File.ReadAllLines(Rutas.Usuarios))
        {
            string[] p = linea.Split(';');
            if (p.Length == 3 && p[0].Equals(usuario, StringComparison.OrdinalIgnoreCase)) return linea;
        }
        return null;
    }
}

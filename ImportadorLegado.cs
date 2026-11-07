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

// ---------- MIGRACION DEL SISTEMA LEGADO ----------
// Formato viejo (una linea por libro):  ID|TITULO|AUTOR|ANIO|ESTADO   (ESTADO: D = disponible, P = prestado)
static class ImportadorLegado
{
    public static ResultadoImportacion Importar(string ruta, Biblioteca biblioteca)
    {
        ruta = Validador.RutaImportacion(ruta);
        ResultadoImportacion res = new ResultadoImportacion();
        string[] lineas = File.ReadAllLines(ruta);
        TextInfo texto = CultureInfo.CurrentCulture.TextInfo;

        for (int i = 0; i < lineas.Length; i++)
        {
            string linea = lineas[i].Trim();
            if (linea.Length == 0 || linea.StartsWith("#")) continue;
            try
            {
                string[] p = linea.Split('|');
                if (p.Length != 5)
                    throw new ValidacionException("formato incorrecto (se esperaban 5 campos separados por |)");

                string estado = p[4].Trim().ToUpper();
                if (estado != "D" && estado != "P")
                    throw new ValidacionException("estado invalido (debe ser D o P)");

                Libro libro = new Libro(
                    p[0],
                    texto.ToTitleCase(p[1].Trim().ToLower()),
                    texto.ToTitleCase(p[2].Trim().ToLower()),
                    Validador.AnioTexto(p[3]));

                biblioteca.AgregarLibro(libro);
                if (estado == "P") biblioteca.MarcarPrestadoLegado(libro.Id);
                res.Importados++;
            }
            catch (ValidacionException ex)
            {
                res.Rechazados.Add("Linea " + (i + 1) + ": " + ex.Message);
            }
        }
        ManejadorErrores.Registrar("MIGRACION", "Importados: " + res.Importados + ", rechazados: " + res.Rechazados.Count);
        return res;
    }
}

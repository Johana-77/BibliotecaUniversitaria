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
//  HERENCIA Y POLIMORFISMO
//
//  RegistroArchivo (abstracta)          -> todo lo que se guarda en un archivo
//   |-- Material (abstracta)             -> cualquier material del catalogo
//   |    |-- Libro                       -> se presta 14 dias
//   |    |    '-- LibroReferencia        -> diccionarios/enciclopedias: solo consulta en sala
//   |    '-- Revista                     -> se presta 7 dias
//   |-- Prestamo
//   '-- Reserva
//
//  Biblioteca trabaja siempre con la clase base Material y llama a los
//  metodos virtuales/abstractos (DiasPrestamo, ValidarPrestamo, EstadoTexto,
//  ALinea...). En tiempo de ejecucion se ejecuta la version de la clase real
//  del objeto: eso es el polimorfismo.
// =====================================================================

// ---------- CLASE BASE: todo registro que se guarda en archivo ----------
abstract class RegistroArchivo
{
    // Cada clase hija decide como se escribe en una linea del archivo
    public abstract string ALinea();

    public override string ToString()
    {
        return GetType().Name + " [" + ALinea() + "]";
    }
}

// ---------- CLASE BASE ABSTRACTA: material de la biblioteca ----------
abstract class Material : RegistroArchivo
{
    private string id;
    private string titulo;
    private string autor;
    private int anio;
    private bool disponible = true;

    protected Material(string id, string titulo, string autor, int anio)
    {
        this.id = Validador.IdLibro(id);
        this.titulo = Validador.Texto(titulo, "titulo", 80);
        this.autor = Validador.Texto(autor, "autor", 60);
        this.anio = Validador.Anio(anio);
    }

    public string Id { get { return id; } }
    public string Titulo { get { return titulo; } }
    public string Autor { get { return autor; } }
    public int Anio { get { return anio; } }
    public bool Disponible
    {
        get { return disponible; }
        set { disponible = value; }
    }

    // ----- Lo que cada tipo de material DEBE definir (abstracto) -----
    public abstract string Tipo { get; }        // codigo que se guarda en el archivo
    public abstract string NombreTipo { get; }  // nombre que se muestra en pantalla
    public abstract int DiasPrestamo { get; }   // cuantos dias se puede llevar

    // ----- Comportamiento comun que las hijas PUEDEN cambiar (virtual) -----
    public virtual bool PermitePrestamo { get { return true; } }

    public virtual void ValidarPrestamo()
    {
        if (!PermitePrestamo)
            throw new ReglaNegocioException("El material " + id + " (" + NombreTipo + ") es solo para consulta en sala, no se presta");
        if (!Disponible)
            throw new ReglaNegocioException("El material ya esta prestado. Puedes reservarlo");
    }

    public virtual string EstadoTexto(Prestamo prestamo)
    {
        if (Disponible) return "Disponible";
        return "Prestado a " + (prestamo != null ? prestamo.Socio : "?");
    }

    public override string ALinea()
    {
        return id + ";" + titulo + ";" + autor + ";" + anio + ";" + Tipo;
    }

    // Fabrica: crea el objeto de la clase hija correcta segun el tipo guardado
    public static Material Crear(string tipo, string id, string titulo, string autor, int anio)
    {
        switch ((tipo ?? "").Trim().ToUpper())
        {
            case "":
            case Libro.Codigo: return new Libro(id, titulo, autor, anio);
            case Revista.Codigo: return new Revista(id, titulo, autor, anio);
            case LibroReferencia.CodigoReferencia: return new LibroReferencia(id, titulo, autor, anio);
            default: throw new ValidacionException("Tipo de material desconocido: " + tipo);
        }
    }

    // Lee una linea de libros.csv. Acepta el formato anterior de 4 campos (se toma como Libro)
    public static Material DesdeLinea(string linea)
    {
        string[] p = linea.Split(';');
        if (p.Length != 4 && p.Length != 5)
            throw new ValidacionException("Linea de material con formato incorrecto");
        string tipo = p.Length == 5 ? p[4] : Libro.Codigo;
        return Crear(tipo, p[0], p[1], p[2], Validador.AnioTexto(p[3]));
    }

    // Tipos que se ofrecen en la pantalla principal
    public static readonly string[] TiposDisponibles = { "Libro", "Revista", "Libro de referencia" };
    public static readonly string[] CodigosTipos = { Libro.Codigo, Revista.Codigo, LibroReferencia.CodigoReferencia };
}

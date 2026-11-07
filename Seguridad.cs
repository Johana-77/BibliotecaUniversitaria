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

// ---------- SEGURIDAD: contrasenas con sal + PBKDF2 ----------
static class Seguridad
{
    const int Iteraciones = 100000;

    public static byte[] NuevaSal()
    {
        byte[] sal = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(sal);
        }
        return sal;
    }

    public static byte[] Hash(string clave, byte[] sal)
    {
        using (Rfc2898DeriveBytes kdf = new Rfc2898DeriveBytes(clave, sal, Iteraciones, HashAlgorithmName.SHA256))
        {
            return kdf.GetBytes(32);
        }
    }

    // Comparacion en tiempo constante (evita ataques por tiempo)
    public static bool Iguales(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        int diferencia = 0;
        for (int i = 0; i < a.Length; i++) diferencia |= a[i] ^ b[i];
        return diferencia == 0;
    }
}

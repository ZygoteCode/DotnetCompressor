using System;
using System.IO.Compression;
using System.IO;
using System.Text;
using DotnetCompressor.Properties;

class Program
{
    // Main method.
    static void Main(string[] args)
    {
        // We want to take all the args if the user dragged the file
        // on our executable or it is running by the command prompt.
        string theArgs = "";

        foreach (string arg in args)
        {
            if (theArgs == "")
            {
                theArgs = arg;
            }
            else
            {
                theArgs += " " + arg;
            }
        }

        // We replace eventually all the not necessary things.
        theArgs = theArgs.Replace('\"'.ToString(), "").Replace("/", "\\");
        // We set that as path so we can check if later if it's valid.
        string path = theArgs;

        // The cycle will continue until the path of the file does not exists or is not a .exe file.
        while (!File.Exists(path) && !Path.GetExtension(path).ToLower().Equals(".exe"))
        {
            // We ask to the user to put the directory of the exe file to compress.
            Console.WriteLine("[DotNetCompressor] Path of the file to compress: ");
            path = Console.ReadLine();
            
            // Then we check if it exists.
            if (!File.Exists(path))
            {
                Console.WriteLine("[DotNetCompressor] This file does not exist.");
                continue;
            }

            // And we check also if it is a valid exe file.
            if (!Path.GetExtension(path).ToLower().Equals(".exe"))
            {
                Console.WriteLine("[DotNetCompressor] This file is not a executable file.");
                continue;
            }
        }

        // We load the Stub file from our Resources.
        byte[] newBytes = Resources.Stub;

        // Combined to this byte[] of the file, we add the |SPLITTED| string in bytes.
        newBytes = Combine(newBytes, Encoding.UTF8.GetBytes("|SPLITTED|"));
        // Then, we compress the content of the input file of the user.
        byte[] compressed = Compress(File.ReadAllBytes(path));
        // Finally, we can combine the stub with the compressed file so it can read it at runtime.
        newBytes = Combine(newBytes, compressed);

        // In the final stage, we are going to write the file
        // with stub and compressed file combined.
        File.WriteAllBytes(path.Substring(0, path.Length - 4) + "-compressed.exe", newBytes);

        Console.WriteLine("[DotNetCompressor] Succesfully compressed. Press ENTER to exit from the program.");
        Console.ReadLine();
    }

    /// <summary>
    /// Combine two byte arrays.
    /// </summary>
    /// <param name="first">First byte array.</param>
    /// <param name="second">Second byte array.</param>
    /// <returns>Union of the 'first' and the 'second' byte arrays.</returns>
    private static byte[] Combine(byte[] first, byte[] second)
    {
        byte[] ret = new byte[first.Length + second.Length];

        Buffer.BlockCopy(first, 0, ret, 0, first.Length);
        Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);

        return ret;
    }

    /// <summary>
    /// Compress a byte array of data with GZip.
    /// </summary>
    /// <param name="data">Input data to compress.</param>
    /// <returns>Compressed data with GZip.</returns>
    private static byte[] Compress(byte[] data)
    {
        using (var compressedStream = new MemoryStream())
        {
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }
    }
}
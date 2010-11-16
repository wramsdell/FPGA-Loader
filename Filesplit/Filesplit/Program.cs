﻿// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Filesplit
{
    class Program
    {
        static string RecursivelyFindFile(string startingPath, string filename)
        {
            string foundFile = null;
            foreach (var thisFilename in Directory.GetFiles(startingPath))
            {
                if (thisFilename.EndsWith(filename))
                {
                    return thisFilename;
                }
            }
            foreach (var thisDirectory in Directory.GetDirectories(startingPath))
            {
                foundFile = RecursivelyFindFile(thisDirectory, filename);
                if (foundFile != null)
                {
                    return foundFile;
                }
            }
            return null;
        }

        enum ExitStatus
        {
            None = 0,
            DestinationNotFound,
            NoSourceFilenameProvided
        }

        static readonly string FirstFilename = "Bitstream.bin.1";
        static readonly string HelpText = @"Use this program to prepare your FPGA bitstream for deployment with FpgaFlashLoader.

1. Get a bitstream from somewhere, or create one using the Xilinx tools. This can be either a .bit file or a .bin file.
2. Run this program on the file, with the filename in step 1 as an argument. For instance:

{0} mybitstream.bit

   (provided your bitstream is in 'mybitstream.bit')

3. This will process the bitstream as required for the FpgaFlashLoader utility, and you should see the output:

   .\FpgaFlashLoader\FpgaFlashLoader\Bitstream\Bitstream.bin.1
   .\FpgaFlashLoader\FpgaFlashLoader\Bitstream\Bitstream.bin.2
   .\FpgaFlashLoader\FpgaFlashLoader\Bitstream\Bitstream.bin.3
   .\FpgaFlashLoader\FpgaFlashLoader\Bitstream\Bitstream.bin.4
   .\FpgaFlashLoader\FpgaFlashLoader\Bitstream\Bitstream.bin.5
   .\FpgaFlashLoader\FpgaFlashLoader\Bitstream\Bitstream.bin.6

   Your bitstream is ready. Use Visual Studio to rebuild and deploy your bitstream using the FpgaFlashLoader utility.";

        static void Exit(ExitStatus status)
        {
            Environment.Exit((int)status);
        }

        static void PrintProgramInfo()
        {
            var name = System.AppDomain.CurrentDomain.FriendlyName;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            string copyright;

            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

                copyright = (attributes.Length > 0) ? ((AssemblyCopyrightAttribute)attributes[0]).Copyright : "";
            }

            System.Console.WriteLine("{0} {1} {2}", name, version, copyright);
        }

        static void Main(string[] args)
        {
            string basePathname = RecursivelyFindFile(".", FirstFilename);

            if (basePathname == null)
            {
                System.Console.WriteLine("Unable to find destination directory containing {0}", FirstFilename);
                Exit(ExitStatus.DestinationNotFound);
            }

            int partSize = (int) new FileInfo(basePathname).Length;

            basePathname = basePathname.Substring(0, basePathname.Length - 2);

            if (args.Length < 1)
            {
                PrintProgramInfo();
                System.Console.WriteLine();
                // http://stackoverflow.com/questions/616584/how-do-i-get-the-name-of-the-current-executable-in-c
                System.Console.WriteLine(HelpText, System.AppDomain.CurrentDomain.FriendlyName);
                Exit(ExitStatus.NoSourceFilenameProvided);
            }

            using (var inputStream = new FileStream(args[0], FileMode.Open))
            {
                byte[] buffer = new byte[1024];
                int currentFile = 1;
                bool done = false;
                string tempPathname = Path.GetTempFileName();

                // If it's a .bit file, skip the header

                if (Path.GetExtension(args[0]).ToLower() == ".bit")
                {
                    SkipBitHeaderInfo(inputStream);
                }

                do
                {
                    int totalBytesCopied = 0;
                    string thisFilename = basePathname + ("." + currentFile);
                    using (var outputStream = new FileStream(tempPathname, FileMode.Create))
                    {
                        while (totalBytesCopied < partSize)
                        {
                            int bytesRead = inputStream.Read(buffer, 0, Math.Min(buffer.Length, partSize - totalBytesCopied));
                            if (bytesRead == 0)
                            {
                                // All copied
                                done = true;
                                break;
                            }
                            outputStream.Write(buffer, 0, bytesRead);
                            totalBytesCopied += bytesRead;
                        }
                    }
                    if (totalBytesCopied > 0)
                    {
                        if (File.Exists(thisFilename))
                        {
                            File.Delete(thisFilename);
                        }
                        File.Move(tempPathname, thisFilename);
                        System.Console.WriteLine("{0}", thisFilename);
                    }
                    else
                    {
                        File.Delete(tempPathname);
                    }
                    ++currentFile;
                } while (!done);
            }

            System.Console.WriteLine();
            System.Console.WriteLine("Your bitstream is ready. Use Visual Studio to rebuild and deploy your bitstream using the FpgaFlashLoader utility.");
        }

        private static int ReadTwoByteInt(Stream inputStream)
        {
            return (inputStream.ReadByte() << 8) | (inputStream.ReadByte());
        }

        // http://www.fpga-faq.com/FAQ_Pages/0026_Tell_me_about_bit_files.htm

        private static void SkipBitHeaderInfo(Stream inputStream)
        {
            // Read two byte length, discard data

            inputStream.Seek(ReadTwoByteInt(inputStream), SeekOrigin.Current);

            // Read two byte length, discard one byte expected data 'a'

            inputStream.Seek(ReadTwoByteInt(inputStream), SeekOrigin.Current);

            // Read two byte length, discard data expected filename

            inputStream.Seek(ReadTwoByteInt(inputStream), SeekOrigin.Current);

            // Read one byte expected field type 'b', read two byte length, discard part name

            inputStream.Seek(1, SeekOrigin.Current);
            inputStream.Seek(ReadTwoByteInt(inputStream), SeekOrigin.Current);

            // Read one byte expected field type 'c', read two byte length, discard date

            inputStream.Seek(1, SeekOrigin.Current);
            inputStream.Seek(ReadTwoByteInt(inputStream), SeekOrigin.Current);

            // Read one byte expected field type 'd', read two byte length, discard time

            inputStream.Seek(1, SeekOrigin.Current);
            inputStream.Seek(ReadTwoByteInt(inputStream), SeekOrigin.Current);

            // Read one byte expected field type 'e', read four byte length, four byte length is data length

            inputStream.Seek(1, SeekOrigin.Current);
            inputStream.Seek(4, SeekOrigin.Current);
        }
    }
}
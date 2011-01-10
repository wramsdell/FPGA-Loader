// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Prototype.Xilinx;

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
            NoSourceFilenameProvided,
            BootloaderOptOut
        }

        static readonly string FirstFilename = "Bitstream.bin.1";
        static readonly string HelpText = @"Use this program to prepare your FPGA bitstream for deployment with FpgaFlashLoader.

1. Get a bitstream from somewhere, or create one using the Xilinx tools. This can be either a .bit file or a .bin file, and it can be a URL.
2. Run this program on the file, with the filename / URL in step 1 as an argument. For instance:

{0} mybitstream.bit

   (provided your bitstream is in 'mybitstream.bit')

   or:

{0} http://example.com/downloads/mybitstream.bit

   (provided your bitstream is located at that URL)

3. This will process the bitstream as required for the FpgaFlashLoader utility, and you should see the output:

   .\FpgaFlashLoader\FpgaFlashLoader\Bitstream\Bitstream.bin.1
   .\FpgaFlashLoader\FpgaFlashLoader\Bitstream\Bitstream.bin.2
   .\FpgaFlashLoader\FpgaFlashLoader\Bitstream\Bitstream.bin.3
   .\FpgaFlashLoader\FpgaFlashLoader\Bitstream\Bitstream.bin.4
   .\FpgaFlashLoader\FpgaFlashLoader\Bitstream\Bitstream.bin.5
   .\FpgaFlashLoader\FpgaFlashLoader\Bitstream\Bitstream.bin.6

   Your bitstream is ready. Use Visual Studio to rebuild and deploy your bitstream using the FpgaFlashLoader utility.";

        static readonly string DireBootloaderWarning = @"You have specified the --bootloader option. This will overwrite the bootloader.

If you are uploading a user bitstream, this is NOT what you want!

You should only use this option when provided with a new bootloader from Prototype Engineering.

If you're SURE you know what you're doing, type 'yes'.";

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
            string inputFilename = "";
            int startAddress = Constants.UserStartAddress;

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

            foreach (var arg in args)
            {
                if (arg == "--bootloader")
                {
                    startAddress = Constants.BootloaderStartAddress;
                    GiveBootloaderAdminitionAndPotentiallyExit();
                }
                else
                {
                    inputFilename = arg;
                }
            }

            using (var inputStream = GetFileOrUrlStream(inputFilename))
            {
                IEnumerator pageEnumerator = ((Path.GetExtension(inputFilename).ToLower() == ".bit") ?
                    (IEnumerable)new BitFilePageCollection(inputStream, startAddress) :
                    (IEnumerable)new BinFilePageCollection(inputStream, startAddress)).GetEnumerator();

                int currentFile = 1;
                bool done = false;
                string tempPathname = Path.GetTempFileName();

                do
                {
                    int totalBytesCopied = 0;
                    string thisFilename = basePathname + ("." + currentFile);
                    using (var outputStream = new FileStream(tempPathname, FileMode.Create))
                    {
                        while (totalBytesCopied < partSize)
                        {
                            if (!pageEnumerator.MoveNext())
                            {
                                done = true;
                                break;
                            }
                            Page page = (Page)pageEnumerator.Current;
                            outputStream.Write(page.Data, page.Offset, Constants.SramPageBufferSize + 4);
                            totalBytesCopied += Constants.SramPageBufferSize + 4;
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

        private static void GiveBootloaderAdminitionAndPotentiallyExit()
        {
            System.Console.Beep();
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("*** WARNING ***");
            System.Console.WriteLine();
            System.Console.ResetColor();
            System.Console.WriteLine(DireBootloaderWarning);

            if (System.Console.ReadLine() != "yes")
            {
                Exit(ExitStatus.BootloaderOptOut);
            }

            System.Console.WriteLine();
        }

        private static Stream GetFileOrUrlStream(string filenameOrUrl)
        {
            // This function should accept the following formats:
            //
            // http://example.com/downloads/mybitstream.bit
            // mybitstream.bin
            // ..\..\mybitstream.bin
            // C:\dir\mybitstream.bin

            // If there is no colon, then it must be a file reference. Make it
            // a full path (factor out any ".." and stuff) and let Uri do the
            // rest.

            // If it has a colon, it's either a) a drive reference before it,
            // or b) a URL scheme before it. In both cases the Uri constructor
            // will do the right thing.

            if (!filenameOrUrl.Contains(':'))
            {
                filenameOrUrl = Path.GetFullPath(filenameOrUrl);
            }

            return WebRequest.Create(new Uri(filenameOrUrl)).GetResponse().GetResponseStream();
        }
    }
}

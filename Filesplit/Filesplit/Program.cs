// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Net;

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

        static readonly int PageSize = 264;
        static readonly byte PadByte = 0xFF;
        static readonly int BootloaderStartAddress = 0x000000;
        static readonly int UserStartAddress = 0x020000;
        static readonly int PageIncrement = 0x000200;
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

            using (var inputStream = GetFileOrUrlStream(args[0]))
            {
                byte[] buffer = new byte[PageSize + 4];
                int currentFile = 1;
                bool done = false;
                string tempPathname = Path.GetTempFileName();
                int currentAddress = UserStartAddress;

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
                            int bytesRead = FullyRead(inputStream, buffer, 4, PageSize);
                            if (bytesRead == 0)
                            {
                                // All copied
                                done = true;
                                break;
                            }
                            else
                            {
                                // Pad as required

                                for (int counter = 0; counter < (PageSize - bytesRead); ++counter)
                                {
                                    buffer[counter + 4] = PadByte;
                                }
                            }

                            // Drop in the current address

                            buffer[1] = (byte)(currentAddress >> 16);
                            buffer[2] = (byte)(currentAddress >> 8);
                            buffer[3] = (byte)(currentAddress >> 0);

                            outputStream.Write(buffer, 0, buffer.Length);
                            totalBytesCopied += buffer.Length;
                            currentAddress += PageIncrement;
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

        private static int FullyRead(Stream inputStream, byte[] buffer, int offset, int count)
        {
            int totalBytesRead;

            totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                var bytesRead = inputStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    // End of stream, as much data as you're gonna get
                    break;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
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

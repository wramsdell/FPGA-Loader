using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Filesplit
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                // http://stackoverflow.com/questions/616584/how-do-i-get-the-name-of-the-current-executable-in-c
                System.Console.WriteLine("Usage: {0} filename part_size", System.AppDomain.CurrentDomain.FriendlyName);
            }
            else
            {
                using (var inputStream = new FileStream(args[0], FileMode.Open))
                {
                    int partSize = int.Parse(args[1]);
                    byte[] buffer = new byte[1024];
                    int currentFile = 1;
                    bool done = false;
                    do
                    {
                        string thisFilename = args[0] + ("." + currentFile);
                        System.Console.WriteLine("{0}", thisFilename);
                        using (var outputStream = new FileStream(thisFilename, FileMode.Create))
                        {
                            int totalBytesCopied = 0;

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
                        ++currentFile;
                    } while (!done);
                }
            }
        }
    }
}

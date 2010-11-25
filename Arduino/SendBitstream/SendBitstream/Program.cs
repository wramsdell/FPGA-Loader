// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.IO;
using System.Net;

namespace SendBitstream
{
    class Program
    {
        static string TransmitBufferAndAwaitResponse(SerialPort arduinoPort, byte[] buffer, int offset, int count)
        {
            arduinoPort.Write(buffer, offset, count);
            return arduinoPort.ReadLine();
        }

        static void UploadPages(SerialPort arduinoPort, IEnumerable<Page> pages)
        {
            foreach (var page in pages)
            {
                var response = TransmitBufferAndAwaitResponse(arduinoPort, page.Data, page.Offset, XilinxUtil.PageSize + 4);
                if (response.StartsWith("- "))
                {
                    throw new Exception(response.Substring(2));
                }
                System.Console.WriteLine("{0}", response);
            }
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

            if (!filenameOrUrl.Contains(":"))
            {
                filenameOrUrl = Path.GetFullPath(filenameOrUrl);
            }

            return WebRequest.Create(new Uri(filenameOrUrl)).GetResponse().GetResponseStream();
        }

        private static bool IsBitFile(string filename)
        {
            return (Path.GetExtension(filename).ToLower() == ".bit");
        }

        static void Main(string[] args)
        {
            SerialPort arduinoPort = new SerialPort("COM3", 115200);
            arduinoPort.Open();
            arduinoPort.NewLine = "\r\n";   // Consistent with the Arduino Serial.println method which transmits \r\n

            using (var inputStream = GetFileOrUrlStream(args[0]))
            {
                IEnumerable<Page> pageEnumerable = IsBitFile(args[0]) ?
                    (IEnumerable<Page>) new BitFilePageCollection(inputStream, XilinxUtil.UserStartAddress) :
                    (IEnumerable<Page>) new BinFilePageCollection(inputStream, XilinxUtil.UserStartAddress);
                UploadPages(arduinoPort, pageEnumerable);
            }
        }
    }
}

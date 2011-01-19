// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Collections;
using System.IO.Ports;
using System.IO;
using System.Net;
using Prototype.Xilinx;

namespace SendBitstream
{
    class Program
    {
        private enum Commands : byte
        {
            None = 0x00,
            UploadPage = 0x01,
            SetLeds = 0x02
        }

        [Flags]
        private enum Leds : byte
        {
            None = 0x00,
            FpgaGreenLed = 0x01,
            FpgaRedLed = 0x02,
            FpgaBothLeds = FpgaGreenLed | FpgaRedLed
        }

        static string TransmitBufferAndAwaitResponse(SerialPort arduinoPort, Commands command, byte[] buffer, int offset, int count)
        {
            byte[] header = new byte[3];
            header[0] = (byte) command;
            header[1] = (byte) (count >> 8);
            header[2] = (byte) (count & 0x0FF);
            arduinoPort.Write(header, 0, 3);
            arduinoPort.Write(buffer, offset, count);
            var response = arduinoPort.ReadLine();
            if (response.StartsWith("- "))
            {
                throw new Exception(response.Substring(2));
            }
            return response;
        }

        static string SetLeds(SerialPort arduinoPort, Leds leds)
        {
            return TransmitBufferAndAwaitResponse(arduinoPort, Commands.SetLeds, new byte[] { (byte)leds }, 0, 1);
        }

        static void UploadPages(SerialPort arduinoPort, IEnumerable pages)
        {
            int pageNumber = 0;
            var statusTwiddle = @"|/-\";
            SetLeds(arduinoPort, Leds.FpgaBothLeds);
            try
            {
                foreach (Page page in pages)
                {
                    var response = TransmitBufferAndAwaitResponse(arduinoPort, Commands.UploadPage, page.Data, page.Offset, Constants.SramPageBufferSize + 4);
                    System.Console.Write("\r{0} {1:D3} {2}", statusTwiddle[pageNumber % statusTwiddle.Length], pageNumber, response.Substring(2));
                    ++pageNumber;
                }
                SetLeds(arduinoPort, Leds.FpgaGreenLed);
            }
            catch
            {
                SetLeds(arduinoPort, Leds.FpgaRedLed);
                throw;
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
                IEnumerable pageEnumerable = IsBitFile(args[0]) ?
                    (IEnumerable) new BitFilePageCollection(inputStream, Constants.UserStartAddress) :
                    (IEnumerable) new BinFilePageCollection(inputStream, Constants.UserStartAddress);
                UploadPages(arduinoPort, pageEnumerable);
            }
        }
    }
}

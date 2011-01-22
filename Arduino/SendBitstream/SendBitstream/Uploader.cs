// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Collections;
using System.IO.Ports;
using Prototype.Xilinx;

namespace SendBitstream
{
    class Uploader
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

        private static string TransmitBufferAndAwaitResponse(SerialPort arduinoPort, Commands command, byte[] buffer, int offset, int count)
        {
            byte[] header = new byte[3];
            header[0] = (byte)command;
            header[1] = (byte)(count >> 8);
            header[2] = (byte)(count & 0x0FF);
            arduinoPort.Write(header, 0, 3);
            arduinoPort.Write(buffer, offset, count);
            var response = arduinoPort.ReadLine();
            if (response.StartsWith("- "))
            {
                throw new Exception(response.Substring(2));
            }
            return response;
        }

        private static string SetLeds(SerialPort arduinoPort, Leds leds)
        {
            return TransmitBufferAndAwaitResponse(arduinoPort, Commands.SetLeds, new byte[] { (byte)leds }, 0, 1);
        }

        public static void UploadPages(SerialPort arduinoPort, IEnumerable pages)
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

    }
}

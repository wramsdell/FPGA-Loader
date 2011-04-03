// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;
using System.IO.Ports;

namespace Prototype.Xilinx.Uploader.Arduino
{
    public class ArduinoConnection
    {
        private SerialPort arduinoPort;

        public ArduinoConnection(string port, int speed)
        {
            arduinoPort = new SerialPort(port, speed);
        }

        private void Open()
        {
            arduinoPort.Open();
            arduinoPort.NewLine = "\r\n";   // Consistent with the Arduino Serial.println method which transmits \r\n
            IsOpen = true;
        }

        private string SetLeds(Leds leds)
        {
            return TransmitBufferAndAwaitResponse(Commands.SetLeds, new byte[] { (byte)leds }, 0, 1);
        }

        private string TransmitBufferAndAwaitResponse(Commands command, byte[] buffer, int offset, int count)
        {
            byte[] header = new byte[3];
            header[0] = (byte)command;
            header[1] = (byte)(count >> 8);
            header[2] = (byte)(count & 0x0FF);
            if (!IsOpen)
            {
                Open();
            }
            arduinoPort.Write(header, 0, 3);
            arduinoPort.Write(buffer, offset, count);
            var response = arduinoPort.ReadLine();
            if (response.StartsWith("- "))
            {
                throw new ArduinoConnectionException(response.Substring(2));
            }
            return response;
        }

        public void UploadPages(IEnumerable pages)
        {
            int pageNumber = 0;
            var statusTwiddle = @"|/-\";
            SetLeds(Leds.FpgaBothLeds);
            try
            {
                foreach (Page page in pages)
                {
                    var response = TransmitBufferAndAwaitResponse(Commands.UploadPage, page.Data, page.Offset, Constants.SramPageBufferSize + 4);
                    System.Console.Write("\r{0} {1:D3} {2}", statusTwiddle[pageNumber % statusTwiddle.Length], pageNumber, response.Substring(2));
                    ++pageNumber;
                }
                SetLeds(Leds.FpgaGreenLed);
            }
            catch
            {
                SetLeds(Leds.FpgaRedLed);
                throw;
            }
            System.Console.WriteLine();
        }

        public bool IsOpen { get; set; }
    }
}

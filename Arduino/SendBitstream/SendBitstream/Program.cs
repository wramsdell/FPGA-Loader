// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.IO.Ports;
using System.Threading;

namespace SendBitstream
{
    class Program
    {
        static string TransmitBufferAndAwaitResponse(SerialPort arduinoPort, byte[] buffer, int offset, int count)
        {
            arduinoPort.Write(buffer, offset, count);
            return arduinoPort.ReadLine();
        }

        static void Main(string[] args)
        {
            PrintSerialPortList();

            SerialPort arduinoPort = new SerialPort("COM3", 115200);
            arduinoPort.Open();
            arduinoPort.NewLine = "\r\n";   // Consistent with the Arduino Serial.println method which transmits \r\n

            System.Console.WriteLine("That dude said {0} so there", TransmitBufferAndAwaitResponse(arduinoPort, new byte[268], 0, 268));
        }

        private static void PrintSerialPortList()
        {
            foreach (var name in SerialPort.GetPortNames())
            {
                System.Console.WriteLine("Found port {0} -- give it a whirl.", name);
            }
        }
    }
}

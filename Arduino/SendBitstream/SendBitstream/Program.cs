// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.IO.Ports;
using System.Threading;

namespace SendBitstream
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPort arduinoPort = new SerialPort("COM3", 115200);
            arduinoPort.Open();
            arduinoPort.DataReceived += delegate(object sender, SerialDataReceivedEventArgs e) { System.Console.Write(arduinoPort.ReadExisting()); };
            // Honk out 268 bytes, which should cause some data to be received
            for (int counter = 0; counter < 268; ++counter)
            {
                arduinoPort.Write("a");
            }
            Thread.Sleep(5000);
        }
    }
}

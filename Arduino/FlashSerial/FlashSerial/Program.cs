// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using Prototype.Xilinx.Uploader.Arduino;

namespace FlashSerial
{
    class Program
    {
        static void Main(string[] args)
        {
            ArduinoConnection arduinoConnection = new ArduinoConnection("COM3", 115200);
            if (args.Length > 0)
            {
                arduinoConnection.ProgramSecurityRegisterUserFieldData(args[0]);
            }
            System.Console.WriteLine(arduinoConnection.ReadSecurityRegister());
        }
    }
}

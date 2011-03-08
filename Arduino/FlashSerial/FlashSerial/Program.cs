// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace FlashSerial
{
    class Program
    {
        private enum Commands : byte
        {
            None = 0x00,
            ReadSecurityRegister = 0x03,
            ProgramSecurityRegister = 0x04  // NOTE: This command can only be executed *once*
        }
        private const int UserFieldLength = 64;

        public class UploaderException : Exception
        {
            public UploaderException()
                : base()
            {
            }
            public UploaderException(string message)
                : base(message)
            {
            }
            public UploaderException(string message, Exception inner)
                : base(message, inner)
            {
            }
            public UploaderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context)
            {
            }
        }

        private static string TransmitBufferAndAwaitResponse(SerialPort arduinoPort, Commands command, byte[] buffer, int offset, int count)
        {
            byte[] header = new byte[3];
            header[0] = (byte)command;
            header[1] = (byte)(count >> 8);
            header[2] = (byte)(count & 0x0FF);
            arduinoPort.Write(header, 0, 3);
            if (count > 0)
            {
                arduinoPort.Write(buffer, offset, count);
            }
            var response = arduinoPort.ReadLine();
            if (response.StartsWith("- "))
            {
                throw new UploaderException(response.Substring(2));
            }
            return response;
        }

        private static void ProgramSecurityRegisterUserFieldData(SerialPort arduinoPort, string userFieldString)
        {
            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(userFieldString);
            byte[] finalBytes = new byte[UserFieldLength];
            if (utf8Bytes.Length > UserFieldLength)
            {
                throw new UploaderException(String.Format("User field length cannot exceed {0:D} bytes", UserFieldLength));
            }
            Array.Copy(utf8Bytes, finalBytes, utf8Bytes.Length);
            System.Console.Write("  ");
            HexDump(finalBytes, 0, finalBytes.Length);
            TransmitBufferAndAwaitResponse(arduinoPort, Commands.ProgramSecurityRegister, finalBytes, 0, finalBytes.Length);
        }

        private static void HexDump(byte[] bytes, int offset, int length)
        {
            for (int counter = 0; counter < length; ++counter)
            {
                System.Console.Write("{0:X2}", bytes[offset + counter]);
            }
            System.Console.WriteLine();
        }

        static void Main(string[] args)
        {
            using (SerialPort arduinoPort = new SerialPort("COM3", 115200))
            {
                arduinoPort.Open();
                arduinoPort.NewLine = "\r\n";   // Consistent with the Arduino Serial.println method which transmits \r\n

                if (args.Length > 0)
                {
                    ProgramSecurityRegisterUserFieldData(arduinoPort, args[0]);
                }
                System.Console.WriteLine(TransmitBufferAndAwaitResponse(arduinoPort, Commands.ReadSecurityRegister, null, 0, 0));
            }
        }
    }
}

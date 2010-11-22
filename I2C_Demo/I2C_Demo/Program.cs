using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace NetduinoApplication1
{
    public class Program
    {
        public static void Main()
        {
            int I2CClockRateKhz = 100;
            ushort I2CAddress = 0x3c;
            I2CDevice FPGA = new I2CDevice(new I2CDevice.Configuration(I2CAddress, I2CClockRateKhz));
            SetOE(FPGA, 0, 0xFF);
            SetData(FPGA, 0, 0xAA);
            Debug.Print(GetData(FPGA, 0).ToString());
            SetData(FPGA, 0, 0x55);
            Debug.Print(GetData(FPGA, 0).ToString());
            SetOE(FPGA, 0, 0x00);
            Debug.Print(GetData(FPGA, 0).ToString());
        }
        public static void SetOE(I2CDevice FPGA, byte Port, byte OE)
        {
            int I2CTimeout = 1000;
            byte PortAddr = (byte)(Port + (8));
            byte[] buffer = new byte[2];
            buffer[0]=PortAddr;
            buffer[1]=OE;
            var transaction = new I2CDevice.I2CTransaction[]
            {
                I2CDevice.CreateWriteTransaction(buffer)
            };
            FPGA.Execute(transaction, I2CTimeout);
        }
        public static void SetData(I2CDevice FPGA, byte Port, byte Data)
        {
            int I2CTimeout = 1000;
            byte PortAddr = Port;
            byte[] buffer = new byte[2];
            buffer[0] = PortAddr;
            buffer[1] = Data;
            var transaction = new I2CDevice.I2CTransaction[]
            {
                I2CDevice.CreateWriteTransaction(buffer)
            };
            FPGA.Execute(transaction, I2CTimeout);
        }

        public static byte GetData(I2CDevice FPGA, byte Port)
        {
            int I2CTimeout = 1000;
            byte[] buffer = new byte[1];
            var transaction = new I2CDevice.I2CTransaction[]
            {
                I2CDevice.CreateWriteTransaction(new byte[] {Port}),
                I2CDevice.CreateReadTransaction(buffer)
            };
            FPGA.Execute(transaction, I2CTimeout);
            return buffer[0];
        }

    }
}

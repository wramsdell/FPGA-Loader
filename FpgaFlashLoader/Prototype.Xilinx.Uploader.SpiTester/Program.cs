// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using Microsoft.SPOT.Hardware;
using System.Threading;

namespace Prototype.Xilinx.Uploader.SpiTester
{
    public class Program
    {
        public static void Main()
        {
            SPI.Configuration spiConfig = new SPI.Configuration(
                ShieldConfiguration.CurrentConfiguration.SpiChipSelectPin,
                false,
                100,
                100,
                false,
                true,
                1000,
                ShieldConfiguration.CurrentConfiguration.SpiModule
            );
            var spi = new SPI(spiConfig);
            var statusBuffer = new byte[2];
            var redFpgaLed = new OutputPort(ShieldConfiguration.CurrentConfiguration.RedLedPin, false);
            var greenFpgaLed = new OutputPort(ShieldConfiguration.CurrentConfiguration.GreenLedPin, false);

            // Just flash the FPGA LEDs so we know we're alive

            greenFpgaLed.Write(true);
            redFpgaLed.Write(false);
            Thread.Sleep(500);
            greenFpgaLed.Write(false);
            redFpgaLed.Write(true);
            Thread.Sleep(500);
            greenFpgaLed.Write(true);
            redFpgaLed.Write(false);
            Thread.Sleep(500);
            greenFpgaLed.Write(false);
            redFpgaLed.Write(false);
            Thread.Sleep(500);

            // So the code is fairly simple. All we do is send the Status
            // Register Read command to the FPGA ISF. If the status register
            // looks right, we light the green LED. If it doesn't, we light
            // the red LED.

            while (true)
            {
                statusBuffer[0] = 0xD7;
                spi.WriteRead(statusBuffer, statusBuffer);
                greenFpgaLed.Write(statusBuffer[1] == 0x8C);
                redFpgaLed.Write(statusBuffer[1] != 0x8C);
            }
        }
    }
}

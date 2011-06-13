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

            // Watch the LEDs on UberShield. If they are showing the bootloader
            // flashing pattern, there's no SPI connectivity. If the lights
            // alternate off / red / green / redgreen then you're quad-winning.
            // If they're off, you're not in bootloader mode.

            while (true)
            {
                statusBuffer[0] = 0x01;
                for (byte counter = 0; counter <= 3; ++counter)
                {
                    statusBuffer[1] = (byte)((counter << 2) | 0x03);
                    spi.Write(statusBuffer);
                    Thread.Sleep(500);
                }
            }
        }
    }
}

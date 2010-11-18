// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using Microsoft.SPOT.Hardware;

namespace FpgaFlashLoader
{
    class AdvancedSpi
    {
        private SPI spi;

        public AdvancedSpi(Cpu.Pin chipSelectPin, SPI.SPI_module spiModule)
        {
            SPI.Configuration spiConfig = new SPI.Configuration(
                chipSelectPin,
                false,
                100,
                100,
                false,
                true,
                1000,
                spiModule
            );
            spi = new SPI(spiConfig);
        }

        public void Write(byte[] writeBuffer)
        {
            spi.Write(writeBuffer);
        }

        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            spi.WriteRead(writeBuffer, readBuffer);
        }
    }
}

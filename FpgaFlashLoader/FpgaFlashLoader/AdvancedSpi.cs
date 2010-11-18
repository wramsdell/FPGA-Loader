// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using Microsoft.SPOT.Hardware;

namespace FpgaFlashLoader
{
    class AdvancedSpi
    {
        private SPI spi;
        private OutputPort chipSelectPort;

        public AdvancedSpi(Cpu.Pin chipSelectPin, SPI.SPI_module spiModule)
        {
            SPI.Configuration spiConfig = new SPI.Configuration(
                Cpu.Pin.GPIO_NONE,
                false,
                100,
                100,
                false,
                true,
                1000,
                spiModule
            );
            spi = new SPI(spiConfig);
            this.chipSelectPort = new OutputPort(chipSelectPin, true);
        }

        private void ChipSelectActive()
        {
            chipSelectPort.Write(false);
        }

        private void ChipSelectInactive()
        {
            chipSelectPort.Write(true);
        }

        public void Write(byte[] writeBuffer)
        {
            ChipSelectActive();
            spi.Write(writeBuffer);
            ChipSelectInactive();
        }

        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            ChipSelectActive();
            spi.WriteRead(writeBuffer, readBuffer);
            ChipSelectInactive();
        }
    }
}

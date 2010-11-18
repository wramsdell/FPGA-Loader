// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using Microsoft.SPOT.Hardware;

namespace FpgaFlashLoader
{
    class AdvancedSpi
    {
        private SPI spi;
        private OutputPort chipSelectPort;
        private byte[] readBuffer;
        private static readonly int ReadBufferSize = XilinxUploader.SramPageBufferSize;

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
            readBuffer = new byte[ReadBufferSize];
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

        internal void Write(byte[] writeBuffer1, int writeBuffer1Offset, int writeBuffer1Length, byte[] writeBuffer2, int writeBuffer2Offset, int writeBuffer2Length)
        {
            ChipSelectActive();
            spi.WriteRead(writeBuffer1, writeBuffer1Offset, writeBuffer1Length, readBuffer, 0, writeBuffer1Length, 0);
            spi.WriteRead(writeBuffer2, writeBuffer2Offset, writeBuffer2Length, readBuffer, 0, writeBuffer2Length, 0);
            ChipSelectInactive();
        }
    }
}

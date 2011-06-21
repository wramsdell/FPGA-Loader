// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using Microsoft.SPOT.Hardware;

namespace Prototype.Xilinx.Uploader
{
    class SpotSpi : ISpi
    {
        private readonly SPI _spi;

        public SpotSpi(SPI.Configuration spiConfig)
        {
            _spi = new SPI(spiConfig);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _spi.WriteRead(buffer, offset, count, null, 0, 0, 0);
        }

        public void Write(byte[] buffer)
        {
            _spi.Write(buffer);
        }

        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            _spi.WriteRead(writeBuffer, readBuffer);
        }
    }
}

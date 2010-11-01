using System;
using System.IO;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace FpgaFlashLoader
{
    public class Program
    {
        private class ReadHelper
        {
            public Stream InputStream { get; private set; }
            public int BufferSize { get; private set; }
            private byte[] readBuffer;
            private int readBufferOffset;
            private int readBufferLength;

            public ReadHelper(Stream inputStream, int bufferSize)
            {
                InputStream = inputStream;
                BufferSize = bufferSize;
                readBuffer = new byte[BufferSize];
                readBufferOffset = 0;
                readBufferLength = 0;
            }

            private int Min(int a, int b)
            {
                return (a < b) ? a : b;
            }

            private int CopyBufferBytes(byte[] buffer, int offset, int count)
            {
                int bytesToCopy = Min(count, readBufferLength - readBufferOffset);

                for (int counter = 0; counter < bytesToCopy; ++counter)
                {
                    buffer[offset + counter] = readBuffer[readBufferOffset + counter];
                }
                readBufferOffset += bytesToCopy;
                return bytesToCopy;
            }

            private int RefillBuffer()
            {
                readBufferLength = InputStream.Read(readBuffer, 0, readBuffer.Length);
                readBufferOffset = 0;
                return readBufferLength;
            }

            public int FullyRead(byte[] buffer, int offset, int count)
            {
                int bytesCopied = 0;
                while (true)
                {
                    bytesCopied += CopyBufferBytes(buffer, offset + bytesCopied, count - bytesCopied);

                    if (bytesCopied == count)
                    {
                        break;
                    }

                    if (RefillBuffer() == 0)
                    {
                        break;
                    }
                }
                return bytesCopied;
            }
        }

        // http://www.xilinx.com/support/documentation/user_guides/ug333.pdf

        enum SpiCommands : byte
        {
            PageToBuffer1Compare = 0x60,
            PageProgramThroughBuffer1 = 0x82,
            StatusRegisterRead = 0xD7,
        }

        public class SpiException : Exception
        {
            public SpiException()
                : base()
            {
            }
            public SpiException(string message)
                : base(message)
            {
            }
            public SpiException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        private static readonly int SramPageBufferSize = 264;

        public static byte WaitUntilReady(SPI spi)
        {
            var buffer = new byte[2];

            for (int counter = 0; counter < 100; ++counter)
            {
                buffer[0] = (byte) SpiCommands.StatusRegisterRead;
                spi.WriteRead(buffer, buffer);
                if ((buffer[1] & 0x80) != 0)
                {
                    return buffer[1];
                }
            }

            throw new SpiException("Timeout waiting for ISF READY status");
        }

        public delegate void PageUploadedDelegate(int startAddress, int currentAddress);

        public static void UploadImage(Stream inputStream, SPI spi, int address, PageUploadedDelegate pageUploadedDelegate)
        {
            int currentAddress = address;
            var readHelper = new ReadHelper(inputStream, 256);

            // Create a buffer the size of the SRAM area in the FPGA
            // The extra 4 bytes is for the transfer command and the address

            var readBuffer = new byte[SramPageBufferSize + 4];

            // Fill in the command

            readBuffer[0] = (byte)SpiCommands.PageProgramThroughBuffer1;

            var verifyBuffer = new byte[4];

            verifyBuffer[0] = (byte)SpiCommands.PageToBuffer1Compare;

            while (true)
            {
                // Read a page from the stream

                var bytesRead = readHelper.FullyRead(readBuffer, 4, SramPageBufferSize);
                if (bytesRead == 0)
                {
                    // All copied. You rule!

                    break;
                }

                // Put in the current address

                readBuffer[1] = (byte) (currentAddress >> 16);
                readBuffer[2] = (byte) (currentAddress >> 8);
                readBuffer[3] = (byte) (currentAddress >> 0);

                verifyBuffer[1] = (byte) (currentAddress >> 16);
                verifyBuffer[2] = (byte) (currentAddress >> 8);
                verifyBuffer[3] = (byte) (currentAddress >> 0);

                bool verifyFailed = false;
                int verifyFailedCount = 0;

                do
                {
                    // Wait until ready

                    WaitUntilReady(spi);

                    // Write it to the ISF

                    spi.Write(readBuffer);

                    // Wait until ready

                    WaitUntilReady(spi);

                    // Verify it wrote

                    spi.Write(verifyBuffer);

                    // Wait until ready, and when it is, the compare result
                    // comes back in bit 6. Set is bad.

                    verifyFailed = ((WaitUntilReady(spi) & 0x40) != 0);
                    if (verifyFailed)
                    {
                        Debug.Print("Failed to write block address " + currentAddress);
                        ++verifyFailedCount;
                        if (verifyFailedCount == 3)
                        {
                            throw new SpiException("Failed to write block address " + currentAddress);
                        }
                    }
                    else
                    {
                        pageUploadedDelegate(address, currentAddress);
                    }
                } while (verifyFailed);

                // Adjust the current address by one page

                currentAddress += 0x000200;
            }
        }

        public static void Main()
        {
            SPI.Configuration spiConfig = new SPI.Configuration(
                Pins.GPIO_PIN_D0,
                false,
                100,
                100,
                false,
                true,
                1000,
                SPI_Devices.SPI1
            );
            SPI spi = new SPI(spiConfig);

            string fpgaImagePath = @"\SD\Sample.bin";

            using (var inputStream = new FileStream(fpgaImagePath, FileMode.Open))
            {
                UploadImage(inputStream, spi, 0x020000, delegate(int startAddress, int currentAddress) { Debug.Print("Uploaded page " + currentAddress); });
            }
        }
    }
}
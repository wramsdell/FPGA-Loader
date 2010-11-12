using System;
using System.IO;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Collections;

namespace FpgaFlashLoader
{
    public class Program
    {
        public interface ISimpleReadStream
        {
            int Read(byte[] buffer, int offset, int count);
        }

        public class StreamSimpleReadStream : ISimpleReadStream
        {
            private Stream baseStream;

            public StreamSimpleReadStream(Stream baseStream)
            {
                this.baseStream = baseStream;
            }

            public int Read(byte[] buffer, int offset, int count)
            {
                return baseStream.Read(buffer, offset, count);
            }
        }

        public class MultiBinaryResourceSimpleReadStream : ISimpleReadStream
        {
            private IEnumerator resourceIds;
            private byte[] currentByteArray;
            private int currentByteArrayOffset;

            public MultiBinaryResourceSimpleReadStream(IEnumerator resourceIds)
            {
                this.resourceIds = resourceIds;
                SetUpNextByteArray();
            }

            private void SetUpNextByteArray()
            {
                currentByteArray = null;
                currentByteArray = resourceIds.MoveNext() ? Resources.GetBytes((Resources.BinaryResources)resourceIds.Current) : null;
                currentByteArrayOffset = 0;
            }

            public int Read(byte[] buffer, int offset, int count)
            {
                if (currentByteArray == null)
                {
                    return 0;
                }

                int bytesToCopy = System.Math.Min(count, currentByteArray.Length - currentByteArrayOffset);

                for (int counter = 0; counter < bytesToCopy; ++counter)
                {
                    buffer[offset + counter] = currentByteArray[currentByteArrayOffset + counter];
                }

                currentByteArrayOffset += bytesToCopy;

                if (currentByteArrayOffset >= currentByteArray.Length)
                {
                    SetUpNextByteArray();
                }

                return bytesToCopy;
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

        public static int FullyRead(ISimpleReadStream inputStream, byte[] buffer, int offset, int count)
        {
            int totalBytesRead;

            totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                var bytesRead = inputStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    // End of stream, as much data as you're gonna get
                    break;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }

        public static void UploadImage(ISimpleReadStream inputStream, SPI spi, int address)
        {
            int currentAddress = address;

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

                var bytesRead = FullyRead(inputStream, readBuffer, 4, SramPageBufferSize);
                if (bytesRead == 0)
                {
                    // All copied. You rule!

                    break;
                }
                else if (bytesRead < SramPageBufferSize)
                {
                    // Set the unused bytes to known values

                    for (int offset = 4 + bytesRead; offset < SramPageBufferSize + 4; ++offset)
                    {
                        readBuffer[offset] = 0xFF;
                    }
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
                        // Page successfully uploaded
                    }
                } while (verifyFailed);

                // Adjust the current address by one page

                currentAddress += 0x000200;
            }
        }

        public static IEnumerator GetResources()
        {
            yield return Resources.BinaryResources.Bitstream_bin1;
            yield return Resources.BinaryResources.Bitstream_bin2;
            yield return Resources.BinaryResources.Bitstream_bin3;
            yield return Resources.BinaryResources.Bitstream_bin4;
            yield return Resources.BinaryResources.Bitstream_bin5;
            yield return Resources.BinaryResources.Bitstream_bin6;
        }

        public static void Main()
        {
            OutputPort onboardLed = new OutputPort(Pins.ONBOARD_LED, false);
            OutputPort redFpgaLed = new OutputPort(Pins.GPIO_PIN_D1, false);
            OutputPort greenFpgaLed = new OutputPort(Pins.GPIO_PIN_D2, false);

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

            try
            {
                UploadImage(new MultiBinaryResourceSimpleReadStream(GetResources()), spi, 0x020000);
            }
            catch
            {
                // Red LED is lit if the image failed to upload

                redFpgaLed.Write(true);
                throw;
            }

            // Green LED is lit if the image was successfully uploaded

            greenFpgaLed.Write(true);
        }
    }
}

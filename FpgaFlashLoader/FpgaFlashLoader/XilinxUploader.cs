// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Prototype.Xilinx;

namespace FpgaFlashLoader
{
    class XilinxUploader
    {
        private SPI spi;
        private byte[] statusBuffer;
        private static readonly int MaxPageWriteRetries = 3;

        public XilinxUploader(SPI spi)
        {
            this.spi = spi;
            statusBuffer = new byte[2];
        }

        public class XilinxUploaderException : Exception
        {
            public XilinxUploaderException()
                : base()
            {
            }
            public XilinxUploaderException(string message)
                : base(message)
            {
            }
            public XilinxUploaderException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        private byte GetStatusRegister()
        {
            statusBuffer[0] = (byte)Constants.SpiCommands.StatusRegisterRead;
            spi.WriteRead(statusBuffer, statusBuffer);
            return statusBuffer[1];
        }

        private static readonly byte StatusRegisterReadyMask = 0x80;
        private static readonly byte StatusRegisterCompareMask = 0x40;
        private static readonly byte StatusRegisterMemorySizeMask = 0x3C;

        private enum IsfMemorySize : byte
        {
            Unknown = 0,
            OneMegabit = 0x0C,
        }

        private IsfMemorySize GetFpgaMemorySize()
        {
            return (IsfMemorySize)(GetStatusRegister() & StatusRegisterMemorySizeMask);
        }

        public bool IsShieldInBootloaderMode()
        {
            // If the SPI responds to the StatusRegisterRead command and
            // returns the expected memory size, we must be cool.

            return GetFpgaMemorySize() == IsfMemorySize.OneMegabit;
        }

        private byte WaitUntilReady()
        {
            for (int counter = 0; counter < 100; ++counter)
            {
                var statusRegister = GetStatusRegister();
                if ((statusRegister & StatusRegisterReadyMask) != 0)
                {
                    return statusRegister;
                }
            }

            throw new XilinxUploaderException("Timeout waiting for ISF READY status");
        }

        public void UploadBitstream(PageCollection pageCollection)
        {
            foreach (Page page in pageCollection)
            {
                bool verifyFailed = false;
                int verifyFailedCount = 0;

                do
                {
                    // Wait until ready

                    WaitUntilReady();

                    // Write it to the ISF

                    page.Data[page.Offset] = (byte)Constants.SpiCommands.PageProgramThroughBuffer1;

                    spi.WriteRead(page.Data, page.Offset, Constants.SramPageBufferSize + 4, null, 0, 0, 0);

                    // Wait until ready

                    WaitUntilReady();

                    // Verify it wrote

                    page.Data[page.Offset] = (byte)Constants.SpiCommands.PageToBuffer1Compare;

                    spi.WriteRead(page.Data, page.Offset, 4, null, 0, 0, 0);

                    // Wait until ready, and when it is, the compare result
                    // comes back in bit 6. Set is bad.

                    verifyFailed = ((WaitUntilReady() & StatusRegisterCompareMask) != 0);
                    if (verifyFailed)
                    {
                        Debug.Print("Failed to write page");
                        ++verifyFailedCount;
                        if (verifyFailedCount == MaxPageWriteRetries)
                        {
                            throw new XilinxUploaderException("Failed to write page");
                        }
                    }
                    else
                    {
                        // Page successfully uploaded
                    }
                } while (verifyFailed);
            }
        }
    }
}

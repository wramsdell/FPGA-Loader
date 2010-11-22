// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Collections;

namespace FpgaFlashLoader
{
    class XilinxUploader
    {
        private SPI spi;
        private byte[] statusBuffer;
        public static readonly int SramPageBufferSize = 264;
        private static readonly int MaxPageWriteRetries = 3;

        public XilinxUploader(SPI spi)
        {
            this.spi = spi;
            statusBuffer = new byte[2];
        }

        // http://www.xilinx.com/support/documentation/user_guides/ug333.pdf

        private enum SpiCommands : byte
        {
            PageToBuffer1Compare = 0x60,
            PageProgramThroughBuffer1 = 0x82,
            StatusRegisterRead = 0xD7,
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
            statusBuffer[0] = (byte)SpiCommands.StatusRegisterRead;
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

                    page.Data[page.Offset] = (byte)SpiCommands.PageProgramThroughBuffer1;

                    spi.WriteRead(page.Data, page.Offset, SramPageBufferSize + 4, null, 0, 0, 0);

                    // Wait until ready

                    WaitUntilReady();

                    // Verify it wrote

                    page.Data[page.Offset] = (byte)SpiCommands.PageToBuffer1Compare;

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

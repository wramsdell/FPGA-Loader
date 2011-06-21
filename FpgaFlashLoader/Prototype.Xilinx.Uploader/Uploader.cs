// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Prototype.Xilinx;
using System.Threading;

namespace Prototype.Xilinx.Uploader
{
    public class Uploader
    {
        private ISpi spi;
        private byte[] statusBuffer;
        private static readonly int MaxPageWriteRetries = 3;

        public Uploader(ISpi spi)
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
            statusBuffer[0] = (byte)SpiCommands.StatusRegisterRead;
            spi.WriteRead(statusBuffer, statusBuffer);
            return statusBuffer[1];
        }

        private IsfMemorySize GetFpgaMemorySize()
        {
            return (IsfMemorySize)(GetStatusRegister() & Constants.StatusRegisterMemorySizeMask);
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
                if ((statusRegister & Constants.StatusRegisterReadyMask) != 0)
                {
                    return statusRegister;
                }
            }

            throw new XilinxUploaderException("Timeout waiting for ISF READY status");
        }

        public byte[] SecurityRegisterRead()
        {
            var securityRegisterBuffer = new byte[4 + Constants.SecurityRegisterTotalLength];

            securityRegisterBuffer[0] = (byte) SpiCommands.SecurityRegisterRead;
            spi.WriteRead(securityRegisterBuffer, securityRegisterBuffer);

            return securityRegisterBuffer;
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

                    spi.Write(page.Data, page.Offset, Constants.SramPageBufferSize + 4);

                    // Wait until ready

                    WaitUntilReady();

                    // Verify it wrote

                    page.Data[page.Offset] = (byte)SpiCommands.PageToBuffer1Compare;

                    spi.Write(page.Data, page.Offset, 4);

                    // Wait until ready, and when it is, the compare result
                    // comes back in bit 6. Set is bad.

                    verifyFailed = ((WaitUntilReady() & Constants.StatusRegisterCompareMask) != 0);
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

        public static void Upload(PageCollection pageCollection)
        {
            OutputPort onboardLed = new OutputPort(ShieldConfiguration.CurrentConfiguration.OnboardLedPin, true);

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
            var spi = new SpotSpi(spiConfig);
            var uploader = new Uploader(spi);

            if (uploader.IsShieldInBootloaderMode())
            {
                var uploadStatusIndicator = new UploadStatusIndicator(
                    spi
                    );

                try
                {
                    //var securityRegisterData = uploader.SecurityRegisterRead();
                    //var userData = new byte[Constants.SecurityRegisterUserFieldLength];
                    //Array.Copy(securityRegisterData, 4, userData, 0, userData.Length);
                    //var userDataString = new string(System.Text.Encoding.UTF8.GetChars(userData));
                    //Debug.Print("Programming \"" + userDataString + "\"");
                    uploadStatusIndicator.Status = UploadStatusIndicator.UploadStatus.Uploading;
                    uploader.UploadBitstream(pageCollection);
                    uploadStatusIndicator.Status = UploadStatusIndicator.UploadStatus.Succeeded;
                }
                catch
                {
                    // Any exception is a failed upload

                    uploadStatusIndicator.Status = UploadStatusIndicator.UploadStatus.Failed;
                    throw;
                }
                finally
                {
                    onboardLed.Write(false);
                }
            }
            else
            {
                // Flash the onboard LED to indicate upload failure due to
                // not being in bootstrapping mode

                Thread.Sleep(500);
                onboardLed.Write(false);
                Thread.Sleep(500);
                onboardLed.Write(true);
                Thread.Sleep(500);
                onboardLed.Write(false);
            }
        }
    }
}

// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;

namespace FpgaFlashLoader
{
    public class Program
    {
        public static IEnumerator GetResources()
        {
            yield return Resources.BinaryResources.Bitstream_bin1;
            yield return Resources.BinaryResources.Bitstream_bin2;
            yield return Resources.BinaryResources.Bitstream_bin3;
            yield return Resources.BinaryResources.Bitstream_bin4;
            yield return Resources.BinaryResources.Bitstream_bin5;
            yield return Resources.BinaryResources.Bitstream_bin6;
        }

        private class UploadStatusIndicator
        {
            private OutputPort redFpgaLed;
            private OutputPort greenFpgaLed;

            public enum UploadStatus
            {
                None,
                Uploading,
                Succeeded,
                Failed
            }

            public UploadStatus Status
            {
                set
                {
                    switch (value)
                    {
                        case UploadStatus.None:
                            redFpgaLed.Write(false);
                            greenFpgaLed.Write(false);
                            break;
                        case UploadStatus.Failed:
                            redFpgaLed.Write(true);
                            greenFpgaLed.Write(false);
                            break;
                        case UploadStatus.Succeeded:
                            redFpgaLed.Write(false);
                            greenFpgaLed.Write(true);
                            break;
                        case UploadStatus.Uploading:
                            redFpgaLed.Write(true);
                            greenFpgaLed.Write(true);
                            break;
                    }
                }
            }

            public UploadStatusIndicator()
            {
                redFpgaLed = new OutputPort(Pins.GPIO_PIN_D1, false);
                greenFpgaLed = new OutputPort(Pins.GPIO_PIN_D2, false);
                Status = UploadStatus.None;
            }
        }

        public static void Main()
        {
            OutputPort onboardLed = new OutputPort(Pins.ONBOARD_LED, true);

            var spi = new AdvancedSpi(Pins.GPIO_PIN_D0, SPI_Devices.SPI1);
            var uploader = new XilinxUploader(spi);

            if (uploader.IsShieldInBootloaderMode())
            {
                var uploadStatusIndicator = new UploadStatusIndicator();

                try
                {
                    uploadStatusIndicator.Status = UploadStatusIndicator.UploadStatus.Uploading;
                    uploader.UploadBitstream(new BinaryResourceCollectionPageCollection(GetResources()), 0x020000);
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

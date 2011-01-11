// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;
using System.Threading;
using Microsoft.SPOT.Hardware;
using Prototype.Xilinx.Uploader;

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

        public static void Main()
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
                SPI.SPI_module.SPI1
            );
            var spi = new SPI(spiConfig);
            var uploader = new Uploader(spi);

            if (uploader.IsShieldInBootloaderMode())
            {
                var uploadStatusIndicator = new UploadStatusIndicator(
                    ShieldConfiguration.CurrentConfiguration.RedLedPin,
                    ShieldConfiguration.CurrentConfiguration.GreenLedPin
                    );

                try
                {
                    uploadStatusIndicator.Status = UploadStatusIndicator.UploadStatus.Uploading;
                    uploader.UploadBitstream(new BinaryResourceCollectionPageCollection(GetResources()));
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

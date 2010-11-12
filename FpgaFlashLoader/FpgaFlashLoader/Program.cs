// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

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
            private UploadStatus _status;

            public enum UploadStatus
            {
                None,
                Uploading,
                Succeeded,
                Failed
            }

            public UploadStatus Status
            {
                get { return _status; }
                set
                {
                    _status = value;
                    switch (_status)
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
            var uploadStatusIndicator = new UploadStatusIndicator();

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
                uploadStatusIndicator.Status = UploadStatusIndicator.UploadStatus.Uploading;
                XilinxUtil.UploadBitstream(new MultiBinaryResourceSimpleReadStream(GetResources()), spi, 0x020000);
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
    }
}

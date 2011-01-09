// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace FpgaFlashLoader
{
    public class Program
    {
        private static class NetduinoConstants
        {
            public const Cpu.Pin GPIO_PIN_D0 = (Cpu.Pin) 27;
            public const Cpu.Pin GPIO_PIN_D1 = (Cpu.Pin) 28;
            public const Cpu.Pin GPIO_PIN_D2 = (Cpu.Pin) 0;
            public const Cpu.Pin GPIO_PIN_D3 = (Cpu.Pin) 1;
            public const Cpu.Pin GPIO_PIN_D4 = (Cpu.Pin) 12;
            public const Cpu.Pin ONBOARD_LED = (Cpu.Pin) 55;
        }

        private static class FezConstants
        {
            public const Cpu.Pin LED = (Cpu.Pin) 4;
            public const Cpu.Pin Di4 = (Cpu.Pin) 19;
            public const Cpu.Pin Di3 = (Cpu.Pin) 31;
            public const Cpu.Pin Di2 = (Cpu.Pin) 33;
        }

        private class ShieldConfiguration
        {
            public Cpu.Pin RedLedPin { get; set;  }
            public Cpu.Pin GreenLedPin { get; set;  }
            public Cpu.Pin SpiChipSelectPin { get; set;  }
            public Cpu.Pin OnboardLedPin { get; set; }
        }

        // Bootlader v1 configuration
        private static ShieldConfiguration OldNetduinoShieldConfiguration = new ShieldConfiguration
        {
            RedLedPin = NetduinoConstants.GPIO_PIN_D1,
            GreenLedPin = NetduinoConstants.GPIO_PIN_D2,
            SpiChipSelectPin = NetduinoConstants.GPIO_PIN_D0,
            OnboardLedPin = NetduinoConstants.ONBOARD_LED
        };

        // Bootlader v2 configuration
        private static ShieldConfiguration NetduinoShieldConfiguration = new ShieldConfiguration
        {
            RedLedPin = NetduinoConstants.GPIO_PIN_D3,
            GreenLedPin = NetduinoConstants.GPIO_PIN_D4,
            SpiChipSelectPin = NetduinoConstants.GPIO_PIN_D2,
            OnboardLedPin = NetduinoConstants.ONBOARD_LED
        };

        private static ShieldConfiguration FezShieldConfiguration = new ShieldConfiguration
        {
            RedLedPin = FezConstants.Di3,
            GreenLedPin = FezConstants.Di4,
            SpiChipSelectPin = FezConstants.Di2,
            OnboardLedPin = FezConstants.LED
        };

        private static ShieldConfiguration CurrentShieldConfiguration = null;

        // SetupCurrentShieldConfiguration is a little fragile -- it uses
        // Microsoft.SPOT.Hardware.SystemInfo.OEMString to try and figure out
        // which configuration to use. If that string isn't handled, you will
        // not survive.

        private static void SetupCurrentShieldConfiguration()
        {
            var oemString = SystemInfo.OEMString;

            switch (oemString)
            {
                case "GHI Electronics, LLC":
                    CurrentShieldConfiguration = FezShieldConfiguration;
                    break;

                case "Netduino by Secret Labs LLC":
                    CurrentShieldConfiguration = NetduinoShieldConfiguration;
                    break;

                default:
                    break;
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

            public UploadStatusIndicator(Cpu.Pin redLedPin, Cpu.Pin greenLedPin)
            {
                redFpgaLed = new OutputPort(redLedPin, false);
                greenFpgaLed = new OutputPort(greenLedPin, false);
                Status = UploadStatus.None;
            }
        }

        public static void Main()
        {
            SetupCurrentShieldConfiguration();

            OutputPort onboardLed = new OutputPort(CurrentShieldConfiguration.OnboardLedPin, true);

            SPI.Configuration spiConfig = new SPI.Configuration(
                CurrentShieldConfiguration.SpiChipSelectPin,
                false,
                100,
                100,
                false,
                true,
                1000,
                SPI.SPI_module.SPI1
            );
            var spi = new SPI(spiConfig);
            var uploader = new XilinxUploader(spi);

            if (uploader.IsShieldInBootloaderMode())
            {
                var uploadStatusIndicator = new UploadStatusIndicator(CurrentShieldConfiguration.RedLedPin, CurrentShieldConfiguration.GreenLedPin);

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

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

        public static void Main()
        {
            OutputPort onboardLed = new OutputPort(Pins.ONBOARD_LED, true);
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
                XilinxUtil.UploadImage(new MultiBinaryResourceSimpleReadStream(GetResources()), spi, 0x020000);
            }
            catch
            {
                // Red LED is lit if the image failed to upload

                redFpgaLed.Write(true);
                throw;
            }
            finally
            {
                onboardLed.Write(false);
            }

            // Green LED is lit if the image was successfully uploaded

            greenFpgaLed.Write(true);
        }
    }
}

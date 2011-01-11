// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using Microsoft.SPOT.Hardware;

namespace Prototype.Xilinx.Uploader
{
    public class ShieldConfiguration
    {
        public Cpu.Pin RedLedPin { get; set;  }
        public Cpu.Pin GreenLedPin { get; set;  }
        public Cpu.Pin SpiChipSelectPin { get; set;  }
        public Cpu.Pin OnboardLedPin { get; set; }

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

        private static ShieldConfiguration shieldConfiguration;

        // SetupCurrentShieldConfiguration is a little fragile -- it uses
        // Microsoft.SPOT.Hardware.SystemInfo.OEMString to try and figure out
        // which configuration to use. If that string isn't handled, you will
        // not survive.

        public static ShieldConfiguration CurrentConfiguration
        {
            get
            {
                if (shieldConfiguration == null)
                {
                    var oemString = SystemInfo.OEMString;

                    switch (oemString)
                    {
                        case "GHI Electronics, LLC":
                            shieldConfiguration = FezShieldConfiguration;
                            break;

                        case "Netduino by Secret Labs LLC":
                            shieldConfiguration = NetduinoShieldConfiguration;
                            break;

                        default:
                            break;
                    }
                }

                return shieldConfiguration;
            }
        }
    }
}

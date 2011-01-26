// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;
using Microsoft.SPOT.Hardware;

namespace Prototype.Xilinx.Uploader
{
    public class ShieldConfiguration
    {
        public Cpu.Pin RedLedPin { get; set;  }
        public Cpu.Pin GreenLedPin { get; set;  }
        public Cpu.Pin SpiChipSelectPin { get; set;  }
        public Cpu.Pin OnboardLedPin { get; set; }

        public class ShieldConfigurationException : System.Exception
        {
            public ShieldConfigurationException()
                : base()
            {
            }

            public ShieldConfigurationException(string message)
                : base(message)
            {
            }

            public ShieldConfigurationException(string message, System.Exception innerException)
                : base(message, innerException)
            {
            }
        }

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

        private class ShieldTableEntry
        {
            public string Name { get; set; }
            public ShieldConfiguration Configuration { get; set; }
        }

        private static ArrayList shieldList = new ArrayList()
        {
            new ShieldTableEntry { Name = "GHI Electronics, LLC",   Configuration = FezShieldConfiguration },
            new ShieldTableEntry { Name = "Netduino",               Configuration = NetduinoShieldConfiguration },
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

                    foreach (ShieldTableEntry entry in shieldList)
                    {
                        if (oemString.IndexOf(entry.Name) > -1)
                        {
                            shieldConfiguration = entry.Configuration;
                        }
                    }

                    if (shieldConfiguration == null)
                    {
                        throw new ShieldConfigurationException("Unable to identify board \"" + oemString + "\"");
                    }
                }

                return shieldConfiguration;
            }
        }
    }
}

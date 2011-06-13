// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Collections;
using Microsoft.SPOT.Hardware;

namespace Prototype.Xilinx.Uploader
{
    public class ShieldConfiguration
    {
        public Cpu.Pin SpiChipSelectPin { get; set;  }
        public Cpu.Pin OnboardLedPin { get; set; }
        public SPI.SPI_module SpiModule { get; set; }

        public class ShieldConfigurationException : Exception
        {
            public ShieldConfigurationException()
            {
            }

            public ShieldConfigurationException(string message) : base(message)
            {
            }

            public ShieldConfigurationException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

// ReSharper disable InconsistentNaming
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

        private static class ChipworkXConstants
        {
            public const Cpu.Pin PC5 = (Cpu.Pin) 69;
            public const Cpu.Pin PA19 = (Cpu.Pin) 19;
            public const Cpu.Pin PA21 = (Cpu.Pin) 21;
            public const Cpu.Pin PA22 = (Cpu.Pin) 22;

        }
// ReSharper restore InconsistentNaming

        // Bootlader v1 configuration
        private static readonly ShieldConfiguration OldNetduinoShieldConfiguration = new ShieldConfiguration
        {
            SpiChipSelectPin = NetduinoConstants.GPIO_PIN_D0,
            OnboardLedPin = NetduinoConstants.ONBOARD_LED,
            SpiModule = SPI.SPI_module.SPI1
        };

        // Bootlader v2 configuration
        private static readonly ShieldConfiguration NetduinoShieldConfiguration = new ShieldConfiguration
        {
            SpiChipSelectPin = NetduinoConstants.GPIO_PIN_D2,
            OnboardLedPin = NetduinoConstants.ONBOARD_LED,
            SpiModule = SPI.SPI_module.SPI1
        };

        private static readonly ShieldConfiguration FezShieldConfiguration = new ShieldConfiguration
        {
            SpiChipSelectPin = FezConstants.Di2,
            OnboardLedPin = FezConstants.LED,
            SpiModule = SPI.SPI_module.SPI1
        };

        private static readonly ShieldConfiguration ChipworkXShieldConfiguration = new ShieldConfiguration
        {
            SpiChipSelectPin = ChipworkXConstants.PA19,
            OnboardLedPin = ChipworkXConstants.PC5,
            SpiModule = SPI.SPI_module.SPI2
        };

        private class ShieldTableEntry
        {
            public string Name { get; set; }
            public ShieldConfiguration Configuration { get; set; }
        }

        private static readonly ArrayList ShieldList = new ArrayList
        {
            new ShieldTableEntry { Name = "GHI Electronics, LLC",   Configuration = FezShieldConfiguration },
            //new ShieldTableEntry { Name = "GHI Electronics, LLC",   Configuration = ChipworkXShieldConfiguration },
            new ShieldTableEntry { Name = "Netduino",               Configuration = NetduinoShieldConfiguration },
        };

        private static ShieldConfiguration _shieldConfiguration;

        // SetupCurrentShieldConfiguration is a little fragile -- it uses
        // Microsoft.SPOT.Hardware.SystemInfo.OEMString to try and figure out
        // which configuration to use. If that string isn't handled, you will
        // not survive.

        public static ShieldConfiguration CurrentConfiguration
        {
            get
            {
                if (_shieldConfiguration == null)
                {
                    var oemString = SystemInfo.OEMString;

                    foreach (ShieldTableEntry entry in ShieldList)
                    {
                        if (oemString.IndexOf(entry.Name) > -1)
                        {
                            _shieldConfiguration = entry.Configuration;
                        }
                    }

                    if (_shieldConfiguration == null)
                    {
                        throw new ShieldConfigurationException("Unable to identify board \"" + oemString + "\"");
                    }
                }

                return _shieldConfiguration;
            }
        }
    }
}

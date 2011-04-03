// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;

namespace Prototype.Xilinx.Uploader.Arduino
{
    [Flags]
    public enum Leds : byte
    {
        None = 0x00,
        FpgaGreenLed = 0x01,
        FpgaRedLed = 0x02,
        FpgaBothLeds = FpgaGreenLed | FpgaRedLed
    }
}

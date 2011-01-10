// Copyright (C) Prototype Engineering, LLC. All rights reserved.

namespace Prototype.Xilinx
{
    public enum SpiCommands : byte
    {
        PageToBuffer1Compare = 0x60,
        PageProgramThroughBuffer1 = 0x82,
        StatusRegisterRead = 0xD7,
    }
}

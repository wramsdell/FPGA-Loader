// Copyright (C) Prototype Engineering, LLC. All rights reserved.

namespace Prototype.Xilinx
{
    public class Constants
    {
        // http://www.xilinx.com/support/documentation/user_guides/ug333.pdf

        public enum SpiCommands : byte
        {
            PageToBuffer1Compare = 0x60,
            PageProgramThroughBuffer1 = 0x82,
            StatusRegisterRead = 0xD7,
        }

        public static readonly int SramPageBufferSize = 264;
    }
}

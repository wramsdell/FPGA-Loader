// Copyright (C) Prototype Engineering, LLC. All rights reserved.

namespace Prototype.Xilinx
{
    public class Constants
    {
        // http://www.xilinx.com/support/documentation/user_guides/ug333.pdf

        public static readonly int SramPageBufferSize = 264;
        public const byte PadByte = 0xFF;
        public const int BootloaderStartAddress = 0x000000;
        public const int UserStartAddress = 0x020000;
        public const int PageIncrement = 0x000200;

        public static readonly byte StatusRegisterReadyMask = 0x80;
        public static readonly byte StatusRegisterCompareMask = 0x40;
        public static readonly byte StatusRegisterMemorySizeMask = 0x3C;
    }
}

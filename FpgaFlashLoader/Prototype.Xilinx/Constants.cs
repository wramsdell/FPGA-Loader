// Copyright (C) Prototype Engineering, LLC. All rights reserved.

namespace Prototype.Xilinx
{
    public class Constants
    {
        // http://www.xilinx.com/support/documentation/user_guides/ug333.pdf

        public const byte PadByte = 0xFF;

        public static readonly int SramPageBufferSize = 264;

        public static readonly int BootloaderStartAddress = 0x000000;
        public static readonly int UserStartAddress = 0x020000;
        public static readonly int PageIncrement = 0x000200;

        public static readonly byte StatusRegisterReadyMask = 0x80;
        public static readonly byte StatusRegisterCompareMask = 0x40;
        public static readonly byte StatusRegisterMemorySizeMask = 0x3C;

        public static readonly int SecurityRegisterUserFieldLength = 64;
        public static readonly int SecurityRegisterUniqueIdentifierLength = 64;
        public static readonly int SecurityRegisterTotalLength = SecurityRegisterUserFieldLength +
                                                                 SecurityRegisterUniqueIdentifierLength;
    }
}

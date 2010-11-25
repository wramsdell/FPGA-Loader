// Copyright (C) Prototype Engineering, LLC. All rights reserved.

namespace SendBitstream
{
    class XilinxUtil
    {
        public const int PageSize = 264;
        public const byte PadByte = 0xFF;
        public const int BootloaderStartAddress = 0x000000;
        public const int UserStartAddress = 0x020000;
        public const int PageIncrement = 0x000200;

        internal static void Encode3ByteAddress(int address, byte[] buffer, int offset)
        {
            buffer[offset + 0] = (byte)(address >> 16);
            buffer[offset + 1] = (byte)(address >> 8);
            buffer[offset + 2] = (byte)(address >> 0);
        }
    }
}

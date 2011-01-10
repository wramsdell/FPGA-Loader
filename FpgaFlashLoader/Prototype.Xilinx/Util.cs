// Copyright (C) Prototype Engineering, LLC. All rights reserved.

namespace Prototype.Xilinx
{
    public class Util
    {
        public static void Encode3ByteAddress(int address, byte[] buffer, int offset)
        {
            buffer[offset + 0] = (byte)(address >> 16);
            buffer[offset + 1] = (byte)(address >> 8);
            buffer[offset + 2] = (byte)(address >> 0);
        }
    }
}

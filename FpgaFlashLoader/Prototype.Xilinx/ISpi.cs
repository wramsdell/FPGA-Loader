// Copyright (C) Prototype Engineering, LLC. All rights reserved.

namespace Prototype.Xilinx
{
    public interface ISpi
    {
        void Write(byte[] buffer, int offset, int count);
        void Write(byte[] buffer);
        void WriteRead(byte[] writeBuffer, byte[] readBuffer);
    }
}

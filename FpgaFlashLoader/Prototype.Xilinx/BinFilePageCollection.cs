// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Collections;
using System.IO;

namespace Prototype.Xilinx
{
    public class BinFilePageCollection : PageCollection
    {
        private Stream inputStream;
        private Page page;
        private byte[] buffer;
        private int address;

        public BinFilePageCollection(Stream inputStream, int address)
        {
            this.inputStream = inputStream;
            this.buffer = new byte[Constants.SramPageBufferSize + 4];
            this.page = new Page() { Data = buffer, Offset = 0 };
            this.address = address;
        }

        private static int FullyRead(Stream inputStream, byte[] buffer, int offset, int count)
        {
            int totalBytesRead;

            totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                var bytesRead = inputStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    // End of stream, as much data as you're gonna get
                    break;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }

        public override IEnumerator GetEnumerator()
        {
            while (true)
            {
                var bytesRead = FullyRead(inputStream, buffer, 4, Constants.SramPageBufferSize);

                if (bytesRead == 0)
                {
                    break;
                }

                Util.Encode3ByteAddress(address, buffer, 1);
                address += Constants.PageIncrement;

                // Pad as required

                for (int counter = 0; counter < (Constants.SramPageBufferSize - bytesRead); ++counter)
                {
                    buffer[counter + 4 + bytesRead] = Constants.PadByte;
                }

                yield return page;
            }
        }
    }
}

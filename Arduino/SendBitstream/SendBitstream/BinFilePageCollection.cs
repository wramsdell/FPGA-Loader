// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;

namespace SendBitstream
{
    class BinFilePageCollection : IEnumerable<Page>
    {
        private Stream inputStream;
        private Page page;
        private byte[] buffer;
        private int address;

        public BinFilePageCollection(Stream inputStream, int address)
        {
            this.inputStream = inputStream;
            this.buffer = new byte[XilinxUtil.PageSize + 4];
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

        public IEnumerator<Page> GetEnumerator()
        {
            while (true)
            {
                var bytesRead = FullyRead(inputStream, buffer, 4, XilinxUtil.PageSize);

                if (bytesRead == 0)
                {
                    break;
                }

                XilinxUtil.Encode3ByteAddress(address, buffer, 1);
                System.Console.WriteLine("Address {0:x6}", address);
                address += XilinxUtil.PageIncrement;

                // Pad as required

                for (int counter = 0; counter < (XilinxUtil.PageSize - bytesRead); ++counter)
                {
                    buffer[counter + 4 + bytesRead] = XilinxUtil.PadByte;
                }

                yield return page;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}

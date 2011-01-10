// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;
using System.IO;
using Prototype.Xilinx;

namespace SendBitstream
{
    class BitFilePageCollection : PageCollection
    {
        private BinFilePageCollection delegateCollection;

        public BitFilePageCollection(Stream inputStream, int address)
        {
            Header = BitFileHeader.FromStream(inputStream);
            delegateCollection = new BinFilePageCollection(inputStream, address);
        }

        public override IEnumerator GetEnumerator()
        {
            return delegateCollection.GetEnumerator();
        }

        public BitFileHeader Header { get; private set; }
    }
}

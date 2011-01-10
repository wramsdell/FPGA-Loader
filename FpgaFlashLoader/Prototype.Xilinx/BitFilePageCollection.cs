// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;
using System.IO;

namespace Prototype.Xilinx
{
    public class BitFilePageCollection : PageCollection
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

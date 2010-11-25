// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections.Generic;
using System.IO;

namespace SendBitstream
{
    class BitFilePageCollection : IEnumerable<Page>
    {
        private BinFilePageCollection delegateCollection;

        public BitFilePageCollection(Stream inputStream, int address)
        {
            Header = BitFileHeader.FromStream(inputStream);
            delegateCollection = new BinFilePageCollection(inputStream, address);
        }

        public IEnumerator<Page> GetEnumerator()
        {
            return delegateCollection.GetEnumerator();
        }

        public BitFileHeader Header { get; private set; }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return delegateCollection.GetEnumerator();
        }
    }
}

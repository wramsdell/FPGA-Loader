// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.IO;

namespace FpgaFlashLoader
{
    public class StreamSimpleReadStream : ISimpleReadStream
    {
        private Stream baseStream;

        public StreamSimpleReadStream(Stream baseStream)
        {
            this.baseStream = baseStream;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return baseStream.Read(buffer, offset, count);
        }
    }
}

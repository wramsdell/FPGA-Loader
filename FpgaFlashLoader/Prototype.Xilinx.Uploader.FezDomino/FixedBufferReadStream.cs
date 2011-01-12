// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.IO;

namespace Prototype.Xilinx.Uploader.FezDomino
{
    // There are some stream implementations that behave poorly when given
    // random read sizes. In order to mitigate this, FixedBufferReadStream
    // will wrap an underlying stream, and only do reads of a fixed size.
    // Note that this will "over-read" from the underlying stream by as
    // much as the buffer size.
    //
    // This class does not take ownership of the underlying stream. Most
    // notably, Dispose will not do anything to it.

    class FixedBufferReadStream : Stream
    {
        private Stream stream;
        private byte[] buffer;
        private int lastBytesRead;
        private int offset;

        public FixedBufferReadStream(Stream stream, int bufferSize)
        {
            this.stream = stream;
            this.buffer = new byte[bufferSize];
            RefillBuffer();
        }

        private void RefillBuffer()
        {
            this.lastBytesRead = stream.Read(buffer, 0, buffer.Length);
            this.offset = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesToCopy = (count < (lastBytesRead - this.offset)) ? count : (lastBytesRead - this.offset);

            for (int counter = 0; counter < bytesToCopy; ++counter)
            {
                buffer[offset + counter] = this.buffer[this.offset + counter];
            }
            this.offset += bytesToCopy;

            if (this.offset == this.lastBytesRead)
            {
                RefillBuffer();
            }

            return bytesToCopy;
        }

        #region Unimplemented Stream members
        public override bool CanRead
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool CanSeek
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool CanTimeout
        {
            get
            {
                return base.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get { throw new System.NotImplementedException(); }
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override long Length
        {
            get { throw new System.NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}

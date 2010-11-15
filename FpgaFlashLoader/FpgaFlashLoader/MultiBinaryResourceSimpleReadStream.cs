// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;

namespace FpgaFlashLoader
{
    public class MultiBinaryResourceSimpleReadStream : ISimpleReadStream
    {
        private IEnumerator resourceIds;
        private byte[] currentByteArray;
        private int currentByteArrayBytesRemaining;
        private int currentByteArrayOffset;

        public MultiBinaryResourceSimpleReadStream(IEnumerator resourceIds)
        {
            this.resourceIds = resourceIds;
            SetUpNextByteArray();
        }

        private void SetUpNextByteArray()
        {
            currentByteArray = null;
            currentByteArray = resourceIds.MoveNext() ? Resources.GetBytes((Resources.BinaryResources)resourceIds.Current) : null;
            currentByteArrayBytesRemaining = (currentByteArray != null) ? currentByteArray.Length : 0;
            currentByteArrayOffset = 0;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (currentByteArray == null)
            {
                return 0;
            }

            int bytesToCopy = (count < currentByteArrayBytesRemaining) ? count : currentByteArrayBytesRemaining;

            for (int counter = 0; counter < bytesToCopy; ++counter)
            {
                buffer[offset + counter] = currentByteArray[currentByteArrayOffset + counter];
            }

            currentByteArrayOffset += bytesToCopy;
            currentByteArrayBytesRemaining -= bytesToCopy;

            if (currentByteArrayBytesRemaining == 0)
            {
                SetUpNextByteArray();
            }

            return bytesToCopy;
        }
    }
}

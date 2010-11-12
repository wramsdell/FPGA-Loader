using System.Collections;

namespace FpgaFlashLoader
{
    public class MultiBinaryResourceSimpleReadStream : ISimpleReadStream
    {
        private IEnumerator resourceIds;
        private byte[] currentByteArray;
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
            currentByteArrayOffset = 0;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (currentByteArray == null)
            {
                return 0;
            }

            int bytesToCopy = System.Math.Min(count, currentByteArray.Length - currentByteArrayOffset);

            for (int counter = 0; counter < bytesToCopy; ++counter)
            {
                buffer[offset + counter] = currentByteArray[currentByteArrayOffset + counter];
            }

            currentByteArrayOffset += bytesToCopy;

            if (currentByteArrayOffset >= currentByteArray.Length)
            {
                SetUpNextByteArray();
            }

            return bytesToCopy;
        }
    }
}

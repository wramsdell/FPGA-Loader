// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;
using Prototype.Xilinx;

namespace FpgaFlashLoader
{
    class BinaryResourceCollectionPageCollection : PageCollection
    {
        private IEnumerator resourceIds;
        private Page currentPage;

        public BinaryResourceCollectionPageCollection(IEnumerator resourceIds)
        {
            this.resourceIds = resourceIds;
            this.currentPage = new Page();
            SetUpNextByteArray();
        }

        private void SetUpNextByteArray()
        {
            currentPage.Data = null;
            currentPage.Data = resourceIds.MoveNext() ? Resources.GetBytes((Resources.BinaryResources)resourceIds.Current) : null;
            currentPage.Offset = 0;
        }

        public override IEnumerator GetEnumerator()
        {
            while (currentPage.Data != null)
            {
                yield return currentPage;
                currentPage.Offset += (Constants.SramPageBufferSize + 4);
                if (currentPage.Offset >= currentPage.Data.Length)
                {
                    SetUpNextByteArray();
                }
            }
        }
    }
}

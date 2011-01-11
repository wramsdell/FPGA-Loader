// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;
using Prototype.Xilinx.Uploader;

namespace FpgaFlashLoader
{
    public class Program
    {
        public static IEnumerator GetResources()
        {
            yield return Resources.BinaryResources.Bitstream_bin1;
            yield return Resources.BinaryResources.Bitstream_bin2;
            yield return Resources.BinaryResources.Bitstream_bin3;
            yield return Resources.BinaryResources.Bitstream_bin4;
            yield return Resources.BinaryResources.Bitstream_bin5;
            yield return Resources.BinaryResources.Bitstream_bin6;
        }

        public static void Main()
        {
            Uploader.Upload(new BinaryResourceCollectionPageCollection(GetResources()));
        }
    }
}

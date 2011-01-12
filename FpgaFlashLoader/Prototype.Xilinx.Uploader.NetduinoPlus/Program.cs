// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.IO;

namespace Prototype.Xilinx.Uploader.NetduinoPlus
{
    public class Program
    {
        public static void Main()
        {
            PageCollection finalPageCollection = null;

            foreach (var filename in Directory.GetFiles(@"\SD"))
            {
                var extension = Path.GetExtension(filename);
                var basename = Path.GetFileNameWithoutExtension(filename);
                int address = Constants.UserStartAddress;

                switch (basename.ToLower())
                {
                    case "bootloader":
                        address = Constants.BootloaderStartAddress;
                        break;

                    case "bootlader":
                        address = Constants.BootloaderStartAddress;
                        break;

                    default:
                        address = Constants.UserStartAddress;
                        break;
                }

                switch (extension.ToLower())
                {
                    case ".bit":
                        finalPageCollection = new BitFilePageCollection(
                            new FileStream(
                                filename,
                                FileMode.Open
                                ),
                            address
                            );
                        break;

                    case ".bin":
                        finalPageCollection = new BinFilePageCollection(
                            new FileStream(
                                filename,
                                FileMode.Open
                                ),
                            address
                            );
                        break;
                }

                if (finalPageCollection != null)
                {
                    break;
                }
            }

            Uploader.Upload(finalPageCollection);
        }
    }
}

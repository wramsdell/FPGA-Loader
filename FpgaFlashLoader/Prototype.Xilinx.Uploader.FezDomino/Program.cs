// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.IO;
using GHIElectronics.NETMF.IO;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;

namespace Prototype.Xilinx.Uploader.FezDomino
{
    public class Program
    {
        public static void Main()
        {
            PageCollection finalPageCollection = null;
            var persistentStorage = new PersistentStorage("SD");
            persistentStorage.MountFileSystem();

            foreach (var volumeInfo in VolumeInfo.GetVolumes())
            {
                foreach (var filename in Directory.GetFiles(volumeInfo.RootDirectory))
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
                                new FixedBufferReadStream(
                                    new FileStream(
                                        filename,
                                        FileMode.Open
                                        ),
                                    256
                                    ),
                                address
                                );
                            Debug.Print(((BitFilePageCollection)finalPageCollection).Header.FileName);
                            break;

                        case ".bin":
                            finalPageCollection = new BinFilePageCollection(
                                new FixedBufferReadStream(
                                    new FileStream(
                                        filename,
                                        FileMode.Open
                                        ),
                                    256
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
                if (finalPageCollection != null)
                {
                    break;
                }
            }

            Uploader.Upload(finalPageCollection);
        }
    }
}

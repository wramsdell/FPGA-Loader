// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;

namespace Prototype.Xilinx.Uploader
{
    public class UploadStatusIndicator
    {
        private readonly ISpi _spi;
        private readonly byte[] _spiCommand = new byte[2];

        public enum UploadStatus
        {
            None,
            Uploading,
            Succeeded,
            Failed
        }

        [Flags]
        private enum UbershieldLedControlSpiParameter : byte
        {
            None = 0x00,
            RedHostControl = 0x01,
            GreenHostControl = 0x02,
            RedOn = 0x04,
            GreenOn = 0x08
        }

        private enum UbershieldSpiCommands : byte
        {
            None = 0x00,
            LedControl = 0x01,
            BootUserImage = 0x02
        }

        public UploadStatus Status
        {
            set
            {
                _spiCommand[1] =
                    (byte) (UbershieldLedControlSpiParameter.RedHostControl | UbershieldLedControlSpiParameter.GreenHostControl);
                switch (value)
                {
                    case UploadStatus.None:
                        break;
                    case UploadStatus.Failed:
                        _spiCommand[1] |=
                            (byte)
                            (UbershieldLedControlSpiParameter.RedOn);
                        break;
                    case UploadStatus.Succeeded:
                        _spiCommand[1] |=
                            (byte)
                            (UbershieldLedControlSpiParameter.GreenOn);
                        break;
                    case UploadStatus.Uploading:
                        _spiCommand[1] |=
                            (byte)
                            (UbershieldLedControlSpiParameter.RedOn | UbershieldLedControlSpiParameter.GreenOn);
                        break;
                }
                _spi.Write(_spiCommand);
            }
        }

        public UploadStatusIndicator(ISpi spi)
        {
            _spi = spi;
            _spiCommand[0] = (byte) UbershieldSpiCommands.LedControl;
            Status = UploadStatus.None;
        }
    }
}

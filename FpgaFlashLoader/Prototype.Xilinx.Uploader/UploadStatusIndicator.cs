// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using Microsoft.SPOT.Hardware;

namespace Prototype.Xilinx.Uploader
{
    public class UploadStatusIndicator
    {
        private OutputPort redFpgaLed;
        private OutputPort greenFpgaLed;

        public enum UploadStatus
        {
            None,
            Uploading,
            Succeeded,
            Failed
        }

        public UploadStatus Status
        {
            set
            {
                switch (value)
                {
                    case UploadStatus.None:
                        redFpgaLed.Write(false);
                        greenFpgaLed.Write(false);
                        break;
                    case UploadStatus.Failed:
                        redFpgaLed.Write(true);
                        greenFpgaLed.Write(false);
                        break;
                    case UploadStatus.Succeeded:
                        redFpgaLed.Write(false);
                        greenFpgaLed.Write(true);
                        break;
                    case UploadStatus.Uploading:
                        redFpgaLed.Write(true);
                        greenFpgaLed.Write(true);
                        break;
                }
            }
        }

        public UploadStatusIndicator(Cpu.Pin redLedPin, Cpu.Pin greenLedPin)
        {
            redFpgaLed = new OutputPort(redLedPin, false);
            greenFpgaLed = new OutputPort(greenLedPin, false);
            Status = UploadStatus.None;
        }
    }
}

// Copyright (C) Prototype Engineering, LLC. All rights reserved.

namespace Prototype.Xilinx.Uploader.Arduino
{
    enum Commands : byte
    {
        None = 0x00,
        UploadPage = 0x01,
        SetLeds = 0x02,
        ReadSecurityRegister = 0x03,
        ProgramSecurityRegister = 0x04  // NOTE: This command can only be executed *once*
    }
}

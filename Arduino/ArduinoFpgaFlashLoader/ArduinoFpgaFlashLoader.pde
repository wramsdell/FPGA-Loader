// Copyright (C) Prototype Engineering, LLC. All rights reserved.

#include <SPI.h>

void setup()
{
  Serial.begin(115200);

  xilinx_init();
}

void loop()
{
  if (Serial.available() > 0)
  {
    if (block_read_data())
    {
      // Block is completed
      if (xilinx_is_in_bootloader_mode())
      {
        if (xilinx_upload_page())
        {
          status_report_success("Page uploaded");
        }
        else
        {
          status_report_failure("Page failed to upload");
        }
      }
      else
      {
        status_report_failure("Shield not in bootloader mode");
      }
      block_completed();
    }
  }
}


// Copyright (C) Prototype Engineering, LLC. All rights reserved.

#include <SPI.h>

#include "Block.h"
#include "Xilinx.h"

Block block;
Xilinx xilinx;

void setup()
{
  Serial.begin(115200);
}

void loop()
{
  if (Serial.available() > 0)
  {
    if (block.read_data())
    {
      // Block is completed
      if (xilinx.is_in_bootloader_mode())
      {
        if (xilinx.upload_page(block.get_data()))
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
      block.completed();
    }
  }
}


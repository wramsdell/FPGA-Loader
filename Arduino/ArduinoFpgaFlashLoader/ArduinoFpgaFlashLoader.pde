// Copyright (C) Prototype Engineering, LLC. All rights reserved.

#include <SPI.h>

#include "Block.h"
#include "Xilinx.h"

Block block;
Xilinx xilinx;

const byte COMMAND_UPLOAD_PAGE = 0x01;
const byte COMMAND_SET_LEDS    = 0x02;
const byte COMMAND_READ_SECURITY_REGISTER = 0x03;
const byte COMMAND_PROGRAM_SECURITY_REGISTER = 0x04;

void setup()
{
  Serial.begin(115200);
}

void loop()
{
  byte* security_register_contents;

  if (Serial.available() > 0)
  {
    if (block.read_data())
    {
      // Block is completed
      if (xilinx.is_in_bootloader_mode())
      {
        switch (block.get_command())
        {
          case COMMAND_UPLOAD_PAGE:
            if (xilinx.upload_page(block.get_data()))
            {
              status_report_success("Page uploaded");
            }
            else
            {
              status_report_failure("Page failed to upload");
            }
            break;

          case COMMAND_SET_LEDS:
            xilinx.set_leds(block.get_data()[0]);

            status_report_success("LED state changed");
            break;

          case COMMAND_READ_SECURITY_REGISTER:
            security_register_contents = xilinx.security_register_read();
            if (security_register_contents)
            {
              status_report_success_binary(security_register_contents, security_register_length);
            }
            else
            {
              status_report_failure("Failed to read security register");
            }
            break;

          case COMMAND_PROGRAM_SECURITY_REGISTER:
            if (xilinx.security_register_program(block.get_data()))
            {
              status_report_success("Program security register sent");
            }
            else
            {
              status_report_failure("Program security register failed");
            }
            break;

          default:
            status_report_failure("Unknown command");
            break;
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


// Copyright (C) Prototype Engineering, LLC. All rights reserved.

#include <SPI.h>

#include "Block.h"
#include "Xilinx.h"

Block block;
Xilinx xilinx;

const byte COMMAND_UPLOAD_PAGE = 0x01;
const byte COMMAND_SET_LEDS    = 0x02;

const byte GREEN_LED_MASK = 0b00000001;
const byte RED_LED_MASK   = 0b00000010;

const int fpga_red_led_pin   = 3;
const int fpga_green_led_pin = 4;

static bool leds_set_up = false;

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
            if (!leds_set_up)
            {
              pinMode(fpga_green_led_pin, OUTPUT);
              pinMode(fpga_red_led_pin, OUTPUT);

              leds_set_up = true;
            }

            digitalWrite(fpga_green_led_pin, ((block.get_data()[0] & GREEN_LED_MASK) != 0) ? HIGH : LOW);
            digitalWrite(fpga_red_led_pin, ((block.get_data()[0] & RED_LED_MASK) != 0) ? HIGH : LOW);

            status_report_success("LED state changed");
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


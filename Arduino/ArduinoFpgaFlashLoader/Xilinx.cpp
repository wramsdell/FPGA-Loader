// Copyright (C) Prototype Engineering, LLC. All rights reserved.

// http://www.xilinx.com/support/documentation/user_guides/ug333.pdf

#include <SPI.h>

#include "Xilinx.h"

const int xilinx_spi_chip_select_pin = 2;

const byte xilinx_spi_command_page_to_buffer_1_compare = 0x60;
const byte xilinx_spi_command_page_program_through_buffer_1 = 0x82;
const byte xilinx_spi_command_status_register_read = 0xD7;

const byte xilinx_spi_status_register_memory_size_mask = 0b00111100;
const byte xilinx_spi_status_register_compare_mask     = 0b01000000;
const byte xilinx_spi_status_register_ready_mask       = 0b10000000;

const byte xilinx_spi_status_register_memory_size_one_megabit = 0b00001100;

const int xilinx_max_verify_failed_count = 3;

Xilinx::Xilinx()
{
  SPI.begin();
  pinMode(xilinx_spi_chip_select_pin, OUTPUT);

  // This digitalWrite may not be required -- are we already HIGH?

  digitalWrite(xilinx_spi_chip_select_pin, HIGH);

  _have_queried_bootloader_mode = false;
  _bootloader_mode = false;
}

byte Xilinx::get_status_register()
{
  byte return_value;
  digitalWrite(xilinx_spi_chip_select_pin, LOW);
  SPI.transfer(xilinx_spi_command_status_register_read);
  return_value = SPI.transfer(0);
  digitalWrite(xilinx_spi_chip_select_pin, HIGH);

  return return_value;
}

bool Xilinx::wait_until_ready()
{
  byte status_register;
  for (int counter = 0;counter < 1600;++counter)
  {
    status_register = get_status_register();
    if ((status_register & xilinx_spi_status_register_ready_mask) != 0)
    {
      return true;
    }
  }

  return false;
}

byte Xilinx::get_isf_memory_size()
{
  return get_status_register() & xilinx_spi_status_register_memory_size_mask;
}

bool Xilinx::is_in_bootloader_mode()
{
  if (!_have_queried_bootloader_mode)
  {
    _have_queried_bootloader_mode = true;

    _bootloader_mode = (get_isf_memory_size() == xilinx_spi_status_register_memory_size_one_megabit);
  }

  return _bootloader_mode;
}

bool Xilinx::upload_page(byte* page_data)
{
  if (!wait_until_ready())
  {
    return false;
  }

  bool verify_failed = false;
  int verify_failed_count = 0;

  do
  {
    // Transfer the data to the SRAM register

    page_data[0] = xilinx_spi_command_page_program_through_buffer_1;

    digitalWrite(xilinx_spi_chip_select_pin, LOW);
    for (int counter = 0;counter < 268;++counter)
    {
      SPI.transfer(page_data[counter]);
    }
    digitalWrite(xilinx_spi_chip_select_pin, HIGH);

    if (!wait_until_ready())
    {
      return false;
    }

    // Verify it wrote

    page_data[0] = xilinx_spi_command_page_to_buffer_1_compare;

    digitalWrite(xilinx_spi_chip_select_pin, LOW);
    for (int counter = 0;counter < 4;++counter)
    {
      SPI.transfer(page_data[counter]);
    }
    digitalWrite(xilinx_spi_chip_select_pin, HIGH);

    if (!wait_until_ready())
    {
      return false;
    }

    verify_failed = ((get_status_register() & xilinx_spi_status_register_compare_mask) != 0);
    if (verify_failed)
    {
      verify_failed_count++;
    }
  } while ((verify_failed) && (verify_failed_count < xilinx_max_verify_failed_count));

  return verify_failed_count < xilinx_max_verify_failed_count;
}


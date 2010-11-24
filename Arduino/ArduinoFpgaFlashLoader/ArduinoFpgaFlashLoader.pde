#include <SPI.h>

// Block data and functions

const size_t block_size = 268;
static int block_bytes_read = 0;
static byte block_data[block_size];

static bool block_read_data()
{
  while ((Serial.available() > 0) && (block_bytes_read < block_size))
  {
    block_data[block_bytes_read] = Serial.read();
    ++block_bytes_read;
  }

  return block_bytes_read == block_size;
}

static void block_completed()
{
  block_bytes_read = 0;
}

// End of block data and functions

// Xilinx data and functions
// http://www.xilinx.com/support/documentation/user_guides/ug333.pdf

static bool xilinx_have_queried_bootloader_mode = false;
static bool xilinx_bootloader_mode = false;
const int xilinx_spi_chip_select_pin = 7;

const byte xilinx_spi_command_page_to_buffer_1_compare = 0x60;
const byte xilinx_spi_command_page_program_through_buffer_1 = 0x82;
const byte xilinx_spi_command_status_register_read = 0xD7;

const byte xilinx_spi_status_register_memory_size_mask = 0b00111100;
const byte xilinx_spi_status_register_compare_mask     = 0b01000000;
const byte xilinx_spi_status_register_ready_mask       = 0b10000000;

const byte xilinx_spi_status_register_memory_size_one_megabit = 0b00001100;

const int xilinx_max_verify_failed_count = 3;

static void xilinx_init()
{
  SPI.begin();
  pinMode(xilinx_spi_chip_select_pin, OUTPUT);

  // This digitalWrite may not be required -- are we already HIGH?

  digitalWrite(xilinx_spi_chip_select_pin, HIGH);
}

static byte xilinx_get_status_register()
{
  byte return_value;
  digitalWrite(xilinx_spi_chip_select_pin, LOW);
  SPI.transfer(xilinx_spi_command_status_register_read);
  return_value = SPI.transfer(0);
  digitalWrite(xilinx_spi_chip_select_pin, HIGH);
}

static bool xilinx_wait_until_ready()
{
  byte status_register;
  for (int counter = 0;counter < 100;++counter)
  {
    status_register = xilinx_get_status_register();
    if ((status_register & xilinx_spi_status_register_ready_mask) != 0)
    {
      return true;
    }
  }

  return false;
}

static byte xilinx_get_isf_memory_size()
{
  return xilinx_get_status_register() & xilinx_spi_status_register_memory_size_mask;
}

static bool xilinx_is_in_bootloader_mode()
{
  if (!xilinx_have_queried_bootloader_mode)
  {
    xilinx_have_queried_bootloader_mode = true;

    xilinx_bootloader_mode = (xilinx_get_isf_memory_size() == xilinx_spi_status_register_memory_size_one_megabit);
  }

  return xilinx_bootloader_mode;
}

static bool xilinx_upload_page()
{
  if (!xilinx_wait_until_ready())
  {
    return false;
  }

  int verify_failed_count = 0;

  do
  {
    // Transfer the data to the SRAM register

    block_data[0] = xilinx_spi_command_page_program_through_buffer_1;

    digitalWrite(xilinx_spi_chip_select_pin, LOW);
    for (int counter = 0;counter < block_size;++counter)
    {
      SPI.transfer(block_data[counter]);
    }
    digitalWrite(xilinx_spi_chip_select_pin, HIGH);

    if (!xilinx_wait_until_ready())
    {
      return false;
    }

    // Verify it wrote

    block_data[0] = xilinx_spi_command_page_to_buffer_1_compare;

    digitalWrite(xilinx_spi_chip_select_pin, LOW);
    for (int counter = 0;counter < 4;++counter)
    {
      SPI.transfer(block_data[counter]);
    }
    digitalWrite(xilinx_spi_chip_select_pin, HIGH);

    if (!xilinx_wait_until_ready())
    {
      return false;
    }

    if ((xilinx_get_status_register() & xilinx_spi_status_register_compare_mask) != 0)
    {
      verify_failed_count++;
    }
  } while (verify_failed_count < xilinx_max_verify_failed_count);

  return verify_failed_count < xilinx_max_verify_failed_count;
}

// End of Xilinx data and functions

// Start of status data and functions

static void status_report_success(char* status_string)
{
  Serial.print("+ ");
  Serial.println(status_string);
}

static void status_report_failure(char* status_string)
{
  Serial.print("- ");
  Serial.println(status_string);
}

// End of status data and functions

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


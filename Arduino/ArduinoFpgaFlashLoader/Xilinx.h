// Copyright (C) Prototype Engineering, LLC. All rights reserved.

#ifndef Xilinx_h
#define Xilinx_h

#include "WProgram.h"

const size_t security_register_user_field_length = 64;
const size_t security_register_length = security_register_user_field_length + 64;

class Xilinx
{
  public:
    Xilinx();
    bool is_in_bootloader_mode();
    bool upload_page(byte* page_data);
    byte* security_register_read();
    bool security_register_program(const byte* user_field_data);
    bool set_leds(byte led_state);

  private:
    byte get_status_register();
    bool wait_until_ready();
    byte get_isf_memory_size();
    bool xilinx_is_in_bootloader_mode();

    bool _have_queried_bootloader_mode;
    bool _bootloader_mode;

    byte _security_register_contents[security_register_length];
};

#endif /* Xilinx_h */


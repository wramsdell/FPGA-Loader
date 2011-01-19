// Copyright (C) Prototype Engineering, LLC. All rights reserved.

#ifndef Xilinx_h
#define Xilinx_h

#include "WProgram.h"

class Xilinx
{
  public:
    Xilinx();
    bool is_in_bootloader_mode();
    bool upload_page(byte* page_data);

  private:
    byte get_status_register();
    bool wait_until_ready();
    byte get_isf_memory_size();
    bool xilinx_is_in_bootloader_mode();

    bool _have_queried_bootloader_mode;
    bool _bootloader_mode;
};

#endif /* Xilinx_h */


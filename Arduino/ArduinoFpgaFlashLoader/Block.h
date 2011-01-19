// Copyright (C) Prototype Engineering, LLC. All rights reserved.

#ifndef Block_h
#define Block_h

#include "WProgram.h"

class Block
{
  public:
    Block();
    ~Block();
    bool read_data();
    void completed();
    byte* get_data();
    byte get_command();

  private:
    size_t _bytes_read;
    byte* _data;
    byte _command;
    size_t _length;
    int _state;
};

#endif /* Block_h */

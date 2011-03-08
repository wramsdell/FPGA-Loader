// Copyright (C) Prototype Engineering, LLC. All rights reserved.

#include "Block.h"

const size_t block_size = 268;

const int STATE_WAITING_FOR_COMMAND        = 0;
const int STATE_WAITING_FOR_LENGTH_BYTE_1  = 1;
const int STATE_WAITING_FOR_LENGTH_BYTE_2  = 2;
const int STATE_WAITING_FOR_MORE_DATA      = 3;
const int STATE_BLOCK_RECEIVED             = 4;

Block::Block()
{
  _data = (byte*) malloc(block_size);
  completed();
}

Block::~Block()
{
  free(_data);
  _data = NULL;
}

bool Block::read_data()
{
  while ((Serial.available() > 0) && (_state != STATE_BLOCK_RECEIVED))
  {
    switch (_state)
    {
      case STATE_WAITING_FOR_COMMAND:
        _command = Serial.read();
        _state = STATE_WAITING_FOR_LENGTH_BYTE_1;
        break;

      case STATE_WAITING_FOR_LENGTH_BYTE_1:
        _length = Serial.read();
        _state = STATE_WAITING_FOR_LENGTH_BYTE_2;
        break;

      case STATE_WAITING_FOR_LENGTH_BYTE_2:
        _length = (_length << 8) | Serial.read();
        _state = (_length > 0) ? STATE_WAITING_FOR_MORE_DATA : STATE_BLOCK_RECEIVED;
        break;

      case STATE_WAITING_FOR_MORE_DATA:
        if (_bytes_read < block_size)
        {
          _data[_bytes_read] = Serial.read();
        }
        else
        {
          // Block overrun -- this is bad
        }

        ++_bytes_read;

        if (_bytes_read == _length)
        {
          _state = STATE_BLOCK_RECEIVED;
        }
        break;

      default:
        // Spaz out, man
        break;
    }
  }

  return _state == STATE_BLOCK_RECEIVED;
}

void Block::completed()
{
  _bytes_read = 0;
  _state = STATE_WAITING_FOR_COMMAND;
}

byte* Block::get_data()
{
  return _data;
}

byte Block::get_command()
{
  return _command;
}


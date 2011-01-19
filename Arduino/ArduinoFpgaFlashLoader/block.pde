// Copyright (C) Prototype Engineering, LLC. All rights reserved.

const size_t block_size = 268;
int block_bytes_read = 0;
byte block_data[block_size];
byte block_command;
size_t block_length;

const int STATE_WAITING_FOR_COMMAND        = 0;
const int STATE_WAITING_FOR_LENGTH_BYTE_1  = 1;
const int STATE_WAITING_FOR_LENGTH_BYTE_2  = 2;
const int STATE_WAITING_FOR_MORE_DATA      = 3;
const int STATE_BLOCK_RECEIVED             = 4;

int block_state = STATE_WAITING_FOR_COMMAND;

static bool block_read_data()
{
  while ((Serial.available() > 0) && (block_state != STATE_BLOCK_RECEIVED))
  {
    switch (block_state)
    {
      case STATE_WAITING_FOR_COMMAND:
        block_command = Serial.read();
        block_state = STATE_WAITING_FOR_LENGTH_BYTE_1;
        break;

      case STATE_WAITING_FOR_LENGTH_BYTE_1:
        block_length = Serial.read();
        block_state = STATE_WAITING_FOR_LENGTH_BYTE_2;
        break;

      case STATE_WAITING_FOR_LENGTH_BYTE_2:
        block_length = (block_length << 8) | Serial.read();
        block_state = STATE_WAITING_FOR_MORE_DATA;
        break;

      case STATE_WAITING_FOR_MORE_DATA:
        if (block_bytes_read < block_size)
        {
          block_data[block_bytes_read] = Serial.read();
        }
        else
        {
          // Block overrun -- this is bad
        }

        ++block_bytes_read;

        if (block_bytes_read == block_length)
        {
          block_state = STATE_BLOCK_RECEIVED;
        }
        break;

      default:
        // Spaz out, man
        break;
    }
  }

  return block_state == STATE_BLOCK_RECEIVED;
}

static void block_completed()
{
  block_bytes_read = 0;
  block_state = STATE_WAITING_FOR_COMMAND;
}


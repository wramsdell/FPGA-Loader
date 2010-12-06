// Copyright (C) Prototype Engineering, LLC. All rights reserved.

const size_t block_size = 268;
int block_bytes_read = 0;
byte block_data[block_size];

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


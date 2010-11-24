const size_t block_size = 268;
int block_bytes_read = 0;
unsigned char block_data[block_size];

void setup()
{
  Serial.begin(115200);
}

bool read_block_data()
{
  while ((Serial.available() > 0) && (block_bytes_read < block_size))
  {
    block_data[block_bytes_read] = Serial.read();
    ++block_bytes_read;
  }

  return block_bytes_read == block_size;
}

void block_completed()
{
  block_bytes_read = 0;
}

void loop()
{
  if (Serial.available() > 0)
  {
    if (read_block_data())
    {
      // Block is completed
      Serial.println("Block completed!");
      block_completed();
    }
  }
}


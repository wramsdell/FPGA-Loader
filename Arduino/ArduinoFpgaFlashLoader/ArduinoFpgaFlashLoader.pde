// Block data and functions

const size_t block_size = 268;
int block_bytes_read = 0;
unsigned char block_data[block_size];

bool block_read_data()
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

// End of block data and functions

void setup()
{
  Serial.begin(115200);
}

void loop()
{
  if (Serial.available() > 0)
  {
    if (block_read_data())
    {
      // Block is completed
      Serial.println("+ Block completed!");
      block_completed();
    }
  }
}


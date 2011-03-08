// Copyright (C) Prototype Engineering, LLC. All rights reserved.

static byte hex_chars[] = "0123456789ABCDEF";

static void status_report_success(char* status_string)
{
  Serial.print("+ ");
  Serial.println(status_string);
}

static void status_report_success_binary(byte* response_data, size_t response_data_length)
{
  Serial.print("+ ");
  for (size_t counter = 0;counter < response_data_length;++counter)
  {
    Serial.write(hex_chars + ((response_data[counter] >> 4) & 0x0f), 1);
    Serial.write(hex_chars + ((response_data[counter] >> 0) & 0x0f), 1);
  }
  Serial.println("");
}

static void status_report_failure(char* status_string)
{
  Serial.print("- ");
  Serial.println(status_string);
}


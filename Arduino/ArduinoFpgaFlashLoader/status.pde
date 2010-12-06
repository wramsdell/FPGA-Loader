// Copyright (C) Prototype Engineering, LLC. All rights reserved.

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


// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;

namespace Prototype.Xilinx.Uploader.Arduino
{
    public class ArduinoConnectionException : Exception
    {
        public ArduinoConnectionException()
            : base()
        {
        }
        public ArduinoConnectionException(string message)
            : base(message)
        {
        }
        public ArduinoConnectionException(string message, Exception inner)
            : base(message, inner)
        {
        }
        public ArduinoConnectionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}

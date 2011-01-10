// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System;
using System.IO;
using System.Text;

namespace Prototype.Xilinx
{
    public class BitFileHeader
    {
        private static int ReadTwoByteInt(Stream inputStream)
        {
            return (inputStream.ReadByte() << 8) | (inputStream.ReadByte());
        }

        private static void StupidSkipBytes(Stream inputStream, int numberBytesToSkip)
        {
            for (int counter = 0; counter < numberBytesToSkip; ++counter)
            {
                inputStream.ReadByte();
            }
        }

        private static string ReadTwoByteLengthAndAsciiZString(Stream inputStream)
        {
            int stringLength = ReadTwoByteInt(inputStream);
            var builder = "";

            for (int counter = 0; counter < stringLength - 1; ++counter)
            {
                builder += (char)inputStream.ReadByte();
            }

            // Ditch the zero at the end

            StupidSkipBytes(inputStream, 1);

            return builder.ToString();
        }

        private static void ExpectedByte(Stream inputStream, int expected)
        {
            if (inputStream.ReadByte() != expected)
            {
                throw new IOException("Incompatible data (expected " + expected + ")");
            }
        }

        private static void ExpectedTwoByteLengthOneByteData(Stream inputStream, int expected)
        {
            if ((ReadTwoByteInt(inputStream) != 1) || (inputStream.ReadByte() != expected))
            {
                throw new IOException("Incompatible data (expected " + expected + ")");
            }
        }

        // http://www.fpga-faq.com/FAQ_Pages/0026_Tell_me_about_bit_files.htm

        private void ReadHeaderInfo(Stream inputStream)
        {
            // Read two byte length, discard data

            StupidSkipBytes(inputStream, ReadTwoByteInt(inputStream));

            // Read two byte length, discard one byte expected data 'a'

            ExpectedTwoByteLengthOneByteData(inputStream, 'a');

            // Read two byte length, discard data expected filename

            FileName = ReadTwoByteLengthAndAsciiZString(inputStream);

            // Read one byte expected field type 'b', read two byte length, discard part name

            ExpectedByte(inputStream, 'b');
            PartName = ReadTwoByteLengthAndAsciiZString(inputStream);

            // Read one byte expected field type 'c', read two byte length, discard date

            ExpectedByte(inputStream, 'c');
            Date = ReadTwoByteLengthAndAsciiZString(inputStream);

            // Read one byte expected field type 'd', read two byte length, discard time

            ExpectedByte(inputStream, 'd');
            Time = ReadTwoByteLengthAndAsciiZString(inputStream);

            // Read one byte expected field type 'e', read four byte length, four byte length is data length

            ExpectedByte(inputStream, 'e');
            StupidSkipBytes(inputStream, 4);
        }

        public static BitFileHeader FromStream(Stream inputStream)
        {
            var returnHeader = new BitFileHeader();

            returnHeader.ReadHeaderInfo(inputStream);

            return returnHeader;
        }

        public string FileName { get; private set; }

        public string PartName { get; private set; }

        public string Date { get; private set; }

        public string Time { get; private set; }
    }
}

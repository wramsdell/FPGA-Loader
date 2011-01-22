// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SendBitstream;
using System;

namespace TestSendBitstream
{
    [TestClass]
    public class TestArguments
    {
        class OneStringArgument
        {
            [Argument("a", "description")]
            public string Value { get; private set; }
        }

        [TestMethod]
        public void TestOneStringArgument()
        {
            var args = new OneStringArgument();

            Arguments.Parse(new string[] { "/a", "value" }, args);

            Assert.AreEqual("value", args.Value);
        }

        class OneIntArgument
        {
            [Argument("a", "description")]
            public int Value { get; private set; }
        }

        [TestMethod]
        public void TestOneIntArgument()
        {
            var args = new OneIntArgument();

            Arguments.Parse(new string[] { "/a", "12345" }, args);

            Assert.AreEqual(12345, args.Value);
        }

        class TwoArguments
        {
            [Argument("port", "Select the COM port to use")]
            public string Port { get; private set; }

            [Argument("speed", "Select the COM port baud rate")]
            public int Speed { get; private set; }
        }

        [TestMethod]
        public void TestTwoArguments()
        {
            var args = new TwoArguments();

            Arguments.Parse(new string[] { "/port", "COM3", "/speed", "115200" }, args);

            Assert.AreEqual("COM3", args.Port);
            Assert.AreEqual(115200, args.Speed);
        }

        [TestMethod]
        public void TestDescriptions()
        {
            var args = new TwoArguments();

            var descriptionText = Arguments.GetDescriptionText(args);

            Assert.AreEqual(@"/port Select the COM port to use
/speed Select the COM port baud rate
", descriptionText);
        }

        private static void AssertArraysAreEqual<T>(T[] expected, T[] actual)
        {
            if (expected.Length != actual.Length)
            {
                throw new AssertFailedException(String.Format("Expected length {0:d}, actual length {1:d}", expected.Length, actual.Length));
            }

            for (int counter = 0; counter < expected.Length;++counter)
            {
                if (!expected[counter].Equals(actual[counter]))
                {
                    throw new AssertFailedException(String.Format("Expected <{0:s}>, actual <{1:s}> at element {2:d}", expected[counter], actual[counter], counter));
                }
            }
        }

        [TestMethod]
        public void TestArgumentsWithStuffLeftAfterArguments()
        {
            var args = new TwoArguments();

            var remainingArgs = Arguments.Parse(new string[] { "/port", "COM3", "/speed", "115200", "bootloader.bit" }, args);

            Assert.AreEqual("COM3", args.Port);
            Assert.AreEqual(115200, args.Speed);

            AssertArraysAreEqual(new string[] { "bootloader.bit" }, remainingArgs);
        }

        class OneBoolArgument
        {
            [Argument("a", "description")]
            public bool Value { get; private set; }
        }

        [TestMethod]
        public void TestBooleanArgument()
        {
            var args = new OneBoolArgument();

            Arguments.Parse(new string[] { "/a" }, args);

            Assert.AreEqual(true, args.Value);
        }
    }
}

// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SendBitstream;

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

        //class OneIntArgumentWithDefault
        //{
        //    [Argument("a", "description", Default=115200)]
        //    public int Value { get; private set; }
        //}

        //[TestMethod]
        //public void TestOneIntArgumentWithDefault()
        //{
        //    var args = new OneIntArgument();

        //    Arguments.Parse(new string[] { }, args);

        //    Assert.AreEqual(115200, args.Value);
        //}
    }
}

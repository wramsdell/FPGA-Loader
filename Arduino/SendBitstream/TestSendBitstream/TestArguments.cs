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

// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SendBitstream;

namespace TestSendBitstream
{
    [TestClass]
    public class TestArgumentAttribute
    {
        class NameDescriptionTest
        {
            [Argument("name", "description")]
            public int Value { get; set; }
        }

        [TestMethod]
        public void TestNameAndDescription()
        {
            var attribute = (ArgumentAttribute)typeof(NameDescriptionTest).GetProperties()[0].GetCustomAttributes(typeof(ArgumentAttribute), true)[0];
            Assert.AreEqual("name", attribute.Name);
            Assert.AreEqual("description", attribute.Description);
        }

        [TestMethod]
        public void TestDefaultValueNoValue()
        {
            var attribute = (ArgumentAttribute)typeof(NameDescriptionTest).GetProperties()[0].GetCustomAttributes(typeof(ArgumentAttribute), true)[0];
            Assert.IsNull(attribute.Default);
        }

        class DefaultValueBoxedTest
        {
            [Argument("name", "description", Default=12345)]
            public int Value { get; set; }
        }

        [TestMethod]
        public void TestDefaultValueBoxedValue()
        {
            var attribute = (ArgumentAttribute) typeof(DefaultValueBoxedTest).GetProperties()[0].GetCustomAttributes(typeof(ArgumentAttribute), true)[0];

            Assert.AreEqual(12345, attribute.Default);
        }

        class DefaultValueObjectTest
        {
            [Argument("name", "description", Default = "default")]
            public string Value { get; set; }
        }

        [TestMethod]
        public void TestDefaultValueObjectValue()
        {
            var attribute = (ArgumentAttribute)typeof(DefaultValueObjectTest).GetProperties()[0].GetCustomAttributes(typeof(ArgumentAttribute), true)[0];

            Assert.AreEqual("default", attribute.Default);
        }
    }
}

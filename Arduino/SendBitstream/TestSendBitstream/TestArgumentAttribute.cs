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
    }
}

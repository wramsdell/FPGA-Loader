// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;

namespace Prototype.Xilinx
{
    public abstract class PageCollection : IEnumerable
    {
        public abstract IEnumerator GetEnumerator();
    }
}

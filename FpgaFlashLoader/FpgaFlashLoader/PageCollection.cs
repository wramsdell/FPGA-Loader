// Copyright (C) Prototype Engineering, LLC. All rights reserved.

using System.Collections;

namespace FpgaFlashLoader
{
    abstract class PageCollection : IEnumerable
    {
        public abstract IEnumerator GetEnumerator();
    }
}

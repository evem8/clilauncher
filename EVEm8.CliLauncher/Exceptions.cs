// Copyright (c) 2015 Kali Izia
// Use of this source code is governed by the MIT license that can be found in the LICENSE file.

using System;

namespace EVEm8.CliLauncher
{
    public class VersionException : Exception
    {
        public VersionException(Exception inner)
            : base(inner.Message, inner)
        {
        }
    }

    public class IndexException : Exception
    {
        public IndexException(Exception inner)
            : base(inner.Message, inner)
        {
        }
    }

    public class ResourceException : Exception
    {
        public ResourceException(Exception inner)
            : base(inner.Message, inner)
        {
        }
    }

    public class TokenException : Exception
    {
        public TokenException(Exception inner)
            : base(inner.Message, inner)
        {
        }
    }
}

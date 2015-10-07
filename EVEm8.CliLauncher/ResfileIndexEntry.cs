// Copyright (c) 2015 Kali Izia
// Use of this source code is governed by the MIT license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVEm8.CliLauncher
{
    class ResfileIndexEntry
    {
        public string fileName;
        public string cacheName;
        public string checksum;
        public int uncompressedSize;
        public int compressedSize;

        public ResfileIndexEntry(string fileName, string cacheName, string checksum, int uncompressedSize, int compressedSize)
        {
            this.fileName = fileName;
            this.cacheName = cacheName;
            this.checksum = checksum;
            this.uncompressedSize = uncompressedSize;
            this.compressedSize = compressedSize;
        }
    }
}

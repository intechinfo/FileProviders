﻿using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace Intech.FileProviders.GitFileProvider
{
    internal class FileInfoDirectory : IFileInfo
    {
        bool _exists;
        string _physicalPath;
        string _name;

        public FileInfoDirectory(string physicalPath, string name)
        {
            _physicalPath = physicalPath;
            _name = name;
        }

        public bool Exists => true;

        public long Length => -1;

        public string PhysicalPath => _physicalPath;

        public string Name => _name;

        public DateTimeOffset LastModified => DateTimeOffset.MinValue;

        public bool IsDirectory => true;

        public Stream CreateReadStream()
        {
            return null;
        }
    }
}


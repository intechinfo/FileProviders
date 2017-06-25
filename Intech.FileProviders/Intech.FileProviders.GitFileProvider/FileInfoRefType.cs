using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace Intech.FileProviders.GitFileProvider
{
    internal class FileInfoRefType : IFileInfo
    {
        string _physicalPath;
        string _name;

        public FileInfoRefType(string physicalPath, string name)
        {
            _physicalPath = physicalPath;
            _name = name;
        }

        public bool Exists => false;

        public long Length => -1;

        public string PhysicalPath => _physicalPath;

        public string Name => _name;

        public DateTimeOffset LastModified => DateTimeOffset.MinValue;

        public bool IsDirectory => true;

        public Stream CreateReadStream()
        {
            throw new NotImplementedException();
        }
    }
}


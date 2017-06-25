using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using LibGit2Sharp;

namespace Intech.FileProviders.GitFileProvider
{
    internal class FileInfoFile : IFileInfo
    {
        readonly GitFileProvider _gfp;
        readonly long _length;
        readonly string _physicalPath;
        readonly string _name;
        public FileInfoFile(GitFileProvider gfp, string physicalPath, string name, long length)
        {
            _gfp = gfp;
            _physicalPath = physicalPath;
            _name = name;
            _length = length;
        }

        public bool Exists => true;

        public long Length => _length;

        public string PhysicalPath => _physicalPath;

        public string Name => _name;

        public DateTimeOffset LastModified => DateTimeOffset.MinValue;

        public bool IsDirectory => false;

        public Stream CreateReadStream()
        {
            return _gfp.GetFileContent(PhysicalPath);
        }
    }
}

using LibGit2Sharp;
using System;

namespace Intech.FileProviders.GitFileProvider
{
    internal class RepositoryWrapper : IDisposable
    {
        readonly string _path;

        internal Repository Repo { get; private set; }
        internal int StreamWrapperCount { get; set; }

        public RepositoryWrapper(string rootPath)
        {
            _path = rootPath;
            Repo = new Repository(rootPath);
        }

        public RepositoryWrapper Open()
        {
            // This is the lock because we are totally private.
            lock (this)
            {
                if (++StreamWrapperCount == 1)
                {
                    Repo = new Repository(_path);
                }
                return this;
            }
        }

        public void Close()
        {
            lock (this)
            {
                if (--StreamWrapperCount == 0)
                {
                    Repo.Dispose();
                    Repo = null;
                }
            }
        }

        void IDisposable.Dispose() => Close();
    }
}

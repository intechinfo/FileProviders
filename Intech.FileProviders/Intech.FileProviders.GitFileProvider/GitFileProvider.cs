using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using LibGit2Sharp;
using System.Text.RegularExpressions;

enum GITKEYWORD { Unhandled, Root, Branches, Tags, Commits, Head };

namespace Intech.FileProviders.GitFileProvider
{
    public class GitFileProvider : IFileProvider
    {
        readonly string _rootPath;
        readonly bool _exist;
        readonly RepositoryWrapper _repoWrapper;
        readonly Dictionary<string, GITKEYWORD> _keywordDictionary;
        const string INVALID_PATH = "Invalid path";
        const string INVALID_BRANCH = "The branch doesn't exist in the repository";
        const string INVALID_TAG = "The tag doesn't exist in the repository";
        const string INVALID_COMMIT = "The commit doesn't exist in the repository";
        const string INVALID_REPOSITORY = "The Repository doesn't exist";
        const string INVALID_HEAD = "The repository doesn't contain head";
        const string INVALID_COMMAND = "The command doesn't exist";

        /// <summary>
        /// Initialize GitFileProvider for a specific repository
        /// </summary>
        /// <param name="rootPath">The physical path of the repository</param>
        public GitFileProvider(string rootPath)
        {
            _rootPath = rootPath;
            _exist = IsCorrectGitRepository();
            _keywordDictionary = InstantiateKeywordDictionnary();
            if (_exist) _repoWrapper = new RepositoryWrapper(rootPath);
        }


        /// <summary>
        /// Return <see cref="IDirectoryContents"/>
        /// </summary>
        /// <param name="subPath"></param>
        /// <returns></returns>
        public IDirectoryContents GetDirectoryContents(string subPath)
        {
            if (subPath == null) return NotFoundDirectoryContents.Singleton;
            if (!_exist) return NotFoundDirectoryContents.Singleton;
            string[] splitPath = PathDecomposition(subPath, out GITKEYWORD type, out char flag);
            switch (type)
            {
                case GITKEYWORD.Unhandled:
                    return GetDirectoryHead(splitPath, subPath, 0);
                case GITKEYWORD.Root:
                    return GetDirectoryRoot(subPath);
                case GITKEYWORD.Branches:
                    return GetDirectoryBranch(splitPath, subPath, flag);
                case GITKEYWORD.Tags:
                    return GetDirectoryTags(splitPath, subPath, flag);
                case GITKEYWORD.Commits:
                    return GetDirectoryCommit(splitPath, subPath, flag);
                case GITKEYWORD.Head:
                    return GetDirectoryHead(splitPath, subPath);
                default:
                    return NotFoundDirectoryContents.Singleton;
            }
        }

        public IFileInfo GetFileInfo(string subPath)
        {
            if (subPath == null)
                return new NotFoundFileInfo(INVALID_COMMAND);
            if (!_exist)
                return new NotFoundFileInfo(INVALID_REPOSITORY);
            string[] splitPath = PathDecomposition(subPath, out GITKEYWORD type, out char flag);
            switch (type)
            {
                case GITKEYWORD.Branches:
                    return GetFileBranch(splitPath, subPath, flag);
                case GITKEYWORD.Tags:
                    return GetFileTag(splitPath, subPath, flag);
                case GITKEYWORD.Commits:
                    return GetFileCommit(splitPath, subPath, flag);
                case GITKEYWORD.Head:
                    return GetFileHead(splitPath, subPath, flag);
                default:
                    return new NotFoundFileInfo(INVALID_COMMAND);
            }
        }

        private bool IsCorrectGitRepository()
        {
            string fullpath = _rootPath;
            if (!Regex.IsMatch(_rootPath, @".git\\?$"))
            {
                fullpath = fullpath + @"\.git";
            }
            return Directory.Exists(fullpath);
        }

        private Dictionary<string, GITKEYWORD> InstantiateKeywordDictionnary()
        {
            Dictionary<string, GITKEYWORD> keywordDictionary = new Dictionary<string, GITKEYWORD>();
            keywordDictionary.Add("branches", GITKEYWORD.Branches);
            keywordDictionary.Add("", GITKEYWORD.Root);
            keywordDictionary.Add("tags", GITKEYWORD.Tags);
            keywordDictionary.Add("commits", GITKEYWORD.Commits);
            keywordDictionary.Add("head", GITKEYWORD.Head);
            return keywordDictionary;
        }


        private string[] PathDecomposition(string subPath, out GITKEYWORD type, out char flag)
        {
            string[] decomposition = subPath.Split(Path.DirectorySeparatorChar);
            if (decomposition == null)
            {
                decomposition = new string[1];
                decomposition[0] = subPath.Trim();
            }
            flag = (char)0;

            if (!_keywordDictionary.TryGetValue(decomposition[0], out type))
            {
                type = GITKEYWORD.Unhandled;
            }
            if (decomposition.Length <= 1 || decomposition[1].Equals("*"))
                flag = '*';
            return decomposition;
        }

        private IDirectoryContents CreateDirectoryInfo(Tree tree, string subPath)
        {
            if (tree == null) return NotFoundDirectoryContents.Singleton;
            List<IFileInfo> files = new List<IFileInfo>();
            if (subPath == null) subPath = "";
            if (subPath != "" && !subPath.LastOrDefault().Equals(Path.DirectorySeparatorChar)) subPath = subPath + Path.DirectorySeparatorChar;
            foreach (var file in tree)
            {
                IFileInfo f = file.TargetType != TreeEntryTargetType.Blob ? new FileInfoDirectory(subPath + file.Name, file.Name) as IFileInfo
                                                          : new FileInfoFile(this, subPath + file.Name, file.Name, (file.Target as Blob).Size);
                files.Add(f);
            }
            DirectoryInfo fDir = new DirectoryInfo(files);
            return fDir;
        }

        private IDirectoryContents GetDirectoryBranch(string[] splitPath, string subPath, char flag)
        {
            if (flag == '*')
            {
                List<IFileInfo> files = new List<IFileInfo>();
                foreach (var b in _repoWrapper.Repo.Branches)
                {
                    IFileInfo file = new FileInfoDirectory("branches" + Path.DirectorySeparatorChar + b.FriendlyName, b.FriendlyName);
                    files.Add(file);
                }
                return new DirectoryInfo(files);
            }
            Branch branch = _repoWrapper.Repo.Branches.FirstOrDefault(c => c.FriendlyName == splitPath[1]);
            if (branch == null || branch.Tip == null)
                return NotFoundDirectoryContents.Singleton;
            string relativePath = GetRelativePath(splitPath);
            if (String.IsNullOrEmpty(relativePath))
                return CreateDirectoryInfo(branch.Tip.Tree, subPath);
            var dir = branch.Tip.Tree[relativePath];
            if (dir?.TargetType != TreeEntryTargetType.Tree) return NotFoundDirectoryContents.Singleton;
            return CreateDirectoryInfo(dir.Target as Tree, subPath);
        }

        private IDirectoryContents GetDirectoryRoot(string subPath)
        {
            Branch head = _repoWrapper.Repo.Head;
            if (head == null || head.Tip == null) return NotFoundDirectoryContents.Singleton;
            return CreateDirectoryInfo(head.Tip.Tree, subPath);
        }

        private IDirectoryContents GetDirectoryCommit(string[] splitPath, string subPath, char flag)
        {
            if (flag == '*')
            {
                List<IFileInfo> files = new List<IFileInfo>();
                foreach (var b in _repoWrapper.Repo.Commits)
                {
                    IFileInfo file = new FileInfoDirectory("commits" + Path.DirectorySeparatorChar + b.Message, b.Message);
                    files.Add(file);
                }
                return new DirectoryInfo(files);
            }
            Commit commit = _repoWrapper.Repo.Lookup<Commit>(splitPath[1]);
            if (commit == null) return NotFoundDirectoryContents.Singleton;
            string relativePath = GetRelativePath(splitPath);
            TreeEntry node = commit.Tree[relativePath];
            if (node?.TargetType != TreeEntryTargetType.Tree) return NotFoundDirectoryContents.Singleton;
            return CreateDirectoryInfo(node.Target as Tree, subPath);
        }

        private IDirectoryContents GetDirectoryTags(string[] splitPath, string subPath, char flag)
        {
            if (flag == '*')
            {
                List<IFileInfo> files = new List<IFileInfo>();
                foreach (var b in _repoWrapper.Repo.Tags)
                {
                    IFileInfo file = new FileInfoDirectory("tags" + Path.DirectorySeparatorChar + b.FriendlyName, b.FriendlyName);
                    files.Add(file);
                }
                return new DirectoryInfo(files);
            }
            Tag tag = _repoWrapper.Repo.Tags.FirstOrDefault(c => c.FriendlyName == splitPath[1]);
            if (tag == null) return NotFoundDirectoryContents.Singleton;
            string relativePath = GetRelativePath(splitPath);
            TreeEntry node = _repoWrapper.Repo.Lookup<Commit>(tag.Target.Id)?.Tree[relativePath];
            if (node?.TargetType != TreeEntryTargetType.Tree) return NotFoundDirectoryContents.Singleton;
            return CreateDirectoryInfo(node.Target as Tree, subPath);
        }

        private IDirectoryContents GetDirectoryHead(string[] splitPath, string subPath, int index = 1)
        {
            Branch head = _repoWrapper.Repo.Head;
            if (head == null || head.Tip == null) return NotFoundDirectoryContents.Singleton;
            string relativePath = GetRelativePath(splitPath, index);
            if (String.IsNullOrEmpty(relativePath))
                return GetDirectoryRoot(subPath);
            var dir = head.Tip.Tree[relativePath];
            if (dir?.TargetType != TreeEntryTargetType.Tree) return NotFoundDirectoryContents.Singleton;
            return CreateDirectoryInfo(dir.Target as Tree, subPath);
        }

        private IFileInfo GetFileBranch(string[] splitPath, string subPath, char flag)
        {
            if (flag == '*')
                return new FileInfoRefType(_rootPath + @"\branches", "branches");
            Branch b = _repoWrapper.Repo.Branches.ToList().FirstOrDefault(c => c.FriendlyName == splitPath[1]);
            return BranchFileManager(b,splitPath, subPath);
        }

        private IFileInfo GetFileCommit(string[] splitPath, string subPath, char flag)
        {
            if (flag == '*')
                return new FileInfoRefType(_rootPath + @"\commits", "commits");
            string commitHash = splitPath[1];
            Commit commit = _repoWrapper.Repo.Lookup<Commit>(commitHash);
            if (commit == null)
                return new NotFoundFileInfo(INVALID_COMMIT);
            string relativePath = GetRelativePath(splitPath);
            if (String.IsNullOrEmpty(relativePath))
                return new NotFoundFileInfo(INVALID_PATH);
            TreeEntry node = commit.Tree[relativePath];
            if (node == null)
                return new NotFoundFileInfo(INVALID_PATH);
            if (node.TargetType == TreeEntryTargetType.Tree)
                return new FileInfoDirectory(subPath, node.Name);
            if (node.TargetType == TreeEntryTargetType.Blob)
                return new FileInfoFile(this, subPath, node.Name, (node.Target as Blob).Size);
            return new NotFoundFileInfo(INVALID_PATH);
        }

        private IFileInfo GetFileTag(string[] splitPath, string subPath, char flag)
        {
            if (flag == '*')
                return new FileInfoRefType(_rootPath + @"\tags", "tags");
            Tag tag = _repoWrapper.Repo.Tags.FirstOrDefault(t => t.FriendlyName == splitPath[1]);
            if (tag == null || tag.Target == null) return new NotFoundFileInfo(INVALID_TAG);
            string relativePath = GetRelativePath(splitPath);
            if (relativePath == null) return new NotFoundFileInfo(INVALID_PATH);
            var commit = _repoWrapper.Repo.Lookup<Commit>(tag.Target.Sha);
            if (commit == null) return new NotFoundFileInfo(INVALID_TAG);
            var node = commit.Tree[relativePath];
            if (node == null) return new NotFoundFileInfo(INVALID_PATH);
            if (node.TargetType == TreeEntryTargetType.Tree)
                return new FileInfoDirectory(subPath, node.Name);
            if (node.TargetType == TreeEntryTargetType.Blob)
                return new FileInfoFile(this, subPath, node.Name, (node.Target as Blob).Size);
            return new NotFoundFileInfo(INVALID_PATH);
        }

        private IFileInfo GetFileHead(string[] splitPath, string subPath, char flag)
        {
            return BranchFileManager(_repoWrapper.Repo.Head, splitPath, subPath, 1);
        }

        private IFileInfo BranchFileManager(Branch branch, string[] splitPath, string subPath, int index = 2)
        {
            if (branch == null || branch.Tip == null)
                return new NotFoundFileInfo(INVALID_BRANCH);
            string relativePath = GetRelativePath(splitPath, index);
            if (String.IsNullOrEmpty(relativePath))
                return new NotFoundFileInfo(INVALID_PATH);
            TreeEntry node = branch[relativePath];
            if (node == null)
                return new NotFoundFileInfo(INVALID_PATH);
            if (node.TargetType == TreeEntryTargetType.Tree)
                return new FileInfoDirectory(subPath, node.Name);
            if (node.TargetType == TreeEntryTargetType.Blob)
                return new FileInfoFile(this, subPath, node.Name, (node.Target as Blob).Size);
            return new NotFoundFileInfo(INVALID_PATH);
        }

        private string GetRelativePath(string[] splitPath, int index = 2)
        {
            string RelativePath = "";
            for (int i = index; i < splitPath.Length; i++)
                RelativePath += splitPath[i] + Path.DirectorySeparatorChar;
            if (String.IsNullOrEmpty(RelativePath)) return RelativePath;
            return RelativePath.Substring(0, RelativePath.Length - 1).Trim();
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }

        internal Stream GetFileContent(string subPath)
        {
            using (_repoWrapper.Open())
            {
                string[] splitPath = PathDecomposition(subPath, out GITKEYWORD type, out char flag);
                string relativePath = GetRelativePath(splitPath);
                TreeEntry node;
                switch (type)
                {
                    case GITKEYWORD.Branches:
                        Branch branch = _repoWrapper.Repo.Branches.ToList().FirstOrDefault(c => c.FriendlyName == splitPath[1]);
                        node = branch[relativePath];
                        break;
                    case GITKEYWORD.Tags:
                        Tag tag = _repoWrapper.Repo.Tags.FirstOrDefault(t => t.FriendlyName == splitPath[1]);
                        Commit tagCommit = _repoWrapper.Repo.Lookup<Commit>(tag.Target.Sha);
                        node = tagCommit.Tree[relativePath];
                        break;
                    case GITKEYWORD.Commits:
                        string commitHash = splitPath[1];
                        Commit commit = _repoWrapper.Repo.Lookup<Commit>(commitHash);
                        node = commit.Tree[relativePath];
                        break;
                    case GITKEYWORD.Head:
                        node = _repoWrapper.Repo.Head[relativePath];
                        break;
                    default:
                        return null;
                }
                Stream s = (node.Target as Blob).GetContentStream();
                return new ReadStreamDecorator(s, _repoWrapper.Open());
            }
        }

    }
}

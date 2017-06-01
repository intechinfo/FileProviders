﻿using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;

namespace Intech.FileProviders.GitFileProvider.Tests
{
    [TestFixture]
    public class GitFileProviderTest
    {
        private string ProjectRootPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.Parent.FullName;
        const string INVALID_PATH = "Invalid path";
        const string INVALID_BRANCH = "The branch doesn't exist in the repository";
        const string INVALID_TAG = "The tag doesn't exist in the repository";
        const string INVALID_COMMIT = "The commit doesn't exist in the repository";
        const string INVALID_REPOSITORY = "The Repository doesn't exist";
        const string INVALID_HEAD = "The repository doesn't contain head";
        const string INVALID_COMMAND = "The command doesn't exist";

        [Test]
        public void Create_GitFileProvider_And_Tests_Correct_And_Incorrect_Git_Repository_Paths()
        {
            bool exist;

            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            exist = (bool)typeof(GitFileProvider).GetField("_exist", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(git);
            exist.Should().Be(true);

            git = new GitFileProvider(ProjectRootPath + @"\");
            exist = (bool)typeof(GitFileProvider).GetField("_exist", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(git);
            exist.Should().Be(true);

            GitFileProvider git1 = new GitFileProvider(ProjectRootPath + @"\.git");
            exist = (bool)typeof(GitFileProvider).GetField("_exist", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(git1);
            exist.Should().Be(true);

            GitFileProvider git2 = new GitFileProvider(ProjectRootPath + @"\.git\");
            exist = (bool)typeof(GitFileProvider).GetField("_exist", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(git2);
            exist.Should().Be(true);

            GitFileProvider git3 = new GitFileProvider(ProjectRootPath + @"\Wrong\Path\");
            exist = (bool)typeof(GitFileProvider).GetField("_exist", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(git3);
            exist.Should().Be(false);

            GitFileProvider git4 = new GitFileProvider(ProjectRootPath + @"\Wrong\Path\.git");
            exist = (bool)typeof(GitFileProvider).GetField("_exist", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(git4);
            exist.Should().Be(false);
        }

        [Test]
        public void FileInfo_Of_Null_String_Should_Return_Default_FileInfo_With_Name_Invalid()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo file = git.GetFileInfo(null);
            file.Should().NotBeNull();
            file.Name.Should().Be(INVALID_COMMAND);
            file.PhysicalPath.Should().Be(null);
            file.Length.Should().Be(-1);
            file.Exists.Should().Be(false);
        }

        [Test]
        public void FileInfo_Of_Invalid_String_Should_Return_Default_FileInfo_With_Name_Invalid()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo file = git.GetFileInfo("IdontExist");
            file.Should().NotBeNull();
            file.Name.Should().Be(INVALID_COMMAND);
            file.PhysicalPath.Should().Be(null);
            file.Length.Should().Be(-1);
            file.Exists.Should().Be(false);
        }

        [Test]
        public void FileInfo_Of_Root_Should_Return_FileInfoRefType_With_Name_Root_And_PhysicalPath_Equals_ProjectRootPath()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo rootInfo = git.GetFileInfo("");

            rootInfo.Exists.Should().BeFalse();
            rootInfo.Name.Should().Be(INVALID_COMMAND);
            rootInfo.Length.Should().Be(-1);
            rootInfo.LastModified.Should().Be(default(DateTimeOffset));
        }

        [Test]
        public void FileInfo_Of_Branches()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo branchesFile = git.GetFileInfo("branches");

            branchesFile.Exists.Should().BeFalse();
            branchesFile.Name.Should().Be("branches");
            branchesFile.PhysicalPath.Should().Be(ProjectRootPath + @"\branches");
            branchesFile.Length.Should().Be(-1);
            branchesFile.LastModified.Should().Be(default(DateTimeOffset));
        }

        [Test]
        public void FileInfo_Of_not_existing_branch()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo branchesFile = git.GetFileInfo(@"branches\xxx");

            branchesFile.Exists.Should().BeFalse();
            branchesFile.Length.Should().Be(-1);
        }

        [Test]
        public void FileInfo_Of_Head_Exist_File()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo fileInBranch = git.GetFileInfo(@"head\Intech.FileProviders\Intech.FileProviders.GitFileProvider\GitFileProvider.cs");

            fileInBranch.Exists.Should().BeTrue();
            fileInBranch.Name.Should().Be("GitFileProvider.cs");
            fileInBranch.Length.Should().BeGreaterThan(-1);
            fileInBranch.LastModified.Should().BeAfter(default(DateTimeOffset));
        }

        [Test]
        public void FileInfo_Of_Head_Doesnt_Exist_File()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo branchesFile = git.GetFileInfo(@"head\GoM.GitFileProvider\GitFileProvider.cs");

            branchesFile.Exists.Should().BeFalse();
            branchesFile.Name.Should().Be(INVALID_PATH);

        }

        [Test]
        public void FileInfo_Of_Not_Existing_File_In_Branch()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo branchesFile = git.GetFileInfo(@"branches\origin/dev-Guillaume\GoM.GitFileProvider\appsd.config");

            branchesFile.Exists.Should().BeFalse();
            branchesFile.Name.Should().Be(INVALID_PATH);
        }

        [Test]
        public void FileInfo_Of_A_Directory()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo branchesFile = git.GetFileInfo(@"branches\origin/dev-Guillaume\Intech.FileProviders\Intech.FileProviders.GitFileProvider");

            branchesFile.Exists.Should().BeTrue();
            branchesFile.IsDirectory.Should().BeTrue();
        }

        [Test]
        public void FileInfo_Of_Unexisting_Branch()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo namedBranch = git.GetFileInfo(@"branches\origin/perso_yazman");

            namedBranch.Exists.Should().BeFalse();
            namedBranch.Name.Should().Be(INVALID_BRANCH);
            namedBranch.Length.Should().Be(-1);
        }

        [Test]
        public void FileInfo_Of_Unexisting_Branch_WIth_No_Path()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo namedBranch = git.GetFileInfo(@"branches\origin/dev-Guillaume\");

            namedBranch.Exists.Should().BeFalse();
            namedBranch.Name.Should().Be(INVALID_PATH);
            namedBranch.Length.Should().Be(-1);
        }

        [Test]
        public void FileInfo_Of_File_In_Specific_Branch()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo fileInBranch = git.GetFileInfo(@"branches\origin/dev-Guillaume\Intech.FileProviders\Intech.FileProviders.GitFileProvider\GitFileProvider.cs");

            fileInBranch.Exists.Should().BeTrue();
            fileInBranch.Name.Should().Be("GitFileProvider.cs");
            fileInBranch.PhysicalPath.Should().Be(@"Intech.FileProviders\Intech.FileProviders.GitFileProvider\GitFileProvider.cs");
            fileInBranch.Length.Should().BeGreaterThan(-1);
            fileInBranch.LastModified.Should().BeAfter(default(DateTimeOffset));
        }

        [Test]
        public void FileInfo_Read_Commits()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo fileInfo = git.GetFileInfo("commits");
            fileInfo.Name.Should().Be("commits");
        }

        [Test]
        public void FileInfo_Read_Commits_With_Specific_Commit_Hash_Wrong_Hash()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo fileInfo = git.GetFileInfo(@"commits\WrongHash");
            fileInfo.Name.Should().Be(INVALID_COMMIT);
        }

        [Test]
        public void FileInfo_Read_Commits_With_Specific_Commit_Hash_Right_Hash()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo fileInfo = git.GetFileInfo(@"commits\24c1e7f2a9ed9ca14f107fd295c91da0a592e95a\");
            fileInfo.Name.Should().Be(INVALID_PATH);
        }

        [Test]
        public void FileInfo_Read_Commits_With_Specific_Hash_And_Relative_Path_From_Hash()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo fileInfo = git.GetFileInfo(@"commits\24c1e7f2a9ed9ca14f107fd295c91da0a592e95a\LICENSE");
            fileInfo.Name.Should().Be("LICENSE");
            fileInfo.PhysicalPath.Should().Be(@"LICENSE");
            fileInfo.Length.Should().BeGreaterOrEqualTo(0);
        }

        [Test]
        public void FileInfo_Read_Tags_With_Specific_Tag_Name_And_Relative_Path_From_Tag()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo fileInfo = git.GetFileInfo(@"tags\FirstCommit\LICENSE");
            fileInfo.PhysicalPath.Should().Be(@"LICENSE");
            fileInfo.Name.Should().Be("LICENSE");
            fileInfo.Length.Should().BeGreaterOrEqualTo(0);
        }

        [Test]
        public void FileInfo_Read_Tags_With_Specific_Tag_Name_And_Relative_Path_From_Tag_With_Wrong_Tag()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo fileInfo = git.GetFileInfo(@"tags\GitWatchers\");
            fileInfo.Name.Should().Be(INVALID_TAG);
        }

        [Test]
        public void FileInfo_Read_Tags_With_Specific_Tag_Name_And_Relative_Path_From_Tag_With_Wrong_Path()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo fileInfo = git.GetFileInfo(@"tags\GitWatchers\");
            fileInfo.Name.Should().Be(INVALID_TAG);
            fileInfo = git.GetFileInfo(@"tags\GitWatchers\GoM.GitFileProvider\GitFileProvider.cs");
            fileInfo.Name.Should().Be(INVALID_TAG);
        }

        [Test]
        public void FileInfo_Read_Different_File_In_Same_Branches_Should_Not_Be_Equal()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo fileInBranchMPT = git.GetFileInfo(@"branches\origin/dev-Guillaume\Intech.FileProviders\Intech.FileProviders.GitFileProvider\GitFileProvider.cs");
            Stream stream1 = fileInBranchMPT.CreateReadStream();

            byte[] buffer1 = new byte[1024];
            stream1.Read(buffer1, 0, 1024);


            IFileInfo fileInBranchGuillaume = git.GetFileInfo(@"branches\origin/dev-Guillaume\Intech.FileProviders\Intech.FileProviders.GitFileProvider\DirectoryInfo.cs");
            Stream stream2 = fileInBranchGuillaume.CreateReadStream();
            byte[] buffer2 = new byte[1024];
            stream2.Read(buffer2, 0, 1024);
            buffer1.Should().NotBeEquivalentTo(buffer2);
            stream1.Dispose();
            stream2.Dispose();
            buffer1 = null;
            buffer2 = null;
        }

        [Test]
        public void FileInfo_Read_Same_File_In_Commit_Or_Tag_Should_Be_Equal()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IFileInfo fileInCommit = git.GetFileInfo(@"commits\9b3bd5db5082c0d4cc41b1a480df897c049ac70b\Intech.FileProviders\Intech.FileProviders.GitFileProvider\GitFileProvider.cs");
            Stream stream1 = fileInCommit.CreateReadStream();

            byte[] buffer1 = new byte[1024];
            stream1.Read(buffer1, 0, 1024);


            IFileInfo fileInTags = git.GetFileInfo(@"tags\FirstCommit\Intech.FileProviders\Intech.FileProviders.GitFileProvider\GitFileProvider.cs");
            Stream stream2 = fileInTags.CreateReadStream();
            byte[] buffer2 = new byte[1024];
            stream2.Read(buffer2, 0, 1024);
            buffer1.Should().BeEquivalentTo(buffer2);
            stream1.Dispose();
            stream2.Dispose();
            buffer1 = null;
            buffer2 = null;
        }

        [Test]
        public void Watch_With_Invalid_File()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            IChangeToken token = git.Watch("test");
            token.Should().Be(NullChangeToken.Singleton);
        }
    }
}
using FluentAssertions;
using NUnit.Framework;
using System.IO;

namespace Intech.FileProviders.GitFileProvider.Tests
{
    [TestFixture]
    public class GitFilesProvierDirectory
    {
        private string ProjectRootPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.Parent.FullName;

        [Test]
        public void Get_directory_with_no_parameters()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var rootDir = git.GetDirectoryContents("");

            rootDir.Exists.Should().BeTrue();
            foreach (var item in rootDir)
            {
                item.Exists.Should().BeTrue();
                item.PhysicalPath.Should().Be(item.Name);
            }
        }
        [Test]
        public void Get_directory_with_no_parameters_and_path()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var rootDir = git.GetDirectoryContents(@"Intech.FileProviders\Intech.FileProviders.GitFileProvider");

            rootDir.Exists.Should().BeTrue();
            foreach (var item in rootDir)
            {
                item.Exists.Should().BeTrue();
                item.PhysicalPath.Should().Be(@"Intech.FileProviders\Intech.FileProviders.GitFileProvider\" + item.Name);
            }
        }
        [Test]
        public void Get_directory_with_no_existing_path()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var rootDir = git.GetDirectoryContents(@"Intech\Intech.FileProviders.GitFileProvider");

            rootDir.Exists.Should().BeFalse();
        }
        [Test]
        public void Get_directory_with_branch_parram()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var rootDir = git.GetDirectoryContents(@"branches\dev-Guillaume\Intech.FileProviders\Intech.FileProviders.GitFileProvider");

            rootDir.Exists.Should().BeTrue();
            foreach (var item in rootDir)
            {
                item.Exists.Should().BeTrue();
                item.PhysicalPath.Should().Be(@"branches\dev-Guillaume\Intech.FileProviders\Intech.FileProviders.GitFileProvider" + Path.DirectorySeparatorChar + item.Name);
            }
        }
        [Test]
        public void Get_directory_with_branch_unexisting_parram()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var rootDir = git.GetDirectoryContents(@"branches\dev-Guillaume\Intech\Intech.FileProviders.GitFileProvider");

            rootDir.Exists.Should().BeFalse();
        }
        [Test]
        public void Get_directory_with_head_parram()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var headDir = git.GetDirectoryContents(@"head\Intech.FileProviders\Intech.FileProviders.GitFileProvider");
            headDir.Exists.Should().BeTrue();
            foreach (var item in headDir)
            {
                item.Exists.Should().BeTrue();
                item.PhysicalPath.Should().Be(@"head\Intech.FileProviders\Intech.FileProviders.GitFileProvider" + Path.DirectorySeparatorChar + item.Name);
            }
        }
        [Test]
        public void Get_directory_with_head_bad_parram()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var headDir = git.GetDirectoryContents(@"head\Intech\Intech.FileProviders.GitFileProvider");
            headDir.Exists.Should().BeFalse();
        }
        [Test]
        public void Get_directory_with_commit_parram()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var rootDir = git.GetDirectoryContents(@"commits\9b3bd5db5082c0d4cc41b1a480df897c049ac70b\Intech.FileProviders\Intech.FileProviders.GitFileProvider");
            rootDir.Exists.Should().BeTrue();
            foreach (var item in rootDir)
            {
                item.Exists.Should().BeTrue();
                item.PhysicalPath.Should().Be(@"commits\9b3bd5db5082c0d4cc41b1a480df897c049ac70b\Intech.FileProviders\Intech.FileProviders.GitFileProvider\" + item.Name);

            }
        }
        [Test]
        public void Get_directory_with_commit_bad_parram()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var rootDir = git.GetDirectoryContents(@"commits\9b3bd5db5082c0d4cc41b1a480df897c049ac70b\Intech\Intech.FileProviders.GitFileProvider");
            rootDir.Exists.Should().BeFalse();
        }
        [Test]
        public void Get_directory_with_tag_parram()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var rootDir = git.GetDirectoryContents(@"tags\FirstCommit\Intech.FileProviders\Intech.FileProviders.GitFileProvider");
            rootDir.Exists.Should().BeTrue();
            foreach (var item in rootDir)
            {
                item.Exists.Should().BeTrue();
                item.PhysicalPath.Should().Be(@"tags\FirstCommit\Intech.FileProviders\Intech.FileProviders.GitFileProvider\" + item.Name);

            }
        }
        [Test]
        public void Get_directory_with_tag_bad_parram()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var rootDir = git.GetDirectoryContents(@"tags\FirstCommit\Intech\Intech.FileProviders.GitFileProvider");
            rootDir.Exists.Should().BeFalse();
        }
        [Test]
        public void Get_directory_with_null_parameters()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var rootDir = git.GetDirectoryContents(null);

            rootDir.Exists.Should().BeFalse();
        }

        [Test]
        public void GlobingPatern()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var rootDir = git.GetDirectoryContents(@"branches\dev-Guillaume\*");
            var file = git.GetFileInfo(@"branches\dev-Guillaume\*");

            rootDir.Exists.Should().BeFalse();
            file.Exists.Should().BeFalse();
        }

        [Test]
        public void Get_all_branches_of_the_repository()
        {
            GitFileProvider git = new GitFileProvider(ProjectRootPath);
            var rootDir = git.GetDirectoryContents(@"branches");

            rootDir.Exists.Should().BeTrue();
            foreach (var item in rootDir)
            {
                var branchDir = git.GetDirectoryContents(item.PhysicalPath + @"Intech.FileProviders.GitFileProvider");
                foreach (var dir in branchDir)
                {
                    dir.Exists.Should().BeTrue();
                    if (dir.IsDirectory)
                    {
                        dir.PhysicalPath.Should().Be(ProjectRootPath + Path.DirectorySeparatorChar + item.Name + Path.DirectorySeparatorChar);

                    }
                    else if (dir.Name.Contains("cs"))
                    {
                        dir.Length.Should().BeGreaterThan(0);
                    }
                    else
                        dir.PhysicalPath.Should().Be(ProjectRootPath + Path.DirectorySeparatorChar + item.Name);
                }
            }
        }


    }
}


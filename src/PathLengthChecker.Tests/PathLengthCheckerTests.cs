using System.Linq;
using FluentAssertions;
using System;
using System.IO;
using System.Collections.Generic;
using Xunit;
using System.Threading;

namespace PathLengthChecker.Tests
{
	public class FilesFixtureForClassSetupAndTeardown : IDisposable
	{
		public List<PathInfo> Directories { get; }
		public List<PathInfo> Files { get; }
		public List<PathInfo> AllPaths { get; }
		public string RootPath { get; }
		public string EmptyDirectoryPath { get; }

		public FilesFixtureForClassSetupAndTeardown()
		{
			RootPath = Path.Combine(Environment.CurrentDirectory, "UnitTestTemp");
			EmptyDirectoryPath = Path.Combine(RootPath, "EmptyDir");

			Directories = new List<PathInfo>
			{
				new PathInfo(){ Path = Path.Combine(RootPath, "TestDir1") },
				new PathInfo(){ Path = Path.Combine(RootPath, "TestDir2") },
				new PathInfo(){ Path = Path.Combine(RootPath, "TestDir2", "TestDir3") },
				new PathInfo(){ Path = EmptyDirectoryPath }
			};

			Files = new List<PathInfo>
			{
				new PathInfo(){ Path = Path.Combine(RootPath, "TestFile0.test") },
				new PathInfo(){ Path = Path.Combine(RootPath, "TestDir1", "TestFile1.test") },
				new PathInfo(){ Path = Path.Combine(RootPath, "TestDir2", "TestFile2.test") },
				new PathInfo(){ Path = Path.Combine(RootPath, "TestDir2", "TestDir3", "TestFile3.test") }
			};

			AllPaths = new List<PathInfo>(Directories);
			AllPaths.AddRange(Files);

			CreateDirectoriesAndFiles();
		}

		public void Dispose()
		{
			DeleteDirectoriesAndFiles();
		}

		private void CreateDirectoriesAndFiles()
		{
			foreach (var directory in Directories)
			{
				if (!Directory.Exists(directory.Path))
					Directory.CreateDirectory(directory.Path);
			}

			foreach (var file in Files)
			{
				if (!File.Exists(file.Path))
				{
					using (File.Create(file.Path)) { }
				}
			}
		}

		private void DeleteDirectoriesAndFiles()
		{
			if (Directory.Exists(RootPath))
			{
				try
				{
					Directory.Delete(RootPath, true);
				}
				catch
				{
					// best effort cleanup
				}
			}
		}
	}

	public class PathLengthCheckerTests : IClassFixture<FilesFixtureForClassSetupAndTeardown>
	{
		private readonly FilesFixtureForClassSetupAndTeardown _filesFixture;

		public PathLengthCheckerTests(FilesFixtureForClassSetupAndTeardown filesFixture)
		{
			_filesFixture = filesFixture;
		}

		[Fact]
		public void GetAllPaths()
		{
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			paths.Should().Contain(_filesFixture.Directories).And.Contain(_filesFixture.Files);
			paths.Should().OnlyContain(p => _filesFixture.Directories.Contains(p) || _filesFixture.Files.Contains(p));
		}

		[Fact]
		public void GetAllDirectories()
		{
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.Directories,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);
			paths.Should().Contain(_filesFixture.Directories);
			paths.Should().OnlyContain(p => _filesFixture.Directories.Contains(p));
		}

		[Fact]
		public void GetAllFiles()
		{
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.Files,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);
			paths.Should().Contain(_filesFixture.Files);
			paths.Should().OnlyContain(p => _filesFixture.Files.Contains(p));
		}

		[Fact]
		public void GetAllPathsInTopLevelDirectory()
		{
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.TopDirectoryOnly,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);
			paths.Should().Contain(p => string.Equals(Path.GetDirectoryName(p.Path), _filesFixture.RootPath));
			paths.Should().OnlyContain(p => string.Equals(Path.GetDirectoryName(p.Path), _filesFixture.RootPath));
		}

		[Fact]
		public void GetAllDirectoriesInTopLevelDirectory()
		{
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.TopDirectoryOnly,
				TypesToGet = FileSystemTypes.Directories,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);
			paths.Should().Contain(p => string.Equals(Path.GetDirectoryName(p.Path), _filesFixture.RootPath));
			paths.Should().OnlyContain(p => string.Equals(Path.GetDirectoryName(p.Path), _filesFixture.RootPath));
		}

		[Fact]
		public void GetAllFilesInTopLevelDirectory()
		{
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.TopDirectoryOnly,
				TypesToGet = FileSystemTypes.Files,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);
			paths.Should().Contain(p => string.Equals(Path.GetDirectoryName(p.Path), _filesFixture.RootPath));
			paths.Should().OnlyContain(p => string.Equals(Path.GetDirectoryName(p.Path), _filesFixture.RootPath));
		}

		[Fact]
		public void GetAllFilesFromEmptyDirectory()
		{
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.EmptyDirectoryPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);
			paths.Should().HaveCount(0);
		}

		[Fact]
		public void InvalidDirectorySoShouldThrowException()
		{
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = Path.Combine(_filesFixture.RootPath, "ADirectoryThatDoesNotExist"),
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			Action act = () => PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None).Count();
			act.Should().Throw<DirectoryNotFoundException>();
		}

		[Fact]
		public void GetAllPathsLessThanXCharacters()
		{
			int maxPathLength = _filesFixture.AllPaths.Min(p => p.Length) + 1;
			var expectedPaths = _filesFixture.AllPaths.Where(p => p.Length <= maxPathLength);

			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = maxPathLength,
				UrlEncodePaths = false
			};

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);
			paths.Should().Contain(expectedPaths);
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[Fact]
		public void GetAllPathsMoreThanXCharacters()
		{
			int minPathLength = _filesFixture.AllPaths.Max(p => p.Length) - 1;
			var expectedPaths = _filesFixture.AllPaths.Where(p => p.Length >= minPathLength);

			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = minPathLength,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);
			paths.Should().Contain(expectedPaths);
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[Fact]
		public void GetAllPathsMoreThanXAndLessThanYCharacters()
		{
			int minPathLength = _filesFixture.AllPaths.Min(p => p.Length) + 1;
			int maxPathLength = _filesFixture.AllPaths.Max(p => p.Length) - 1;
			var expectedPaths = _filesFixture.AllPaths.Where(p => p.Length >= minPathLength && p.Length <= maxPathLength);

			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = minPathLength,
				MaximumPathLength = maxPathLength,
				UrlEncodePaths = false
			};

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);
			paths.Should().Contain(expectedPaths);
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[Fact]
		public void MinimumPathLengthGreaterThanMaximumPathLengthSoShouldThrowException()
		{
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = 2,
				MaximumPathLength = 1,
				UrlEncodePaths = false
			};

			Action act = () => PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None).Count();
			act.Should().Throw<MinPathLengthGreaterThanMaxPathLengthException>();
		}

		[Fact]
		public void ReplacingTheStartingDirectoryShouldAlterThePathsProperly()
		{
			var newRootDirectoryName = "NewRootDirectory";
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = newRootDirectoryName,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			var expectedPaths = _filesFixture.AllPaths.Select(p =>
			{
				return new PathInfo()
				{
					Path = p.Path.Replace(_filesFixture.RootPath, newRootDirectoryName)
				};
			});

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			paths.Should().NotContain(_filesFixture.AllPaths);
			paths.Should().Contain(expectedPaths);
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[Fact]
		public void UrlEncodingThePathsShouldAlterThePathsProperly()
		{
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = true
			};

			var expectedPaths = _filesFixture.AllPaths.Select(p =>
			{
				return new PathInfo()
				{
					Path = Uri.EscapeDataString(p.Path)
				};
			});

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			paths.Should().NotContain(_filesFixture.AllPaths);
			paths.Should().Contain(expectedPaths);
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[Fact]
		public void ReplacingTheStartingDirectoryAndUsingUrlEncodingShouldAlterThePathsProperly()
		{
			var newRootDirectoryName = "NewRootDirectory";
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = newRootDirectoryName,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = true
			};

			var expectedPaths = _filesFixture.AllPaths.Select(p =>
			{
				var replaced = p.Path.Replace(_filesFixture.RootPath, newRootDirectoryName);
				return new PathInfo()
				{
					Path = Uri.EscapeDataString(replaced)
				};
			});

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			paths.Should().NotContain(_filesFixture.AllPaths);
			paths.Should().Contain(expectedPaths);
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[Fact]
		public void ReplacementAffectsMinLengthFilter_ForOneDriveMock()
		{
			// Without replacement nothing is huge; with a long OneDrive prefix, min length filter should use scored length.
			var longPrefix = @"C:\Users\jdoe\OneDrive - Contoso\General\Department\Team";
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.Files,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = longPrefix,
				MinimumPathLength = longPrefix.Length, // all files under replacement should meet this
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None).ToList();
			paths.Should().NotBeEmpty();
			paths.Should().OnlyContain(p => p.Length >= longPrefix.Length);
			paths.Should().OnlyContain(p => p.Path.StartsWith(longPrefix));
			// Original path still available for Explorer / relative display
			paths.Should().OnlyContain(p => p.OriginalPath.StartsWith(_filesFixture.RootPath));
		}
	}
}

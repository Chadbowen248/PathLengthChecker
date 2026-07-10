using System.Collections.Generic;
using System.IO;
using System.Threading;
using SearchOption = System.IO.SearchOption;

namespace PathLengthChecker
{
	/// <summary>
	/// Class used to retrieve file system objects in a given path.
	/// Returns original on-disk paths only; formatting is applied later by PathFormatter.
	/// </summary>
	public static class PathRetriever
	{
		/// <summary>
		/// Gets the original paths under the root directory.
		/// </summary>
		public static IEnumerable<string> GetPaths(PathSearchOptions searchOptions, CancellationToken cancellationToken)
		{
			if (!Directory.Exists(searchOptions.RootDirectory))
			{
				throw new DirectoryNotFoundException($"The specified root directory '{searchOptions.RootDirectory}' does not exist. Please provide a valid directory.");
			}

			if (string.IsNullOrEmpty(searchOptions.SearchPattern))
				searchOptions.SearchPattern = "*";

			foreach (var path in EnumeratePaths(searchOptions))
			{
				if (cancellationToken.IsCancellationRequested)
					yield break;

				yield return path;
			}
		}

		private static IEnumerable<string> EnumeratePaths(PathSearchOptions searchOptions)
		{
			var enumerationOptions = new EnumerationOptions
			{
				RecurseSubdirectories = searchOptions.SearchOption == SearchOption.AllDirectories,
				IgnoreInaccessible = true,
				// Skip reparse points (junctions/symlinks) similar to AlphaFS SkipReparsePoints.
				AttributesToSkip = FileAttributes.ReparsePoint,
				MatchCasing = MatchCasing.CaseInsensitive,
				MatchType = MatchType.Simple,
				ReturnSpecialDirectories = false
			};

			// Enumerate files and/or directories based on TypesToGet.
			if (searchOptions.TypesToGet == FileSystemTypes.Files)
			{
				return Directory.EnumerateFiles(searchOptions.RootDirectory, searchOptions.SearchPattern, enumerationOptions);
			}

			if (searchOptions.TypesToGet == FileSystemTypes.Directories)
			{
				return Directory.EnumerateDirectories(searchOptions.RootDirectory, searchOptions.SearchPattern, enumerationOptions);
			}

			// All: files + directories matching the pattern.
			// EnumerateFileSystemEntries returns both.
			return Directory.EnumerateFileSystemEntries(searchOptions.RootDirectory, searchOptions.SearchPattern, enumerationOptions);
		}
	}
}

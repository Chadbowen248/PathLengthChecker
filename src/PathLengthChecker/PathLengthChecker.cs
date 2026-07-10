using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PathLengthChecker
{
	/// <summary>
	/// Class used to retrieve file system objects in a given path along with their path lengths.
	/// </summary>
	public static class PathLengthChecker
	{
		/// <summary>
		/// Gets the paths with lengths. Length is based on the scored path (after root replacement / URL encode).
		/// OriginalPath is preserved for display modes and Explorer open.
		/// </summary>
		public static IEnumerable<PathInfo> GetPathsWithLengths(PathLengthSearchOptions options, CancellationToken cancellationToken)
		{
			foreach (var pathInfo in RetrievePaths(options, cancellationToken))
			{
				yield return pathInfo;
			}
		}

		/// <summary>
		/// Gets all of the paths, along with their lengths, as a string.
		/// Honors DisplayMode / StripPrefix export options when set.
		/// </summary>
		public static string GetPathsWithLengthsAsString(PathLengthSearchOptions options, CancellationToken cancellationToken)
		{
			var paths = GetPathsWithLengths(options, cancellationToken);
			return PathFormatter.FormatPathsAsPlainText(
				paths,
				options.RootDirectory,
				options.DisplayMode,
				includeLength: true,
				stripPrefix: options.StripPrefixOnExport,
				stripPrefixText: options.StripPrefixText);
		}

		private static IEnumerable<PathInfo> RetrievePaths(PathLengthSearchOptions options, CancellationToken cancellationToken)
		{
			if (options.MinimumPathLength > options.MaximumPathLength && options.MinimumPathLength >= 0 && options.MaximumPathLength >= 0)
				throw new MinPathLengthGreaterThanMaxPathLengthException();

			var originalPaths = PathRetriever.GetPaths(options, cancellationToken);

			foreach (var original in originalPaths)
			{
				if (cancellationToken.IsCancellationRequested)
					yield break;

				var scorePath = PathFormatter.GetScorePath(original, options);
				var length = scorePath.Length;

				if (length < options.MinimumPathLength)
					continue;

				if (options.MaximumPathLength > 0 && length > options.MaximumPathLength)
					continue;

				var pathInfo = new PathInfo
				{
					OriginalPath = original,
					Path = scorePath
				};
				pathInfo.DisplayPath = PathFormatter.GetDisplayPath(pathInfo, options.RootDirectory, options.DisplayMode);
				yield return pathInfo;
			}
		}
	}
}

using System;
using System.IO;
using System.Text;

namespace PathLengthChecker
{
	/// <summary>
	/// Formats paths for scoring (length), display, and client-facing copy/export
	/// without re-scanning the filesystem.
	/// </summary>
	public static class PathFormatter
	{
		/// <summary>
		/// Builds the scored path used for length (root replacement then optional URL encode).
		/// Matches historical PathLengthChecker behavior.
		/// </summary>
		public static string GetScorePath(string originalPath, PathSearchOptions options)
		{
			if (originalPath is null) throw new ArgumentNullException(nameof(originalPath));
			if (options is null) throw new ArgumentNullException(nameof(options));

			var path = originalPath;

			if (options.RootDirectoryReplacement != null)
			{
				path = ReplaceRoot(originalPath, options.RootDirectory, options.RootDirectoryReplacement);
			}

			if (options.UrlEncodePaths)
			{
				path = Uri.EscapeDataString(path);
			}

			return path;
		}

		/// <summary>
		/// Replace root directory prefix in the original path (ordinal, first match semantics via string.Replace).
		/// </summary>
		public static string ReplaceRoot(string originalPath, string rootDirectory, string rootReplacement)
		{
			if (string.IsNullOrEmpty(rootDirectory))
				return originalPath;

			return originalPath.Replace(rootDirectory, rootReplacement);
		}

		/// <summary>
		/// Path relative to the starting directory (no leading separator).
		/// Falls back to original if it is not under the root.
		/// </summary>
		public static string GetRelativePath(string originalPath, string rootDirectory)
		{
			if (string.IsNullOrEmpty(originalPath))
				return originalPath ?? string.Empty;

			if (string.IsNullOrEmpty(rootDirectory))
				return originalPath;

			try
			{
				var relative = Path.GetRelativePath(rootDirectory, originalPath);
				if (relative == "." || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
					|| relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
					|| relative == "..")
				{
					// Not under root; try simple string strip as fallback (mirrors ReplaceRoot style).
					if (originalPath.StartsWith(rootDirectory, StringComparison.OrdinalIgnoreCase))
					{
						return TrimLeadingSeparators(originalPath.Substring(rootDirectory.Length));
					}
					return originalPath;
				}
				return relative;
			}
			catch
			{
				if (originalPath.StartsWith(rootDirectory, StringComparison.OrdinalIgnoreCase))
					return TrimLeadingSeparators(originalPath.Substring(rootDirectory.Length));
				return originalPath;
			}
		}

		/// <summary>
		/// Strips a prefix from a path for client handoff (e.g. remove OneDrive local root).
		/// Case-insensitive; trims leftover leading separators.
		/// </summary>
		public static string StripPrefix(string path, string? prefix)
		{
			if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(prefix))
				return path ?? string.Empty;

			if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				return TrimLeadingSeparators(path.Substring(prefix.Length));
			}

			// Also try with trailing separator variants so "C:\OneDrive" matches "C:\OneDrive\a".
			var prefixTrimmed = prefix.TrimEnd('\\', '/');
			if (prefixTrimmed.Length > 0
				&& path.StartsWith(prefixTrimmed, StringComparison.OrdinalIgnoreCase)
				&& (path.Length == prefixTrimmed.Length
					|| path[prefixTrimmed.Length] == '\\'
					|| path[prefixTrimmed.Length] == '/'))
			{
				return TrimLeadingSeparators(path.Substring(prefixTrimmed.Length));
			}

			return path;
		}

		public static string GetDisplayPath(PathInfo pathInfo, string rootDirectory, PathDisplayMode mode)
		{
			if (pathInfo is null) throw new ArgumentNullException(nameof(pathInfo));

			return mode switch
			{
				PathDisplayMode.Original => pathInfo.OriginalPath,
				PathDisplayMode.Relative => GetRelativePath(pathInfo.OriginalPath, rootDirectory),
				_ => pathInfo.Path // Destination / scored
			};
		}

		/// <summary>
		/// Path string for copy/export: choose display mode, then optionally strip a prefix.
		/// Length is never altered by this method.
		/// </summary>
		public static string GetExportPath(
			PathInfo pathInfo,
			string rootDirectory,
			PathDisplayMode mode,
			bool stripPrefix,
			string? stripPrefixText)
		{
			var path = GetDisplayPath(pathInfo, rootDirectory, mode);
			if (stripPrefix)
			{
				// Default strip text: if using destination mode and strip text empty, do nothing.
				path = StripPrefix(path, stripPrefixText);
			}
			return path;
		}

		public static string FormatPathsAsPlainText(
			System.Collections.Generic.IEnumerable<PathInfo> paths,
			string rootDirectory,
			PathDisplayMode mode,
			bool includeLength,
			bool stripPrefix,
			string? stripPrefixText)
		{
			var sb = new StringBuilder();
			foreach (var p in paths)
			{
				var exportPath = GetExportPath(p, rootDirectory, mode, stripPrefix, stripPrefixText);
				if (includeLength)
					sb.Append(p.Length).Append(": ").AppendLine(exportPath);
				else
					sb.AppendLine(exportPath);
			}
			return sb.ToString().TrimEnd();
		}

		public static string FormatPathsAsCsv(
			System.Collections.Generic.IEnumerable<PathInfo> paths,
			string rootDirectory,
			PathDisplayMode mode,
			bool includeLength,
			bool stripPrefix,
			string? stripPrefixText)
		{
			var sb = new StringBuilder();
			sb.AppendLine(includeLength ? "Length,\"Path\"" : "\"Path\"");
			foreach (var p in paths)
			{
				var exportPath = GetExportPath(p, rootDirectory, mode, stripPrefix, stripPrefixText);
				// Escape quotes in path for CSV
				var escaped = exportPath.Replace("\"", "\"\"");
				if (includeLength)
					sb.Append(p.Length).Append(",\"").Append(escaped).AppendLine("\"");
				else
					sb.Append('"').Append(escaped).AppendLine("\"");
			}
			return sb.ToString().TrimEnd();
		}

		private static string TrimLeadingSeparators(string value)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;
			return value.TrimStart('\\', '/');
		}
	}
}

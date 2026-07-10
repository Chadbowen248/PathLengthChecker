using System;

namespace PathLengthChecker
{
	/// <summary>
	/// Holds info about a path found during a scan.
	/// OriginalPath is the on-disk path. Path is the scored path used for length
	/// (after root replacement / URL encode). DisplayPath is for UI/export and can
	/// differ without changing Length.
	/// </summary>
	public sealed class PathInfo : IEquatable<PathInfo>
	{
		private string? _originalPath;
		private string _path = string.Empty;
		private string? _displayPath;

		/// <summary>
		/// Path as discovered on disk (before replacement / URL encode).
		/// </summary>
		public string OriginalPath
		{
			get => _originalPath ?? _path;
			init => _originalPath = value;
		}

		/// <summary>
		/// Scored path used for length calculations (after replacement / URL encode).
		/// Kept as "Path" for backwards compatibility with existing tests and callers.
		/// </summary>
		public string Path
		{
			get => _path;
			init => _path = value ?? string.Empty;
		}

		/// <summary>
		/// Path shown in the UI / used for client-facing export when set.
		/// Defaults to the scored Path.
		/// </summary>
		public string DisplayPath
		{
			get => _displayPath ?? Path;
			set => _displayPath = value;
		}

		public int Length => Path.Length;

		public bool Equals(PathInfo? other)
		{
			if (other is null) return false;
			return string.Equals(Path, other.Path, StringComparison.Ordinal);
		}

		public override bool Equals(object? obj) => Equals(obj as PathInfo);

		public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Path);

		public override string ToString() => $"{Length}: {Path}";
	}
}

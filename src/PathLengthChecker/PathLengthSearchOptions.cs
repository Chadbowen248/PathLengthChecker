namespace PathLengthChecker
{
	/// <summary>
	/// Options used when retrieving paths with their lengths.
	/// </summary>
	public class PathLengthSearchOptions : PathSearchOptions
	{
		/// <summary>
		/// The Minimum Length that a Path must have to be included in the search results.
		/// Specify a value of 0 or less to ignore the minimum path length.
		/// </summary>
		public int MinimumPathLength = 0;

		/// <summary>
		/// The Maximum Length that a Path must have to be included in the search results.
		/// Specify a value of -1 or 0 to ignore the maximum path length.
		/// </summary>
		public int MaximumPathLength = MaximumPathLengthMaxValue;

		public const int MaximumPathLengthMaxValue = 999999;

		/// <summary>
		/// Microsoft OneDrive / SharePoint documented max path length (characters).
		/// Use as a MinPathLength preset when finding paths that would exceed the limit.
		/// </summary>
		public const int OneDriveMaxPathLength = 400;

		/// <summary>
		/// Indicates the type of result that is output once the search completes.
		/// Valid values are MinLength, MaxLength, or Paths. Default is Paths.
		/// </summary>
		public OutputTypes OutputType = OutputTypes.Paths;

		/// <summary>
		/// How paths are displayed / exported after scoring (does not affect Length).
		/// </summary>
		public PathDisplayMode DisplayMode = PathDisplayMode.Destination;

		/// <summary>
		/// When true, export/copy strips <see cref="StripPrefixText"/> from the displayed path.
		/// </summary>
		public bool StripPrefixOnExport = false;

		/// <summary>
		/// Prefix to remove from paths when copying/exporting for client handoff.
		/// Typically the OneDrive destination root. Does not change Length.
		/// </summary>
		public string? StripPrefixText = null;

		/// <summary>
		/// Optional path to write results (plain text). Used by CLI.
		/// </summary>
		public string? ExportFile = null;
	}
}

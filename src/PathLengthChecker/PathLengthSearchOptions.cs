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
		/// Safe path length when destinations will also be used as Windows shortcuts / classic paths.
		/// Windows MAX_PATH is 260; many tools treat ~255 as the ceiling. 240 leaves headroom.
		/// (OneDrive/SharePoint cloud alone can allow ~400; raise MinLength manually if you only care about cloud.)
		/// </summary>
		public const int WindowsSafePathLength = 240;

		/// <summary>
		/// Microsoft OneDrive / SharePoint approximate full-path ceiling (characters).
		/// Prefer <see cref="WindowsSafePathLength"/> for the default preset when shortcuts are involved.
		/// </summary>
		public const int OneDriveCloudPathLength = 400;

		/// <summary>
		/// Example destination path used when applying the Windows/OneDrive preset (matches UI tooltips).
		/// Replace Contoso / jdoe with the real tenant and user before scanning for production.
		/// </summary>
		public const string ExampleOneDriveDestinationPath = @"C:\Users\jdoe\OneDrive - Contoso\General";

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

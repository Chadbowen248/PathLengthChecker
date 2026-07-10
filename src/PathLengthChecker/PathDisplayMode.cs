namespace PathLengthChecker
{
	/// <summary>
	/// How paths are shown in the UI / exported for clients after a scan.
	/// Does not change the scored Length (which always uses the destination/mock path).
	/// </summary>
	public enum PathDisplayMode
	{
		/// <summary>
		/// Destination / replaced path (same as scored path when replacement is enabled).
		/// </summary>
		Destination = 0,

		/// <summary>
		/// Path relative to the starting (root) directory.
		/// </summary>
		Relative = 1,

		/// <summary>
		/// Original on-disk path from the scan.
		/// </summary>
		Original = 2
	}
}

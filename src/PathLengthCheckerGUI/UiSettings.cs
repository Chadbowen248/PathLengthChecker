using System.IO;
using System.Text.Json;
using PathLengthChecker;

namespace PathLengthCheckerGUI
{
	/// <summary>
	/// Lightweight persisted UI settings (JSON in LocalAppData).
	/// </summary>
	public sealed class UiSettings
	{
		public double WindowWidth { get; set; } = 980;
		public double WindowHeight { get; set; } = 760;
		public double WindowLeft { get; set; } = 80;
		public double WindowTop { get; set; } = 40;
		public string WindowState { get; set; } = "Normal";

		public string RootDirectory { get; set; } = string.Empty;
		public bool ReplaceRootDirectory { get; set; }
		public string RootDirectoryReplacementText { get; set; } = string.Empty;
		public bool IncludeSubdirectories { get; set; } = true;
		public string TypesToInclude { get; set; } = nameof(FileSystemTypes.All);
		public int MinPathLength { get; set; } = 0;
		public int MaxPathLength { get; set; } = PathLengthSearchOptions.MaximumPathLengthMaxValue;
		public string SearchPattern { get; set; } = string.Empty;
		public bool UrlEncodePaths { get; set; }

		public string DisplayMode { get; set; } = nameof(PathDisplayMode.Destination);
		public bool StripPrefixOnCopy { get; set; } = true;
		public string StripPrefixText { get; set; } = string.Empty;
		public bool IncludeLengthsOnCopy { get; set; } = true;
		public bool DarkMode { get; set; }

		private static string SettingsPath =>
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"PathLengthChecker", "ui-settings.json");

		public static UiSettings Load()
		{
			try
			{
				if (File.Exists(SettingsPath))
				{
					var json = File.ReadAllText(SettingsPath);
					return JsonSerializer.Deserialize<UiSettings>(json) ?? new UiSettings();
				}
			}
			catch
			{
				// ignore corrupt settings
			}
			return new UiSettings();
		}

		public void Save()
		{
			try
			{
				var dir = Path.GetDirectoryName(SettingsPath);
				if (!string.IsNullOrEmpty(dir))
					Directory.CreateDirectory(dir);
				var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
				File.WriteAllText(SettingsPath, json);
			}
			catch
			{
				// non-fatal
			}
		}
	}
}

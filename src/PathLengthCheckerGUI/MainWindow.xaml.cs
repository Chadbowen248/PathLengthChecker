using PathLengthChecker;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SearchOption = System.IO.SearchOption;

namespace PathLengthCheckerGUI
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public void NotifyPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private DateTime _timePathSearchingStarted = DateTime.MinValue;
		private CancellationTokenSource _searchCancellationTokenSource = new();
		private UiSettings _settings = new();
		private string _lastRootDirectory = string.Empty;
		private PathSearchOptions _lastScoreOptions = new();

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
			SetWindowTitle();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_settings = UiSettings.Load();
			ApplySettingsToUi(_settings);

			if (cmbTypesToInclude.SelectedItem == null)
				cmbTypesToInclude.SelectedItem = FileSystemTypes.All;
			if (cmbDisplayMode.SelectedItem == null)
				cmbDisplayMode.SelectedItem = PathDisplayMode.Destination;
		}

		private void Window_Closed(object? sender, EventArgs e)
		{
			SaveUiToSettings();
			_settings.Save();
		}

		private void SetWindowTitle()
		{
			var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "2.0.0";
			Title = $"Path Length Checker v{version} — OneDrive migration helpers";
		}

		public ObservableCollection<PathInfo> Paths
		{
			get => _paths;
			set
			{
				_paths = value;
				NotifyPropertyChanged(nameof(Paths));
			}
		}
		private ObservableCollection<PathInfo> _paths = new();

		private IEnumerable<PathInfo> PathsFromUiDataGrid => dgPaths.Items.Cast<PathInfo>();

		public PathInfo? SelectedPath
		{
			get => (PathInfo?)GetValue(SelectedPathProperty);
			set => SetValue(SelectedPathProperty, value);
		}
		public static readonly DependencyProperty SelectedPathProperty =
			DependencyProperty.Register(nameof(SelectedPath), typeof(PathInfo), typeof(MainWindow), new PropertyMetadata(null));

		private void btnBrowseForRootDirectory_Click(object sender, RoutedEventArgs e)
		{
			var folderDialog = new Microsoft.Win32.OpenFolderDialog
			{
				Title = "Select the directory that contains the paths whose length you want to check..."
			};

			if (folderDialog.ShowDialog(this) == true)
				txtRootDirectory.Text = folderDialog.FolderName;
		}

		private void btnBrowseForReplaceRootDirectory_Click(object sender, RoutedEventArgs e)
		{
			var folderDialog = new Microsoft.Win32.OpenFolderDialog
			{
				Title = "Select the path that should replace the Starting Directory in scored results..."
			};

			if (folderDialog.ShowDialog(this) == true)
			{
				txtReplaceRootDirectory.Text = folderDialog.FolderName;
				if (string.IsNullOrWhiteSpace(txtStripPrefix.Text))
					txtStripPrefix.Text = folderDialog.FolderName;
			}
		}

		private async void btnGetPathLengths_Click(object sender, RoutedEventArgs e)
		{
			_searchCancellationTokenSource = new CancellationTokenSource();
			btnGetPathLengths.IsEnabled = false;
			btnGetPathLengths.Visibility = Visibility.Collapsed;
			btnCancelGetPathLengths.IsEnabled = true;
			btnCancelGetPathLengths.Visibility = Visibility.Visible;

			Paths.Clear();
			txtNumberOfPaths.Text = string.Empty;
			txtMinAndMaxPathLengths.Text = string.Empty;

			RecordAndDisplayTimeSearchStarted();

			try
			{
				await BuildSearchOptionsAndGetPaths(
					txtRootDirectory.Text.Trim(),
					txtReplaceRootDirectory.Text.Trim(),
					txtSearchPattern.Text,
					_searchCancellationTokenSource.Token);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred while retrieving paths:{Environment.NewLine}{Environment.NewLine}{ex.Message}", "Error Occurred");
				Debug.WriteLine(ex.ToString());
			}

			DisplayResultsMetadata();

			btnGetPathLengths.IsEnabled = true;
			btnGetPathLengths.Visibility = Visibility.Visible;
			btnCancelGetPathLengths.IsEnabled = false;
			btnCancelGetPathLengths.Visibility = Visibility.Collapsed;
		}

		private void RecordAndDisplayTimeSearchStarted()
		{
			_timePathSearchingStarted = DateTime.Now;
			txtNumberOfPaths.Text = $"Started searching at {_timePathSearchingStarted:h:mm:ss tt}...";
		}

		private async Task BuildSearchOptionsAndGetPaths(string rootDirectory, string rootDirectoryReplacement, string searchPattern, CancellationToken cancellationToken)
		{
			try
			{
				rootDirectory = Path.GetFullPath(rootDirectory);
			}
			catch
			{
				MessageBox.Show($"The Starting Directory \"{rootDirectory}\" does not exist. Please specify a valid directory.", "Invalid Starting Directory");
				return;
			}

			if (!Directory.Exists(rootDirectory))
			{
				MessageBox.Show($"The Starting Directory \"{rootDirectory}\" does not exist. Please specify a valid directory.", "Invalid Starting Directory");
				return;
			}

			if (!int.TryParse(numMinPathLength.Text.Trim(), out int minPathLength))
				minPathLength = 0;
			if (!int.TryParse(numMaxPathLength.Text.Trim(), out int maxPathLength))
				maxPathLength = PathLengthSearchOptions.MaximumPathLengthMaxValue;

			if (!(chkReplaceRootDirectory.IsChecked ?? false))
				rootDirectoryReplacement = null!;

			var displayMode = cmbDisplayMode.SelectedItem is PathDisplayMode dm ? dm : PathDisplayMode.Destination;

			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = rootDirectory,
				SearchPattern = searchPattern,
				SearchOption = (chkIncludeSubdirectories.IsChecked ?? false) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly,
				TypesToGet = cmbTypesToInclude.SelectedValue is FileSystemTypes t ? t : FileSystemTypes.All,
				RootDirectoryReplacement = rootDirectoryReplacement,
				UrlEncodePaths = chkUrlEncodePaths.IsChecked ?? false,
				MinimumPathLength = minPathLength,
				MaximumPathLength = maxPathLength,
				DisplayMode = displayMode
			};

			_lastRootDirectory = rootDirectory;
			_lastScoreOptions = new PathSearchOptions
			{
				RootDirectory = rootDirectory,
				RootDirectoryReplacement = rootDirectoryReplacement,
				UrlEncodePaths = searchOptions.UrlEncodePaths
			};

			// If strip prefix empty and replacement set, default strip to replacement.
			if (string.IsNullOrWhiteSpace(txtStripPrefix.Text) && !string.IsNullOrEmpty(rootDirectoryReplacement))
				txtStripPrefix.Text = rootDirectoryReplacement;

			var newPaths = await Task.Run(() =>
			{
				var paths = PathLengthChecker.PathLengthChecker.GetPathsWithLengths(searchOptions, cancellationToken);
				return new ObservableCollection<PathInfo>(paths.ToList());
			}, cancellationToken);

			Paths = newPaths;
			ApplyDisplayModeToPaths();
			SetDefaultSortLongestFirst();
		}

		private void ApplyDisplayModeToPaths()
		{
			var mode = cmbDisplayMode.SelectedItem is PathDisplayMode dm ? dm : PathDisplayMode.Destination;
			foreach (var p in Paths)
			{
				p.DisplayPath = PathFormatter.GetDisplayPath(p, _lastRootDirectory, mode);
			}
			// Refresh grid bindings
			CollectionViewSource.GetDefaultView(dgPaths.ItemsSource)?.Refresh();
		}

		private void SetDefaultSortLongestFirst()
		{
			var view = CollectionViewSource.GetDefaultView(dgPaths.ItemsSource);
			if (view == null) return;
			using (view.DeferRefresh())
			{
				view.SortDescriptions.Clear();
				view.SortDescriptions.Add(new SortDescription(nameof(PathInfo.Length), ListSortDirection.Descending));
				view.SortDescriptions.Add(new SortDescription(nameof(PathInfo.DisplayPath), ListSortDirection.Ascending));
			}
			foreach (var column in dgPaths.Columns)
			{
				if (column.SortMemberPath == nameof(PathInfo.Length))
					column.SortDirection = ListSortDirection.Descending;
				else if (column.SortMemberPath == nameof(PathInfo.DisplayPath))
					column.SortDirection = ListSortDirection.Ascending;
				else
					column.SortDirection = null;
			}
		}

		private void DisplayResultsMetadata()
		{
			var timeSinceSearchingStarted = DateTime.Now - _timePathSearchingStarted;
			var text = $"{Paths.Count} paths found in {timeSinceSearchingStarted:mm\\:ss\\.f}";

			if (_searchCancellationTokenSource.IsCancellationRequested)
				text += " - Search Cancelled";

			txtNumberOfPaths.Text = text;

			int shortestPathLength = Paths.Count > 0 ? Paths.Min(p => p.Length) : 0;
			int longestPathLength = Paths.Count > 0 ? Paths.Max(p => p.Length) : 0;
			int over400 = Paths.Count(p => p.Length > PathLengthSearchOptions.OneDriveMaxPathLength);
			txtMinAndMaxPathLengths.Text =
				$"Shortest: {shortestPathLength}, Longest: {longestPathLength} characters" +
				(over400 > 0 ? $"  |  {over400} over OneDrive 400-char limit" : string.Empty);
		}

		private void cmbDisplayMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsLoaded || Paths.Count == 0) return;
			ApplyDisplayModeToPaths();
		}

		private void chkStripPrefixOnCopy_Changed(object sender, RoutedEventArgs e)
		{
			// No re-scan needed; only affects copy/export.
		}

		private void btnCopyPaths_Click(object sender, RoutedEventArgs e)
		{
			var text = BuildExportText(asCsv: false);
			SetClipboardText(text);
		}

		private void btnCopyCsv_Click(object sender, RoutedEventArgs e)
		{
			var text = BuildExportText(asCsv: true);
			SetClipboardText(text);
		}

		private void btnExport_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new Microsoft.Win32.SaveFileDialog
			{
				Filter = "CSV (*.csv)|*.csv|Text (*.txt)|*.txt|All files (*.*)|*.*",
				FileName = "path-length-report",
				DefaultExt = ".csv"
			};
			if (dlg.ShowDialog() != true) return;

			var asCsv = string.Equals(Path.GetExtension(dlg.FileName), ".csv", StringComparison.OrdinalIgnoreCase);
			var text = BuildExportText(asCsv);
			File.WriteAllText(dlg.FileName, text, Encoding.UTF8);
			MessageBox.Show($"Exported {Paths.Count} path(s) to:\n{dlg.FileName}", "Export complete");
		}

		private string BuildExportText(bool asCsv)
		{
			var mode = cmbDisplayMode.SelectedItem is PathDisplayMode dm ? dm : PathDisplayMode.Destination;
			var strip = chkStripPrefixOnCopy.IsChecked ?? false;
			var stripText = string.IsNullOrWhiteSpace(txtStripPrefix.Text)
				? txtReplaceRootDirectory.Text.Trim()
				: txtStripPrefix.Text.Trim();
			var includeLength = chkIncludeLengthsOnCopy.IsChecked ?? true;

			if (asCsv)
			{
				return PathFormatter.FormatPathsAsCsv(
					PathsFromUiDataGrid,
					_lastRootDirectory,
					mode,
					includeLength,
					strip,
					strip ? stripText : null);
			}

			return PathFormatter.FormatPathsAsPlainText(
				PathsFromUiDataGrid,
				_lastRootDirectory,
				mode,
				includeLength,
				strip,
				strip ? stripText : null);
		}

		private static void SetClipboardText(string text)
		{
			int maxAttempts = 100;
			int millisecondsBetweenAttempts = 10;
			for (int attempts = 1; attempts <= maxAttempts; attempts++)
			{
				try
				{
					Clipboard.SetText(text);
					return;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.ToString());
					if (attempts == maxAttempts)
					{
						MessageBox.Show($"An error occurred while copying text to the clipboard:{Environment.NewLine}{Environment.NewLine}{ex.Message}", "Error Occurred Copying To Clipboard");
					}
				}
				Thread.Sleep(millisecondsBetweenAttempts);
			}
		}

		private void btnCancelGetPathLengths_Click(object sender, RoutedEventArgs e)
		{
			if (!_searchCancellationTokenSource.IsCancellationRequested)
			{
				_searchCancellationTokenSource.Cancel();
				btnCancelGetPathLengths.IsEnabled = false;
			}
		}

		private void dgPaths_LoadingRow(object sender, DataGridRowEventArgs e)
		{
			e.Row.Header = (e.Row.GetIndex() + 1).ToString();
		}

		private void MenuItem_OpenDirectoryInFileExplorer_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedPath == null)
			{
				MessageBox.Show("No path selected.", "Cannot Open Directory");
				return;
			}

			// Always use OriginalPath so Explorer works even when display is replaced/stripped.
			var candidate = SelectedPath.OriginalPath;
			var directoryPath = string.Empty;

			if (Directory.Exists(candidate))
				directoryPath = candidate;
			else if (File.Exists(candidate))
				directoryPath = Directory.GetParent(candidate)?.FullName ?? string.Empty;

			if (string.IsNullOrWhiteSpace(directoryPath))
			{
				MessageBox.Show(
					$"The following directory (or file's directory) either does not exist anymore, you don't have permissions to access it, or its path is greater than 260 characters, so it cannot be opened.{Environment.NewLine}{Environment.NewLine}{candidate}",
					"Cannot Open Directory");
			}
			else
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = directoryPath,
					UseShellExecute = true
				});
			}
		}

		protected internal void SetUIControlsFromSearchOptions(PathLengthSearchOptions argSearchOptions)
		{
			txtRootDirectory.Text = argSearchOptions.RootDirectory;
			txtSearchPattern.Text = argSearchOptions.SearchPattern;
			chkIncludeSubdirectories.IsChecked = argSearchOptions.SearchOption == SearchOption.AllDirectories;
			cmbTypesToInclude.SelectedValue = argSearchOptions.TypesToGet;

			if (!string.IsNullOrEmpty(argSearchOptions.RootDirectoryReplacement))
			{
				txtReplaceRootDirectory.Text = argSearchOptions.RootDirectoryReplacement;
				chkReplaceRootDirectory.IsChecked = true;
			}
			chkUrlEncodePaths.IsChecked = argSearchOptions.UrlEncodePaths;
			numMinPathLength.Text = argSearchOptions.MinimumPathLength.ToString();
			numMaxPathLength.Text = argSearchOptions.MaximumPathLength.ToString();
			cmbDisplayMode.SelectedItem = argSearchOptions.DisplayMode;
			chkStripPrefixOnCopy.IsChecked = argSearchOptions.StripPrefixOnExport;
			if (!string.IsNullOrEmpty(argSearchOptions.StripPrefixText))
				txtStripPrefix.Text = argSearchOptions.StripPrefixText;
		}

		private void btnResetAllOptions_Click(object sender, RoutedEventArgs e)
		{
			txtRootDirectory.Text = string.Empty;
			txtSearchPattern.Text = string.Empty;
			numMinPathLength.Text = "0";
			numMaxPathLength.Text = PathLengthSearchOptions.MaximumPathLengthMaxValue.ToString();
			chkIncludeSubdirectories.IsChecked = true;
			cmbTypesToInclude.SelectedValue = FileSystemTypes.All;
			txtReplaceRootDirectory.Text = string.Empty;
			chkReplaceRootDirectory.IsChecked = false;
			chkUrlEncodePaths.IsChecked = false;
			cmbDisplayMode.SelectedItem = PathDisplayMode.Destination;
			chkStripPrefixOnCopy.IsChecked = true;
			txtStripPrefix.Text = string.Empty;
			chkIncludeLengthsOnCopy.IsChecked = true;
		}

		private void btnOneDrivePreset_Click(object sender, RoutedEventArgs e)
		{
			numMinPathLength.Text = PathLengthSearchOptions.OneDriveMaxPathLength.ToString();
			numMaxPathLength.Text = PathLengthSearchOptions.MaximumPathLengthMaxValue.ToString();
			cmbDisplayMode.SelectedItem = PathDisplayMode.Destination;
			chkStripPrefixOnCopy.IsChecked = true;
			if (chkReplaceRootDirectory.IsChecked == true && !string.IsNullOrWhiteSpace(txtReplaceRootDirectory.Text))
			{
				if (string.IsNullOrWhiteSpace(txtStripPrefix.Text))
					txtStripPrefix.Text = txtReplaceRootDirectory.Text.Trim();
			}
			chkIncludeLengthsOnCopy.IsChecked = true;
			MessageBox.Show(
				"OneDrive preset applied:\n" +
				$"- Min path length = {PathLengthSearchOptions.OneDriveMaxPathLength}\n" +
				"- Display = Destination (mock path)\n" +
				"- Strip prefix when copying = ON\n\n" +
				"Set the destination replacement to the future OneDrive path, scan, then Copy/Export for the client.",
				"OneDrive preset");
		}

		private void Window_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effects = DragDropEffects.Copy;
			else
				e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void Window_Drop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
			if (e.Data.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0) return;
			var path = files[0];
			if (Directory.Exists(path))
			{
				txtRootDirectory.Text = path;
			}
		}

		private void ApplySettingsToUi(UiSettings s)
		{
			Width = s.WindowWidth > 200 ? s.WindowWidth : Width;
			Height = s.WindowHeight > 200 ? s.WindowHeight : Height;
			if (s.WindowLeft >= 0) Left = s.WindowLeft;
			if (s.WindowTop >= 0) Top = s.WindowTop;
			if (Enum.TryParse(s.WindowState, out WindowState ws))
				WindowState = ws;

			txtRootDirectory.Text = s.RootDirectory ?? string.Empty;
			chkReplaceRootDirectory.IsChecked = s.ReplaceRootDirectory;
			txtReplaceRootDirectory.Text = s.RootDirectoryReplacementText ?? string.Empty;
			chkIncludeSubdirectories.IsChecked = s.IncludeSubdirectories;
			if (Enum.TryParse(s.TypesToInclude, out FileSystemTypes types))
				cmbTypesToInclude.SelectedItem = types;
			numMinPathLength.Text = s.MinPathLength.ToString();
			numMaxPathLength.Text = s.MaxPathLength.ToString();
			txtSearchPattern.Text = s.SearchPattern ?? string.Empty;
			chkUrlEncodePaths.IsChecked = s.UrlEncodePaths;
			if (Enum.TryParse(s.DisplayMode, out PathDisplayMode mode))
				cmbDisplayMode.SelectedItem = mode;
			chkStripPrefixOnCopy.IsChecked = s.StripPrefixOnCopy;
			txtStripPrefix.Text = s.StripPrefixText ?? string.Empty;
			chkIncludeLengthsOnCopy.IsChecked = s.IncludeLengthsOnCopy;
		}

		private void SaveUiToSettings()
		{
			_settings.WindowWidth = Width;
			_settings.WindowHeight = Height;
			_settings.WindowLeft = Left;
			_settings.WindowTop = Top;
			_settings.WindowState = WindowState.ToString();
			_settings.RootDirectory = txtRootDirectory.Text;
			_settings.ReplaceRootDirectory = chkReplaceRootDirectory.IsChecked ?? false;
			_settings.RootDirectoryReplacementText = txtReplaceRootDirectory.Text;
			_settings.IncludeSubdirectories = chkIncludeSubdirectories.IsChecked ?? true;
			_settings.TypesToInclude = (cmbTypesToInclude.SelectedItem as FileSystemTypes?)?.ToString() ?? nameof(FileSystemTypes.All);
			if (int.TryParse(numMinPathLength.Text, out var min)) _settings.MinPathLength = min;
			if (int.TryParse(numMaxPathLength.Text, out var max)) _settings.MaxPathLength = max;
			_settings.SearchPattern = txtSearchPattern.Text;
			_settings.UrlEncodePaths = chkUrlEncodePaths.IsChecked ?? false;
			_settings.DisplayMode = (cmbDisplayMode.SelectedItem as PathDisplayMode?)?.ToString() ?? nameof(PathDisplayMode.Destination);
			_settings.StripPrefixOnCopy = chkStripPrefixOnCopy.IsChecked ?? true;
			_settings.StripPrefixText = txtStripPrefix.Text;
			_settings.IncludeLengthsOnCopy = chkIncludeLengthsOnCopy.IsChecked ?? true;
		}
	}
}

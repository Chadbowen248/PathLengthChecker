using System;
using System.Collections.Generic;
using System.Linq;
using SearchOption = System.IO.SearchOption;

namespace PathLengthChecker
{
	public class ArgumentParser
	{
		public readonly static string ArgumentUsage =
			"Parameters and example:\n" +
			"RootDirectory= | Path to the directory to search through and list the paths of. Required.\n" +
			"RootDirectoryReplacement=[null] | Path to replace the Root Directory with in the scored results. Specify 'null' to not replace the Root Directory. Default is null.\n" +
			"SearchOption=[TopDirectory|All] | Specifies whether sub-directories should be searched or not. Default is All.\n" +
			"TypesToInclude=[OnlyFiles|OnlyDirectories|All] | Specifies what types of paths should be returned in the results; files, directories, or both. Default is All.\n" +
			"SearchPattern= | The pattern to match files against. '*' is a wildcard character. Default is '*' to match against everything.\n" +
			"MinLength= | An integer indicating the minimum length that a path must contain in order to be returned in the results. Default is -1 to ignore this flag.\n" +
			"MaxLength= | An integer indicating the maximum length that a path may have in order to be returned in the results. Default is -1 to ignore this flag.\n" +
			"UrlEncodePaths=[True|False] | If true the scored paths will be URL encoded. Default is false.\n" +
			"Output=[MinLength|MaxLength|Paths] | Indicates if you just want the Min/Max path length to be outputted, or if you want all of the paths to be outputted. Default is Paths.\n" +
			"DisplayMode=[Destination|Relative|Original] | How paths are printed (does not change Length). Default is Destination.\n" +
			"StripPrefix= | Optional prefix to strip from printed paths (client handoff). Does not change Length.\n" +
			"ExportFile= | Optional file path to write results instead of only stdout.\n" +
			"\n" +
			"Example: PathLengthChecker.exe RootDirectory=\"C:\\MyDir\" TypesToInclude=OnlyFiles SearchPattern=*FindThis* MinLength=25\n" +
			"OneDrive/Windows example: PathLengthChecker.exe RootDirectory=\"\\\\fs\\Share\" RootDirectoryReplacement=\"C:\\Users\\jdoe\\OneDrive - Contoso\\General\" MinLength=240 DisplayMode=Destination StripPrefix=\"C:\\Users\\jdoe\\OneDrive - Contoso\\General\"";

		/// <summary>
		/// Parses the specified args array into a PathLengthSearchOptions object instance.
		/// </summary>
		public static PathLengthSearchOptions ParseArgs(IEnumerable<string> args)
		{
			var searchOptions = new PathLengthSearchOptions();

			foreach (var arg in args)
			{
				var parameter = arg.Split(new[] { '=' }, 2);
				if (parameter.Length < 2)
					throw new ArgumentException("All parameters must be of the format 'Parameter=Value'");

				var command = parameter[0];
				var value = parameter[1];

				switch (command)
				{
					default:
						throw new ArgumentException("Unrecognized command: " + command);
					case "RootDirectory":
						searchOptions.RootDirectory = value;
						break;
					case "RootDirectoryReplacement":
						searchOptions.RootDirectoryReplacement = string.Equals(value, "null", StringComparison.InvariantCultureIgnoreCase) ? null : value;
						break;
					case "SearchOption":
						searchOptions.SearchOption = string.Equals("TopDirectory", value, StringComparison.InvariantCultureIgnoreCase) ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
						break;
					case "TypesToInclude":
						FileSystemTypes typesToInclude = FileSystemTypes.All;
						if (string.Equals("OnlyFiles", value, StringComparison.InvariantCultureIgnoreCase))
							typesToInclude = FileSystemTypes.Files;
						else if (string.Equals("OnlyDirectories", value, StringComparison.InvariantCultureIgnoreCase))
							typesToInclude = FileSystemTypes.Directories;

						searchOptions.TypesToGet = typesToInclude;
						break;
					case "SearchPattern":
						searchOptions.SearchPattern = value;
						break;
					case "MinLength":
						if (int.TryParse(value, out int minLength))
							searchOptions.MinimumPathLength = minLength;
						break;
					case "MaxLength":
						if (int.TryParse(value, out int maxLength))
							searchOptions.MaximumPathLength = maxLength;
						break;
					case "UrlEncodePaths":
						if (string.Equals(value, "True", StringComparison.OrdinalIgnoreCase))
							searchOptions.UrlEncodePaths = true;
						break;
					case "Output":
						if (Enum.TryParse(value, ignoreCase: true, out OutputTypes outputType))
						{
							searchOptions.OutputType = outputType;
						}
						break;
					case "DisplayMode":
						if (Enum.TryParse(value, ignoreCase: true, out PathDisplayMode displayMode))
						{
							searchOptions.DisplayMode = displayMode;
						}
						break;
					case "StripPrefix":
					case "CopyStripPrefix":
						if (!string.IsNullOrEmpty(value) && !string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
						{
							searchOptions.StripPrefixText = value;
							searchOptions.StripPrefixOnExport = true;
						}
						break;
					case "ExportFile":
						searchOptions.ExportFile = string.Equals(value, "null", StringComparison.OrdinalIgnoreCase) ? null : value;
						break;
				}
			}

			return searchOptions;
		}
	}
}

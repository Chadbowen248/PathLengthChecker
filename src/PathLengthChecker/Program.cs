using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PathLengthChecker
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var searchOptions = ArgumentParser.ParseArgs(args);

				if (searchOptions.OutputType == OutputTypes.MinLength || searchOptions.OutputType == OutputTypes.MaxLength)
				{
					var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, System.Threading.CancellationToken.None).ToList();

					if (searchOptions.OutputType == OutputTypes.MinLength)
						Console.WriteLine(paths.Count > 0 ? paths.Min(p => p.Length) : 0);
					else
						Console.WriteLine(paths.Count > 0 ? paths.Max(p => p.Length) : 0);
				}
				else
				{
					var pathsText = PathLengthChecker.GetPathsWithLengthsAsString(searchOptions, System.Threading.CancellationToken.None);
					Console.WriteLine(pathsText);

					if (!string.IsNullOrWhiteSpace(searchOptions.ExportFile))
					{
						File.WriteAllText(searchOptions.ExportFile, pathsText);
						Console.Error.WriteLine($"Wrote results to {searchOptions.ExportFile}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
				Console.Error.WriteLine();
				Console.Error.WriteLine(ArgumentParser.ArgumentUsage);
			}
			finally
			{
				if (Debugger.IsAttached)
					Console.ReadKey();
			}
		}
	}
}

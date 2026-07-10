using FluentAssertions;
using Xunit;

namespace PathLengthChecker.Tests
{
	public class PathFormatterTests
	{
		[Fact]
		public void ReplaceRoot_ChangesPrefix()
		{
			var original = @"C:\Share\Folder\file.txt";
			var result = PathFormatter.ReplaceRoot(original, @"C:\Share", @"C:\Users\jdoe\OneDrive - Contoso\General");
			result.Should().Be(@"C:\Users\jdoe\OneDrive - Contoso\General\Folder\file.txt");
		}

		[Fact]
		public void GetScorePath_UsesReplacementForLength()
		{
			var options = new PathSearchOptions
			{
				RootDirectory = @"C:\Share",
				RootDirectoryReplacement = @"C:\Users\jdoe\OneDrive - Contoso\General",
				UrlEncodePaths = false
			};

			var score = PathFormatter.GetScorePath(@"C:\Share\a\b.txt", options);
			score.Should().Be(@"C:\Users\jdoe\OneDrive - Contoso\General\a\b.txt");
			score.Length.Should().BeGreaterThan(@"C:\Share\a\b.txt".Length);
		}

		[Fact]
		public void StripPrefix_DoesNotChangeScoredLength_WhenUsedOnlyOnExport()
		{
			var info = new PathInfo
			{
				OriginalPath = @"C:\Share\Clients\Acme\Very Long Folder Name\doc.pdf",
				Path = @"C:\Users\jdoe\OneDrive - Contoso\General\Clients\Acme\Very Long Folder Name\doc.pdf"
			};

			var scoredLength = info.Length;
			var export = PathFormatter.GetExportPath(
				info,
				@"C:\Share",
				PathDisplayMode.Destination,
				stripPrefix: true,
				stripPrefixText: @"C:\Users\jdoe\OneDrive - Contoso\General");

			info.Length.Should().Be(scoredLength);
			export.Should().Be(@"Clients\Acme\Very Long Folder Name\doc.pdf");
			export.Should().NotContain("OneDrive");
		}

		[Fact]
		public void GetRelativePath_ReturnsPathUnderRoot()
		{
			var relative = PathFormatter.GetRelativePath(
				@"C:\Share\Clients\Acme\file.txt",
				@"C:\Share");
			// Normalize separators so the assertion is OS-agnostic.
			var normalized = relative.Replace('/', '\\');
			normalized.Should().Be(@"Clients\Acme\file.txt");
		}

		[Fact]
		public void GetDisplayPath_Original_IgnoresReplacement()
		{
			var info = new PathInfo
			{
				OriginalPath = @"C:\Share\a.txt",
				Path = @"C:\OneDrive\a.txt"
			};
			PathFormatter.GetDisplayPath(info, @"C:\Share", PathDisplayMode.Original)
				.Should().Be(@"C:\Share\a.txt");
			PathFormatter.GetDisplayPath(info, @"C:\Share", PathDisplayMode.Destination)
				.Should().Be(@"C:\OneDrive\a.txt");
		}

		[Fact]
		public void UrlEncode_AfterReplacement_MatchesHistoricalOrder()
		{
			var options = new PathSearchOptions
			{
				RootDirectory = @"C:\My Dir",
				RootDirectoryReplacement = @"D:\New Root",
				UrlEncodePaths = true
			};
			var score = PathFormatter.GetScorePath(@"C:\My Dir\file name.txt", options);
			// Replace first, then EscapeDataString
			var expected = Uri.EscapeDataString(@"D:\New Root\file name.txt");
			score.Should().Be(expected);
		}

		[Fact]
		public void FormatPathsAsCsv_EscapesQuotesAndOptionallyStrips()
		{
			var paths = new[]
			{
				new PathInfo
				{
					OriginalPath = @"C:\Share\a""b.txt",
					Path = @"C:\OneDrive\a""b.txt"
				}
			};

			var csv = PathFormatter.FormatPathsAsCsv(
				paths,
				@"C:\Share",
				PathDisplayMode.Destination,
				includeLength: true,
				stripPrefix: true,
				stripPrefixText: @"C:\OneDrive");

			csv.Should().Contain("Length,\"Path\"");
			csv.Should().Contain("a\"\"b.txt");
			csv.Should().NotContain("OneDrive");
		}
	}
}

# Path Length Checker (OneDrive migration fork)

Stand-alone tool that lists files and directories under a starting path and reports each path's character length.

This repository is a fork of [deadlydog/PathLengthChecker](https://github.com/deadlydog/PathLengthChecker) (MIT), modernized for **.NET 10** with a cleaner UI and features aimed at **AD file-share → OneDrive migrations**.

## Why this fork?

When clients have used deep, verbose folder names on a file share, paths often exceed OneDrive's **400-character** limit after migration. You need to:

1. Measure lengths **as they will look under the future OneDrive destination** (mock the destination prefix).
2. Still return only (or especially) the paths that would **blow the limit**.
3. Hand the list to the client so **they** can rename/shorten folders.
4. Avoid showing noise like `C:\Users\...\OneDrive - Tenant - General\` in the client-facing list.

Upstream already supported **Replace the Starting Directory in the returned paths** (critical for mock destination lengths). This fork keeps that, and adds:

- **Original path** kept separately from the **scored (length) path**
- **Display modes** after scan (Destination / Relative / Original) without re-scanning
- **Strip prefix when copying/exporting** (client handoff)
- **Export to file** (.csv / .txt)
- **OneDrive preset** (min length 400, strip-on-copy, destination display)
- Modernized WPF UI

## Quick start (GUI)

1. Build or download `PathLengthCheckerGUI`.
2. Set **Starting directory** to the share (or local copy) to scan.
3. Enable **Replace the Starting Directory...** and set the future OneDrive path, e.g.  
   `C:\Users\jdoe\OneDrive - Contoso\General`
4. Click **OneDrive preset (400)** (or set Min length to `400`).
5. Click **Get path lengths**.
6. Results are sorted longest-first. **Length** is the full destination/mock path length.
7. Leave **Strip prefix when copying/exporting** checked (defaults to the destination prefix).
8. **Copy paths** or **Export…** and send the list to the client.

Clients then see paths like:

```text
412: Clients\Acme Corp\Projects\2024\Very Long Project Name\...\file.pdf
```

instead of:

```text
412: C:\Users\jdoe\OneDrive - Contoso\General\Clients\Acme Corp\...
```

## Command line

```bash
PathLengthChecker RootDirectory="\\fs\DeptShare" \
  RootDirectoryReplacement="C:\Users\jdoe\OneDrive - Contoso\General" \
  MinLength=400 \
  DisplayMode=Destination \
  StripPrefix="C:\Users\jdoe\OneDrive - Contoso\General" \
  ExportFile="over-limit.txt"
```

Parameters (subset; run with no args / bad args for full help):

| Parameter | Meaning |
|---|---|
| `RootDirectory` | Starting directory (required) |
| `RootDirectoryReplacement` | Mock destination prefix for **length** |
| `MinLength` / `MaxLength` | Filter by scored length |
| `TypesToInclude` | `OnlyFiles` / `OnlyDirectories` / `All` |
| `SearchPattern` | Wildcard (`*`, `?`) |
| `DisplayMode` | `Destination` / `Relative` / `Original` |
| `StripPrefix` | Strip this prefix from printed paths (does not change Length) |
| `ExportFile` | Write results to a file |
| `UrlEncodePaths` | URL-encode scored paths |
| `Output` | `Paths` / `MinLength` / `MaxLength` |

## Build

Requires the .NET 10 SDK (library/tests) and Windows to build/run the WPF GUI.

```bash
# Library + tests (works on macOS/Linux/Windows)
dotnet test src/PathLengthChecker.Tests/PathLengthChecker.Tests.csproj -c Release

# GUI (Windows only)
dotnet build src/PathLengthCheckerGUI/PathLengthCheckerGUI.csproj -c Release

# Publish for a tech USB stick (run on Windows)
./build/publish-win-x64.sh
```

## Architecture notes

- **Scan once** → store `OriginalPath`
- **Score path** = original with optional root replacement + optional URL encode → **Length**
- **Display / export path** can be destination, relative, or original; optional **strip prefix** for client lists
- Changing display mode or strip options does **not** require a re-scan

## License and attribution

MIT. Original work by Daniel Schroeder / deadlydog. See [License.md](License.md).

## Changelog

See [Changelog.md](Changelog.md).

# Changelog

## Unreleased

- Default Windows path preset min length is **240** (shortcut-safe under 255), not 400. OneDrive cloud ~400 remains documented as an optional higher ceiling.

## v2.0.0 — OneDrive migration fork

Fork of deadlydog/PathLengthChecker, modernized for client-facing OneDrive path cleanup workflows.

### Packaging

- `build/build-release.ps1` — one-shot Windows publish (self-contained single-file GUI + CLI + zips).
- GitHub Actions workflow **Build and Release** publishes win-x64 zips on `v*` tags (and manual workflow_dispatch).

### Features

- .NET 10 SDK-style projects (library, CLI, WPF GUI, xUnit tests).
- `PathInfo` keeps **OriginalPath** separate from scored **Path** (length).
- New `PathFormatter` for root replacement, relative paths, strip-prefix, plain/CSV export.
- GUI: modern layout, Windows path preset (min length **240** for shortcut-safe limits; cloud OneDrive ~400 is optional), display mode dropdown (Destination / Relative / Original) without re-scan.
- GUI: **Strip prefix when copying/exporting** so clients do not see `C:\Users\...\OneDrive - ...\` noise.
- GUI: Export to `.csv` / `.txt`; copy plain or CSV; default sort longest-first.
- CLI: `DisplayMode`, `StripPrefix` / `CopyStripPrefix`, `ExportFile`.
- Explorer "Open directory" uses **OriginalPath** so it still works when display is replaced/stripped.
- Settings persisted as JSON under LocalAppData.

### Behavior preserved from upstream

- Root directory replacement for scored lengths.
- Min/max path length filters (against scored length).
- Search pattern, files/directories/all, subdirectories.
- URL encode paths (replace then encode).
- Cancel long searches, drag-drop folder onto window, CLI args on GUI.

### Technical

- Replaced AlphaFS with `System.IO` `EnumerationOptions` (`IgnoreInaccessible`, skip reparse points).
- Removed Extended.Wpf.Toolkit dependency.

---

## Upstream history (deadlydog/PathLengthChecker)

### v1.11.7 - August 26, 2021

Fixes:

- Update Search Pattern tooltip to mention the `*` is a wildcard character.

### v1.11.2 - February 21, 2021

Fixes:

- Fix bug where Starting Directory Replacement wouldn't take effect when also using URL Encoding.

### v1.11.0 - February 20, 2021

Features:

- Add new URL Encode Paths option for the both the GUI and command line app.
- Reset Options button now also clears out grid sorting.
- Update UI to group Search Options and Replacement Options separately.

(See git history of upstream for full pre-fork changelog.)

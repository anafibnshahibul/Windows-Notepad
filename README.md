# Windows Notepad

A modern, high-performance Windows 11 text editor built with WPF (.NET 8) and a native C++ engine for large-file handling — designed to replicate and go beyond the built-in Windows 11 Notepad.

![Platform](https://img.shields.io/badge/platform-Windows%2011-blue)
![License](https://img.shields.io/badge/license-GPLv3-green)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)

---

## Features

### Core
- Multi-tab editing with reordering, tearing/duplication, and session restore on relaunch
- Status bar: line/column/word/char counters, encoding selector (UTF-8, UTF-16, ANSI), zoom
- Find & Replace with regex, case sensitivity, and batch replace
- Word wrap, zoom (Ctrl+Scroll / Ctrl+/-), line number gutter
- Native Mica/Acrylic backdrop, rounded corners, auto dark/light theme sync

### Advanced
- Syntax highlighting: C, C++, C#, Python, HTML/XML, CSS, JSON, SQL, Markdown
- Live Markdown preview (split-pane, WebView2-rendered)
- Zen/Focus mode (F11) — distraction-free full screen
- Smart editing: move line up/down (Alt+↑/↓), duplicate line (Ctrl+D), line sorting, case conversion, timestamp insert (F5)
- Background auto-save/draft recovery to protect against crashes or reboots
- Native C++ engine (memory-mapped I/O + regex) for files >100MB

---

## Project Structure
```
winapp/
├── AdvancedNotepad.sln # Solution file
├── AdvancedNotepad.App.csproj # WPF app (.NET 8)
├── NativeTextEngine.vcxproj # C++17 native DLL
├── NativeTextEngine.cpp
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / .xaml.cs
├── MainViewModel.cs
├── EditorTabViewModel.cs
├── FindReplacePanel.xaml / .xaml.cs
├── FindReplaceViewModel.cs
├── MarkdownPreviewControl.xaml / .xaml.cs
├── SyntaxDetector.cs
├── SessionStateService.cs
├── AutoSaveService.cs
├── NativeTextEngineInterop.cs
├── InverseBooleanToVisibilityConverter.cs
├── Product.wxs # WiX v4 installer definition
├── ContextMenu.reg # Shell "Open in Advanced Notepad" entry
├── License.rtf, app.ico, banner.bmp, dialog.bmp
├── build.bat # Self-installing Windows build script
└── .gitignore
```


---

## Building

**Requires Windows 11** (WPF + native C++ DLL do not build or run on Linux/macOS).

### One-command build
Right-click `build.bat` → **Run as administrator**.

The script automatically installs anything missing via `winget`:
- .NET 8 SDK
- Visual Studio 2022 Build Tools (C++/VCTools + MSBuild workloads)
- WiX Toolset CLI + UI extension

Then it restores, builds the solution, and packages `AdvancedNotepad.msi`.

### Manual build
```powershell
dotnet restore AdvancedNotepad.sln
msbuild AdvancedNotepad.sln /p:Configuration=Release /p:Platform=x64
wix build Product.wxs -ext WixToolset.UI.wixext -arch x64 -o AdvancedNotepad.msi
```

---

## Installing

Run the generated `AdvancedNotepad.msi`. The installer:
- Shows a EULA (`License.rtf`) — GNU GPLv3
- Lets you pick the install directory
- Creates Start Menu and Desktop shortcuts
- Registers "Open in Advanced Notepad" in the right-click context menu for all files

---

## Keyboard Shortcuts

| Shortcut | Action |
|---|---|
| Ctrl+N | New tab |
| Ctrl+O | Open file |
| Ctrl+S | Save |
| Ctrl+H | Find/Replace |
| Ctrl+D | Duplicate line |
| Alt+↑ / Alt+↓ | Move line up/down |
| F5 | Insert timestamp |
| F11 | Zen/Focus mode |
| Ctrl+= / Ctrl+- | Zoom in/out |

---

## Status

This is an in-progress skeleton: buildable project structure, MVVM editor core, native interop boundary, and installer packaging are wired up. Not yet implemented / left as extension points:

- Tab drag-tear-to-new-window logic (`MainWindow.xaml.cs` has the event hook stub)
- `.xshd` grammar files for Python/SQL/Markdown (`SyntaxDetector.RegisterCustomHighlighting` looks for them under `Resources/` but they aren't shipped yet)
- Selection-based upper/lowercase commands in `EditorTabViewModel`
- Unit tests

---

## License

GNU General Public License v3.0 — see [License.rtf](./License.rtf) or https://www.gnu.org/licenses/gpl-3.0.html# Windows-Notepad

# OneNote MindMap (脑图)

> **English** | [中文](README.zh-CN.md)

**OneNote MindMap** is a Microsoft OneNote COM add-in that converts OneNote pages into editable mind maps and vice versa. It provides a standalone WPF editor with multiple layouts, themes, and shapes — no AI, no cloud, fully offline.

## Features

### From Page to Mind Map
- **Generate from Page** — Parse OneNote page outline hierarchy into a mind map tree
- **Edit Mind Map** — Open the WPF editor to drag, add, delete, and edit nodes
- **Save** — Store mind map data back to the page as hidden Meta (not visible text)

### From Mind Map to Page
- **Convert to Outline** — Render the mind map tree back into OneNote outline structure
- **Insert Preview** — Export the mind map as a PNG preview image and insert it into the page

### WPF Editor
- **Three layouts**: RightTree, BothSides, OrgChart
- **Five themes**: Default, Purple, Minimal, Fresh, Warm
- **Three node shapes**: Rounded, Rectangle, Pill
- **Export**: Save as PNG or SVG

### Ribbon Buttons (8 total)

| Button | Action |
|--------|--------|
| **New** | Create a new blank mind map |
| **Generate** | Parse current page content into a mind map |
| **Edit** | Open the WPF editor for the current mind map |
| **Outline** | Convert the mind map back into OneNote outline text |
| **Insert Preview** | Insert/update a PNG preview image of the mind map |
| **Export PNG** | Export the mind map as a PNG file |
| **Export SVG** | Export the mind map as an SVG file |
| **Help** | Show usage instructions |

## Screenshots

| Ribbon | Editor |
|--------|--------|
| ![Ribbon](docs/001.png) | ![Editor](docs/002.png) |

| Layouts | Themes |
|---------|--------|
| ![Layouts](docs/003.png) | ![Themes](docs/004.png) |

## Requirements

- Windows 7 or later
- Microsoft OneNote 2013 / 2016 / 2019 / Microsoft 365 (Desktop)
- .NET Framework 4.8

## Installation

1. Download the installer from [Releases](https://github.com/oldding/OneNoteMindMap/releases)
   - Choose `OneNoteMindMapSetup-1.3.1-x64.exe` for 64-bit OneNote
   - Choose `OneNoteMindMapSetup-1.3.1-x86.exe` for 32-bit OneNote
2. Run the installer (admin privileges required)
3. Restart OneNote
4. You'll see a new **脑图** tab in the ribbon

## Build from Source

```powershell
.\build.ps1
```

Requires:
- Visual Studio 2022 Build Tools (or full VS) with MSBuild
- Inno Setup 6 (with ISCC.exe in PATH)

## Project Structure

```
src/
├── OneNoteMindMap.Core/          # Shared library (model, layout, serialization, parsing)
│   ├── Layout/                   # Layout engine (RightTree, BothSides, OrgChart)
│   ├── Model/                    # MindMapDocument, MindMapNode, MindMapSettings
│   ├── Parsing/                  # OutlineToMindMapBuilder (OneNote XML → mind map)
│   ├── Rendering/                # SvgRenderer, Theme definitions
│   └── Serialization/            # JSON codec, serializer
├── OneNoteMindMap.AddIn/         # COM add-in DLL
│   ├── AddIn/                    # Entry point (Connect.cs, RibbonHandler.cs)
│   ├── Features/                 # 8 feature command implementations
│   ├── OneNote/                  # OneNote COM interop (provider, parser, writer, page store)
│   ├── UI/                       # UI helpers & i18n Strings
│   ├── Logging/                  # File logger
│   └── Ribbon/                   # Custom UI XML & icons
├── OneNoteMindMap.Editor/        # WPF mind map editor
│   ├── MindMapEditorWindow.xaml  # Main editor window
│   ├── NodeControl.xaml          # Node UI control
│   ├── MindMapEditorViewModel.cs # ViewModel
│   ├── LinkGeometryBuilder.cs    # Connector line geometry
│   └── PngExporter.cs            # WPF RenderTargetBitmap export
└── OneNoteMindMap.Installer/     # Inno Setup installer scripts
```

## Tech Stack

- **Language**: C# 9.0
- **Framework**: .NET Framework 4.8
- **Runtime**: COM Add-in (IDTExtensibility2 + IRibbonExtensibility)
- **OneNote Interop**: Microsoft.Office.Interop.OneNote (v15.0)
- **WPF**: PresentationFramework, RenderTargetBitmap
- **JSON**: Built-in lightweight JSON serializer (no external dependencies)
- **Installer**: Inno Setup 6 (x86/x64)
- **Build**: Visual Studio 2022+ / MSBuild

## License

[MIT](LICENSE) © 2026 OneNoteMindMap Contributors

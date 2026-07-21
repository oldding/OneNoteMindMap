# Contributing to OneNoteMindMap

Thank you for your interest in contributing!

## Getting Started

1. Fork the repository and clone it locally.
2. Open `OneNoteMindMap.sln` in Visual Studio 2022.
3. Build the solution (`Ctrl+Shift+B`).
4. Test your changes by registering the COM add-in with OneNote.

## Build Requirements

- Visual Studio 2022 (or Build Tools) with MSBuild
- .NET Framework 4.8 targeting pack
- Inno Setup 6 (for installer builds)

## Project Structure

| Project | Description |
|---------|-------------|
| `OneNoteMindMap.Core` | Shared library — model, layout, serialization, parsing |
| `OneNoteMindMap.AddIn` | COM add-in DLL — OneNote interop, ribbon, commands |
| `OneNoteMindMap.Editor` | WPF mind map editor |
| `OneNoteMindMap.Installer` | Inno Setup installer scripts |

## Code Style

- Follow the existing C# conventions (4-space indent, PascalCase for public members).
- Use `EditorStrings.If(zh, en)` / `Strings.If(zh, en)` for user-facing strings.
- Keep COM interop code isolated in `OneNote/Interop/`.
- Add XML doc comments on public API methods.

## Pull Request Process

1. Create a feature branch from `main`.
2. Keep changes focused — one feature or fix per PR.
3. Ensure the solution builds without warnings.
4. Update README if user-facing behavior changes.
5. Submit a PR with a clear description of what and why.

## Reporting Issues

- Use the GitHub issue tracker.
- Include OneNote version, Windows version, and steps to reproduce.
- Attach the log file from `%LOCALAPPDATA%\OneNoteMindMap\log.txt` if relevant.

# DayZTypesHelper

A lightweight **Windows WinForms** helper for editing DayZ `types.xml` data across many classnames.

**Author:** Psyern

## Features

- Import classname list from plain text (`.txt`) files.
- Ignore empty lines and comment lines (`#` and `//`) during class import.
- **Import from existing `types.xml`** – loads all classnames and their values, auto-populates Categories, Tags, UsageFlags, ValueFlags.
- **Import / Edit / Export DayZ Expansion Market JSON** files (prices, stock, quantity, attachments, variants).
- **Create new Market JSON** from selected classnames.
- Searchable classname list.
- **Multi-select classnames** (Ctrl/Shift) for **bulk editing**.
- **Bulk Apply** – apply current editor values to all selected classnames at once.
- Per-class in-memory cache of edits.
- **Undo / Redo** per classname (Ctrl+Z / Ctrl+Y).
- Debounced autosave (500ms) to selected destination `types.xml`.
- Save on class switch and on app close.
- Export current or all classnames.
- **Dark Mode** toggle with modern dark colour scheme.
- **Robust error handling** for corrupt/invalid XML files.

## Tech Stack

- .NET 9
- WinForms
- C#
- `System.Xml.Linq` for XML parsing/writing
- `System.Text.Json` for Market JSON parsing/writing
- xUnit for unit tests
- No external NuGet dependencies (main project)

## Project Structure

- `DayZTypesHelper.sln`
- `DayZTypesHelper/`
  - `DayZTypesHelper.csproj`
  - `Program.cs`
  - `MainForm.cs`
  - `DarkModeHelper.cs`
  - `Models/TypeEntry.cs`
  - `Models/MarketItem.cs`
  - `Services/ClassnameListService.cs`
  - `Services/TypesXmlService.cs`
  - `Services/MarketJsonService.cs`
  - `Services/UndoRedoService.cs`
- `DayZTypesHelper.Tests/`
  - `DayZTypesHelper.Tests.csproj`
  - `ClassnameListServiceTests.cs`
  - `TypesXmlServiceTests.cs`
  - `TypeEntryTests.cs`
  - `UndoRedoServiceTests.cs`

## Build & Run (Windows)

1. Install .NET 9 SDK.
2. Open `DayZTypesHelper.sln` in Visual Studio 2022.
3. Build and run.

Or via CLI:

```bash
dotnet build DayZTypesHelper.sln
dotnet run --project DayZTypesHelper/DayZTypesHelper.csproj
```

### Publish as standalone EXE

```bash
dotnet publish DayZTypesHelper/DayZTypesHelper.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish-standalone
```

This creates a single `DayZTypesHelper.exe` (~108 MB) that runs on any Windows 64-bit PC without needing .NET installed.

## Running Tests

```bash
dotnet test DayZTypesHelper.sln
```

## Quick Manual Test Checklist

1. **Import Classnamelist**
   - Ensure classes are loaded, sorted, and searchable.
2. **Import types.xml**
   - Click "Import types.xml" to load classnames + values from an existing file.
3. **Select destination types.xml**
   - Use existing or new destination file.
4. **Edit values**
   - Modify numeric fields/flags/list selections.
   - Switch class and verify values persist.
5. **Multi-select & Bulk Apply**
   - Select multiple classes (Ctrl+Click), edit values, click "Bulk Apply".
6. **Undo / Redo**
   - Make changes, press Ctrl+Z to undo, Ctrl+Y to redo.
7. **Validation**
   - `quantmin`/`quantmax` must be `-1` or `1..100`.
   - MinPriceThreshold ≤ MaxPriceThreshold (auto-clamped).
   - MinStockThreshold ≤ MaxStockThreshold (auto-clamped).
8. **Autosave/Export**
   - Wait ~500ms after edit and verify autosave.
   - Use **Export now** and verify write.
9. **Dark Mode**
    - Toggle "Dark Mode" checkbox to switch theme.
10. **Close app**
    - Verify latest valid edits persist.
11. **Corrupt XML**
    - Try loading a corrupt XML file – expect a clear error message.

## Notes

- This tool targets **Windows only** (`net9.0-windows`).
- If no destination is selected, autosave/export-to-file is skipped.
- Dark Mode is enabled by default.

## License

This project is licensed under the **MIT License** – see below.

```
MIT License

Copyright (c) 2026 Psyern

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

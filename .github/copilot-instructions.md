# ComicRackCE - Copilot Instructions

## Project Overview

ComicRack Community Edition is a Windows desktop comic book reader and library manager.
It is a .NET 9 Windows Forms application with Python scripting support via Python.NET.

- **Solution:** `ComicRack.sln`
- **Framework:** .NET 9.0-windows (our `dotnet9` branch)
- **Build command:** `dotnet build ComicRack.sln -c Debug`
- **Build verification (Release):** `dotnet build ComicRack\ComicRack.csproj -c Release`

### CRITICAL: .NET 9 vs Upstream .NET Framework 4.8

The upstream repo (`maforget/ComicRackCE` master) targets **.NET Framework 4.8** and builds with `msbuild`.
Our `dotnet9` branch targets **.NET 9.0-windows** and builds with `dotnet build`.

Key differences:
- Our branch has `ComicRack.Plugins` project with `LogManager`, `PythonRuntimeManager`, `PythonCommand`
- Our branch has `ComicRack.Plugins.LegacyHost` project for IronPython sidecar
- Upstream does NOT have these projects — do NOT add `using ComicRack.Plugins;` to upstream code
- When resolving conflicts, the sync branch is based on `dotnet9` (our code), and you merge upstream into it

## Repository Structure

This is a **fork** of `maforget/ComicRackCE`. The `dotnet9` branch contains our .NET 9 migration
with additional features (Python tracing, script management, IronPython sidecar hosting).

### Key Projects

| Project | Purpose |
|---------|---------|
| `ComicRack/` | Main WinForms application (UI, dialogs, program entry) |
| `ComicRack.Engine/` | Core engine (comic book model, I/O providers, image formats) |
| `ComicRack.Plugins/` | Plugin system (Python.NET integration, script runtime) |
| `ComicRack.Plugins.LegacyHost/` | IronPython 2.7 sidecar process for legacy plugin support |
| `cYo.Common/` | Common utilities |
| `cYo.Common.Windows/` | Windows Forms utilities and custom controls |

### Important Files

- `ComicRack/Program.cs` — Application entry point, startup/shutdown lifecycle
- `ComicRack.Plugins/PythonRuntimeManager.cs` — Python.NET integration
- `ComicRack/Config/ExtendedSettings.cs` — CLI switches and INI settings
- `ComicRack.Engine/EngineConfiguration.cs` — Engine configuration properties

## Upstream Sync Pattern

An automated workflow (`sync-upstream.yml`) syncs from `maforget/ComicRackCE` (master)
into our `dotnet9` branch. When merge conflicts occur, the PR needs manual resolution.

### Conflict Resolution Rules

When resolving upstream sync conflicts:

**Step-by-step process:**
1. You are on a `sync-upstream-*` branch based on `dotnet9` (our .NET 9 code)
2. The latest commit on the branch **already contains the conflicted merge** — look for `<<<<<<<`, `=======`, `>>>>>>>` conflict markers in the listed files
3. Open each conflicted file and resolve the markers (keep code from both sides per the rules below)
4. Stage the resolved files: `git add <file>`
5. Amend the commit: `git commit --amend --no-edit` (or create a new commit)
6. Verify the build: `dotnet build ComicRack.sln -c Debug`
7. Push the branch

**Rules:**

1. **Always preserve both sides' intent:**
   - Our `dotnet9` additions (Python tracing, logging, sidecar hosting) must be kept
   - Upstream's new features and fixes must be integrated

2. **Common conflict patterns in `Program.cs`:**
   - Our branch adds `LogManager.Debug()` calls around shutdown steps
   - Our branch adds `PythonRuntimeManager.Instance.Shutdown()` in `CleanUp()`
   - Our branch has `Process.Kill()` failsafe at the end of `CleanUp()`
   - Our branch has `Restart` logic in `CleanUp()` (restart process before kill)
   - Upstream may add new guards (e.g., `!ExtendedSettings.DisableBackupManager`)
   - **Resolution:** Keep our logging/Python shutdown/kill sequence AND apply upstream's guard conditions
   - The `using ComicRack.Plugins;` directive is ALREADY present in our dotnet9 branch — do not add it if not there

3. **For `ExtendedSettings.cs` conflicts:**
   - Our branch may add new settings, upstream may add different ones
   - **Resolution:** Include both sets of new properties

4. **For Designer files (`.Designer.cs`):**
   - Never manually merge designer files — take whichever version has the needed controls
   - If both sides add controls, prefer upstream's designer output and add our controls after

5. **General rule:** When in doubt, keep ALL code from both sides in logical order.
   The goal is feature parity — nothing should be lost from either branch.

## Code Conventions

- Use `LogManager.Debug("System", "message")` for debug logging (not `Console.WriteLine`)
- Use `PascalCase` for methods and properties, `_camelCase` for private fields
- Follow existing patterns in surrounding code
- Preserve XML doc comments and inline comments

## Build Verification

After any change, verify:
```bash
dotnet build ComicRack.sln -c Debug
```

For Release verification (matches CI):
```bash
dotnet build ComicRack\ComicRack.csproj -c Release
```

**Important:** Do NOT use `msbuild` — that is the upstream's .NET Framework 4.8 build system.
Our branch always uses `dotnet build`.

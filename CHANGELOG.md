# Changelog

## Unreleased (Modernization)

If you are coming from the original ComicRack (circa 2014), you will notice significant architectural changes. This release represents a comprehensive modernization of the codebase.

### Core Architecture
- **.NET 9 Upgrade**: The entire application has been migrated from .NET Framework 4.5 to .NET 9. This ensures compatibility with modern operating systems, improves performance, and simplifies the build process.
- **Python Engine Implementation**: The deprecated IronPython engine has been replaced with **Python.NET**. Scripts now execute using an embedded CPython 3.12 engine, offering better compatibility and performance than the legacy implementation.

### Developer Experience
- **Asynchronous Console**: The Script Console has been rewritten to run asynchronously, preventing the UI from freezing during script execution.
- **Enhanced Tracing**: A new tracing system generates detailed execution logs for Python scripts using `trace_report_*.log` files, simplifying the debugging process.
- **Type Bridging**: A new `clr_bridge.py` layer manages type conversions between .NET and Python, handling common issues like Integer/Decimal interoperability automatically.

### General Improvements
- **High DPI Support**: Updated UI components to render correctly on high-resolution displays.
- **Stability Fixes**: Resolved numerous "Object reference not set" exceptions and stability issues present in the decompiled source.
- **Self-Contained Build**: The build system now automatically handles the downloading and embedding of the Python runtime.

---

*(For the comprehensive list of file-by-file changes, check `Fork.txt` in the output folder)*

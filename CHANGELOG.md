# Changelog

## Unreleased (Modernization)

Hey! If you're coming from the original ComicRack (circa 2014), things look a little different under the hood. Here's a relaxed breakdown of what we've been tearing apart and putting back together.

### The Big Stuff
- **Goodbye .NET 4.5, Hello .NET 9**: We dragged the code kicking and screaming from 2012 into the modern era. The app is faster, lighter, and actually builds on modern machines without headers from Windows 7.
- **IronPython is Dead**: The old IronPython engine was holding us back. We ripped it out and wired in **Python.NET**. This means scripts now run on a *real* embedded CPython 3.12 engine. You can arguably use `pip` packages now (if you're brave).

### for the Developers
- **No More freezing**: The Script Console used to lock up the entire UI. we rewrote it to run asynchronously. Type away.
- **We fixed the tracing**: Trying to debug scripts was like reading tea leaves. We added a full execution tracer that dumps exactly what line your script crashed on into a log file.
- **The Bridge**: We wrote a new `clr_bridge.py` layer that sits between the C# app and Python. It handles all the weird type casting (like turning Python integers into .NET Decimals) so you don't have to think about it as much.

### Fixes & Tweaks
- **High DPI**: The UI shouldn't look fuzzy on 4K monitors anymore.
- **Crash Fixes**: Fixed a ton of "Object reference not set" errors that plagued the legacy version.
- **Build System**: The build script now downloads its own Python runtime. No more "Please install Python 2.7" errors.

---

*(For the classic boring changelog, check `Changes.txt` in the output folder)*

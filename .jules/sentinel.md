## 2026-07-26 - Path Traversal in Filename Components
**Vulnerability:** The `BuildOutputPath` function accepted user-provided filenames and appended them directly to an output directory using `Path.Combine`, allowing path traversal via absolute paths or directory traversal sequences.
**Learning:** `Path.GetFileName()` or `Path.GetFileNameWithoutExtension()` alone can be insufficient to sanitize mixed cross-platform slashes (e.g., Linux path parsing ignoring backslashes).
**Prevention:** Normalize all cross-platform slashes `.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)` before applying `Path.GetFileName()`, and provide a safe default like "output" if the resulting string is empty.

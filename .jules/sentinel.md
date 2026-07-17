## 2024-07-17 - [Path Traversal bypass due to Path.GetFileName on Linux]
**Vulnerability:** The `theme` directive could bypass `Path.GetFileName` sanitation on Linux environments using backslashes (e.g. `..\..\etc\passwd`), allowing arbitrary file reads during diagram conversion.
**Learning:** In .NET, `Path.GetFileName` respects only the host OS directory separator. On Linux, a backslash is treated as a valid filename character, not a path separator.
**Prevention:** Always normalize slashes (`.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)`) before passing untrusted paths to `Path.GetFileName` or `Path.GetFileNameWithoutExtension`.

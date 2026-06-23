
## 2024-05-15 - [Path Traversal in Path.Combine]
**Vulnerability:** The application used `Path.Combine(outputDir, fileName)` where `fileName` was partially controlled by the user. If a user provided a filename like `..\..\file.vsdx` or `C:\file.vsdx`, it would traverse out of the intended directory or completely overwrite the base path due to how `Path.Combine` handles absolute paths.
**Learning:** `Path.Combine` does not automatically sanitize its arguments. If the second argument is an absolute path or contains path traversal sequences, it can override the base path.
**Prevention:** Always rigorously sanitize untrusted input before appending it via `Path.Combine`. Normalize cross-platform slashes (e.g., `.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)`) before applying `System.IO.Path.GetFileName()`, and provide a safe default string (e.g., 'output') if the result is empty.

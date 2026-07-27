## 2024-05-24 - Fix Path.Combine Path Traversal
**Vulnerability:** Path traversal in `BuildOutputPath` via `Path.Combine(outputDir, fileName)` where `fileName` was not sanitized. An absolute path or `..` could override the output directory.
**Learning:** `System.IO.Path.Combine` doesn't sanitize arguments. If the second argument is an absolute path, it overrides the base path. Also, cross-platform slashes must be normalized before using `Path.GetFileName()`.
**Prevention:** Always normalize slashes and use `Path.GetFileName()` to extract just the filename from user input, and handle the empty string edge case, before using it in `Path.Combine()`.

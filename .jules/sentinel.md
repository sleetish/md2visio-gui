
## 2024-05-18 - Path Traversal via Unsanitized Path.Combine
**Vulnerability:** Path traversal vulnerability in `BuildOutputPath` due to passing unsanitized user input (`fileName`) directly as the second argument to `Path.Combine`. If the second argument is an absolute path or contains traversal characters, it can escape the intended directory.
**Learning:** In .NET, `Path.Combine` does not sanitize arguments. If the second argument is an absolute path, it completely overrides the first argument. Also, `Path.GetFileName` may fail to strip paths if the slashes don't match the OS separator, so cross-platform slash normalization is required before extraction.
**Prevention:** Always sanitize user-provided filenames. Normalize slashes (`Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)`), use `Path.GetFileName()` to extract only the filename component, and provide a safe default if the resulting string is empty.

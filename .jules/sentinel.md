## 2025-02-27 - [Fix Path Traversal in BuildOutputPath]
**Vulnerability:** Path traversal vulnerability in `BuildOutputPath` due to unsanitized `fileName` input appending directly to `outputDir` using `Path.Combine`.
**Learning:** `Path.Combine` in .NET does not sanitize paths. If the second argument is an absolute path or contains path traversal sequences, it overrides the base path or escapes the intended directory.
**Prevention:** Always normalize path separators and use `Path.GetFileName()` to extract only the file name portion from untrusted input before using `Path.Combine`. Always provide a safe fallback string if the sanitized filename ends up empty.

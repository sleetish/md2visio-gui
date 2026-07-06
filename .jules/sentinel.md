## 2024-05-24 - Path Traversal in ConversionService
**Vulnerability:** Path traversal in `BuildOutputPath` in `md2visio.GUI/Services/ConversionService.cs` where an attacker could control the `fileName` parameter and write output to arbitrary locations using `Path.Combine`.
**Learning:** `Path.Combine` doesn't sanitize paths and will override the base directory if the second argument is an absolute path or uses `..\` traversal sequences.
**Prevention:** Always normalize slashes and use `Path.GetFileName` on user-provided filenames before passing them to `Path.Combine`. Handle edge cases where the sanitized string is empty.

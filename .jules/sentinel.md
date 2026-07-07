## 2025-02-14 - Fix Path Traversal in BuildOutputPath
**Vulnerability:** User-provided filename components passed to `Path.Combine` without sanitization can lead to path traversal overriding the base path entirely in .NET, which allows saving `.vsdx` files to arbitrary locations.
**Learning:** `Path.Combine` does not sanitize arguments, and if the second argument is an absolute path or contains path traversal sequences, it overrides the base path entirely.
**Prevention:** Always sanitize user-provided filename components by normalizing cross-platform slashes and applying `Path.GetFileName()` before passing them to `Path.Combine`. Handle empty strings.

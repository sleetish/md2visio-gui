## 2025-01-31 - [Path Traversal in Path.Combine]
**Vulnerability:** Path.Combine allows overriding the base path if the second argument is an absolute path or contains directory traversal sequences (e.g., `../`). This can lead to arbitrary file write.
**Learning:** `System.IO.Path.Combine` does not automatically sanitize its arguments.
**Prevention:** Always rigorously sanitize untrusted input before appending it via `Path.Combine` by normalizing cross-platform slashes and applying `Path.GetFileName()`, handling the edge case where the sanitized result is empty by providing a safe default string.

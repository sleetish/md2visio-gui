## 2024-07-19 - [Path Traversal via Path.Combine]
**Vulnerability:** Path.Combine allows path traversal if the user-controlled second parameter is an absolute path or contains traversal sequences.
**Learning:** `Path.Combine` does not sanitize arguments. If the second argument is an absolute path, it overrides the base path entirely.
**Prevention:** Always sanitize untrusted input before appending it via `Path.Combine` by normalizing cross-platform slashes and extracting just the filename with `Path.GetFileName()`, followed by providing a default if empty.

## 2024-05-18 - [Path Traversal via Path.Combine]
**Vulnerability:** Path.Combine allows absolute paths or path traversal sequences in the secondary argument to override the base directory, allowing arbitrary file writes.
**Learning:** System.IO.Path.Combine does not sanitize arguments. If the second argument is an absolute path, it overrides the base path entirely.
**Prevention:** Always sanitize untrusted input (e.g., using Path.GetFileName) before appending it via Path.Combine.

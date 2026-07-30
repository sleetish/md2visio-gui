## 2024-07-30 - Path Traversal in Path.Combine
**Vulnerability:** Path.Combine allows path traversal if the second argument is an absolute path or contains path traversal sequences like "../".
**Learning:** System.IO.Path.Combine does not sanitize input arguments.
**Prevention:** Always sanitize untrusted input by replacing cross-platform slashes and applying Path.GetFileName() before appending it via Path.Combine, ensuring a fallback string if the result is empty.

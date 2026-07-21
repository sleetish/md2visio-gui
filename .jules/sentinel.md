## 2024-07-21 - Path Traversal in Path.Combine
**Vulnerability:** Path.Combine allows overriding the base directory if the user-supplied filename is an absolute path or contains path traversal sequences like '../'.
**Learning:** .NET Path.Combine does not sanitize input. If user-controlled filenames are passed directly, it can write outside the intended directory.
**Prevention:** Always normalize slashes and use Path.GetFileName() on user-provided components before appending them via Path.Combine, handling the case where it becomes empty.

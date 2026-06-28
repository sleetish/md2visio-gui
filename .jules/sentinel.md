## 2024-06-28 - Path Traversal in Path.Combine
**Vulnerability:** Path.Combine appended unsanitized user-provided filename strings, allowing path traversal (e.g., passing "../" to escape the target directory).
**Learning:** Path.Combine does not sanitize input. If the second argument contains directory traversal characters or is an absolute path, it overrides the base path.
**Prevention:** Always normalize slashes and use Path.GetFileName() on untrusted components before appending them via Path.Combine.

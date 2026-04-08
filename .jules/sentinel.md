## 2024-05-18 - Prevent Path Traversal when using Path.Combine
**Vulnerability:** Path Traversal
**Learning:** `System.IO.Path.Combine` does not sanitize paths. If the second argument is an absolute path or contains path traversal sequences like `../`, it can override the base path or navigate outside intended directories.
**Prevention:** Always sanitize user-provided filename components to prevent path traversal vulnerabilities. Normalize cross-platform slashes (e.g., `.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)`) before applying `System.IO.Path.GetFileName()`. Always handle the edge case where the sanitized result is empty by providing a safe default string.

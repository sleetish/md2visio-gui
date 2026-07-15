## 2025-01-20 - Fix Path Traversal in ConversionService
**Vulnerability:** User-provided filename is passed directly to `Path.Combine` without sanitization in `BuildOutputPath`, leading to path traversal and arbitrary file write vulnerabilities.
**Learning:** `Path.Combine` does not sanitize paths. If the second argument is an absolute path or contains path traversal sequences like `..\`, it will override the base path or navigate outside the intended directory.
**Prevention:** Always sanitize user-provided filename components to prevent path traversal vulnerabilities. Normalize cross-platform slashes, apply `Path.GetFileName()`, and handle edge cases where the sanitized result might be empty.

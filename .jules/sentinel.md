## 2024-05-24 - Path Traversal in Path.Combine
**Vulnerability:** Path.Combine allows absolute paths or path traversal sequences in the second argument to completely overwrite the base path or navigate out of the intended directory.
**Learning:** In .NET, Path.Combine does not inherently sanitize arguments. Path components coming from untrusted sources must be explicitly sanitized, as Linux ignores backslashes and Path.GetFileName might not be sufficient for mixed slashes.
**Prevention:** Normalize cross-platform slashes before applying Path.GetFileName, and ensure a safe default string is used if the resulting filename is empty.

## 2024-05-24 - [Fix Path Traversal in File Name]
**Vulnerability:** Path traversal vulnerability in `BuildOutputPath` where an unsanitized `fileName` argument is appended to a base path using `Path.Combine`.
**Learning:** `Path.Combine` doesn't sanitize input. A malicious `fileName` (e.g., `../../../etc/passwd` or absolute paths like `/root/secret.txt`) overrides the base path entirely. Furthermore, using `Path.GetFileName()` alone might not be sufficient if slashes are cross-platform (e.g., Linux ignoring backslashes).
**Prevention:** Always normalize slashes (`Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)`) before calling `Path.GetFileName()`, and handle edge cases where the resulting filename is empty or whitespace by providing a safe default.

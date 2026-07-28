## 2024-07-28 - Path Traversal Vulnerability
**Vulnerability:** User-controlled filenames were passed unsanitized into Path.Combine, enabling arbitrary file creation via path traversal sequences.
**Learning:** In .NET, Path.Combine does not sanitize input, and secondary arguments with absolute paths or traversal sequences override or traverse the primary path entirely.
**Prevention:** Always normalize path separators and apply Path.GetFileName on untrusted input before using Path.Combine. Handle empty/whitespace results with safe fallbacks.

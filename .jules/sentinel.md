
## 2025-02-17 - [Path Traversal in Output File Parameter]
**Vulnerability:** The `ConversionService.BuildOutputPath` method combined a user-controlled `fileName` directly with an output directory using `Path.Combine()`. Because `Path.Combine` doesn't sanitize the second argument, absolute paths or directory traversal sequences (`../`) could override the output directory and write files elsewhere.
**Learning:** `System.IO.Path.Combine` does not automatically sanitize user input, and naive usage can lead to arbitrary file writes.
**Prevention:** Always sanitize user-provided filename components to prevent path traversal vulnerabilities. Normalize cross-platform slashes (`.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)`) before applying `System.IO.Path.GetFileName()`, and provide a safe fallback (e.g., "output") if the resulting string is empty.

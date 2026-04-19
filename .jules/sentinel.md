## 2024-04-19 - Path Traversal in File Output

**Vulnerability:** Path traversal in `ConversionService.BuildOutputPath` where `Path.Combine(outputDir, fileName)` was used directly without sanitizing the user-controlled `fileName` parameter. An attacker could use mixed slashes (e.g., `..\` or `../`) to bypass simple sanitization and save the Visio file to unintended directories.

**Learning:** `Path.Combine` doesn't sanitize cross-platform path traversal sequences. `Path.GetFileName()` might not work effectively against mixed slashes. When sanitizing filename inputs, it's critical to first normalize slashes to `Path.DirectorySeparatorChar` before using `Path.GetFileName()`, and then verify the remaining string is not empty or handle it safely (e.g., provide a default).

**Prevention:** Always normalize slashes before extracting filenames with `Path.GetFileName` and provide a fallback if the sanitized result is empty. Never trust user-provided filename strings in `Path.Combine` without rigorous sanitization.

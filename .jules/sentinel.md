## 2026-01-07 - Fix Path Traversal in ConversionService
**Vulnerability:** A path traversal vulnerability existed in the `ConversionService.BuildOutputPath` method, where an attacker could provide a malicious `fileName` (e.g., `../../../malicious.vsdx`) to write output files outside the intended `outputDir`.
**Learning:** Using `Path.Combine(outputDir, fileName)` without sanitizing the `fileName` allows the second argument to break out of the base directory if it contains path traversal characters (`../`) or absolute paths.
**Prevention:** Always sanitize untrusted input before using it in `Path.Combine`. Use `Replace` to normalize cross-platform slashes and `Path.GetFileName()` to extract only the filename. Always provide a safe fallback if the result is empty.

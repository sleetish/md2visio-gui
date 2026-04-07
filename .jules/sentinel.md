## 2024-05-18 - Path Traversal in ConversionService
**Vulnerability:** `BuildOutputPath` in `ConversionService.cs` was vulnerable to path traversal because it passed unsanitized user-provided `fileName` directly to `Path.Combine`. This could allow attackers to write files outside the intended `outputDir`.
**Learning:** In .NET, `Path.Combine` doesn't sanitize arguments; if the second argument is an absolute path or contains traversal characters (`../`), it can override or escape the base path.
**Prevention:** Always sanitize user-provided filename components by normalizing cross-platform slashes, using `Path.GetFileName()` to extract only the filename part, and providing a safe default if the result is empty.

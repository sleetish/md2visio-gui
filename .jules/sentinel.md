## 2024-05-11 - Path Traversal in ConversionService
**Vulnerability:** The `BuildOutputPath` function in `md2visio.GUI/Services/ConversionService.cs` constructed file paths using `Path.Combine(outputDir, fileName)` where `fileName` was derived directly from the user-provided or auto-detected output name, allowing arbitrary path traversal (e.g. `../../../passwd`).
**Learning:** `Path.Combine` doesn't sanitize the inputs it's given, relying purely on the caller to ensure they are clean.
**Prevention:** Always normalize the path separators first and then use `Path.GetFileName()` to extract only the final component of a path, handling empty outcomes with a safe fallback.

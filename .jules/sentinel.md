## 2025-04-16 - [Path Traversal in ConversionService]
**Vulnerability:** The `BuildOutputPath` method inside `ConversionService.cs` constructed paths via `Path.Combine` without sanitizing the user-provided `fileName` input, potentially allowing directory traversal attacks (e.g. `../../../etc/passwd` or using `\` on Windows).
**Learning:** `Path.Combine` in .NET doesn't prevent traversal. If `fileName` starts with root paths or `..`, it can overwrite the intended path constraint.
**Prevention:** Always sanitize paths before appending. Replace cross-platform directory slashes and use `Path.GetFileName()` to extract only the filename.

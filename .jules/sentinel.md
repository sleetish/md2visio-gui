## 2024-07-31 - Path Traversal in BuildOutputPath
**Vulnerability:** The `BuildOutputPath` method in `md2visio.GUI/Services/ConversionService.cs` concatenated a user-controlled `fileName` directly into `Path.Combine`. In .NET, if the second argument to `Path.Combine` is an absolute path or contains path traversal sequences, it overrides the base path, allowing arbitrary file writes outside the intended output directory.
**Learning:** `System.IO.Path.Combine` does not sanitize arguments. Always sanitize untrusted input before appending it via `Path.Combine`.
**Prevention:** Normalize cross-platform slashes and use `System.IO.Path.GetFileName()` to extract only the file name component, ensuring an empty or whitespace result is handled with a safe default string.

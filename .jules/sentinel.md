## 2024-05-24 - Fix Path Traversal in Path.Combine
**Vulnerability:** The `BuildOutputPath` method in `md2visio.GUI/Services/ConversionService.cs` used `Path.Combine(outputDir, fileName)` without sanitizing `fileName`, which could allow an attacker to overwrite arbitrary files using path traversal sequences like `../` or absolute paths.
**Learning:** In .NET, `System.IO.Path.Combine` does not sanitize arguments. If the second argument is an absolute path or contains path traversal sequences, it overrides the base path entirely.
**Prevention:** Always sanitize untrusted input before appending it via `Path.Combine`. Normalize cross-platform slashes and use `Path.GetFileName()` to extract only the filename component, providing a safe default if the result is empty.

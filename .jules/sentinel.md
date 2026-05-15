
## 2024-05-18 - [Path Traversal in Output Generation]
**Vulnerability:** The `BuildOutputPath` method in `md2visio.GUI/Services/ConversionService.cs` constructed an output file path using `Path.Combine(outputDir, fileName)` without sanitizing the `fileName` input. If a user provided a filename with path traversal characters (like `..\..\`), it could override the base `outputDir` and allow arbitrary file writing outside the intended directory.
**Learning:** `System.IO.Path.Combine` does not automatically sanitize its arguments. If an argument contains path traversal sequences or is an absolute path, it can escape the intended base directory.
**Prevention:** Always rigorously sanitize untrusted input before using it in file path construction. Replace cross-platform path separators with the system's `Path.DirectorySeparatorChar` before applying `Path.GetFileName()` to extract only the filename component, and ensure a safe default string is used if the result is empty.

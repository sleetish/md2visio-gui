## Sentinel Security Journal

## 2024-05-28 - Path Traversal Vulnerability in GUI Filename Handling
**Vulnerability:** In `md2visio.GUI/Services/ConversionService.cs`, the `BuildOutputPath` method combined the `outputDir` and a user-provided `fileName` directly using `Path.Combine(outputDir, fileName)`. This allowed path traversal (e.g., passing `..\..\..\Windows\System32\cmd.exe` as `fileName`), overriding the base output directory completely and saving output files to arbitrary locations.
**Learning:** `Path.Combine` in .NET is unsafe when the second parameter can be an absolute path or contain traversal sequences (`..\`). It does not sanitize inputs; it merely concatenates them or completely replaces the base path if the suffix is an absolute path. The `fileName` parameter comes from `_fileNameTextBox.Text` in the GUI, which is user-controlled.
**Prevention:** Always extract just the file name using `Path.GetFileName()` after normalizing slashes (because `Path.GetFileName` may fail on Windows-style backslashes running in a non-Windows environment or vice-versa), then handle the edge case where the result is empty by providing a safe default, before using `Path.Combine()`.

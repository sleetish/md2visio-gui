
## 2025-01-20 - Fix Path Traversal in ConversionService
**Vulnerability:** The `BuildOutputPath` method in `md2visio.GUI/Services/ConversionService.cs` directly passed a user-provided or unverified `fileName` variable into `Path.Combine(outputDir, fileName)`. This can lead to a path traversal vulnerability if the `fileName` string contains traversal characters (e.g., `../` or `..\`) or is an absolute path.
**Learning:** `System.IO.Path.Combine` doesn't automatically sanitize arguments. Providing an absolute path or path traversal string as the second argument can cause it to escape the intended `outputDir`.
**Prevention:** Always sanitize inputs that form file paths. Normalize cross-platform slashes and extract just the file name using `System.IO.Path.GetFileName()`. A fallback value should also be specified in case the resulting file name becomes empty.

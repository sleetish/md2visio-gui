## 2024-05-27 - [CRITICAL] Path Traversal in ConversionService
**Vulnerability:** In `md2visio.GUI/Services/ConversionService.cs`, the `BuildOutputPath` method uses `Path.Combine(outputDir, fileName)` where `fileName` is user-provided. If `fileName` contains path traversal characters (e.g., `..\` or an absolute path), it can break out of the intended `outputDir`.
**Learning:** `Path.Combine` doesn't automatically sanitize user inputs. An absolute path in the second argument can override the first.
**Prevention:** Sanitize user-provided filenames using `Path.GetFileName` or explicit character filtering before using them in `Path.Combine`.

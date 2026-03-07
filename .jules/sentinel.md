## 2024-05-24 - Path Traversal in Output Filename
**Vulnerability:** The `ConversionService.BuildOutputPath` method concatenates user-provided filenames with the output directory using `Path.Combine` without sanitization, allowing path traversal (e.g. `..\..\malicious.vsdx`).
**Learning:** `Path.Combine` does not sanitize inputs. If the second argument is an absolute path or contains directory traversal sequences, it bypasses the intended output directory constraint.
**Prevention:** Always use `System.IO.Path.GetFileName()` to extract only the file name component from user-provided file names before combining them with target directory paths.

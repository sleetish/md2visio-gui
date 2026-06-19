
## 2025-02-15 - [Path Traversal in Output Directory Construction]
**Vulnerability:** The application constructs the output file path by passing user-supplied input directly to `Path.Combine()` without sanitizing it in `BuildOutputPath()`. `Path.Combine()` will accept absolute paths or traversal symbols like `..`, potentially enabling directory traversal outside the target directory.
**Learning:** `System.IO.Path.Combine` doesn't protect against absolute paths or path traversal syntax `..`. Always strictly sanitize user-provided file names.
**Prevention:** Normalize all cross-platform slash characters and use `Path.GetFileName()` to extract only the file name from user input, then gracefully default it if the result is empty.

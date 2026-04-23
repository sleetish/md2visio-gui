## 2024-05-18 - Path Traversal Vulnerability in File Output Generation
**Vulnerability:** Path Traversal
**Learning:** `Path.Combine` doesn't automatically sanitize components. If a user provides a file name with traversal characters (like `../../`), they could overwrite unintended files outside the designated output directory. This occurs when `Path.Combine(outputDir, fileName)` executes.
**Prevention:** Always normalize slashes and explicitly extract only the file name part using `Path.GetFileName()` before combining with a trusted base path. Provide a safe default (like 'output') if the sanitized string turns out to be empty to prevent generating invalid paths.

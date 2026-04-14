## 2025-02-28 - [Path Traversal in ConversionService]
**Vulnerability:** Path traversal sequence in `fileName` can override the `outputDir` during `Path.Combine` execution if left unsanitized.
**Learning:** `System.IO.Path.Combine` treats absolute paths or traversal sequences in its second argument as overrides to the base path.
**Prevention:** Always normalize path separators (`Path.DirectorySeparatorChar`) and extract only the filename component via `Path.GetFileName()` before combining. Handle empty sanitized filenames with a safe default.

## 2024-05-18 - [Path Traversal in Path.Combine]
**Vulnerability:** The `ConversionService` passes unsanitized user input (`fileName`) to `Path.Combine`. In .NET, if the second argument to `Path.Combine` is an absolute path or contains path traversal sequences, it overrides the base path entirely, leading to arbitrary file write.
**Learning:** `Path.Combine` does not sanitize inputs automatically. User-provided filenames must always be sanitized using `Path.GetFileName` and replacing cross-platform directory separators before combination.
**Prevention:** Always normalize slashes and use `Path.GetFileName` on untrusted filename inputs before concatenating or combining them into file paths. Handle cases where the resulting filename becomes empty.

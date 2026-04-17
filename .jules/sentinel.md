## 2024-05-24 - Fix Path Traversal in ConversionService
**Vulnerability:** Path Traversal vulnerability when saving output diagram files based on user-provided names, enabling potential arbitrary file write.
**Learning:** `Path.Combine` doesn't sanitize relative components if the suffix contains path traversal paths (like `../`).
**Prevention:** Always normalize slashes and explicitly extract the filename part using `Path.GetFileName` and provide a default fallback name before calling `Path.Combine` with user input.

## 2024-05-18 - Fix Path Traversal in Conversion Service
**Vulnerability:** The `BuildOutputPath` function in `md2visio.GUI/Services/ConversionService.cs` concatenated the `outputDir` and user-controlled `fileName` using `Path.Combine` without any prior sanitization. This allowed Path Traversal payloads like `../../Windows/System32/config/sam.vsdx` to write files outside of the intended directory.
**Learning:** `Path.Combine` on its own is unsafe when the second argument can contain path traversal sequences or absolute paths, as it may result in paths escaping the base directory limit.
**Prevention:** Always normalize slashes and aggressively sanitize user-supplied filenames via `Path.GetFileName()` prior to utilizing `Path.Combine`.

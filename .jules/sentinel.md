## 2024-05-31 - Fix Process.Start security issues
**Vulnerability:** The application was using `Process.Start` with `UseShellExecute = true` directly with user input strings for URLs and Directory paths, exposing to shell execution and path traversal vulnerabilities.
**Learning:** Naively executing untrusted user input using Windows shell features (like `.ToString()` directly opening URLs or `FileName = userText` without normalizing) is dangerous.
**Prevention:** Always validate URL strings for correct schemas (e.g. `Uri.TryCreate` with http/https) and sanitize and normalize directory paths (e.g., using `Path.GetFullPath`) before invoking `Process.Start`.

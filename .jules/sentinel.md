## 2024-05-24 - Process.Start Command Execution and Scheme Abuse Mitigation
**Vulnerability:** Unrestricted use of `Process.Start` with user-supplied URLs and directory paths allowed arbitrary scheme execution (e.g., `file://`) and potential command execution via `UseShellExecute = true`.
**Learning:** Relying on `UseShellExecute = true` for opening files or directories passes the input directly to the OS shell, which can be manipulated if the input is untrusted or malformed.
**Prevention:** Validate URL schemes strictly using `Uri.TryCreate` (allowing only HTTP/HTTPS). For local paths, use `Path.GetFullPath`, invoke `explorer.exe` directly, wrap arguments in quotes, and set `UseShellExecute = false` to prevent shell interpretation.


## 2026-05-01 - [High] Secure Process.Start usage
**Vulnerability:** Unsafe invocation of `Process.Start` with user-supplied or unvalidated strings. In link click handlers, passing unvalidated string data directly to `Process.Start` allows execution of arbitrary URI schemes (e.g., `file://`, `javascript:`). In folder opening handlers, using the user-provided directory string directly as `FileName` with `UseShellExecute = true` can allow command execution if the path is crafted maliciously.
**Learning:** `Process.Start` combined with `UseShellExecute = true` acts like the Windows `Run` dialog. Any URI or command can be executed if not strictly validated.
**Prevention:** 1) For URLs, strictly validate schemes using `Uri.TryCreate` (allowing only http/https). 2) For opening directories, normalize the path via `Path.GetFullPath`, trim trailing backslashes, quote the argument, and invoke `explorer.exe` explicitly with `UseShellExecute = false`.

## 2024-05-28 - Command Injection and Untrusted URL Navigation Risks via Process.Start

**Vulnerability:** The application used `Process.Start` with `UseShellExecute = true` to handle user-supplied URLs and local directory paths without any validation or sanitization. Specifically, `authorLabel.LinkClicked` blindly executed the `LinkData` as a URL, allowing arbitrary protocol handlers (like `file://` or custom URI schemes) to execute code or leak data. Furthermore, `OnOpenOutputClick` used `Process.Start` with the output directory directly as the `FileName` and `UseShellExecute = true`, which is prone to command injection and unintended executable launching if a directory path string contains malicious executables or executable extensions.

**Learning:** `Process.Start` combined with `UseShellExecute = true` relies on Windows shell associations. Passing untrusted, unsanitized strings allows attackers to invoke arbitrary programs via unexpected protocols or by placing maliciously named executables in paths that get resolved by the shell. It is critical to enforce strict validation on URLs and handle local paths explicitly.

**Prevention:**
1. **URL Validation:** For external links, strictly validate the URL scheme (e.g., `http` or `https`) using `Uri.TryCreate` before launching the process.
2. **Secure Directory Opening:** When opening local directories, verify the directory's existence (`Directory.Exists`), resolve it to an absolute path (`Path.GetFullPath`), and explicitly invoke the target executable (`explorer.exe`) with the target path securely enclosed in double quotes. Set `UseShellExecute = false` to avoid unintended shell execution behaviors.

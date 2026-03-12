## 2025-02-14 - Add max depth to MmdJson parser
**Vulnerability:** The recursive custom JSON parser components (`MmdJsonObj` and `MmdJsonArray`) could be subjected to arbitrary nesting depth from malicious Mermaid documents, leading to Denial of Service (DoS) via Stack Overflow.
**Learning:** Custom recursive parsers must enforce a hardcoded maximum recursion depth limit (e.g., `MAX_DEPTH = 50`) to avoid blowing up the call stack.
**Prevention:** Implement `MAX_DEPTH` constants and track/increment the depth parameter on each recursive call, throwing `InvalidOperationException` if the recursion limit is exceeded.

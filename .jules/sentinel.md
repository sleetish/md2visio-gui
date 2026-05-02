## 2024-05-02 - [Denial of Service via Unbounded Recursion in JSON Parsers]
**Vulnerability:** A Denial of Service (DoS) vulnerability existed in `MmdJsonObj` and `MmdJsonArray` due to unbounded recursion during JSON parsing. A maliciously crafted, deeply nested JSON string could exhaust the call stack, leading to a StackOverflowException and application crash.
**Learning:** Custom recursive parsers must implement safeguards against unbounded recursion to prevent DoS attacks via stack exhaustion.
**Prevention:** Hardcode a maximum recursion depth limit (e.g., `MAX_DEPTH = 50`) and pass a `depth` parameter through recursive calls, throwing an `InvalidOperationException` if the limit is exceeded.

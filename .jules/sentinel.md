## 2024-05-15 - DoS via Stack Overflow in Custom JSON Parsers
**Vulnerability:** The custom recursive parsers `MmdJsonObj` and `MmdJsonArray` lack a maximum recursion depth limit, which allows Denial of Service (DoS) attacks via memory exhaustion (Stack Overflow) when processing deeply nested JSON structures from user input.
**Learning:** Custom recursive parsing logic without depth constraints is vulnerable to stack exhaustion, leading to immediate process termination that cannot be easily caught.
**Prevention:** Enforce a hardcoded maximum recursion depth limit (e.g., `MAX_DEPTH = 50`) in all recursive data processing components. Throw an `InvalidOperationException` or similar safe exception when the limit is exceeded.

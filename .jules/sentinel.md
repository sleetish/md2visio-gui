## 2026-03-31 - [Sentinel: Medium] Add Recursion Depth Limit to JSON Parsers
**Vulnerability:** The custom JSON parsing logic in MmdJsonObj and MmdJsonArray implemented recursive constructor calls without any recursion depth limitations. A deeply nested JSON structure provided by the user could trigger a Stack Overflow Exception, leading to a Denial of Service (DoS) attack.
**Learning:** Even custom, non-standard parsers require protections against deeply nested recursive structures.
**Prevention:** Always enforce a hardcoded maximum recursion depth limit (e.g., MAX_DEPTH = 50) when using custom recursive parsers (like MmdJsonObj and MmdJsonArray) and throw an InvalidOperationException when exceeded to prevent DoS via Stack Overflow.

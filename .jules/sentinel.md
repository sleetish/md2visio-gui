## 2024-03-05 - 🛡️ Sentinel: Fix DoS via Stack Overflow in custom JSON parsers
**Vulnerability:** Denial of Service (DoS) via Stack Overflow in custom recursive descent JSON parsers (`MmdJsonObj` and `MmdJsonArray`).
**Learning:** Custom recursive parsers without a hardcoded maximum recursion depth limit are vulnerable to Stack Overflow exceptions when parsing deeply nested inputs.
**Prevention:** Always enforce a hardcoded maximum recursion depth limit (e.g., `MAX_DEPTH = 50`) in custom recursive parsers to prevent Stack Overflow exceptions and DoS.

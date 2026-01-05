# Change: Scope fragment width by content

## Why
Fragments currently span the full diagram width, obscuring intent when only a subset of participants are involved.

## What Changes
- Compute fragment frame width from messages, notes, and activations inside the fragment time window.
- Fall back to full width when a fragment contains no content.

## Impact
- Affected specs: openspec/specs/sequence-layout/spec.md
- Affected code: md2visio/vsdx/VDrawerSeq.cs

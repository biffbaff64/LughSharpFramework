---
name: Naming convention for virtual-bypass wrappers
description: Preferred suffix for non-virtual wrapper methods that suppress constructor virtual-call warnings
type: feedback
originSessionId: 25b8da45-f0ff-482f-b424-8a316a85cb05
---
Use the suffix `Unchecked` (e.g. `GetPrefWidthUnchecked`) for non-virtual wrapper methods whose sole purpose is to suppress IDE/analyzer warnings about calling virtual members from a constructor. Avoid `Safe`, which implies protection from virtual dispatch side-effects rather than suppression of a warning.

**Why:** User confirmed preference during SelectBox code review (May 2026).

**How to apply:** Whenever writing or reviewing wrapper methods that exist only to call a virtual method without triggering a constructor-call warning, name them with the `Unchecked` suffix.

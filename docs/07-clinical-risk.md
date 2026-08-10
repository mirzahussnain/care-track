# Clinical Risk (Portfolio Awareness)

**This document demonstrates awareness of clinical software risk concepts for a portfolio project only. CareTrack is not a certified clinical system and no claim of DCB0129 or other clinical-safety compliance is being made.**

## Starter Hazard Log (Proposed)

| Hazard | Potential consequence | Potential future controls |
|---|---|---|
| Wrong patient selected | Information may be associated with the incorrect patient. | persistent patient identification banner; confirmation before significant actions |
| Unauthorized access | Sensitive information may be exposed. | OpenID Connect; role-based authorization; least privilege |
| Referral accidentally completed | Outstanding work may become less visible. | workflow rules; confirmation; audit history |
| Duplicate referral | Duplicate work. | duplicate detection/warning |
| Concurrent editing | One user's updates may overwrite another's. | optimistic concurrency; row versioning |
| Important referral status not noticed | Workflow delay. | dashboard status visibility; clear workflow queues |

All controls above are planned ideas only and are not implemented in this repository stage.

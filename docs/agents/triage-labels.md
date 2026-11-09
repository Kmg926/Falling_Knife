# Triage Labels

The skills speak in terms of five canonical triage roles. This file maps those roles to the actual label strings used in this repo's issue tracker.

| Label in mattpocock/skills | Label in our tracker | Meaning                                  |
| -------------------------- | -------------------- | ---------------------------------------- |
| `needs-triage`             | `needs-triage`       | Maintainer needs to evaluate this issue  |
| `needs-info`               | `needs-info`         | Waiting on reporter for more information |
| `ready-for-agent`          | `ready-for-agent`    | Fully specified, ready for an AFK agent  |
| `ready-for-human`          | `ready-for-human`    | Requires human implementation            |
| `wontfix`                  | `wontfix`            | Will not be actioned                     |

When a skill mentions a role (e.g. "apply the AFK-ready triage label"), use the corresponding label string from this table.

> 이 프로젝트에서 `ready-for-agent`는 "AI가 `.scratch/<기능>/draft/`에 초안을 쓰고, 내가 직접 다시 타이핑해 `Assets/_Project/Scripts/`에 반영"을 뜻한다. AI는 Scripts 폴더에 직접 쓰지 않는다. (CLAUDE.md 4장)

Edit the "Label in our tracker" column to match whatever vocabulary you actually use.

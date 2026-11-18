# title-menu Ticket 55: 크레딧에서 만든 사람과 도구를 본다

> AI가 Scripts에 직접 씀(title-menu Grilling Q9, CLAUDE.md 4장 예외). Scene 변경 없음(53의 창을 같이 씀). 면담 대비로 읽어 둔다.
> 근거: `../../spec.md`(제안 5·8), Grilling Q6·Q7. 전 코드 `TitleMenu.before.cs`.

## 바꾼 것
| 대상 | 내용 |
|---|---|
| `TitleMenu.cs` | 창 종류 `Credits`, 크레딧 글 칸 `credits`(TextArea), `OpenCredits`·`TurnCredits`·`UpdateCredits`, 닫을 때 본문 넘침 방식 되돌림 |
| `docs/asset-rights.md` 3장 | 초안(글꼴 한 줄) → 게임 안 문구 전체, "제출 전 확인 — 크레딧 표기" 체크 |

## 줄별 핵심
| 부분 | 왜 |
|---|---|
| `panelBody.overflowMode = TextOverflowModes.Page` | 글이 창보다 길면 TMP가 알아서 쪽으로 나눔 → 스크롤 창(Mask)을 새로 만들 필요 없음 |
| `pageToDisplay`·`textInfo.pageCount` | 지금 몇 쪽 / 모두 몇 쪽(`ForceMeshUpdate` 뒤에 정확) |
| `Back()`에서 `overflowMode = bodyOverflow` | 규칙·설정 창은 같은 본문을 쓰므로 크레딧의 쪽 나누기가 남지 않게 |
| ↑↓·W S·Space·Enter·마우스 휠·클릭 | 쪽 넘기기(처음·끝에서 멈춤) |
| 만든 사람 `Kmg926` | 공개 저장소의 코드·씬에 본명·학번을 넣지 않음(Grilling Q7) |

## 테스트 (Ticket 55)
| # | 방법 | 기대 | 결과 |
|---|---|---|---|
| — | 크레딧 열기·쪽 넘기기, 끝·처음에서 더 | 1/2 → 2/2, 2와 1에서 멈춤, 아래 줄 "(n/2)" | ✅ |
| C5 | 글에 본명·학번·학교·교수·이메일·@ | 없음, `Kmg926`·OFL 문구 있음 | ✅ |
| — | 닫고 규칙 다시 열기 | 넘침 방식 원래대로(Overflow), 넘침 0 | ✅ |
| C6 | GameScene 퇴행 T65~T68·T143, Console 오류 | 같음, 0(경고 1 = 글꼴 아틀라스 자동 갱신, 늘 커밋에서 빼는 파일) | ✅ |

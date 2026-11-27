# 122 줄별 설명 — 콘솔 경고 2개 없애기

> 2026-11-27, 사용자 "계속 콘솔에 위험 표시 뜨는데 뭐야?" → AI가 경고 3개 원인 설명 → "고쳐줘"(1·2번). AI가 Scripts에 직접 씀·MCP.

| 경고 | 원인 | 고친 것 |
|---|---|---|
| "The referenced script (Unknown) on this Behaviour is missing!" | GameScene `HUD`에 `TableItemsView`(monitor-trade 44에서 붙임) 자리가 남음 — item-phone 63에서 스크립트 파일만 지움 | MCP `GameObjectUtility.RemoveMonoBehavioursWithMissingScript(HUD)` → 1개 지움, Scene 저장 |
| `UIManager.cs(81)` "DEVELOPMENT_BUILD preprocessor directive has been deprecated" | Unity 6.6이 `#if DEVELOPMENT_BUILD`를 쓰지 말라고 함 | `#if UNITY_EDITOR \|\| DEVELOPMENT_BUILD` … `#endif` → `if (Debug.isDebugBuild && …)`. `Debug.isDebugBuild`는 편집기·개발용 빌드에서 true, 최종 빌드에서 false라 뜻은 같음(최종 빌드에서 F1은 여전히 아무 일도 안 함) |
| "Importer(NativeFormatImporter) generated inconsistent result … Galmuri11 SDF.asset" | TMP 글꼴이 새 글자를 만날 때마다 글자 판을 스스로 늘림 | 안 고침(설정을 바꾸면 빠진 글자 위험이 더 큼) — 이 파일을 커밋에서 빼는 이유와 같음 |
확인: 컴파일 뒤 Scene 다시 열기 → 콘솔 경고 0·오류 0.

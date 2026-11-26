# draft 103: 빌드 준비

> 2026-11-27. `UIManager.before.cs` = 바꾸기 전.

## 줄별 설명 — UIManager.cs (면담 대비)
- `#if UNITY_EDITOR || DEVELOPMENT_BUILD … #endif`: 전처리기 조건 — Unity 편집기이거나 "Development Build"를 켜고 만든 빌드일 때만 안의 코드가 컴파일됨. 최종(릴리스) 빌드에서는 F1 코드가 아예 없어서 개발용 글자를 켤 방법이 없음(#41 "최종 빌드 전 제거"). 코드를 지우지 않아 편집기 시험에는 계속 씀.

## 빌드 절차 (다음에도 같은 순서)
1. 퇴행 T65~T68·T143(`.scratch/regression/README.md`)
2. Project Settings > Player > Resolution and Presentation > **Run In Background 끄기**
3. File > Build Profiles > Windows → `Builds/FallingKnife/FallingKnife.exe`
4. 빌드 실행 → `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Falling_Knife\Player.log`에 오류·예외가 없는지
5. Run In Background 다시 켜기(MCP 시험용)

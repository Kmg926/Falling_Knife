"""gameover 120 — 패배 화면 그림 한 장(640×360, AI 작성).

    python tools/make_art_gameover.py      # → .scratch/art/drafts/gameover/gameover-bg.png + 미리보기

레퍼런스(사용자 2026-11-27): 검은 화면 가운데 붉게 빛나는 "GAME OVER"(1), 글리치 띠·주사선(2). "최대한 간소화".
- 검은 바탕 + 아주 옅은 주사선 + 글자 뒤 붉은 빛(바둑판 점) + 7×9 도트 글자 ×6("GAME OVER", 빨강 3단계 + 검은 테두리)
  + 글자 몇 줄을 옆으로 밀어낸 글리치(빨강·남색) + 화면 가장자리 글리치 띠 몇 개(무작위 없음).
- 플레이 타임·버튼 글자는 게임에서 TMP로. 32색 팔레트만.
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import write_png  # noqa: E402
from make_art_hd import BAYER4, Canvas, check32, load_pal  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "gameover"
W, H = 640, 360
S = 6                      # 글자 한 칸 = 6도트
TITLE_TOP = 112            # 글자 맨 위 y(640 격자) — HUD y 56

FONT = {
    "G": [".#####.", "##...##", "##.....", "##.....", "##..###", "##...##", "##...##", "##...##", ".#####."],
    "A": ["..###..", ".##.##.", "##...##", "##...##", "#######", "##...##", "##...##", "##...##", "##...##"],
    "M": ["##...##", "###.###", "#######", "##.#.##", "##...##", "##...##", "##...##", "##...##", "##...##"],
    "E": ["#######", "##.....", "##.....", "######.", "##.....", "##.....", "##.....", "##.....", "#######"],
    "O": [".#####.", "##...##", "##...##", "##...##", "##...##", "##...##", "##...##", "##...##", ".#####."],
    "V": ["##...##", "##...##", "##...##", "##...##", "##...##", ".##.##.", ".##.##.", "..###..", "...#..."],
    "R": ["######.", "##...##", "##...##", "##...##", "######.", "##.##..", "##..##.", "##...##", "##...##"],
}


def text_mask(word):
    cols = []
    for i, ch in enumerate(word):
        if ch == " ":
            cols += [[False] * 9] * 4
            continue
        if i and word[i - 1] != " ":
            cols.append([False] * 9)
        g = FONT[ch]
        for c in range(7):
            cols.append([g[r][c] == "#" for r in range(9)])
    m = np.array(cols, bool).T                      # 9 × n
    return m.repeat(S, 0).repeat(S, 1)


def main():
    pal = load_pal()
    cv = Canvas(pal, W, H)
    yy, xx = np.mgrid[0:H, 0:W]
    sparse = lambda region, name, level: cv.fill(region & (np.broadcast_to(level, (H, W)) > BAYER4[yy % 4, xx % 4]), name)
    cv.fill(np.ones((H, W), bool), "bg00")
    cv.fill((yy % 3 == 1), "bg0")                                       # 옅은 주사선
    # 글자 자리
    tm = text_mask("GAME OVER")
    th, tw = tm.shape
    x0 = (W - tw) // 2
    t = np.zeros((H, W), bool)
    t[TITLE_TOP:TITLE_TOP + th, x0:x0 + tw] = tm
    # 글자 뒤 붉은 빛: 타원, 가운데 진하게
    cx, cy = W / 2, TITLE_TOP + th / 2
    d = np.hypot((xx - cx) / (tw * 0.62), (yy - cy) / (th * 1.6))
    sparse(d < 1, "red0", np.clip(1 - d, 0, 1) ** 1.2 * 0.75)
    sparse(d < 0.55, "red1", np.clip(0.55 - d, 0, 1) * 0.5)
    # 글자: 검은 테두리 → 몸통 red3 → 위쪽 1/3 red4(빛) → 아래 1/4 red2(그늘)
    edge = np.zeros_like(t)
    for dy, dx in ((-2, 0), (2, 0), (0, -2), (0, 2), (-2, -2), (2, 2), (-2, 2), (2, -2)):
        edge |= np.roll(np.roll(t, dy, 0), dx, 1)
    cv.fill(edge & ~t, "bg00")
    cv.fill(t, "red3")
    rel = (yy - TITLE_TOP) / th
    cv.fill(t & (rel < 0.3), "red4")
    cv.fill(t & (rel > 0.78), "red2")
    # 글리치: 글자 가로 띠 3줄을 옆으로 밀어 다시 찍음(빨강 띠는 오른쪽, 남색 띠는 왼쪽 — 색 어긋남)
    # 처음엔 밀려난 자리를 검게 끊어 글자가 잘 안 읽혔음 → 글자 밖에만 색 어긋남을 얹음(글자 자체는 안 끊음)
    for (y0, h, dx, col) in ((TITLE_TOP + 15, 3, 6, "red1"), (TITLE_TOP + 36, 2, -5, "navy2")):
        band = np.zeros_like(t)
        band[y0:y0 + h] = True
        shifted = np.roll(t & band, dx, 1)
        cv.fill(shifted & ~t & ~edge, col)
    # 가장자리 글리치 띠(무작위 없음): y, x, 길이, 색
    bars = [(34, 24, 70, "red0"), (38, 60, 26, "navy0"), (58, 540, 80, "red0"), (62, 590, 30, "navy2"),
            (276, 8, 90, "red0"), (280, 70, 22, "red1"), (300, 520, 110, "red0"), (304, 600, 24, "navy0"),
            (322, 30, 40, "navy0"), (88, 470, 46, "red0")]
    for (y, x, n, col) in bars:
        cv.fill((slice(y, y + 2), slice(x, min(W, x + n))), col)
    OUT.mkdir(parents=True, exist_ok=True)
    p = OUT / "gameover-bg.png"
    write_png(p, cv.rgba())
    write_png(OUT / "gameover-preview-x2.png", cv.rgba().repeat(2, 0).repeat(2, 1))
    print(f"title {tw}x{th} at x{x0} y{TITLE_TOP} (HUD {x0/2},{TITLE_TOP/2} {tw/2}x{th/2})")
    print("팔레트 검사 (32색):")
    return check32([p], pal)


if __name__ == "__main__":
    sys.exit(0 if main() else 1)

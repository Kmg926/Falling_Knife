"""Falling Knife 주식 모니터·모니터용 큰 캔들 (AI 작성, stock-screen Ticket 40).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art_monitor.py draft   # 시안 A(100x62, 캔들 18)·B(100x54, 캔들 14) 그림 + 팔레트 검사
    python tools/make_art_monitor.py game    # 사용자가 고른 A를 게임 파일 이름(terminal·candle-up-big·candle-down-big)으로
    python tools/make_art_monitor.py flash   # Ticket 47: 굳는 순간 캔들 테두리(위·아래 기준점 2장)·기준선 번쩍임

- 640 격자(지금 좌표 x2), 팔레트 32색. 무작위 없음. 출력: .scratch/art/drafts/hd/monitor/
- 모니터 그림 = 테두리 + 화면(주사선·가장자리 어둡게) + 위 줄 구분선 + 차트 칸(옅은 격자·가격 눈금·가운데 기준선 점선)
  + 오른쪽 "다음" 창 틀. 글자(`GE코인`·`남은 n`·`?`)와 캔들은 그림에 넣지 않는다(Canvas 글자·SpriteRenderer).
- 큰 캔들: 상승은 기준점이 아래(위로 자람), 하락은 기준점이 위(아래로 자람) → Unity에서 기준점만 다르게 가져온다.
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import write_png  # noqa: E402
from make_art_hd import Canvas, check32, load_pal  # noqa: E402
from make_art_hd_items import CANDLE_COLORS  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "hd" / "monitor"

# 시안 규격(지금 좌표). 그림 왼쪽 위 = (112, top). 모든 값은 지금 좌표, 그릴 때 x2.
VARIANTS = {
    "a": dict(top=78, height=62, bezel_bottom=134, screen=(115, 81, 209, 131), ticker_line=93, baseline=112, candle=18,
              next_box=(190, 101, 204, 125)),
    "b": dict(top=86, height=54, bezel_bottom=134, screen=(115, 89, 209, 131), ticker_line=101, baseline=116, candle=14,
              next_box=(190, 104, 204, 128)),
}
LEFT, WIDTH = 112, 100


def monitor(pal, v):
    w, h = WIDTH * 2, v["height"] * 2
    cv = Canvas(pal, w, h)

    def r(x0, y0, x1, y1):                                     # 지금 좌표 사각형 → 그림 안 640 격자 slice
        return (slice((y0 - v["top"]) * 2, (y1 - v["top"]) * 2), slice((x0 - LEFT) * 2, (x1 - LEFT) * 2))

    bb = v["bezel_bottom"]
    # 받침대: 목 + 바닥(테이블 위 그림자 한 줄)
    cv.fill(r(152, bb, 172, bb + 3), "navy0")
    cv.fill(r(152, bb, 154, bb + 3), "navy")
    cv.fill(r(138, bb + 3, 186, v["top"] + v["height"]), "navy")
    cv.fill((slice(h - 2, h), r(138, 0, 186, 1)[1]), "bg00")
    cv.fill((slice((bb + 3 - v["top"]) * 2, (bb + 3 - v["top"]) * 2 + 1), r(138, 0, 186, 1)[1]), "navy2")
    # 테두리(베젤): 남색 판, 위·왼쪽 빛, 아래·오른쪽 그늘, 오른쪽 아래 전원 불빛
    cv.fill(r(LEFT, v["top"], LEFT + WIDTH, bb), "navy")
    cv.fill((slice(0, 1), slice(0, w)), "navy2")
    cv.fill((slice(0, (bb - v["top"]) * 2), slice(0, 1)), "navy2")
    cv.fill((slice((bb - v["top"]) * 2 - 2, (bb - v["top"]) * 2), slice(0, w)), "navy0")
    cv.fill((slice(0, (bb - v["top"]) * 2), slice(w - 2, w)), "navy0")
    ly, lx = (bb - v["top"]) * 2 - 4, w - 10
    cv.fill((slice(ly, ly + 2), slice(lx, lx + 2)), "green2")
    # 화면: 어두운 바탕 + 주사선(새 도트 한 줄 걸러 더 어둡게) + 가장자리 어둡게
    sx0, sy0, sx1, sy1 = v["screen"]
    scr = r(sx0, sy0, sx1, sy1)
    cv.fill(scr, "bg0")
    ys = np.arange(scr[0].start, scr[0].stop)
    for y in ys[1::2]:
        cv.fill((slice(y, y + 1), scr[1]), "bg00")
    cv.fill((slice(scr[0].start, scr[0].start + 2), scr[1]), "bg00")
    cv.fill((slice(scr[0].stop - 2, scr[0].stop), scr[1]), "bg00")
    cv.fill((scr[0], slice(scr[1].start, scr[1].start + 2)), "bg00")
    cv.fill((scr[0], slice(scr[1].stop - 2, scr[1].stop)), "bg00")
    # 위 줄(종목 줄) 구분선
    ty = (v["ticker_line"] - v["top"]) * 2
    cv.fill((slice(ty, ty + 1), slice(scr[1].start + 2, scr[1].stop - 2)), "navy0")
    # 차트 칸: 옅은 격자(점선), 오른쪽 가격 눈금, 가운데 기준선(금색 점선)
    cx0, cx1 = scr[1].start + 4, (v["next_box"][0] - LEFT) * 2 - 4
    cy0, cy1 = ty + 3, scr[0].stop - 3
    for gx in range(cx0 + 10, cx1, 24):
        for y in range(cy0, cy1, 4):
            cv.fill((y, gx), "navy0")
    for gy in range(cy0 + 6, cy1, 12):
        for x in range(cx0, cx1, 4):
            cv.fill((gy, x), "navy0")
    by = (v["baseline"] - v["top"]) * 2
    for x in range(cx0, cx1, 3):
        cv.fill((slice(by, by + 1), slice(x, x + 2)), "gold0")
    for gy in range(cy0, cy1, 6):                               # 가격 눈금(차트 칸 오른쪽 끝)
        cv.fill((slice(gy, gy + 1), slice(cx1 - 4, cx1)), "gray0")
    cv.fill((slice(by, by + 1), slice(cx1 - 6, cx1)), "gold1")
    # "다음" 창: 남색 틀(안은 화면 그대로) — 안에 작은 캔들 또는 `?`(글자)
    nx0, ny0, nx1, ny1 = v["next_box"]
    nb = r(nx0, ny0, nx1, ny1)
    cv.fill((slice(nb[0].start, nb[0].start + 2), nb[1]), "navy")
    cv.fill((slice(nb[0].stop - 2, nb[0].stop), nb[1]), "navy")
    cv.fill((nb[0], slice(nb[1].start, nb[1].start + 2)), "navy")
    cv.fill((nb[0], slice(nb[1].stop - 2, nb[1].stop)), "navy")
    cv.fill((slice(nb[0].start, nb[0].start + 1), nb[1]), "navy2")
    return cv


def big_candle(pal, kind, length):
    """모니터 큰 캔들: 폭 8(새 16), 길이 length(새 2배). 위 심지 3 + 몸통 + 아래 심지 2(지금 좌표)."""
    w, h = 16, length * 2
    cv = Canvas(pal, w, h)
    c = CANDLE_COLORS[kind]
    top_wick, bot_wick = 6, 4
    cv.fill((slice(0, top_wick), slice(7, 9)), "gray1")
    cv.fill((slice(0, 1), slice(7, 9)), "cream0")
    cv.fill((slice(h - bot_wick, h), slice(7, 9)), "gray1")
    cv.fill((slice(top_wick, h - bot_wick), slice(0, w)), c["edge"])
    cv.fill((slice(top_wick + 1, h - bot_wick - 1), slice(1, w - 1)), c["body"])
    cv.fill((slice(top_wick + 1, h - bot_wick - 1), slice(3, 5)), c["light"])
    cv.fill((slice(top_wick + 1, top_wick + 2), slice(1, w - 1)), c["light"])
    cv.fill((slice(top_wick + 1, h - bot_wick - 1), slice(w - 4, w - 1)), c["shade"])
    cv.fill((slice(h - bot_wick - 2, h - bot_wick - 1), slice(1, w - 1)), c["shade"])
    cv.fill((slice(top_wick + 2, top_wick + 5), slice(5, 6)), "cream2" if kind == "up" else "red4")
    return cv


def make_draft(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    files = []
    for key, v in VARIANTS.items():
        for name, cv in ((f"monitor-{key}", monitor(pal, v)),
                         (f"candle-up-big-{key}", big_candle(pal, "up", v["candle"])),
                         (f"candle-down-big-{key}", big_candle(pal, "down", v["candle"]))):
            p = OUT / f"{name}.png"
            write_png(p, cv.rgba())
            files.append(p)
    print("팔레트 검사 (32색):")
    return check32(files, pal)


def make_game(pal):
    """사용자가 고른 시안 A를 게임 파일 이름으로(art-hd처럼 단말기 그림 `terminal.png`에 덮어써 연결 유지)."""
    v = VARIANTS["a"]
    dst = OUT / "game"
    dst.mkdir(parents=True, exist_ok=True)
    files = []
    for name, cv in (("terminal", monitor(pal, v)),
                     ("candle-up-big", big_candle(pal, "up", v["candle"])),
                     ("candle-down-big", big_candle(pal, "down", v["candle"]))):
        p = dst / f"{name}.png"
        write_png(p, cv.rgba())
        files.append(p)
    print("팔레트 검사 (32색):")
    return check32(files, pal)


# ---------- stock-screen 47: 굳는 순간 번쩍임 ----------
def candle_flash(pal, length):
    """큰 캔들(8 x length)과 같은 모양의 **테두리만** cream2 — 굳는 순간 캔들 위에 0.1초."""
    from make_art_hd import edge
    base = big_candle(pal, "up", length)
    m = base.mask
    ring = edge(m, -1, 0) | edge(m, 1, 0) | edge(m, 0, -1) | edge(m, 0, 1)
    cv = Canvas(pal, base.w, base.h)
    cv.fill(ring, "cream2")
    return cv


def baseline_flash(pal):
    """모니터 차트 칸 기준선(지금 좌표 x 117~188, 새 도트 142 x 2)을 잠깐 덮는 밝은 줄 gold2."""
    cv = Canvas(pal, 142, 2)
    cv.fill((slice(0, 2), slice(0, 142)), "gold2")
    return cv


def make_flash(pal):
    dst = OUT / "game47"
    dst.mkdir(parents=True, exist_ok=True)
    v = VARIANTS["a"]
    files = []
    for name, cv in (("candle-flash-up", candle_flash(pal, v["candle"])), ("candle-flash-down", candle_flash(pal, v["candle"])),
                     ("baseline-flash", baseline_flash(pal))):
        p = dst / f"{name}.png"
        write_png(p, cv.rgba())
        files.append(p)
    print("팔레트 검사 (32색):")
    return check32(files, pal)


if __name__ == "__main__":
    cmd = sys.argv[1] if len(sys.argv) > 1 else ""
    if cmd == "draft":
        sys.exit(0 if make_draft(load_pal()) else 1)
    if cmd == "game":
        sys.exit(0 if make_game(load_pal()) else 1)
    if cmd == "flash":
        sys.exit(0 if make_flash(load_pal()) else 1)
    print(__doc__)

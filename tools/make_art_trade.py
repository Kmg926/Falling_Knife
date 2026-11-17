"""Falling Knife 모니터 거래 그림 (AI 작성, monitor-trade Ticket 43).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art_trade.py draft   # 확대 화면·매수/매도 버튼 3종·모니터 금색 테두리·늘린 테이블·광대 모니터 뒷면 + 팔레트 검사
    python tools/make_art_trade.py game    # 게임 파일 이름으로(테이블·버튼·슬롯은 옛 이름에 덮어씀)

- 640 격자(지금 좌표 x2), 32색, 무작위 없음. 출력: .scratch/art/drafts/hd/trade/
- 확대 화면(지금 좌표 240x140, 자리 x 40~280, y 20~160): 테두리 + 화면(주사선) + 위 종목 줄 구분선
  + 왼쪽 차트 칸(격자·기준선·점선 "?" 캔들 자리) + 오른쪽 아이템 칸 틀(2x2) + 세로 구분선. 글자·버튼·아이콘은 따로.
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import write_png  # noqa: E402
from make_art_hd import Canvas, check32, draw_mat, draw_table, load_pal, polygon, edge, VARIANTS as TABLE_VARIANTS  # noqa: E402
from make_art_hd_items import frame_box  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "hd" / "trade"

PANEL = (40, 20, 240, 140)            # 지금 좌표 x, y, w, h
BUTTON = (70, 36)


def zoom_panel(pal, frames=True):
    px, py, pw, ph = PANEL
    w, h = pw * 2, ph * 2
    cv = Canvas(pal, w, h)

    def r(x0, y0, x1, y1):                                     # 지금 좌표 → 그림 안 slice
        return (slice((y0 - py) * 2, (y1 - py) * 2), slice((x0 - px) * 2, (x1 - px) * 2))

    # 테두리(베젤) — 아래쪽이 두껍고 전원 불빛
    cv.fill(r(40, 20, 280, 160), "navy")
    cv.fill((slice(0, 2), slice(0, w)), "navy2")
    cv.fill((slice(0, h), slice(0, 2)), "navy2")
    cv.fill((slice(h - 2, h), slice(0, w)), "navy0")
    cv.fill((slice(0, h), slice(w - 2, w)), "navy0")
    cv.fill((slice(h - 10, h - 8), slice(w - 16, w - 12)), "green2")
    # 화면: 바탕 + 주사선 + 가장자리
    scr = r(44, 24, 276, 151)
    cv.fill(scr, "bg0")
    for y in range(scr[0].start + 1, scr[0].stop, 2):
        cv.fill((slice(y, y + 1), scr[1]), "bg00")
    for s in ((slice(scr[0].start, scr[0].start + 2), scr[1]), (slice(scr[0].stop - 2, scr[0].stop), scr[1]),
              (scr[0], slice(scr[1].start, scr[1].start + 2)), (scr[0], slice(scr[1].stop - 2, scr[1].stop))):
        cv.fill(s, "bg00")
    # 종목 줄 구분선(y 38), 아래 키 안내 줄 구분선(y 135), 가운데 세로 구분선(x 199)
    cv.fill((slice((38 - py) * 2, (38 - py) * 2 + 1), slice(scr[1].start + 4, scr[1].stop - 4)), "navy0")
    cv.fill((slice((135 - py) * 2, (135 - py) * 2 + 1), slice(scr[1].start + 4, scr[1].stop - 4)), "navy0")
    cv.fill((slice((40 - py) * 2, (133 - py) * 2), slice((199 - px) * 2, (199 - px) * 2 + 1)), "navy0")
    # 왼쪽 차트 칸(x 48~195, y 42~90): 옅은 격자 + 금색 기준선(y 66) + 가운데 점선 캔들 자리(?)
    cx0, cx1, cy0, cy1 = (48 - px) * 2, (195 - px) * 2, (42 - py) * 2, (90 - py) * 2
    for gx in range(cx0 + 8, cx1, 24):
        for y in range(cy0, cy1, 4):
            cv.fill((y, gx), "navy0")
    for gy in range(cy0 + 6, cy1, 12):
        for x in range(cx0, cx1, 4):
            cv.fill((gy, x), "navy0")
    by = (66 - py) * 2
    for x in range(cx0, cx1, 3):
        cv.fill((slice(by, by + 1), slice(x, x + 2)), "gold0")
    gx0, gy0 = (117 - px) * 2, (48 - py) * 2              # 점선 캔들 자리 8x36(지금 좌표 x 117~125, y 48~84)
    ghost = np.zeros((h, w), bool)
    ghost[gy0:gy0 + 72, gx0:gx0 + 16] = True
    ring = edge(ghost, -1, 0) | edge(ghost, 1, 0) | edge(ghost, 0, -1) | edge(ghost, 0, 1)
    yy, xx = np.mgrid[0:h, 0:w]
    cv.fill(ring & ((xx + yy) % 4 < 2), "gray0")
    # 오른쪽 아이템 칸 2x2(칸 30x30, 사이 4): x 205~269, y 56~120
    # (게임 판은 칸 틀을 슬롯 그림 `slot-normal/selected`이 그리므로 여기서는 시안에서만 그림)
    for i in range(2 if frames else 0):
        for j in range(2):
            x0, y0 = 205 + 34 * j, 56 + 34 * i
            s = r(x0, y0, x0 + 30, y0 + 30)
            cv.fill((slice(s[0].start, s[0].start + 2), s[1]), "navy")
            cv.fill((slice(s[0].stop - 2, s[0].stop), s[1]), "navy")
            cv.fill((s[0], slice(s[1].start, s[1].start + 2)), "navy")
            cv.fill((s[0], slice(s[1].stop - 2, s[1].stop)), "navy")
    return cv


def trade_button(pal, kind):
    w, h = BUTTON[0] * 2, BUTTON[1] * 2
    cv = Canvas(pal, w, h)
    if kind == "selected":
        frame_box(cv, 0, 0, w, h, "gold1", "wood", t=4, light="wood2", shade="gold0")
        cv.fill((slice(0, 1), slice(0, w)), "gold2")
        cv.fill((slice(h - 1, h), slice(0, w)), "goldD")
    elif kind == "normal":
        frame_box(cv, 0, 0, w, h, "navy", "bg0", t=2, light="navy0", shade="bg00")
        cv.fill((slice(0, 1), slice(1, w - 1)), "navy2")
    else:
        frame_box(cv, 0, 0, w, h, "gray0", "bg0", t=2, shade="bg00")
    return cv


def monitor_outline(pal):
    """모니터(100x62, 테두리 부분 100x56)를 1도트 바깥에서 두르는 금색 테두리: 102x58 → 204x116."""
    w, h = 204, 116
    cv = Canvas(pal, w, h)
    for s in ((slice(0, 2), slice(0, w)), (slice(h - 2, h), slice(0, w)), (slice(0, h), slice(0, 2)), (slice(0, h), slice(w - 2, w))):
        cv.fill(s, "gold1")
    cv.fill((slice(0, 1), slice(0, w)), "gold2")
    return cv


CLOWN_MONITOR = (80, 40)        # 광대 쪽 모니터(멀리 있어 작게, 광대 얼굴을 가리지 않게), 지금 좌표


def clown_monitor_back(pal):
    """광대 앞에 놓인 모니터의 **뒷면**(화면은 광대 쪽): 남색 덮개 + 통풍구 줄 + 가운데 상표 판 + 받침대 목·바닥.
    위쪽 테두리 너머로 화면 빛이 살짝 새어 나옴(navy2 한 줄)."""
    w, h = CLOWN_MONITOR[0] * 2, CLOWN_MONITOR[1] * 2
    cv = Canvas(pal, w, h)
    body_h = h - 12                                            # 아래 12줄(새 도트) = 받침대
    cv.fill((slice(0, body_h), slice(0, w)), "navy0")          # 뒷덮개
    cv.fill((slice(0, 1), slice(2, w - 2)), "navy2")           # 위로 새는 화면 빛
    cv.fill((slice(1, 3), slice(0, w)), "navy")                # 위 테두리
    cv.fill((slice(0, body_h), slice(0, 2)), "navy")           # 왼쪽 테두리(빛 받는 쪽)
    cv.fill((slice(body_h - 2, body_h), slice(0, w)), "bg00")  # 아래 그늘
    cv.fill((slice(0, body_h), slice(w - 2, w)), "bg00")
    for y in range(10, body_h - 8, 6):                         # 통풍구 줄(가운데 판 양옆)
        cv.fill((slice(y, y + 2), slice(16, 50)), "bg00")
        cv.fill((slice(y, y + 2), slice(w - 50, w - 16)), "bg00")
        cv.fill((slice(y + 2, y + 3), slice(16, 50)), "navy")
        cv.fill((slice(y + 2, y + 3), slice(w - 50, w - 16)), "navy")
    cx = w // 2                                                 # 가운데 상표 판(글자 없음)
    cv.fill((slice(26, 42), slice(cx - 20, cx + 20)), "navy")
    cv.fill((slice(26, 27), slice(cx - 20, cx + 20)), "navy2")
    cv.fill((slice(33, 35), slice(cx - 10, cx + 10)), "gold0")
    cv.fill((slice(body_h, body_h + 6), slice(cx - 10, cx + 10)), "navy0")   # 목
    cv.fill((slice(body_h + 6, h), slice(cx - 30, cx + 30)), "navy")         # 바닥
    cv.fill((slice(body_h + 6, body_h + 7), slice(cx - 30, cx + 30)), "navy2")
    cv.fill((slice(h - 1, h), slice(cx - 30, cx + 30)), "bg00")
    return cv


def wide_table(pal):
    """아래 띠가 없어진 뒤: 테이블 앞 모서리를 화면 아래(새 y 352)까지 — 640x160(지금 y 100~180)."""
    v = TABLE_VARIANTS["B"]
    cv = Canvas(pal)
    import make_art_hd as hd
    old_y, old_surface = hd.FRONT_Y, hd.table_surface
    hd.FRONT_Y = 352
    # 옆선도 소실점 쪽 원근 그대로 늘림(화면 밖까지): 먼 끝 x 60/580 → y 352에서의 x
    lx, rx = hd.toward_vp(60, hd.TOP_Y, 352), hd.toward_vp(580, hd.TOP_Y, 352)
    hd.table_surface = lambda corner=None: [(60, hd.TOP_Y), (580, hd.TOP_Y), (rx, 352), (lx, 352)]
    try:
        draw_table(cv, None, False)
    finally:
        hd.FRONT_Y, hd.table_surface = old_y, old_surface
    draw_mat(cv, *v["clown_mat"])
    draw_mat(cv, *v["me_mat"])
    out = Canvas(pal, 640, 160)
    out.idx[:], out.mask[:] = cv.idx[200:360], cv.mask[200:360]
    return out


def make_draft(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    parts = {"zoom-panel": zoom_panel(pal), "trade-button-normal": trade_button(pal, "normal"),
             "trade-button-selected": trade_button(pal, "selected"), "trade-button-disabled": trade_button(pal, "disabled"),
             "monitor-outline": monitor_outline(pal), "table-wide": wide_table(pal),
             "clown-monitor-back": clown_monitor_back(pal)}
    files = []
    for name, cv in parts.items():
        p = OUT / f"{name}.png"
        write_png(p, cv.rgba())
        files.append(p)
    print("팔레트 검사 (32색):")
    return check32(files, pal)




SLOT30 = 30                     # 확대 화면 안 아이템 칸(지금 좌표 30x30)


def slot30(pal, selected):
    """확대 화면 안 아이템 칸 틀 60x60(새 도트): 보통 남색 / 고른 칸 금색(색 역할: 노랑 = 선택)."""
    n = SLOT30 * 2
    cv = Canvas(pal, n, n)
    if selected:
        frame_box(cv, 0, 0, n, n, "gold2", "wood", t=2, light="gold1", shade="gold0")
        cv.fill((slice(0, 1), slice(0, n)), "gold3")
    else:
        for s in ((slice(0, 2), slice(0, n)), (slice(n - 2, n), slice(0, n)), (slice(0, n), slice(0, 2)), (slice(0, n), slice(n - 2, n))):
            cv.fill(s, "navy")
        cv.fill((slice(0, 1), slice(1, n - 1)), "navy2")
    return cv


def make_game(pal):
    """게임 파일 이름으로(덮어쓰기는 연결 유지): 테이블·버튼·슬롯은 옛 이름, 나머지는 새 파일."""
    dst = OUT / "game"
    dst.mkdir(parents=True, exist_ok=True)
    parts = {"Play/table": wide_table(pal), "Play/monitor-outline": monitor_outline(pal),
             "Play/clown-monitor": clown_monitor_back(pal), "HUD/zoom-panel": zoom_panel(pal, frames=False),
             "HUD/button-normal": trade_button(pal, "normal"), "HUD/button-selected": trade_button(pal, "selected"),
             "HUD/button-disabled": trade_button(pal, "disabled"),
             "HUD/slot-normal": slot30(pal, False), "HUD/slot-selected": slot30(pal, True)}
    files = []
    for name, cv in parts.items():
        p = dst / f"{name.replace('/', '__')}.png"
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
    print(__doc__)

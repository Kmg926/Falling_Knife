"""Falling Knife 2배 세밀 캔들·불꽃·레버리지 상자·아이템 아이콘·슬롯·버튼 (AI 작성, art-hd Ticket 37·38).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art_hd_items.py draft   # 시안 한 장(미리보기 4배) + 팔레트 검사
    python tools/make_art_hd_items.py game    # 게임에 넣을 그림 파일(PPU 2용) + 팔레트 검사
    python tools/make_art_hd_items.py heal    # heal-items 89: 배당금·구제 금융 아이콘만(기존 아이콘은 안 건드림)

- 640 격자(지금 좌표 x2). 크기: 캔들 10x36, 불꽃 12x18, 레버리지 상자 120x76, 슬롯 50x60, 버튼 116x60,
  아이콘 42x32(슬롯) / 68x52(확인 패널), 회전 그림 80x52 x 8장.
- 아이콘은 '설계 좌표'(21x16 칸)로 모양을 정해 두고, 원하는 크기에서 도트마다 그 좌표를 계산해 칠한다
  → 같은 아이콘을 슬롯 크기·확인 패널 크기로 따로 깨끗하게 그릴 수 있다(늘려서 흐려지지 않음).
- 팔레트 32색만. 무작위 없음. 출력: .scratch/art/drafts/hd/items/
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import write_png  # noqa: E402
from make_art_hd import Canvas, check32, edge, load_pal  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "hd" / "items"

# 한 단계 어둡게(회전 그림 옆면)
DARK = {"gold3": "gold2", "gold2": "gold1", "gold1": "goldD", "goldD": "gold0", "gold0": "bg00",
        "cream2": "cream", "cream": "cream0", "cream0": "gray1", "gray1": "gray0", "gray0": "bg00",
        "green2": "green", "green": "green0", "green0": "bg00",
        "red4": "red3", "red3": "red2", "red2": "red1", "red1": "red0", "red0": "bg1",
        "navy2": "navy", "navy": "navy0", "navy0": "bg0", "orange2": "orange", "orange": "red1",
        "bg00": "bg00", "bg0": "bg00", "bg1": "bg00", "wood": "wood0", "wood0": "bg00"}


def blank(pal, w, h):
    return Canvas(pal, w, h)


def frame_box(cv, x0, y0, x1, y1, border, inside, t=2, light=None, shade=None):
    """사각 틀: 바깥 t도트 border, 안 inside. light/shade가 있으면 안쪽 위·왼쪽 1줄 빛, 아래·오른쪽 1줄 그늘."""
    cv.fill((slice(y0, y1), slice(x0, x1)), border)
    cv.fill((slice(y0 + t, y1 - t), slice(x0 + t, x1 - t)), inside)
    if light:
        cv.fill((slice(y0 + t, y0 + t + 1), slice(x0 + t, x1 - t)), light)
        cv.fill((slice(y0 + t, y1 - t), slice(x0 + t, x0 + t + 1)), light)
    if shade:
        cv.fill((slice(y1 - t - 1, y1 - t), slice(x0 + t, x1 - t)), shade)
        cv.fill((slice(y0 + t, y1 - t), slice(x1 - t - 1, x1 - t)), shade)


# ---------- 캔들 10x36: 위 심지 6 + 몸통 26 + 아래 심지 4 ----------
CANDLE_COLORS = {
    "up": dict(edge="green0", body="green", light="green2", shade="green0", gap="green0"),
    "down": dict(edge="red0", body="red2", light="red3", shade="red1", gap="red0"),
}


def candle(pal, kind):
    cv = blank(pal, 10, 36)
    if kind == "back":                                        # 뒤집힌 캔들: 남색 몸통·회색 테두리·금색 마름모(어느 쪽인지 모름)
        frame_box(cv, 0, 0, 10, 36, "gray0", "navy", t=1, light="navy2", shade="navy0")
        for dy, half in ((14, 1), (15, 2), (16, 3), (17, 3), (18, 2), (19, 1)):
            cv.fill((slice(dy, dy + 1), slice(5 - half, 5 + half)), "gold1")
        cv.fill((slice(15, 17), slice(3, 4)), "gold2")
        cv.fill((slice(18, 20), slice(6, 7)), "goldD")
        return cv
    c = CANDLE_COLORS[kind]
    cv.fill((slice(0, 6), slice(4, 6)), "gray1")               # 위 심지(꼭대기 한 칸 밝게)
    cv.fill((slice(0, 1), slice(4, 6)), "cream0")
    cv.fill((slice(32, 36), slice(4, 6)), "gray1")             # 아래 심지
    cv.fill((slice(35, 36), slice(4, 6)), "gray0")
    cv.fill((slice(6, 32), slice(0, 10)), c["edge"])           # 몸통 테두리
    cv.fill((slice(7, 31), slice(1, 9)), c["body"])
    cv.fill((slice(7, 31), slice(2, 3)), c["light"])           # 왼쪽 빛 줄
    cv.fill((slice(7, 8), slice(1, 9)), c["light"])            # 윗면 빛
    cv.fill((slice(7, 31), slice(7, 9)), c["shade"])           # 오른쪽 그늘
    cv.fill((slice(30, 31), slice(1, 9)), c["shade"])
    cv.fill((slice(8, 10), slice(3, 4)), "cream2" if kind == "up" else "red4")   # 반짝임
    return cv


# ---------- 불꽃 12x18 ----------
def flame_mask(w=12, h=18):
    yy, xx = np.mgrid[0:h, 0:w] + 0.5
    cx, cy, r = 6.0, 12.0, 5.3
    circle = (xx - cx) ** 2 + (yy - cy) ** 2 <= r * r
    t = np.clip(yy / cy, 0, 1)                                 # 꼭대기(0) → 원 가운데(1)로 넓어지는 혀 모양(살짝 왼쪽으로 휨)
    tongue = (np.abs(xx - (cx + 1.2 * (1 - t))) <= r * t ** 1.4) & (yy < cy)
    return circle | tongue


def flame(pal, lit):
    cv = blank(pal, 12, 18)
    m = flame_mask()
    yy, xx = np.mgrid[0:18, 0:12] + 0.5
    if not lit:                                                # 꺼짐: 주황 테두리 + 어두운 속(빈 칸)
        out = edge(m, -1, 0) | edge(m, 1, 0) | edge(m, 0, -1) | edge(m, 0, 1)
        cv.fill(m & ~out, "bg1")
        cv.fill(out, "orange")
        cv.fill(out & (yy > 14), "red1")
        return cv
    cv.fill(m, "red2")                                         # 바깥 불(아래가 붉음)
    cv.fill(m & (yy < 15), "orange")
    inner = flame_mask()
    inner = m & ((xx - 6) ** 2 / 3.2 ** 2 + (yy - 12.5) ** 2 / 4.2 ** 2 <= 1) | (m & (np.abs(xx - 6.4) < 1.6) & (yy > 6) & (yy < 12))
    cv.fill(inner, "orange2")
    core = (xx - 6) ** 2 / 1.6 ** 2 + (yy - 13) ** 2 / 2.4 ** 2 <= 1
    cv.fill(core & m, "gold2")
    cv.fill(core & m & (yy < 12.5) & (xx < 6.5), "gold3")
    return cv


# ---------- 레버리지 상자 ----------
LEVER_BOX = (120, 76)          # 640 격자 = 지금 60x38 (36이면 글자 3줄이 테두리에 닿아 38로)


def lever_box(pal):
    w, h = LEVER_BOX
    cv = blank(pal, w, h)
    frame_box(cv, 0, 0, w, h, "orange", "bg1", t=2, light="red0", shade="bg00")
    cv.fill((slice(0, 1), slice(2, w - 2)), "orange2")          # 위 테두리 빛
    cv.fill((slice(2, h - 2), slice(0, 1)), "orange2")
    cv.fill((slice(h - 1, h), slice(1, w)), "red1")             # 아래 테두리 그늘
    cv.fill((slice(1, h), slice(w - 1, w)), "red1")
    return cv


# ---------- 슬롯 50x60 · 버튼 116x60 ----------
SLOT = (50, 60)
BUTTON = (116, 60)


def slot(pal, selected):
    w, h = SLOT
    cv = blank(pal, w, h)
    if selected:
        frame_box(cv, 0, 0, w, h, "gold2", "wood", t=2, light="gold1", shade="gold0")
        cv.fill((slice(0, 1), slice(0, w)), "gold3")
    else:
        frame_box(cv, 0, 0, w, h, "navy", "bg0", t=2, light="navy0", shade="bg00")
        cv.fill((slice(0, 1), slice(1, w - 1)), "navy2")
    return cv


def button(pal, kind):
    w, h = BUTTON
    cv = blank(pal, w, h)
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


# ---------- 아이템 아이콘(설계 좌표 21x16) ----------
U_W, U_H = 21.0, 16.0


def grid(w, h):
    yy, xx = np.mgrid[0:h, 0:w] + 0.5
    return xx * U_W / w, yy * U_H / h, U_W / w                 # 설계 좌표 u, v, 도트 1칸 크기(설계 단위)


def seg_dist(u, v, x1, y1, x2, y2):
    dx, dy = x2 - x1, y2 - y1
    t = np.clip(((u - x1) * dx + (v - y1) * dy) / (dx * dx + dy * dy), 0, 1)
    return np.hypot(u - (x1 + t * dx), v - (y1 + t * dy))


def outline(m):
    return edge(m, -1, 0) | edge(m, 1, 0) | edge(m, 0, -1) | edge(m, 0, 1)


def icon_chart(pal, w, h):
    """차트 분석 = 돋보기: 크림색 테 + 남색 유리 안 오르는 초록 선 + 초록 손잡이."""
    cv = blank(pal, w, h)
    u, v, px = grid(w, h)
    d = np.hypot(u - 8, v - 6.6)
    handle = seg_dist(u, v, 12.2, 10.6, 18.6, 15.0) <= 1.25
    cv.fill(handle, "green")
    cv.fill(handle & (seg_dist(u, v, 12.2, 10.6, 18.6, 15.0) <= 1.25) & (v - 10.6 < (u - 12.2) * 0.69 - 0.3), "green2")
    cv.fill(handle & (v - 10.6 > (u - 12.2) * 0.69 + 0.55), "green0")
    cv.fill(outline(handle), "green0")
    glass = d <= 4.3
    cv.fill(glass, "navy0")
    cv.fill(glass & (v > 6.6 + 0.4 * (u - 8)), "navy")       # 유리 아래쪽 반사
    line = (seg_dist(u, v, 4.6, 9.0, 6.4, 7.2) <= 0.55) | (seg_dist(u, v, 6.4, 7.2, 8.0, 8.2) <= 0.55) | (seg_dist(u, v, 8.0, 8.2, 10.6, 4.6) <= 0.55)
    cv.fill(line & glass, "green2")
    ring = (d > 4.3) & (d <= 5.7)
    cv.fill(ring, "cream")
    cv.fill(ring & (u + v > 8 + 6.6 + 1.5), "cream0")            # 오른쪽 아래 그늘
    cv.fill(ring & (u + v < 8 + 6.6 - 4.5), "cream2")            # 왼쪽 위 빛
    cv.fill((d > 5.7) & (d <= 5.7 + px) , "gray0")              # 테 바깥 선
    cv.fill(glass & (np.hypot(u - 6.2, v - 4.6) <= 0.9), "cream2")   # 유리 반짝임
    return cv


def icon_rumor(pal, w, h):
    """찌라시 = 신문: 뒤 장(그늘) + 크림색 종이 + 굵은 제호 + 빨간 사진 + 글줄."""
    cv = blank(pal, w, h)
    u, v, px = grid(w, h)
    back = (u >= 3.2) & (u <= 19.6) & (v >= 2.8) & (v <= 15.0)
    cv.fill(back, "cream0")
    cv.fill(outline(back), "gray1")
    paper = (u >= 1.6) & (u <= 18.2) & (v >= 1.4) & (v <= 13.8)
    cv.fill(paper, "cream")
    cv.fill(paper & (u > 18.2 - 1.2), "cream0")                 # 오른쪽 접힘 그늘
    cv.fill(paper & (v < 1.4 + px * 1.5), "cream2")
    cv.fill(outline(paper), "gray1")
    cv.fill((u >= 3.0) & (u <= 16.8) & (v >= 2.6) & (v <= 3.9), "gray0")      # 제호
    photo = (u >= 3.0) & (u <= 8.6) & (v >= 5.2) & (v <= 11.6)
    cv.fill(photo, "red2")
    cv.fill(photo & (u < 3.0 + 1.2) | photo & (v < 5.2 + 1.0), "red3")
    cv.fill(photo & ((u > 8.6 - 1.0) | (v > 11.6 - 1.0)), "red1")
    for y0 in (5.4, 7.4, 9.4, 11.2):
        cv.fill((u >= 10.0) & (u <= 16.8 - (1.8 if y0 > 11 else 0)) & (v >= y0) & (v <= y0 + 0.8), "gray1")
    return cv


def icon_halt(pal, w, h):
    """거래 정지 = 자물쇠: 회색 고리 + 금색 몸통(빛·그늘) + 열쇠 구멍."""
    cv = blank(pal, w, h)
    u, v, px = grid(w, h)
    d = np.hypot(u - 10.5, v - 6.4)
    shackle = (((d >= 2.6) & (d <= 4.2) & (v <= 6.4)) | ((np.abs(u - 10.5) >= 2.6) & (np.abs(u - 10.5) <= 4.2) & (v > 6.4) & (v <= 8.6)))
    cv.fill(shackle, "gray1")
    cv.fill(shackle & (u < 10.5 - 1.0) & (v < 6.4 + 1.5), "cream0")
    cv.fill(shackle & (u > 10.5 + 1.6), "gray0")
    body = (u >= 4.4) & (u <= 16.6) & (v >= 7.8) & (v <= 15.2)
    cv.fill(body, "gold1")
    cv.fill(body & (v < 7.8 + 1.1), "gold2")
    cv.fill(body & (u < 4.4 + 1.0), "gold2")
    cv.fill(body & ((u > 16.6 - 1.4) | (v > 15.2 - 1.2)), "goldD")
    cv.fill(outline(body), "gold0")
    cv.fill(body & (np.hypot(u - 6.0, v - 9.4) <= 0.6), "gold3")
    hole = (np.hypot(u - 10.5, v - 10.6) <= 1.15) | ((np.abs(u - 10.5) <= 0.5) & (v >= 10.6) & (v <= 13.2))
    cv.fill(hole & body, "bg00")
    return cv


def icon_lottery(pal, w, h):
    """복권: 양옆 홈이 파인 표 + 금 테두리 + 절취선 + 빨간 별."""
    cv = blank(pal, w, h)
    u, v, px = grid(w, h)
    ticket = (u >= 1.0) & (u <= 20.0) & (v >= 3.2) & (v <= 13.0)
    notch = (np.hypot(u - 1.0, v - 8.1) <= 1.7) | (np.hypot(u - 20.0, v - 8.1) <= 1.7)
    ticket &= ~notch
    cv.fill(ticket, "cream")
    rim = outline(ticket)
    inner_rim = ticket & ~rim & outline(ticket & ~rim)
    cv.fill(inner_rim, "gold1")
    cv.fill(rim, "gold0")
    cv.fill(ticket & ~rim & (v > 13.0 - 1.4), "cream0")
    cv.fill(ticket & ~rim & ~inner_rim & (np.abs(u - 6.2) < 0.4) & ((np.floor(v / 1.2) % 2) == 0), "gray1")   # 절취선
    ang = np.arctan2(v - 8.1, u - 13.2)
    r = np.hypot(u - 13.2, v - 8.1)
    k = np.cos(5 * (ang + np.pi / 2)) * 0.5 + 0.5                 # 별 모양: 각도마다 반지름이 바뀜
    star = r <= 1.5 + 1.6 * k
    cv.fill(star & ticket, "red2")
    cv.fill(star & ticket & (v < 8.1 - 0.4) & (u < 13.2 + 0.3), "red3")
    cv.fill(star & ticket & (r < 0.9), "red4")
    cv.fill(ticket & ~rim & (np.hypot(u - 3.2, v - 5.3) <= 0.7), "gold2")
    return cv


def plus_sign(cv, u, v, px, cx, cy, arm=2.0, half=0.75):
    """초록 "+" 하나(회복 표시): 몸통 green2, 둘레 green0."""
    p = ((np.abs(u - cx) <= arm) & (np.abs(v - cy) <= half)) | ((np.abs(v - cy) <= arm) & (np.abs(u - cx) <= half))
    cv.fill(outline(p), "green0")
    cv.fill(p, "green2")
    return p


def icon_dividend(pal, w, h):
    """heal-items 89 — 배당금(잔고 +1) = 금화 한 닢 + 초록 "+".
    금화: 바깥 테(gold0) · 몸통 gold1 · 왼쪽 위 빛 gold2/gold3 · 오른쪽 아래 그늘 goldD · 안쪽 고리(goldD) · 가운데 세로 홈."""
    cv = blank(pal, w, h)
    u, v, px = grid(w, h)
    d = np.hypot(u - 8.6, v - 9.0)
    coin = d <= 6.2
    cv.fill(coin, "gold1")
    cv.fill(coin & (u + v < 8.6 + 9.0 - 3.5), "gold2")             # 왼쪽 위 빛
    cv.fill(coin & (u + v > 8.6 + 9.0 + 4.0), "goldD")             # 오른쪽 아래 그늘
    cv.fill(coin & (np.abs(d - 4.3) <= 0.45), "goldD")             # 안쪽 고리
    cv.fill(coin & (np.abs(u - 8.6) <= 0.55) & (np.abs(v - 9.0) <= 2.4), "goldD")   # 가운데 홈
    cv.fill(coin & (np.hypot(u - 5.6, v - 5.8) <= 0.8), "gold3")   # 반짝임
    cv.fill((d > 6.2) & (d <= 6.2 + px * 1.2), "gold0")            # 바깥 테
    plus_sign(cv, u, v, px, 17.2, 4.4, arm=2.4, half=0.85)
    return cv


def icon_bailout(pal, w, h):
    """heal-items 89 — 구제 금융(잔고 +2) = 끈으로 묶은 동전 자루 + 초록 "++".
    자루: 둥근 몸통 + 끈 위로 부채꼴로 모인 주둥이(크림색 천, 왼쪽 빛·오른쪽 그늘) + 남색 끈 + 가운데 금화 무늬 하나.
    처음 시안(금화 무늬 2개·귀 같은 주둥이)은 얼굴처럼 보여서 바꿈."""
    cv = blank(pal, w, h)
    u, v, px = grid(w, h)
    body = ((u - 8.8) / 6.8) ** 2 + ((v - 10.4) / 5.2) ** 2 <= 1      # 둥근 몸통
    neck = (np.abs(u - 8.8) <= 2.2) & (v >= 5.4) & (v <= 6.8)          # 묶인 목
    fan = (v >= 1.8) & (v < 5.6) & (np.abs(u - 8.8) <= 1.4 + (5.6 - v) * 0.85)   # 위로 퍼지는 주둥이
    sack = body | neck | fan
    cv.fill(sack, "cream")
    cv.fill(sack & (u < 8.8 - 2.8) & (v > 7.5), "cream2")              # 왼쪽 빛
    cv.fill(sack & ((u > 8.8 + 3.6) | (v > 14.0)), "cream0")          # 오른쪽 아래 그늘
    cv.fill(fan & ((np.abs(u - 8.8) % 1.6) < 0.45) & (v < 4.8), "cream0")   # 주둥이 주름
    cv.fill(outline(sack), "gray0")
    tie = (np.abs(v - 5.6) <= 0.65) & (np.abs(u - 8.8) <= 2.6)          # 끈
    cv.fill(tie, "navy")
    cv.fill(tie & (v < 5.4), "navy2")
    d = np.hypot(u - 8.8, v - 10.8)                                     # 가운데 금화 무늬
    cv.fill(body & (d <= 2.6), "gold1")
    cv.fill(body & (d <= 2.6) & (u + v < 8.8 + 10.8 - 1.2), "gold2")
    cv.fill(body & (np.abs(d - 2.6) <= 0.45), "goldD")
    cv.fill(body & (np.abs(u - 8.8) <= 0.45) & (np.abs(v - 10.8) <= 1.4), "goldD")
    plus_sign(cv, u, v, px, 15.6, 3.8, arm=1.8, half=0.7)
    plus_sign(cv, u, v, px, 19.3, 3.8, arm=1.8, half=0.7)
    return cv


def icon_flip(pal, w, h):
    """flip-item 102 — 작전(다음 캔들 뒤집기) = 왼쪽 빨간(하락) 캔들 + 오른쪽 초록(상승) 캔들 + 서로 자리를 바꾸는 금색 화살표 2개.
    위 화살표는 왼쪽 → 오른쪽, 아래 화살표는 오른쪽 → 왼쪽(맞바꿈). 캔들은 심지(가는 세로선) + 몸통(빛·그늘 한 줄씩)."""
    cv = blank(pal, w, h)
    u, v, px = grid(w, h)

    def candle_at(cx, top, bot, wtop, wbot, c):
        wick = (np.abs(u - cx) <= 0.55) & (v >= wtop) & (v <= wbot)
        body = (np.abs(u - cx) <= 2.4) & (v >= top) & (v <= bot)
        cv.fill(wick, c["edge"])
        cv.fill(body, c["body"])
        cv.fill(body & (u < cx - 1.3), c["light"])                 # 왼쪽 빛
        cv.fill(body & (u > cx + 1.4), c["shade"])                 # 오른쪽 그늘
        cv.fill(outline(body), c["edge"])

    candle_at(4.2, 6.0, 12.6, 3.6, 14.6, dict(edge="red0", body="red2", light="red3", shade="red1"))      # 하락
    candle_at(16.8, 3.4, 10.0, 1.4, 12.4, dict(edge="green0", body="green", light="green2", shade="green0"))  # 상승

    def arrow(x1, x2, y, right):
        shaft = (seg_dist(u, v, x1, y, x2, y) <= 0.75)
        tip_x = x2 if right else x1
        s = -1 if right else 1                                     # 화살촉이 꺾이는 방향
        head = (seg_dist(u, v, tip_x, y, tip_x + s * 1.8, y - 1.8) <= 0.75) | (seg_dist(u, v, tip_x, y, tip_x + s * 1.8, y + 1.8) <= 0.75)
        m = shaft | head
        cv.fill(outline(m), "goldD")
        cv.fill(m, "gold2")

    arrow(8.2, 12.8, 4.6, right=True)       # 위: 하락 쪽 → 상승 쪽
    arrow(8.2, 12.8, 11.6, right=False)     # 아래: 상승 쪽 → 하락 쪽
    return cv


ICONS = {"chart": icon_chart, "rumor": icon_rumor, "halt": icon_halt, "lottery": icon_lottery}
HEAL_ICONS = {"dividend": icon_dividend, "bailout": icon_bailout}   # heal-items 89(따로 — 기존 아이콘 파일은 다시 안 그림)
FLIP_ICONS = {"flip": icon_flip}   # flip-item 102(따로 — 기존 아이콘 파일은 다시 안 그림)
ICON_SMALL = (42, 32)          # 슬롯·광대 아이템 (지금 21x16)
ICON_BIG = (68, 52)            # 확인 패널 (지금 34x26)
SPIN_W, SPIN_THICK = 80, 6     # 회전 그림 80x52 (지금 40x26), 두께 6도트


def spin_frames(cv, frames=8):
    """아이콘을 두께 SPIN_THICK인 판으로 세워 Y축으로 45°씩 돌린 정사영. 옆으로 보이는 면은 한 단계 어둡게(DARK)."""
    names = cv.names
    h, w = cv.idx.shape
    cx = w // 2
    out = []
    ys, xs = np.nonzero(cv.mask)
    for f in range(frames):
        a = np.radians(f * 360 / frames)
        ca, sa = np.cos(a), np.sin(a)
        o = Canvas(cv.pal, SPIN_W, h)
        depth = np.full((h, SPIN_W), -1e9)
        front = 1 if ca >= 0 else -1
        half = SPIN_THICK // 2
        for z in range(-half, half + 1):
            px = (xs - cx) * ca - z * sa
            pz = (xs - cx) * sa + z * ca
            sx = np.floor(px + 0.5).astype(int) + SPIN_W // 2
            facing = (z == front * half) and abs(ca) > 0.35
            for y, x, s, d in zip(ys, xs, sx, pz):
                if 0 <= s < SPIN_W and d > depth[y, s]:
                    depth[y, s] = d
                    n = names[cv.idx[y, x]]
                    o.idx[y, s] = names.index(n if facing else DARK.get(n, n))
                    o.mask[y, s] = True
        out.append(o)
    return out


def shadow(pal):
    cv = blank(pal, 64, 6)
    yy, xx = np.mgrid[0:6, 0:64] + 0.5
    e = (xx - 32) ** 2 / 31 ** 2 + (yy - 3) ** 2 / 3 ** 2 <= 1
    cv.fill(e & ((xx.astype(int) + yy.astype(int)) % 2 == 0), "bg00")   # 바둑판 점(반투명 대신)
    return cv


# ---------- 모으기 ----------
def parts(pal):
    p = {}
    for k in ("up", "down", "back"):
        p[f"candle-{k}"] = candle(pal, k)
    p["flame-on"], p["flame-off"] = flame(pal, True), flame(pal, False)
    p["leverage-box"] = lever_box(pal)
    p["slot-normal"], p["slot-selected"] = slot(pal, False), slot(pal, True)
    for k in ("normal", "selected", "disabled"):
        p[f"button-{k}"] = button(pal, k)
    for name, fn in ICONS.items():
        p[f"item-{name}"] = fn(pal, *ICON_SMALL)
        p[f"item-{name}-big"] = fn(pal, *ICON_BIG)
    return p


def paste(sheet, cv, x, y, pal):
    rgb = cv.rgba()
    m = rgb[..., 3] > 0
    reg = sheet[y:y + cv.h, x:x + cv.w]
    reg[m] = rgb[..., :3][m]


def make_draft(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    p = parts(pal)
    sheet = np.tile(pal["bg0"], (300, 640, 1)).astype(np.uint8)
    sheet[0:110] = pal["wood"]                                  # 위쪽: 테이블 색 바탕(캔들·불꽃·상자)
    for i, k in enumerate(("up", "down", "back")):
        paste(sheet, p[f"candle-{k}"], 12 + 16 * i, 12, pal)
    paste(sheet, p["flame-on"], 70, 20, pal)
    paste(sheet, p["flame-off"], 86, 20, pal)
    paste(sheet, p["leverage-box"], 110, 10, pal)
    paste(sheet, p["flame-on"], 120, 42, pal)
    paste(sheet, p["flame-off"], 136, 42, pal)
    paste(sheet, p["flame-off"], 152, 42, pal)
    for i, name in enumerate(ICONS):                            # 확인 패널 크기 아이콘
        paste(sheet, p[f"item-{name}-big"], 250 + 76 * i, 20, pal)
    # 아래쪽: 아래 띠 그대로 — 버튼 2개(선택·보통) + 슬롯 4칸(2번 선택) + 아이콘
    sheet[118:119] = pal["wood"]
    paste(sheet, p["button-selected"], 8, 128, pal)
    paste(sheet, p["button-normal"], 132, 128, pal)
    for i, name in enumerate(ICONS):
        x = 256 + 52 * i
        paste(sheet, p["slot-selected" if i == 1 else "slot-normal"], x, 128, pal)
        paste(sheet, p[f"item-{name}"], x + 4, 128 + 26, pal)
    paste(sheet, p["button-disabled"], 470, 128, pal)
    # 회전 그림 한 줄(복권)
    for f, fr in enumerate(spin_frames(p["item-lottery-big"])):
        paste(sheet, fr, 4 + 79 * f, 200, pal)
    write_png(OUT / "items-draft-x2.png", sheet.repeat(2, 0).repeat(2, 1))
    files = []
    for name, cv in p.items():
        f = OUT / f"{name}.png"
        write_png(f, cv.rgba())
        files.append(f)
    print("팔레트 검사 (32색):")
    return check32(files, pal)


def make_game(pal):
    """게임 파일 이름 그대로 저장(같은 이름에 덮어써 Scene·ItemData 연결 유지). 회전 그림 4 x 8 + 그림자."""
    dst = OUT / "game"
    dst.mkdir(parents=True, exist_ok=True)
    p = parts(pal)
    files = []
    for name, cv in p.items():
        f = dst / f"{name}.png"
        write_png(f, cv.rgba())
        files.append(f)
    for name in ICONS:
        for k, fr in enumerate(spin_frames(p[f"item-{name}-big"])):
            f = dst / f"spin-{name}-{k}.png"
            write_png(f, fr.rgba())
            files.append(f)
    f = dst / "spin-shadow.png"
    write_png(f, shadow(pal).rgba())
    files.append(f)
    print(f"그림 {len(files)}장 → {dst}")
    return check32(files, pal)


def make_heal(pal):
    dst = OUT / "game"
    dst.mkdir(parents=True, exist_ok=True)
    files = []
    for name, fn in HEAL_ICONS.items():
        for suf, size in (("", ICON_SMALL), ("-big", ICON_BIG)):
            f = dst / f"item-{name}{suf}.png"
            write_png(f, fn(pal, *size).rgba())
            files.append(f)
    # 미리보기: 위 = 슬롯 크기 6종(기존 4 + 새 2, 폰 칸 바탕색), 아래 = 확인 패널 크기 6종 — ×3
    allicons = list(ICONS.items()) + list(HEAL_ICONS.items())
    sheet = np.tile(pal["bg0"], (32 + 52 + 12, 6 * 72 + 8, 1)).astype(np.uint8)
    for i, (name, fn) in enumerate(allicons):
        paste(sheet, fn(pal, *ICON_SMALL), 4 + 72 * i + 13, 2, pal)
        paste(sheet, fn(pal, *ICON_BIG), 4 + 72 * i, 40, pal)
    write_png(OUT / "heal-preview-x3.png", sheet.repeat(3, 0).repeat(3, 1))
    print("팔레트 검사 (32색):")
    return check32(files, pal)


def make_flip(pal):
    """flip-item 102: 작전 아이콘 2장(슬롯 42x32·확인 68x52) + 기존 6종과 나란히 미리보기(×3)."""
    dst = OUT / "game"
    dst.mkdir(parents=True, exist_ok=True)
    files = []
    for name, fn in FLIP_ICONS.items():
        for suf, size in (("", ICON_SMALL), ("-big", ICON_BIG)):
            f = dst / f"item-{name}{suf}.png"
            write_png(f, fn(pal, *size).rgba())
            files.append(f)
    allicons = list(ICONS.items()) + list(HEAL_ICONS.items()) + list(FLIP_ICONS.items())
    n = len(allicons)
    sheet = np.tile(pal["bg0"], (32 + 52 + 12, n * 72 + 8, 1)).astype(np.uint8)
    for i, (name, fn) in enumerate(allicons):
        paste(sheet, fn(pal, *ICON_SMALL), 4 + 72 * i + 13, 2, pal)
        paste(sheet, fn(pal, *ICON_BIG), 4 + 72 * i, 40, pal)
    write_png(OUT / "flip-preview-x3.png", sheet.repeat(3, 0).repeat(3, 1))
    print("팔레트 검사 (32색):")
    return check32(files, pal)


if __name__ == "__main__":
    pal = load_pal()
    if len(sys.argv) > 1 and sys.argv[1] == "flip":   # flip-item 102: 작전 아이콘
        sys.exit(0 if make_flip(pal) else 1)
    cmd = sys.argv[1] if len(sys.argv) > 1 else ""
    if cmd == "draft":
        sys.exit(0 if make_draft(pal) else 1)
    if cmd == "game":
        sys.exit(0 if make_game(pal) else 1)
    if cmd == "heal":   # heal-items 89: 배당금·구제 금융 아이콘(슬롯 42x32·확인 68x52) + 기존 4종과 나란히 미리보기
        sys.exit(0 if make_heal(pal) else 1)
    print(__doc__)

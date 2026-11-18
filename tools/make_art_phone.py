"""Falling Knife 아이템 폴더폰 그림 (AI 작성, item-phone Ticket 62).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art_phone.py draft   # 시안 → .scratch/art/drafts/hd/phone/ + 팔레트 검사

- 640 격자(지금 좌표 x2 = "새 도트"), 32색, 무작위 없음. 글자(개수·"+N"·NEW·아이템 이름)는 그림에 넣지 않고 게임에서 TMP로.
- phone-closed.png   80×48  테이블 위에 비스듬히 누운 접힌 내 폰(위 뚜껑 + 앞 옆면 + 오른쪽 경첩 + 바깥 작은 화면)
- phone-closed-outline.png 84×52  열 수 있을 때 금색 테두리(모니터 금색 테두리와 같은 방식, 위에 겹침)
- phone-open.png     200×330 확대된 펼친 내 폰(위 = 화면 86×71 지금 좌표에 제목 + 칸 2×2, 가운데 경첩, 아래 = 키패드 3×4 + 위 버튼 줄)
- phone-key-lit-1~4.png 44×20 키패드 1~4를 금색으로(그 칸에 아이템이 있을 때 위에 겹침)
- phone-clown.png    64×88  펼쳐 세운 광대 폰(위 화면 54×40에 작은 아이콘 2×2 자리)
- phone-clown-flash.png 26×20 광대가 쓰는 칸 반짝임 테두리
- mini-chart / mini-rumor / mini-halt / mini-lottery .png 24×18  광대 폰용 작은 아이템 아이콘
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import write_png  # noqa: E402
from make_art_hd import Canvas, check32, load_pal, polygon, edge  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "hd" / "phone"

# 3×5 숫자 글꼴(키패드 글자). 1 = 칠함
DIGITS = {
    "1": ["010", "110", "010", "010", "111"], "2": ["111", "001", "111", "100", "111"],
    "3": ["111", "001", "111", "001", "111"], "4": ["101", "101", "111", "001", "001"],
    "5": ["111", "100", "111", "001", "111"], "6": ["111", "100", "111", "101", "111"],
    "7": ["111", "001", "010", "010", "010"], "8": ["111", "101", "111", "101", "111"],
    "9": ["111", "101", "111", "001", "111"], "0": ["111", "101", "101", "101", "111"],
    "*": ["000", "101", "010", "101", "000"], "#": ["101", "111", "101", "111", "101"],
}


def glyph(cv, ch, x, y, col, s=2):
    for r, row in enumerate(DIGITS[ch]):
        for c, v in enumerate(row):
            if v == "1":
                cv.fill((slice(y + r * s, y + (r + 1) * s), slice(x + c * s, x + (c + 1) * s)), col)


def round_rect(w, h, x0, y0, x1, y1, r):
    """모서리를 r도트 깎은 사각형 mask."""
    yy, xx = np.mgrid[0:h, 0:w]
    m = (xx >= x0) & (xx < x1) & (yy >= y0) & (yy < y1)
    for cx, cy in ((x0 + r, y0 + r), (x1 - r - 1, y0 + r), (x0 + r, y1 - r - 1), (x1 - r - 1, y1 - r - 1)):
        corner = ((xx < x0 + r) if cx == x0 + r else (xx > x1 - r - 1)) & ((yy < y0 + r) if cy == y0 + r else (yy > y1 - r - 1))
        m &= ~(corner & ((xx - cx) ** 2 + (yy - cy) ** 2 > r * r))
    return m


def dilate(m):
    out = m.copy()
    out[1:] |= m[:-1]
    out[:-1] |= m[1:]
    out[:, 1:] |= m[:, :-1]
    out[:, :-1] |= m[:, 1:]
    return out


def outline_of(mask, t=1):
    """mask 바깥으로 t도트 두께 띠. outline_of(~shape, t) & shape = shape 안쪽 가장자리 t도트."""
    m = mask.copy()
    for _ in range(t):
        m = dilate(m)
    return m & ~mask


# ---------- ① 접힌 내 폰(테이블 위, 80×48) ----------
CW, CH = 80, 48


def closed_mask():
    # 누운 폰을 앞 위에서 본 모양: 둥근 모서리 윗면(뚜껑) + 앞 옆면 두께 5. 오른쪽 = 경첩(둥글게 불룩)
    top = round_rect(CW, CH, 4, 8, 70, 38, 7)
    hinge = round_rect(CW, CH, 62, 6, 78, 40, 6)
    side = round_rect(CW, CH, 4, 30, 78, 44, 6) & ~top & ~hinge
    return top | hinge, side


def phone_closed(pal):
    cv = Canvas(pal, CW, CH)
    top, side = closed_mask()
    yy, xx = np.mgrid[0:CH, 0:CW]
    # 그림자(테이블 위)
    shadow = round_rect(CW, CH, 6, 34, 80, 48, 6) & ~top & ~side
    cv.fill(shadow, "bg00")
    cv.fill(side, "navy0")
    cv.fill(side & (yy >= 42), "bg00")
    cv.fill(top, "navy")
    # 뚜껑 가장자리: 위쪽 빛 1줄, 금색 테 1줄(안쪽)
    cv.fill(outline_of(~top, 1) & top, "gold0")
    cv.fill(outline_of(~top, 1) & top & (yy < 12), "goldD")
    cv.fill(top & (yy == 10) & (xx > 10) & (xx < 58), "navy2")
    # 경첩(오른쪽 원통): 세로 줄무늬
    hinge = top & (xx >= 64) & (xx < 76) & (yy > 8) & (yy < 38)
    cv.fill(hinge, "navy0")
    cv.fill(hinge & (xx >= 66) & (xx < 69), "navy2")
    cv.fill(top & (xx == 63) & (yy > 9) & (yy < 37), "bg00")
    # 바깥 작은 화면(지금 좌표 약 14×8 = 28×16): 금색 틀 + 어두운 화면(숫자는 TMP)
    lx0, ly0 = 24, 14
    cv.fill((slice(ly0, ly0 + 18), slice(lx0, lx0 + 32)), "gold1")
    cv.fill((slice(ly0 + 1, ly0 + 17), slice(lx0 + 1, lx0 + 31)), "goldD")
    cv.fill((slice(ly0 + 2, ly0 + 16), slice(lx0 + 2, lx0 + 30)), "bg00")
    cv.fill((slice(ly0 + 2, ly0 + 3), slice(lx0 + 2, lx0 + 30)), "navy0")
    # 작은 불빛(LED) — 받을 때 깜빡일 자리
    cv.fill((slice(21, 24), slice(13, 16)), "green0")
    return cv


def phone_closed_outline(pal):
    cv = Canvas(pal, CW + 4, CH + 4)
    top, side = closed_mask()
    m = np.zeros((CH + 4, CW + 4), bool)
    m[2:-2, 2:-2] = top | side
    ring = outline_of(m, 2)
    cv.fill(ring, "gold2")
    return cv


# ---------- ② 펼친 내 폰(확대, 200×330 = 지금 100×165) ----------
# 사용자 고침(2026-11-18): "아이템은 2×2, 가로 폭 좁게(폴더폰이니까)" → 처음 300×344(화면 한 줄 4칸)에서 좁힘
OW, OH = 200, 330
LID_H, HINGE_H = 170, 14            # 위 뚜껑 0~170, 경첩 170~184, 아래 180~330
SCREEN = (14, 18, 186, 160)         # 화면 x 14~186, y 18~160 = 172×142(지금 86×71): 제목 줄 + 칸 2×2(28×28, 사이 4)
KEY_W, KEY_H, KEY_GX, KEY_GY = 44, 20, 12, 6
KEYS_X0 = (OW - (3 * KEY_W + 2 * KEY_GX)) // 2
BTN_Y0 = LID_H + HINGE_H + 6         # 위 버튼 줄(통화·선택·끊기)
KEYS_Y0 = BTN_Y0 + 18 + 8           # 키패드 3×4
KEY_LABELS = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "*", "0", "#"]


def key_rect(i):
    r, c = divmod(i, 3)
    x0 = KEYS_X0 + c * (KEY_W + KEY_GX)
    y0 = KEYS_Y0 + r * (KEY_H + KEY_GY)
    return x0, y0, x0 + KEY_W, y0 + KEY_H


def draw_key(cv, i, lit):
    x0, y0, x1, y1 = key_rect(i)
    m = round_rect(cv.w, cv.h, x0, y0, x1, y1, 5)
    cv.fill(m, "gold0" if lit else "navy0")
    yy, xx = np.mgrid[0:cv.h, 0:cv.w]
    cv.fill(m & (yy < y0 + 3), "gold2" if lit else "navy2")
    cv.fill(m & (yy >= y1 - 2), "bg00")
    glyph(cv, KEY_LABELS[i], (x0 + x1) // 2 - 3, (y0 + y1) // 2 - 5, "gold3" if lit else "cream0", 2)


def phone_open(pal):
    cv = Canvas(pal, OW, OH)
    yy, xx = np.mgrid[0:OH, 0:OW]
    # 뚜껑: 화면이 크게 들어가도록 몸체 = 화면 + 테두리
    lid = round_rect(OW, OH, 0, 0, OW, LID_H, 14)
    base = round_rect(OW, OH, 4, LID_H + HINGE_H - 4, OW - 4, OH, 18)
    cv.fill(lid, "navy")
    cv.fill(lid & (yy < 6), "navy2")
    cv.fill(outline_of(~lid, 2) & lid, "gold1")
    cv.fill(outline_of(~lid, 1) & lid, "goldD")
    # 화면 + 위 스피커 홈
    sx0, sy0, sx1, sy1 = SCREEN
    cv.fill((slice(sy0 - 2, sy1 + 2), slice(sx0 - 2, sx1 + 2)), "bg00")
    cv.fill((slice(sy0, sy1), slice(sx0, sx1)), "bg0")
    for y in range(sy0 + 1, sy1, 2):
        cv.fill((slice(y, y + 1), slice(sx0, sx1)), "bg00")
    cv.fill((slice(7, 10), slice(OW // 2 - 18, OW // 2 + 18)), "bg00")       # 스피커
    for x in range(OW // 2 - 16, OW // 2 + 18, 4):
        cv.fill((slice(8, 9), slice(x, x + 2)), "gray0")
    # 경첩(원통 가로 띠)
    hg = (yy >= LID_H - 2) & (yy < LID_H + HINGE_H) & (xx >= 20) & (xx < OW - 20)
    cv.fill(hg, "navy0")
    cv.fill(hg & (yy < LID_H + 4), "gray0")
    cv.fill(hg & (yy >= LID_H + HINGE_H - 3), "bg00")
    # 아래 몸체
    cv.fill(base, "navy")
    cv.fill(outline_of(~base, 2) & base, "gold1")
    cv.fill(outline_of(~base, 1) & base, "goldD")
    cv.fill(base & (yy >= OH - 8), "navy0")
    # 위 버튼 줄: 왼쪽 통화(초록) · 가운데 둥근 선택 · 오른쪽 끊기(빨강)
    by0 = BTN_Y0
    for x0, col in ((KEYS_X0, "green0"), (OW - KEYS_X0 - 36, "red0")):
        m = round_rect(OW, OH, x0, by0, x0 + 36, by0 + 18, 6)
        cv.fill(m, col)
        cv.fill(m & (yy < by0 + 3), "green" if col == "green0" else "red1")
    c = ((xx - OW // 2) ** 2) / 20 ** 2 + ((yy - (by0 + 9)) ** 2) / 11 ** 2 <= 1
    cv.fill(c, "navy0")
    cv.fill(c & (yy < by0 + 2), "navy2")
    cv.fill(((xx - OW // 2) ** 2) / 8 ** 2 + ((yy - (by0 + 9)) ** 2) / 5 ** 2 <= 1, "gray0")
    for i in range(12):
        draw_key(cv, i, False)
    # 마이크 구멍
    cv.fill((slice(OH - 5, OH - 3), slice(OW // 2 - 2, OW // 2 + 2)), "bg00")   # 키 아래(0 키 글자를 가리지 않게)
    return cv


def key_lit(pal, i):
    """키패드 i(0=1번) 자리만 금색으로 다시 그린 조각(52×24) — 게임에서 그 키 위에 겹침."""
    full = Canvas(pal, OW, OH)
    draw_key(full, i, True)
    x0, y0, x1, y1 = key_rect(i)
    cv = Canvas(pal, x1 - x0, y1 - y0)
    cv.idx[:] = full.idx[y0:y1, x0:x1]
    cv.mask[:] = full.mask[y0:y1, x0:x1]
    return cv


# ---------- ③ 광대 폰(펼쳐 세움, 64×88) ----------
KW, KH = 64, 88
K_SCR = (5, 4, 59, 44)              # 화면 54×40: 작은 아이콘 2×2(24×18, 사이 2)


def phone_clown(pal):
    cv = Canvas(pal, KW, KH)
    yy, xx = np.mgrid[0:KH, 0:KW]
    lid = round_rect(KW, KH, 0, 0, KW, 48, 5)
    base = polygon([(2, 50), (62, 50), (64, 84), (0, 84)], KW, KH)
    cv.fill((slice(84, 88), slice(2, 62)), "bg00")                       # 테이블 위 그림자
    cv.fill(lid, "navy")
    cv.fill(outline_of(~lid, 1) & lid, "gold1")
    x0, y0, x1, y1 = K_SCR
    cv.fill((slice(y0 - 1, y1 + 1), slice(x0 - 1, x1 + 1)), "bg00")
    cv.fill((slice(y0, y1), slice(x0, x1)), "bg0")
    for y in range(y0 + 1, y1, 2):
        cv.fill((slice(y, y + 1), slice(x0, x1)), "bg00")
    cv.fill((slice(46, 51), slice(8, 56)), "navy0")                       # 경첩
    cv.fill((slice(46, 47), slice(8, 56)), "gray0")
    cv.fill(base, "navy0")
    cv.fill(base & (yy < 54), "navy")
    cv.fill(outline_of(~base, 1) & base, "goldD")
    for r in range(4):                                                    # 작은 키패드 점(3×4)
        for c in range(3):
            kx, ky = 16 + c * 12, 57 + r * 6
            cv.fill((slice(ky, ky + 3), slice(kx, kx + 8)), "navy2")
    return cv


def clown_flash(pal):
    cv = Canvas(pal, 26, 20)
    m = np.ones((20, 26), bool)
    m[2:-2, 2:-2] = False
    cv.fill(m, "gold2")
    cv.fill((slice(0, 1), slice(0, 26)), "gold3")
    return cv


# ---------- ④ 작은 아이템 아이콘 24×18 ----------
def mini_chart(pal):
    cv = Canvas(pal, 24, 18)
    # 오르내리는 선(초록) + 돋보기(금 테 + 크림 렌즈)
    pts = [(1, 13), (5, 9), (8, 11), (12, 5), (15, 8)]
    for (x0, y0), (x1, y1) in zip(pts, pts[1:]):
        for t in np.linspace(0, 1, 12):
            x, y = int(round(x0 + (x1 - x0) * t)), int(round(y0 + (y1 - y0) * t))
            cv.fill((slice(y, y + 2), slice(x, x + 1)), "green2")
    yy, xx = np.mgrid[0:18, 0:24]
    d = (xx - 16) ** 2 + (yy - 7) ** 2
    cv.fill((d <= 30) & (d > 14), "gold2")
    cv.fill(d <= 14, "cream0")
    cv.fill((d <= 6) & (xx < 16) & (yy < 7), "cream2")
    for i in range(5):
        cv.fill((slice(11 + i, 13 + i), slice(19 + i, 21 + i)), "gold1")
    return cv


def mini_rumor(pal):
    cv = Canvas(pal, 24, 18)
    cv.fill((slice(1, 17), slice(3, 21)), "cream0")
    cv.fill((slice(1, 16), slice(3, 20)), "cream")
    cv.fill((slice(3, 6), slice(5, 18)), "red2")                          # 큰 제목
    for y in (8, 11, 14):
        cv.fill((slice(y, y + 1), slice(5, 18)), "gray1")
    cv.fill((slice(8, 13), slice(13, 18)), "gray0")                       # 사진 칸
    return cv


def mini_halt(pal):
    cv = Canvas(pal, 24, 18)
    yy, xx = np.mgrid[0:18, 0:24]
    d = (xx - 12) ** 2 + (yy - 7) ** 2
    cv.fill((d <= 30) & (d >= 12) & (yy <= 8), "gray1")                   # 고리
    cv.fill((slice(7, 17), slice(5, 19)), "gold1")
    cv.fill((slice(7, 8), slice(5, 19)), "gold3")
    cv.fill((slice(16, 17), slice(5, 19)), "goldD")
    cv.fill((slice(10, 13), slice(11, 13)), "bg00")                       # 열쇠 구멍
    cv.fill((slice(12, 15), slice(11, 13)), "bg00")
    return cv


def mini_lottery(pal):
    cv = Canvas(pal, 24, 18)
    cv.fill((slice(3, 15), slice(1, 23)), "cream")
    cv.fill((slice(3, 4), slice(1, 23)), "cream2")
    cv.fill((slice(14, 15), slice(1, 23)), "cream0")
    for y in (7, 10):                                                      # 양옆 홈
        cv.fill((slice(y, y + 2), slice(1, 2)), "bg0")
        cv.fill((slice(y, y + 2), slice(22, 23)), "bg0")
    cv.fill((slice(4, 14), slice(7, 8)), "gray1")                         # 절취선
    yy, xx = np.mgrid[0:18, 0:24]
    star = polygon([(15, 4), (16.6, 7.6), (20, 8), (17.4, 10.2), (18.2, 14), (15, 12), (11.8, 14),
                    (12.6, 10.2), (10, 8), (13.4, 7.6)], 24, 18)
    cv.fill(star, "red2")
    return cv


def make_draft(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    parts = {"phone-closed": phone_closed(pal), "phone-closed-outline": phone_closed_outline(pal),
             "phone-open": phone_open(pal), "phone-clown": phone_clown(pal), "phone-clown-flash": clown_flash(pal),
             "mini-chart": mini_chart(pal), "mini-rumor": mini_rumor(pal), "mini-halt": mini_halt(pal),
             "mini-lottery": mini_lottery(pal)}
    for i in range(4):
        parts[f"phone-key-lit-{i + 1}"] = key_lit(pal, i)
    files = []
    for name, cv in parts.items():
        p = OUT / f"{name}.png"
        write_png(p, cv.rgba())
        files.append(p)
    # 확대 미리보기(×4, 작은 것만)
    for name in ("phone-closed", "phone-clown", "mini-chart", "mini-rumor", "mini-halt", "mini-lottery"):
        a = parts[name].rgba()
        write_png(OUT / f"{name}-x4.png", a.repeat(4, 0).repeat(4, 1))
    print("팔레트 검사 (32색):")
    return check32(files, pal)


if __name__ == "__main__":
    cmd = sys.argv[1] if len(sys.argv) > 1 else ""
    if cmd == "draft":
        sys.exit(0 if make_draft(load_pal()) else 1)
    print(__doc__)

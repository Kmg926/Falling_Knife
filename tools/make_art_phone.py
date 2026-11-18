"""Falling Knife 아이템 폴더폰 그림 (AI 작성, item-phone Ticket 62).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art_phone.py draft   # 시안 → .scratch/art/drafts/hd/phone/ + 팔레트 검사
    python tools/make_art_phone.py table   # item-phone 66: 테이블 원근으로 눕힌 접힌 폰(phone-table-mine 89×39·phone-table-clown 69×46 + 테두리) — 지금 게임에서 씀
    python tools/make_art_phone.py far     # (거절된 시안) 모양이 다른 입체 폰
    python tools/make_art_phone.py closed3d # (거절된 시안) 폰 자체 입체감

- 640 격자(지금 좌표 x2 = "새 도트"), 32색, 무작위 없음. 글자(개수·"+N"·NEW·아이템 이름)는 그림에 넣지 않고 게임에서 TMP로.
- phone-closed.png   80×48  테이블 위에 비스듬히 누운 접힌 내 폰(위 뚜껑 + 앞 옆면 + 오른쪽 경첩 + 바깥 작은 화면)
- phone-closed-outline.png 84×52  열 수 있을 때 금색 테두리(모니터 금색 테두리와 같은 방식, 위에 겹침)
- phone-open.png     200×354 확대된 펼친 내 폰(위 = 화면 86×71 지금 좌표에 제목 + 칸 2×2, 가운데 경첩, 아래 = 키패드 3×4 + 위 버튼 줄)
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
from make_art_hd import BAYER4, Canvas, check32, load_pal, polygon, edge  # noqa: E402

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


# 바깥 화면 숫자(item-phone 66): 5×7 둥근 픽셀 숫자 — 3×5보다 읽기 쉬움(3×5 "2"는 "ㄹ"처럼 보였음)
DIGITS57 = {
    "0": [".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###."],
    "1": ["..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###."],
    "2": [".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####"],
    "3": ["####.", "....#", "....#", ".###.", "....#", "....#", "####."],
    "4": ["...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#."],
}


def glyph57(cv, ch, x, y, col, s=2):
    for r, row in enumerate(DIGITS57[ch]):
        for c, v in enumerate(row):
            if v == "#":
                cv.fill((slice(y + r * s, y + (r + 1) * s), slice(x + c * s, x + (c + 1) * s)), col)


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


def phone_closed_3d(pal):
    """phone_closed와 같은 모양(같은 mask)에 입체감만 더함(사용자 2026-11-18: "형태는 좋은데 입체감이 없어").
    빛은 왼쪽 위에서: 뚜껑 윗면 왼쪽 위 밝음 → 오른쪽 아래 어두움(바둑판 섞기), 가장자리 안쪽 빛·그늘 줄(볼록한 모서리),
    앞 옆면 = 뚜껑 두께 + 이음새 + 아래 몸체 두께, 경첩 원통 가로 음영(어둠·빛·어둠), 바깥 화면은 움푹(안쪽 위 그늘·아래 빛),
    뚜껑 왼쪽 위 반짝임, 그림자는 오른쪽 아래로."""
    cv = Canvas(pal, CW, CH)
    top, side = closed_mask()
    yy, xx = np.mgrid[0:CH, 0:CW]
    lid = round_rect(CW, CH, 4, 8, 70, 38, 7)
    hinge_m = top & (xx >= 64)                         # 경첩 원통 = 오른쪽 끝(원래 그림과 같은 범위, 뚜껑과 겹침)
    # 그림자: 오른쪽 아래로 밀린 몸통 모양, 가장자리는 바둑판으로 옅게
    body = top | side
    sh = np.zeros_like(body)
    sh[3:, 1:] = body[:-3, :-1]                      # 아래로 3, 오른쪽으로 1(빛이 왼쪽 위 → 그림자는 주로 아래)
    sh2 = np.zeros_like(body)
    sh2[4:, 2:] = body[:-4, :-2]
    cv.fill(sh2 & ~body & ((xx + yy) % 2 == 0), "bg00")
    cv.fill(sh & ~body, "bg00")
    # 앞 옆면: 위(뚜껑 두께) navy0 + 위 모서리 빛 gray0, 이음새 bg00, 아래(몸체 두께) navy0, 바닥 bg00
    cv.fill(side, "navy0")
    cv.fill(side & (yy == 38), "gray0")
    cv.fill(side & (yy == 39), "navy")
    cv.fill(side & (yy == 40), "bg00")                  # 이음새(접힌 틈)
    cv.fill(side & (yy == 41), "navy")
    cv.fill(side & (yy >= 43), "bg00")
    cv.fill(side & (xx >= 72), "bg00")   # 오른쪽 끝 옆면은 더 어둡게
    # 뚜껑 윗면: 왼쪽 위 navy2 → navy(바둑판) → 오른쪽 아래 navy0
    d = (xx - 4) / 66 * 0.6 + (yy - 8) / 30 * 0.4          # 0 = 왼쪽 위, 1 = 오른쪽 아래
    cv.fill(lid, "navy")
    cv.dither(lid & (d < 0.3), "navy", "navy2", np.clip(1 - d / 0.3, 0, 1) * 0.6)   # 왼쪽 위만 밝게(바둑판은 좁은 띠에서만)
    cv.dither(lid & (d > 0.75), "navy0", "navy", np.clip((1.0 - d) / 0.25, 0, 1))   # 오른쪽 아래 끝만 어둡게
    # 볼록한 가장자리: 안쪽 1줄 — 위·왼쪽은 빛(gray0), 아래·오른쪽은 그늘(navy0), 그 바깥 금색 테(위 gold1, 아래 goldD)
    rim = outline_of(~lid, 1) & lid
    cv.fill(rim, "goldD")
    cv.fill(rim & ((yy < 14) | (xx < 8)), "gold1")
    in1 = outline_of(~lid, 2) & lid & ~rim
    cv.fill(in1 & ((yy < 16) | (xx < 10)) & ~((yy > 30) | (xx > 60)), "navy2")
    cv.fill(in1 & ((yy > 32) | (xx > 64)), "navy0")
    # 경첩 원통(오른쪽): 가로로 어둠 → 빛 → 어둠, 위 끝 밝은 마개, 아래 끝 그늘
    cv.fill(hinge_m, "navy0")
    cv.fill(hinge_m & (xx >= 66) & (xx < 73), "navy")
    cv.fill(hinge_m & (xx >= 67) & (xx < 70), "navy2")
    cv.fill(hinge_m & (xx == 68) & (yy > 10) & (yy < 35), "gray0")
    cv.fill(hinge_m & (xx >= 74), "bg00")
    hrim = outline_of(~top, 1) & hinge_m                 # 경첩도 금색 테(위는 밝게)
    cv.fill(hrim, "goldD")
    cv.fill(hrim & (yy < 14), "gold1")
    cv.fill(hinge_m & (yy <= 8), "navy2")
    cv.fill(hinge_m & (yy >= 37), "bg00")
    cv.fill(top & (xx == 63) & (yy > 9) & (yy < 37), "bg00")   # 뚜껑과 경첩 사이 틈
    # 바깥 작은 화면(같은 자리): 금색 틀(왼쪽 위 밝게·오른쪽 아래 어둡게) + 움푹한 화면(안쪽 위·왼쪽 그늘)
    lx0, ly0 = 24, 14
    cv.fill((slice(ly0, ly0 + 18), slice(lx0, lx0 + 32)), "goldD")
    cv.fill((slice(ly0, ly0 + 1), slice(lx0, lx0 + 31)), "gold3")
    cv.fill((slice(ly0, ly0 + 17), slice(lx0, lx0 + 1)), "gold1")
    cv.fill((slice(ly0 + 1, ly0 + 17), slice(lx0 + 1, lx0 + 31)), "gold1")
    cv.fill((slice(ly0 + 2, ly0 + 16), slice(lx0 + 2, lx0 + 30)), "bg00")
    cv.fill((slice(ly0 + 2, ly0 + 4), slice(lx0 + 2, lx0 + 30)), "bg0")    # 위 안쪽 그늘(유리 반사 대신 깊이)
    cv.fill((slice(ly0 + 2, ly0 + 16), slice(lx0 + 2, lx0 + 3)), "bg0")
    cv.fill((slice(ly0 + 15, ly0 + 16), slice(lx0 + 3, lx0 + 30)), "navy0")  # 아래 안쪽 빛 1줄
    cv.fill((slice(ly0 + 3, ly0 + 4), slice(lx0 + 22, lx0 + 27)), "navy")   # 유리 반짝임 짧은 줄
    # LED(볼록): 초록 + 위 밝은 점
    cv.fill((slice(21, 24), slice(13, 16)), "green0")
    cv.fill((slice(21, 22), slice(13, 14)), "green2")
    # 뚜껑 왼쪽 위 반짝임
    cv.fill((slice(11, 12), slice(10, 14)), "cream0")
    cv.fill((slice(12, 13), slice(9, 10)), "navy2")
    return cv


# ---------- ①-c 테이블 위에 눕힌 접힌 폰(item-phone 66, 사용자: "테이블에 올려져 있는 듯한 모습") ----------
# 지금 접힌 폰의 윗면(뚜껑·경첩·바깥 화면)을 위에서 본 그림으로 보고, 테이블과 같은 원근(소실점 VP)으로 눕혀 그린다:
# 먼 쪽이 좁고(옆선이 VP를 향함) 위아래로 눌림 + 가까운 쪽에 두께(옆면) + 테이블에 닿는 그림자.
VP_T = (320, -116)          # make_art_hd.VP와 같음(테이블 옆선이 모이는 점, 640 격자)
# 폰마다 테이블 위 자리(640 격자): 가까운 끝 y, 왼쪽·오른쪽 x, 윗면 높이(눌린 깊이), 두께
TABLE_PHONES = {
    # 내 폰: 처음 dict(yn=326, xl=436, xr=512, depth=26, thick=8)(곧게 눕힘) → 사용자 "숫자 가독성 위해 두 폰 크기 늘리자" → 바닥 원근 계산 방식(돌림 0)·1.35배, 숫자도 그림에
    "mine": dict(cx=478, cy=318, phi=0.8, scale=1.12, theta=18, thick=8),   # 1.35배는 너무 큼(사용자) → 처음(1.03)보다 아주 조금 큰 1.12, 광대 폰과 같은 18° 틀기
    # 광대 폰: 처음 dict(yn=230, xl=434, xr=493, depth=20, thick=6)(곧게 눕힘) → 사용자 "원근감·입체감 더" → 바닥 원근 계산 + 비스듬히 돌림
    "clown": dict(cx=466, cy=224, phi=0.9, scale=0.92, theta=18, thick=5),   # phi 0.6·0.75는 숫자 넘침. scale 0.85 → 1.15(너무 큼) → 0.92
}


def top_view_src(pal, digit=None):
    """phone_closed에서 윗면(뚜껑+경첩)만 잘라 위에서 본 원본으로."""
    cv = phone_closed(pal)
    if digit is not None:   # item-phone 66: 개수 숫자를 바깥 화면 가운데에 미리 그림 → 눕히고 돌릴 때 같이 기울어짐(칸 밖으로 못 나감)
        # 바깥 화면을 위아래로 키움(안쪽 14 → 18줄): 숫자(14줄) 위아래 2줄 여백 — 꽉 차면 금색 틀에 닿음
        cv.fill((slice(11, 33), slice(24, 56)), "gold1")
        cv.fill((slice(12, 32), slice(25, 55)), "goldD")
        cv.fill((slice(13, 31), slice(26, 54)), "bg00")
        cv.fill((slice(13, 14), slice(26, 54)), "navy0")
        glyph57(cv, str(digit), 35, 15, "green2", 2)   # 5×7 둥근 숫자 ×2 = 10×14 — 3×5는 "2"가 "ㄹ"처럼 보임
    top, _ = closed_mask()
    ys, xs = np.nonzero(top)
    y0, y1, x0, x1 = ys.min(), ys.max() + 1, xs.min(), xs.max() + 1
    return cv.idx[y0:y1, x0:x1], top[y0:y1, x0:x1]


def phone_on_table(pal, g):
    idx, msk = top_view_src(pal)
    sh, sw = idx.shape
    yn, xl, xr, depth, thick = g["yn"], g["xl"], g["xr"], g["depth"], g["thick"]
    yf = yn - depth
    k = (yf - VP_T[1]) / (yn - VP_T[1])                     # 먼 쪽 줄어드는 비율
    fl, fr = VP_T[0] + (xl - VP_T[0]) * k, VP_T[0] + (xr - VP_T[0]) * k
    X0, X1 = int(np.floor(min(xl, fl))) - 2, int(np.ceil(max(xr, fr))) + 4
    Y0, Y1 = yf - 1, yn + thick + 4
    W, H = X1 - X0, Y1 - Y0
    cv = Canvas(pal, W, H)
    shadow = np.zeros((H, W), bool)
    face = np.zeros((H, W), bool)
    for ty in range(Y0, Y1):
        for tx in range(X0, X1):
            # 윗면: 이 줄의 왼쪽·오른쪽 끝(먼 쪽 → 가까운 쪽 직선)
            v = (ty + 0.5 - yf) / depth
            a, b = fl + (xl - fl) * v, fr + (xr - fr) * v
            u = (tx + 0.5 - a) / (b - a)
            if 0 <= v < 1 and 0 <= u < 1:
                sy, sx = int(v * sh), int(u * sw)
                if msk[sy, sx]:
                    cv.idx[ty - Y0, tx - X0] = idx[sy, sx]
                    cv.mask[ty - Y0, tx - X0] = True
                    face[ty - Y0, tx - X0] = True
    # 두께(가까운 쪽 옆면): 윗면 맨 아래 줄 모양을 thick줄 아래로 내림 — 위 1줄 빛, 가운데 이음새, 아래 어둠
    bottom = np.zeros_like(face)
    for x in range(W):
        col = np.nonzero(face[:, x])[0]
        if len(col):
            y = col.max()
            for t in range(1, thick + 1):
                if y + t < H and not face[y + t, x]:
                    bottom[y + t, x] = True
                    mid = thick // 2 + 1   # 이음새(뚜껑 옆면 | 아래 몸체 옆면)
                    name = ("gray0" if t == 1 else "navy" if t < mid else "bg00" if t == mid or t == thick else "navy0")
                    cv.fill((y + t, x), name)
            for t in range(thick + 1, thick + 3):            # 테이블에 닿는 그림자(아래 2줄, 오른쪽으로 1)
                if y + t < H and x + 1 < W:
                    shadow[y + t, x + 1] = True
    body = face | bottom
    cv.fill(shadow & ~body, "bg00")
    # 오른쪽 옆(경첩 쪽) 그림자 1줄
    right = np.zeros_like(body)
    right[:, 1:] = body[:, :-1]
    cv.fill(right & ~body & ~shadow & (np.arange(H)[:, None] > depth // 2), "bg00")
    # 바깥 화면 가운데가 이 그림 안 어디로 갔나(개수 글자를 그 위에 놓으려고): 원본 화면 가운데 (40, 23) → 원본 자른 칸 기준 u, v
    tys, txs = np.nonzero(top_view_src(pal)[1])
    u_c, v_c = (40 - 4 + 0.5) / sw, (23 - 6 + 0.5) / sh
    cy = yf + v_c * depth
    cx = (fl + (xl - fl) * v_c) + ((fr + (xr - fr) * v_c) - (fl + (xl - fl) * v_c)) * u_c
    cv.lcd_center = ((cx - X0) / 2, (cy - Y0) / 2)   # 지금 좌표(그림 왼쪽 위 기준)
    return cv, body | shadow, (X0, Y0)


def phone_on_table_rot(pal, g, digit=None):
    """테이블 바닥 원근을 제대로 계산해 눕히고, 바닥 위에서 theta도 돌려 놓는다(광대 폰, 사용자: "원근감·입체감 더").
    바닥 원근: 소실점 높이 VPy에서 화면 y까지 거리 dy일 때 깊이 Z = f/dy, 가로 X = (x - VPx)·dy0/dy(폰 가운데 깊이에서 1도트 = 1).
    f = dy0²/phi → 폰 가운데에서 화면 1줄 = 바닥 1/phi도트(phi = 위아래 눌림). 바닥 좌표를 theta만큼 돌려 원본 그림 칸을 찾는다.
    두께는 화면 아래로 thick줄 밀어 만든 옆면(가까운 쪽 + 안쪽 옆면이 저절로 보임), 음영: 전등(화면 가운데 위) 쪽이 밝고 반대가 어두움."""
    idx, msk = top_view_src(pal, digit)
    sh, sw = idx.shape
    cx, cy, phi, s, th, thick = g["cx"], g["cy"], g["phi"], g["scale"], np.radians(g["theta"]), g["thick"]
    vx, vy = VP_T
    dy0 = cy - vy
    f = dy0 * dy0 / phi
    Z0 = f / dy0
    R = int(max(sw, sh) * s * 0.75) + thick + 6
    X0, X1, Y0, Y1 = cx - R, cx + R, cy - R // 2 - 4, cy + R // 2 + thick + 6
    W, H = X1 - X0, Y1 - Y0
    cv = Canvas(pal, W, H)
    face = np.zeros((H, W), bool)
    lite = np.zeros((H, W))               # 0 = 어둠 ~ 1 = 밝음(윗면 음영용)
    ct, st = np.cos(th), np.sin(th)
    green2_i = cv.c("green2")
    bg00_i, navy0_i_scr = cv.c("bg00"), cv.c("navy0")
    for ty in range(Y0, Y1):
        dy = ty + 0.5 - vy
        if dy <= 1:
            continue
        Z = f / dy
        for tx in range(X0, X1):
            xl = (tx + 0.5 - vx) * dy0 / dy - (cx - vx)       # 바닥 가로(가운데 기준)
            zl = Z - Z0                                       # 바닥 깊이(+ = 먼 쪽)
            a = (ct * xl - st * (-zl)) / s                    # 폰 길이 방향
            b = (st * xl + ct * (-zl)) / s                    # 폰 폭 방향(+ = 가까운 쪽)
            sx, sy = int(np.floor(a + sw / 2)), int(np.floor(b + sh / 2))
            if 0 <= sx < sw and 0 <= sy < sh and msk[sy, sx]:
                cv.idx[ty - Y0, tx - X0] = idx[sy, sx]
                cv.mask[ty - Y0, tx - X0] = True
                face[ty - Y0, tx - X0] = True
                lite[ty - Y0, tx - X0] = 1 - (sx / sw) * 0.7 - (sy / sh) * 0.3   # 경첩 반대쪽·먼 쪽(전등 쪽)이 밝게
                # 숫자 획이 돌리며 끊기지 않게: 이 칸 안 4곳을 더 보고 하나라도 숫자면 숫자 색(획이 1칸씩 굵어짐)
                if digit is not None and idx[sy, sx] in (bg00_i, navy0_i_scr) and 13 <= sy + 6 < 31 and 26 <= sx + 4 < 54:   # 화면 안쪽에서만 굵힘(원본 좌표 = 자른 칸 + (4, 6))
                    for ox, oy in ((-0.3, -0.3), (0.3, -0.3), (-0.3, 0.3), (0.3, 0.3)):
                        dyo = ty + 0.5 + oy - vy
                        xo = (tx + 0.5 + ox - vx) * dy0 / dyo - (cx - vx)
                        zo = f / dyo - Z0
                        ao, bo = (ct * xo - st * (-zo)) / s, (st * xo + ct * (-zo)) / s
                        px, py = int(np.floor(ao + sw / 2)), int(np.floor(bo + sh / 2))
                        if 0 <= px < sw and 0 <= py < sh and idx[py, px] == green2_i and msk[py, px]:
                            cv.idx[ty - Y0, tx - X0] = green2_i
                            break
    # 윗면 음영: 남색 계열만 바꿈(금색 테·화면·LED는 그대로)
    navy_i, navy2_i, navy0_i = cv.c("navy"), cv.c("navy2"), cv.c("navy0")
    yy, xx = np.mgrid[0:H, 0:W]
    thr = (BAYER4[yy % 4, xx % 4])
    on_navy = face & (cv.idx == navy_i)
    cv.idx[on_navy & (lite > 0.80 + thr * 0.15)] = navy2_i
    cv.idx[on_navy & (lite < 0.22 - thr * 0.15)] = navy0_i
    # 두께: 윗면을 화면 아래로 1~thick줄 민 자리 중 윗면이 아닌 곳 = 옆면
    side = np.zeros_like(face)
    level = np.zeros((H, W), int)
    for t in range(1, thick + 1):
        shifted = np.zeros_like(face)
        shifted[t:] = face[:-t]
        new = shifted & ~face & ~side
        side |= new
        level[new] = t
    mid = thick // 2 + 1
    cv.fill(side, "navy0")
    cv.fill(side & (level == 1), "gray0")
    cv.fill(side & (level > 1) & (level < mid), "navy")
    cv.fill(side & ((level == mid) | (level == thick)), "bg00")
    # 옆면 중 경첩 반대(왼쪽, 전등 쪽)는 조금 밝게, 오른쪽은 어둡게
    lx = np.where(side.any(0))[0]
    if len(lx):
        cv.fill(side & (level > 1) & (level != mid) & (level != thick) & (xx < lx.min() + 4), "navy")
        cv.fill(side & (level > 1) & (xx > lx.max() - 4), "bg00")
    body = face | side
    # 그림자: 전등 반대(오른쪽 아래)로, 가까운 것은 진하게·먼 것은 바둑판
    sh1 = np.zeros_like(body)
    sh1[2:, 2:] = body[:-2, :-2]
    sh2 = np.zeros_like(body)
    sh2[3:, 4:] = body[:-3, :-4]
    cv.fill(sh2 & ~body & ((xx + yy) % 2 == 0), "bg00")
    cv.fill(sh1 & ~body, "bg00")
    # 바깥 화면 가운데(개수 글자 자리): 원본 (40, 23) → 폰 좌표 → 바닥 → 화면
    a = ((40 - 4 + 0.5) - sw / 2) * s
    b = ((23 - 6 + 0.5) - sh / 2) * s
    xl = ct * a + st * b
    zl = -(-st * a + ct * b)
    dy = f / (Z0 + zl)
    lx_, ly_ = vx + (xl + (cx - vx)) * dy / dy0, vy + dy
    # 글자 쪽 기울기(화면 안에서 폰 길이 방향이 몇 도인지)
    a2 = a + 10
    xl2, zl2 = ct * a2 + st * b, -(-st * a2 + ct * b)
    dy2 = f / (Z0 + zl2)
    lx2, ly2 = vx + (xl2 + (cx - vx)) * dy2 / dy0, vy + dy2
    cv.lcd_center = ((lx_ - X0) / 2, (ly_ - Y0) / 2)
    cv.lcd_angle = float(np.degrees(np.arctan2(-(ly2 - ly_), lx2 - lx_)))
    # 빈 가장자리 잘라내기
    ys, xs = np.nonzero(cv.mask)
    y0c, x0c = ys.min(), xs.min()
    out = Canvas(pal, xs.max() + 1 - x0c, ys.max() + 1 - y0c)
    out.idx[:] = cv.idx[y0c:ys.max() + 1, x0c:xs.max() + 1]
    out.mask[:] = cv.mask[y0c:ys.max() + 1, x0c:xs.max() + 1]
    out.lcd_center = (cv.lcd_center[0] - x0c / 2, cv.lcd_center[1] - y0c / 2)
    out.lcd_angle = cv.lcd_angle
    return out, body[y0c:ys.max() + 1, x0c:xs.max() + 1], (X0 + x0c, Y0 + y0c)


def make_table(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    files = []
    for name, g in TABLE_PHONES.items():
        cv, body, (x0, y0) = (phone_on_table_rot if "theta" in g else phone_on_table)(pal, g)
        if hasattr(cv, "lcd_angle"):
            print(f"  {name}: 바깥 화면 기울기 {cv.lcd_angle:.1f}도")
        p = OUT / f"phone-table-{name}.png"
        write_png(p, cv.rgba())
        files.append(p)
        # 열 수 있음 테두리(몸통 둘레 1도트 금색, 그림자 제외)
        oc = Canvas(pal, cv.w + 4, cv.h + 4)
        m = np.zeros((cv.h + 4, cv.w + 4), bool)
        # 폰 바깥 윤곽만(검은 화면 칸을 바깥으로 보면 칸 둘레에도 줄이 생김 — 처음 버그)
        m[2:-2, 2:-2] = body if "theta" in g else (cv.mask & (cv.idx != cv.c("bg00")))
        oc.fill(outline_of(m, 1), "gold2")
        po = OUT / f"phone-table-{name}-outline.png"
        write_png(po, oc.rgba())
        files.append(po)
        print(f"  {name}: {cv.w}x{cv.h} 새 도트, 640 격자 왼쪽 위 ({x0}, {y0}) = 지금 좌표 ({x0 / 2:.1f}, {-y0 / 2:.1f}), 화면 가운데(그림 안 지금 좌표) {cv.lcd_center[0]:.1f}, {cv.lcd_center[1]:.1f}")
        write_png(OUT / f"phone-table-{name}-x4.png", cv.rgba().repeat(4, 0).repeat(4, 1))
        # item-phone 66: 광대 폰은 개수 숫자(0~4)를 그림에 넣은 5장 — 숫자도 폰과 같은 원근·각도로 기움(사용자: "각도가 틀어진 만큼 숫자도")
        if "theta" in g:
            for n in range(5):
                cn, _, _ = phone_on_table_rot(pal, g, n)
                pn = OUT / f"phone-table-{name}-{n}.png"
                write_png(pn, cn.rgba())
                files.append(pn)
            write_png(OUT / f"phone-table-{name}-2-x4.png", phone_on_table_rot(pal, g, 2)[0].rgba().repeat(4, 0).repeat(4, 1))
    print("팔레트 검사 (32색):")
    return check32(files, pal)


def make_closed3d(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    new = phone_closed_3d(pal)
    p = OUT / "phone-closed-3d.png"
    write_png(p, new.rgba())
    a, b = phone_closed(pal).rgba(), new.rgba()
    gap = np.zeros((CH, 8, 4), np.uint8)
    both = np.concatenate([a, gap, b], 1)
    write_png(OUT / "phone-closed-compare-x4.png", both.repeat(4, 0).repeat(4, 1))   # 왼쪽 = 지금, 오른쪽 = 입체
    print("팔레트 검사 (32색):")
    return check32([p], pal)


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
OW, OH = 200, 354
# item-phone 63: 상세 화면(아이콘 줄 + 설명 4줄 + 아래 "사용·뒤로" 줄)이 들어가게 화면을 위아래로 늘림(142 → 172), 폰 높이 330 → 354(지금 177)
LID_H, HINGE_H = 200, 14            # 위 뚜껑 0~200, 경첩 200~214, 아래 210~354
SCREEN = (14, 18, 186, 190)         # 화면 x 14~186, y 18~190 = 172×172(지금 86×86): 제목 줄 + 칸 2×2(28×28) + 아래 소프트키 줄
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


# ---------- ①-b 입체 접힌 폰(테이블 먼 쪽 광대 폰, 72×46) ----------
# 사용자(2026-11-18): "광대폰이 입체감이 없어. 입체감 좀 줘" → 앞 위에서 내려다본 3/4 모습:
# 윗면(먼 쪽이 좁은 사다리꼴, 위→아래로 밝음→어두움 바둑판) + 앞 옆면(두께 8, 위 모서리 빛) + 오른쪽 옆면(더 어두움)
# + 먼 쪽 경첩 원통(줄무늬) + 원근 바깥 화면(금색 틀) + 모서리 반짝임 + 테이블 그림자
FW, FH = 72, 46
F_TOP = [(12, 6), (60, 6), (64, 26), (6, 26)]                 # 윗면(뚜껑)
F_FRONT = [(6, 26), (64, 26), (64, 34), (6, 34)]              # 앞 옆면(두께 8)
F_RIGHT = [(60, 6), (66, 10), (68, 30), (64, 34), (64, 26)]   # 오른쪽 옆면
F_LCD = (22, 11, 50, 23)                                      # 바깥 화면 x0, y0, x1, y1(틀 포함) — 개수 글자는 TMP


def far_masks():
    top = polygon(F_TOP, FW, FH)
    front = polygon(F_FRONT, FW, FH) & ~top
    right = polygon(F_RIGHT, FW, FH) & ~top & ~front
    return top, front, right


def phone_closed_far(pal):
    cv = Canvas(pal, FW, FH)
    yy, xx = np.mgrid[0:FH, 0:FW]
    top, front, right = far_masks()
    body = top | front | right
    # 테이블 그림자: 오른쪽 아래로 2~4도트 밀린 몸통 모양
    sh = np.zeros_like(body)
    sh[4:, 3:] = body[:-4, :-3]
    cv.fill(sh & ~body, "bg00")
    # 옆면: 앞 = navy0(위 모서리 1줄 gray0 빛, 아래 2줄 bg00), 오른쪽 = bg00 바탕에 navy0 바둑판
    cv.fill(front, "navy0")
    cv.fill(front & (yy == 26), "gray0")
    cv.fill(front & (yy >= 32), "bg00")
    cv.fill(front & (yy == 29) & (xx > 8) & (xx < 62), "navy2")   # 접힌 이음새(뚜껑과 아래 몸체 사이) — "접힌 폴더폰"으로 보이게
    cv.fill(right, "bg00")
    cv.fill(right & ((xx + yy) % 2 == 0), "navy0")
    # 윗면: 먼 쪽(위) navy2 → 가까운 쪽 navy, 바둑판 섞기(Canvas.dither)
    level = np.clip(1.4 - (yy - 6) / 9, 0, 1)   # 먼 쪽 몇 줄만 밝게(바둑판이 넓게 퍼지면 거칠어 보임)
    cv.dither(top, "navy", "navy2", level)
    # 윗면 테두리: 금색 띠 1줄(위·왼쪽은 밝게 gold1, 아래·오른쪽은 goldD)
    rim = outline_of(~top, 1) & top
    cv.fill(rim, "goldD")
    cv.fill(rim & ((yy <= 7) | (xx <= 12)), "gold1")
    # 먼 쪽 경첩 원통: 윗면 위쪽 띠, 세로 줄무늬
    hinge = round_rect(FW, FH, 16, 2, 56, 10, 3)   # 윗면 먼 쪽 끝에서 위로 살짝 튀어나온 원통
    cv.fill(hinge, "navy0")
    cv.fill(hinge & (yy == 3), "gray0")
    cv.fill(hinge & (yy == 4), "navy2")
    cv.fill(hinge & (yy >= 9), "bg00")
    cv.fill(hinge & (yy > 4) & (yy < 9) & (xx % 8 == 0), "bg00")
    # 바깥 화면(원근: 위가 1도트씩 좁음): 금색 틀 + 어두운 화면 + 위 반사 1줄
    x0, y0, x1, y1 = F_LCD
    lcd = polygon([(x0 + 1, y0), (x1 - 1, y0), (x1, y1), (x0, y1)], FW, FH)
    cv.fill(lcd, "gold1")
    inner = polygon([(x0 + 2, y0 + 1), (x1 - 2, y0 + 1), (x1 - 1, y1 - 1), (x0 + 1, y1 - 1)], FW, FH)
    cv.fill(inner, "bg00")
    cv.fill(inner & (yy == y0 + 1), "navy0")
    # 작은 불빛(LED)·모서리 반짝임
    cv.fill((slice(14, 17), slice(14, 17)), "green0")
    cv.fill((slice(7, 8), slice(13, 16)), "cream2")
    cv.fill((slice(8, 9), slice(12, 13)), "cream0")
    return cv


def phone_closed_far_outline(pal):
    cv = Canvas(pal, FW + 4, FH + 4)
    top, front, right = far_masks()
    m = np.zeros((FH + 4, FW + 4), bool)
    m[2:-2, 2:-2] = top | front | right | round_rect(FW, FH, 16, 2, 56, 10, 3)
    cv.fill(outline_of(m, 2), "gold2")
    return cv


def make_far(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    parts = {"phone-closed-far": phone_closed_far(pal), "phone-closed-far-outline": phone_closed_far_outline(pal)}
    files = []
    for name, cv in parts.items():
        p = OUT / f"{name}.png"
        write_png(p, cv.rgba())
        files.append(p)
    a = parts["phone-closed-far"].rgba()
    write_png(OUT / "phone-closed-far-x4.png", a.repeat(4, 0).repeat(4, 1))
    print("팔레트 검사 (32색):")
    return check32(files, pal)


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
    if cmd == "table":   # item-phone 66 고침: 테이블 원근으로 눕힌 접힌 폰(내 것·광대 것)
        sys.exit(0 if make_table(load_pal()) else 1)
    if cmd == "closed3d":   # item-phone 66 고침: 지금 접힌 폰 모양 그대로 입체감만(사용자: 뜻이 다름 — 테이블 위 느낌이었음)
        sys.exit(0 if make_closed3d(load_pal()) else 1)
    if cmd == "far":   # item-phone 66 고침: 입체 접힌 광대 폰만(사용자가 거절 — 모양이 달라서)
        sys.exit(0 if make_far(load_pal()) else 1)
    if cmd == "draft":
        sys.exit(0 if make_draft(load_pal()) else 1)
    print(__doc__)

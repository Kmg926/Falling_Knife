"""device-art 107·108 — 폰·모니터 그림 다듬기 시안 A/B (AI 작성).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art_device.py draft      # 시안 A·B 그림 + 게임 화면 합성 미리보기 → .scratch/art/drafts/device/
    python tools/make_art_device.py game A     # (고른 뒤) 게임에 넣을 이름으로 → .scratch/art/drafts/device/game/

레퍼런스(사용자 2026-11-27): 노키아식 바 폰·슬림 폰·모토로라식 폴더폰·베이지 CRT. 실제 상표·로고는 그리지 않는다.
- 폰: 기존 make_art_phone.py의 모양(closed_mask·key_rect·SCREEN)·원근 계산(phone_on_table_rot)을 그대로 쓰고
  **겉 그림만** 바꾼다 → 파일 이름·크기·바깥 화면 자리가 같아 개수 표시(0~4)·진동("+N" 깜빡임) 코드가 그대로 동작.
  A = 금속 폴더폰(남색 금속 + 사선 광택 + 회색 금속 테 + 원통 경첩), B = 노키아식(무광 남색 + 화면을 감싼 크림색 링 + 타원 키).
  바깥 화면은 둘 다 초록 LCD(바탕 green, 숫자 bg00 = 가장 어두운 색) + 왼쪽 위 유리 반짝임.
- 모니터: 레트로 CRT. 화면 칸(차트·"다음" 창·기준선)은 화면 위 같은 자리 → 테두리만 왼쪽·오른쪽·위로 4칸, 아래로 2칸 키움(받침을 낮춤).
  A = 어두운 베이지(gray1), B = 밝은 베이지(cream0). 광대 모니터 = 같은 CRT의 뒷면(깊은 몸통·통풍구).
- 32색 팔레트만, 무작위 없음. 그러데이션은 바둑판(Bayer) 점.
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
import make_art_phone as P  # noqa: E402
from make_art import write_png  # noqa: E402
from make_art_hd import BAYER4, Canvas, check32, load_pal  # noqa: E402
from make_rules_screen import read_png  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "device"
CAPTURE = ROOT / ".scratch" / "art" / "drafts" / "rules" / "game-capture-1280.png"   # 내 차례 첫 화면(지금 그림)

ORIG_CLOSED = P.phone_closed
ORIG_OPEN = P.phone_open
ORIG_KEY = P.draw_key


def sparse(cv, region, name, level):
    """level(0~1)만큼만 점을 찍는다(바둑판 순서 = 고르게 성김)."""
    yy, xx = np.mgrid[0:cv.h, 0:cv.w]
    lvl = np.broadcast_to(level, (cv.h, cv.w))
    cv.fill(region & (lvl > BAYER4[yy % 4, xx % 4]), name)


def seg(cv, p0, p1, col, allow=None):
    P.line(cv, p0, p1, col, allow)


# ================= 폰 =================
def lcd_setup(style):
    """바깥 화면 색(초록 LCD + 어두운 숫자). make_art_phone의 바깥 화면 칸·숫자 자리는 그대로."""
    P.LCD_FRAME = ("gray1", "bg00") if style == "A" else ("gray0", "bg00")
    P.LCD_BG, P.LCD_TOP, P.LCD_FG = "green", "green0", "bg00"
    P.LCD_NOISE = ("green2", "bg00", "green0")


def lcd_glint(idx, pal_c, crop=(6, 4)):
    """바깥 화면 왼쪽 위 유리 반짝임(숫자 자리 x≥33을 피함). idx = top_view_src가 자른 원본(좌표 = 원본 − (4, 6))."""
    oy, ox = crop
    for (x, y) in ((27, 14), (28, 14), (29, 14), (27, 15), (27, 16), (30, 14)):
        if idx[y - oy, x - ox] in (pal_c("green"), pal_c("green0")):
            idx[y - oy, x - ox] = pal_c("green2")


def closed_lid(pal, style):
    """위에서 본 접힌 폰(80×48, 원본). 쓰는 곳은 윗면(뚜껑+경첩)뿐 — 원근은 make_art_phone이 씌움."""
    cv = Canvas(pal, P.CW, P.CH)
    top, side = P.closed_mask()
    yy, xx = np.mgrid[0:P.CH, 0:P.CW]
    cv.fill(side, "navy0")
    lid = top & (xx < 63)
    cv.fill(top, "navy")
    if style == "A":
        # 금속 광택: 왼쪽 위 → 오른쪽 아래 사선 띠 2개(밝은 남색 점), 오른쪽 아래는 어둡게
        d = (xx - 4) - (yy - 8) * 1.6
        band = np.clip(1 - np.abs(d - 16) / 7, 0, 1) * 0.75 + np.clip(1 - np.abs(d - 31) / 2.5, 0, 1) * 0.5
        sparse(cv, lid, "navy2", band)
        sparse(cv, lid, "navy0", np.clip(((xx - 30) / 40 + (yy - 20) / 20) * 0.6, 0, 0.55))
        # 회색 금속 테: 위·왼쪽 = gray1(빛), 아래·오른쪽 = gray0
        rim = P.outline_of(~top, 1) & top
        cv.fill(rim, "gray0")
        cv.fill(rim & ((yy < 13) | (xx < 9)), "gray1")
        cv.fill(rim & (yy < 10) & (xx > 12) & (xx < 50), "cream0")
    else:
        # 무광 남색 + 화면을 감싼 크림색 링(노키아식 앞판). 링 = 둥근 사각형 띠, 아래쪽은 그늘
        sparse(cv, lid, "navy0", np.clip(((xx - 20) / 50 + (yy - 14) / 24) * 0.45, 0, 0.45))
        ring = P.round_rect(P.CW, P.CH, 19, 9, 61, 36, 6) & ~P.round_rect(P.CW, P.CH, 22, 10, 58, 34, 4)
        cv.fill(ring, "cream0")
        cv.fill(ring & (yy >= 31), "gray1")
        cv.fill(ring & (yy <= 9), "cream")
        rim = P.outline_of(~top, 1) & top
        cv.fill(rim, "navy0")
        cv.fill(rim & ((yy < 13) | (xx < 9)), "navy2")
    # 경첩(오른쪽 원통): 금속 회색, 가운데 빛 줄, 양 끝 남색 덮개
    hinge = top & (xx >= 64) & (xx < 76) & (yy > 8) & (yy < 38)
    cv.fill(hinge, "gray0")
    cv.fill(hinge & (xx >= 66) & (xx < 68), "gray1")
    cv.fill(hinge & (xx == 67) & (yy > 12) & (yy < 34), "cream0")
    cv.fill(hinge & (xx >= 73), "bg00")
    cv.fill(hinge & ((yy <= 10) | (yy >= 36)), "navy0")
    cv.fill(top & (xx == 63) & (yy > 9) & (yy < 37), "bg00")
    # 바깥 화면 자리(개수 그림에서 다시 칠함) — 여기선 기본 틀만
    cv.fill((slice(11, 33), slice(24, 56)), P.LCD_FRAME[0])
    cv.fill((slice(12, 32), slice(25, 55)), P.LCD_FRAME[1])
    cv.fill((slice(13, 31), slice(26, 54)), P.LCD_BG)
    # 작은 불빛(LED) + 둘레 홈
    cv.fill((slice(20, 25), slice(12, 17)), "bg00")
    cv.fill((slice(21, 24), slice(13, 16)), "green2" if style == "A" else "green")
    # 긁힘(뚜껑 왼쪽 아래·오른쪽 위, 화면은 피함) + 모서리 닳음
    seg(cv, (9, 31), (15, 28), "gray0", lid)
    seg(cv, (11, 33), (14, 32), "navy2", lid)
    seg(cv, (57, 12), (60, 16), "gray0", lid)
    cv.fill(top & (((xx - 5) + (yy - 9)) < 3) & (xx < 8), "gray1")
    return cv


def install_phone(style):
    """make_art_phone의 그리는 함수를 이 스타일로 바꿔 끼운다(원근·개수·광대 흠집 코드는 그대로 씀)."""
    lcd_setup(style)
    P.phone_closed = lambda pal: closed_lid(pal, style)
    orig_tv = P.top_view_src.__wrapped__ if hasattr(P.top_view_src, "__wrapped__") else P.top_view_src

    def tv(pal, digit=None, clown=False):
        idx, keep = orig_tv(pal, digit, clown)
        if digit is not None:
            pal_c = Canvas(pal, 1, 1).c
            lcd_glint(idx, pal_c)
        return idx, keep
    tv.__wrapped__ = orig_tv
    P.top_view_src = tv
    P.draw_key = lambda cv, i, lit: draw_key_v2(cv, i, lit, style)
    P.phone_open = lambda pal: phone_open_v2(pal, style)


def draw_key_v2(cv, i, lit, style):
    """키: A = 남색 둥근 키(위 빛·아래 그늘), B = 밝은 회색 타원 키(노키아식). 불빛(lit) = 금색."""
    x0, y0, x1, y1 = P.key_rect(i)
    yy, xx = np.mgrid[0:cv.h, 0:cv.w]
    r = 6 if style == "A" else 9
    m = P.round_rect(cv.w, cv.h, x0, y0, x1, y1, r)
    if lit:
        face, hi, lo, txt = "gold1", "gold3", "goldD", "bg00"
    elif style == "A":
        face, hi, lo, txt = "navy0", "navy2", "bg00", "cream"
    else:
        face, hi, lo, txt = "gray1", "cream0", "gray0", "navy0"
    cv.fill(m, face)
    cv.fill(m & (yy < y0 + 3), hi)
    cv.fill(m & (yy >= y1 - 3), lo)
    cv.fill(P.outline_of(m, 1) & (yy >= y1 - 1), "bg00")                 # 키 아래 그림자 1줄
    P.glyph(cv, P.KEY_LABELS[i], (x0 + x1) // 2 - 3, (y0 + y1) // 2 - 5, txt, 2)


def phone_open_v2(pal, style):
    OW, OH, LID_H, HINGE_H = P.OW, P.OH, P.LID_H, P.HINGE_H
    cv = Canvas(pal, OW, OH)
    yy, xx = np.mgrid[0:OH, 0:OW]
    lid = P.round_rect(OW, OH, 0, 0, OW, LID_H, 14)
    base = P.round_rect(OW, OH, 4, LID_H + HINGE_H - 4, OW - 4, OH, 18)
    sx0, sy0, sx1, sy1 = P.SCREEN
    for part in (lid, base):
        cv.fill(part, "navy")
        if style == "A":   # 금속: 사선 광택 + 오른쪽 아래 어둡게
            d = xx - yy * 0.55
            sparse(cv, part, "navy2", np.clip(1 - np.abs(d - 40) / 22, 0, 1) * 0.55 + np.clip(1 - np.abs(d - 95) / 6, 0, 1) * 0.4)
            sparse(cv, part, "navy0", np.clip((xx / OW + (yy % 214) / 214) - 1.1, 0, 0.6))
            rim = P.outline_of(~part, 2) & part
            cv.fill(rim, "gray0")
            cv.fill(rim & ((xx < 6) | (yy < (6 if part is lid else LID_H + HINGE_H + 2))), "gray1")
            cv.fill(P.outline_of(~part, 1) & part & (yy < 3), "cream0")
        else:              # 무광 + 어두운 테
            sparse(cv, part, "navy0", np.clip((xx / OW + (yy % 214) / 214) - 0.9, 0, 0.5))
            rim = P.outline_of(~part, 2) & part
            cv.fill(rim, "navy0")
            cv.fill(rim & ((xx < 6) | (yy < 4)), "navy2")
    # 화면 테두리: A = 검은 유리판(둥근 모서리) + 회색 홈, B = 크림색 링(노키아식) + 검은 홈
    glass = P.round_rect(OW, OH, sx0 - 7, sy0 - 7, sx1 + 7, sy1 + 7, 8)
    if style == "A":
        cv.fill(glass, "bg00")
        cv.fill(P.outline_of(glass, 1) & lid, "gray0")
        # 유리판 위 반짝임(화면 밖 테두리 부분만): 왼쪽 위 사선 2줄
        frame_only = glass & ~((xx >= sx0 - 2) & (xx < sx1 + 2) & (yy >= sy0 - 2) & (yy < sy1 + 2))
        for k in (0, 5):
            seg(cv, (sx0 - 6 + k, sy0 + 14 + k), (sx0 + 14 + k, sy0 - 6 + k), "gray0", frame_only)
    else:
        ring = P.round_rect(OW, OH, sx0 - 9, sy0 - 9, sx1 + 9, sy1 + 9, 10)
        cv.fill(ring, "cream0")
        cv.fill(ring & (yy > sy1 + 4), "gray1")
        cv.fill(ring & (yy < sy0 - 7), "cream")
        cv.fill(P.round_rect(OW, OH, sx0 - 3, sy0 - 3, sx1 + 3, sy1 + 3, 3), "bg00")
    # 화면(게임 글자·칸이 위에 올라감): 지금과 같은 바탕 + 주사선 + 둥근 모서리 그늘
    cv.fill((slice(sy0 - 2, sy1 + 2), slice(sx0 - 2, sx1 + 2)), "bg00")
    cv.fill((slice(sy0, sy1), slice(sx0, sx1)), "bg0")
    for y in range(sy0 + 1, sy1, 2):
        cv.fill((slice(y, y + 1), slice(sx0, sx1)), "bg00")
    scr = (xx >= sx0) & (xx < sx1) & (yy >= sy0) & (yy < sy1)
    cv.fill(scr & ~P.round_rect(OW, OH, sx0, sy0, sx1, sy1, 4), "bg00")
    # 스피커(위) — 구멍 줄
    cv.fill((slice(6, 10), slice(OW // 2 - 20, OW // 2 + 20)), "bg00")
    for x in range(OW // 2 - 18, OW // 2 + 19, 3):
        cv.fill((slice(7, 9), slice(x, x + 1)), "gray0")
    # 경첩: 금속 원통(가운데 빛) + 양 끝 덮개
    hg = (yy >= LID_H - 2) & (yy < LID_H + HINGE_H) & (xx >= 20) & (xx < OW - 20)
    cv.fill(hg, "gray0")
    cv.fill(hg & (yy >= LID_H + 2) & (yy < LID_H + 5), "gray1")
    cv.fill(hg & (yy == LID_H + 3), "cream0")
    cv.fill(hg & (yy >= LID_H + HINGE_H - 3), "bg00")
    for cx0 in (20, OW - 34):
        cap = hg & (xx >= cx0) & (xx < cx0 + 14)
        cv.fill(cap, "navy0")
        cv.fill(cap & (yy < LID_H + 3), "navy2")
    # 아래 몸체 맨 아래 그늘
    cv.fill(base & (yy >= OH - 8), "navy0")
    # 위 버튼 줄: 통화(초록)·끊기(빨강) 알약 + 가운데 원형 방향키(금속 링)
    by0 = P.BTN_Y0
    for x0, col, hi in ((P.KEYS_X0, "green0", "green"), (OW - P.KEYS_X0 - 36, "red0", "red1")):
        m = P.round_rect(OW, OH, x0, by0, x0 + 36, by0 + 18, 8)
        cv.fill(m, col)
        cv.fill(m & (yy < by0 + 3), hi)
        cv.fill(m & (yy >= by0 + 16), "bg00")
        cv.fill(P.outline_of(m, 1) & (yy >= by0 + 18), "bg00")
    ring = ((xx - OW // 2) ** 2) / 21 ** 2 + ((yy - (by0 + 9)) ** 2) / 12 ** 2 <= 1
    cv.fill(ring, "gray0")
    cv.fill(ring & (yy < by0 + 4), "gray1")
    inner = ((xx - OW // 2) ** 2) / 16 ** 2 + ((yy - (by0 + 9)) ** 2) / 8 ** 2 <= 1
    cv.fill(inner, "navy0")
    cv.fill(((xx - OW // 2) ** 2) / 7 ** 2 + ((yy - (by0 + 9)) ** 2) / 4 ** 2 <= 1, "gray1")
    cv.fill(((xx - OW // 2) ** 2) / 7 ** 2 + ((yy - (by0 + 8)) ** 2) / 2 ** 2 <= 1, "cream0")
    for i in range(12):
        P.draw_key(cv, i, False)
    # 긁힘(뚜껑 오른쪽 위·몸체 왼쪽 아래 모서리) + 마이크 구멍
    seg(cv, (OW - 26, 8), (OW - 14, 16), "gray0", lid & ~glass)
    seg(cv, (OW - 22, 6), (OW - 16, 10), "navy2", lid & ~glass)
    seg(cv, (10, OH - 30), (18, OH - 22), "gray0", base)
    cv.fill((slice(OH - 5, OH - 3), slice(OW // 2 - 2, OW // 2 + 2)), "bg00")
    return cv


def phone_parts(pal, style):
    """한 스타일의 폰 그림 전부(게임 파일 이름 → Canvas)."""
    install_phone(style)
    parts = {}
    for name, g in P.TABLE_PHONES.items():
        cv, body, _ = P.phone_on_table_rot(pal, g)
        parts[f"phone-table-{name}"] = cv
        oc = Canvas(pal, cv.w + 4, cv.h + 4)
        m = np.zeros((cv.h + 4, cv.w + 4), bool)
        m[2:-2, 2:-2] = body
        oc.fill(P.outline_of(m, 1), "gold2")
        parts[f"phone-table-{name}-outline"] = oc
        labels = [(n, str(n)) for n in range(5)] + [("", "blank")] + [(f"+{k}", f"p{k}") for k in range(1, 5)]
        if g.get("style") == "clown":
            labels.append(("noise", "noise"))
        for lab, suf in labels:
            parts[f"phone-table-{name}-{suf}"] = P.phone_on_table_rot(pal, g, lab)[0]
    parts["phone-open"] = P.phone_open(pal)
    parts["phone-open-clown"] = P.phone_open_clown(pal)
    for i in range(4):
        parts[f"phone-key-lit-{i + 1}"] = P.key_lit(pal, i)
    return parts


# ================= 펼친 폰 v3 (사용자 2026-11-27: "아래를 자르자" + 모토로라식 레퍼런스) =================
# 같은 200×354 그림(게임 Image 크기 그대로). 화면은 13칸(26도트) 아래로 → 위에 스피커·둥근 장식 자리.
# 아래 몸체는 버튼 줄(통화·끊기)까지만 보이고 그림 아래 끝(= 게임 화면 아래 끝)에서 잘린 모습. 키패드 1~4는 없음.
SCREEN3 = (14, 44, 186, 216)                 # 전 (14, 18, 186, 190)
LID3_B, HINGE3 = 246, (246, 280)             # 뚜껑 아래 끝, 경첩 y 범위
BTN3 = {"green": (24, 292, 82, 310), "red": (118, 292, 176, 310)}   # 통화·끊기(게임 GreenKey·RedKey 클릭 칸과 같은 자리)


def phone_open_v3(pal, clown=False):
    OW, OH = P.OW, P.OH
    cv = Canvas(pal, OW, OH)
    yy, xx = np.mgrid[0:OH, 0:OW]
    sx0, sy0, sx1, sy1 = SCREEN3
    # 뚜껑: 위는 크게 둥근 아치, 아래는 작게 둥근 모서리
    lid = P.round_rect(OW, OH, 0, 0, OW, LID3_B, 12) & ~(
        (yy < 34) & (((xx - 34) ** 2 + (yy - 34) ** 2 > 34 ** 2) & (xx < 34) | ((xx - (OW - 35)) ** 2 + (yy - 34) ** 2 > 34 ** 2) & (xx > OW - 35)))
    base = P.round_rect(OW, OH + 20, 6, HINGE3[1] - 8, OW - 6, OH + 20, 16)[:OH]     # 아래로 그림 밖까지 이어짐(잘린 모습)
    for part in (lid, base):
        cv.fill(part, "navy")
        d = xx - yy * 0.55
        sparse(cv, part, "navy2", np.clip(1 - np.abs(d - 30) / 20, 0, 1) * 0.5 + np.clip(1 - np.abs(d - 92) / 5, 0, 1) * 0.4)
        sparse(cv, part, "navy0", np.clip(xx / OW - 0.55, 0, 0.5) + np.clip((yy - LID3_B + 40) / 80, 0, 0.3) * (part is lid))
        rim = P.outline_of(~part, 2) & part
        cv.fill(rim, "gray0")
        cv.fill(rim & ((xx < 6) | (yy < (8 if part is lid else HINGE3[1] - 4))), "gray1")
    cv.fill(P.outline_of(~lid, 1) & lid & (yy < 4), "cream0")
    # 위 장식: 가운데 둥근 판(글자 없음) + 양옆 스피커 홈 + 모서리 나사 2개
    cx, cy = OW // 2, 20
    emb = (xx - cx) ** 2 + (yy - cy) ** 2 <= 11 ** 2
    cv.fill(emb, "gray0")
    cv.fill(emb & ((xx - cx) ** 2 + (yy - cy) ** 2 <= 9 ** 2), "navy0")
    cv.fill(emb & ((xx - cx) ** 2 + (yy - cy) ** 2 <= 9 ** 2) & (yy < cy - 3), "navy2")
    cv.fill(P.outline_of(emb, 1) & (yy > cy), "gray1")
    for x0 in (cx - 64, cx + 18):
        slot = P.round_rect(OW, OH, x0, cy - 4, x0 + 46, cy + 4, 4)
        cv.fill(slot, "bg00")
        cv.fill(P.outline_of(slot, 1) & (yy > cy + 3), "gray0")
        for x in range(x0 + 4, x0 + 43, 4):
            cv.fill((slice(cy - 1, cy + 1), slice(x, x + 2)), "navy0")
    for (sx, sy) in ((14, 30), (OW - 18, 30), (12, LID3_B - 14), (OW - 16, LID3_B - 14)):
        cv.fill((slice(sy, sy + 4), slice(sx, sx + 4)), "gray0")
        cv.fill((slice(sy, sy + 1), slice(sx, sx + 4)), "gray1")
        cv.fill((slice(sy + 1, sy + 3), slice(sx + 1, sx + 3)), "bg00")
    # 화면 테두리: 검은 유리판(둥근 모서리 12) + 회색 홈 + 유리 반짝임(테두리 부분만)
    glass = P.round_rect(OW, OH, sx0 - 8, sy0 - 10, sx1 + 8, sy1 + 10, 12)
    cv.fill(glass, "bg00")
    cv.fill(P.outline_of(glass, 1) & lid, "gray0")
    cv.fill(P.outline_of(glass, 1) & lid & (yy > sy1), "gray1")
    frame_only = glass & ~((xx >= sx0 - 2) & (xx < sx1 + 2) & (yy >= sy0 - 2) & (yy < sy1 + 2))
    for k in (0, 5):
        seg(cv, (sx0 - 7 + k, sy0 + 14 + k), (sx0 + 16 + k, sy0 - 9 + k), "gray0", frame_only)
    # 화면: 바탕 + 주사선 + 둥근 모서리 그늘(r 6)
    cv.fill((slice(sy0 - 2, sy1 + 2), slice(sx0 - 2, sx1 + 2)), "bg00")
    scr = (xx >= sx0) & (xx < sx1) & (yy >= sy0) & (yy < sy1)
    cv.fill(scr, "bg0")
    cv.fill(scr & ((yy - sy0) % 2 == 1), "bg00")
    cv.fill(scr & ~P.round_rect(OW, OH, sx0, sy0, sx1, sy1, 6), "bg00")
    cv.fill(P.outline_of(~P.round_rect(OW, OH, sx0 - 2, sy0 - 2, sx1 + 2, sy1 + 2, 8), 1) & ~scr & glass & (yy < sy0), "navy0")
    # 경첩: 굵은 금속 원통(위 빛·가운데 이음새·아래 그늘) + 양 끝 덮개
    h0, h1 = HINGE3
    hg = (yy >= h0) & (yy < h1) & (xx >= 14) & (xx < OW - 14)
    cv.fill(hg, "gray0")
    cv.fill(hg & (yy >= h0 + 4) & (yy < h0 + 10), "gray1")
    cv.fill(hg & (yy == h0 + 6), "cream0")
    cv.fill(hg & (yy >= h1 - 6), "navy0")
    cv.fill(hg & (yy >= h1 - 2), "bg00")
    cv.fill(hg & (xx >= OW // 2 - 1) & (xx < OW // 2 + 1), "bg00")
    for cx0 in (14, OW - 34):
        cap = hg & (xx >= cx0) & (xx < cx0 + 20)
        cv.fill(cap, "navy0")
        cv.fill(cap & (yy < h0 + 5), "navy2")
        cv.fill(cap & (yy >= h1 - 3), "bg00")
    # 아래 몸체: 통화(초록)·끊기(빨강) 곡선 버튼 + 가운데 방향키 윗부분(아래는 잘림)
    for key, (x0, y0, x1, y1) in BTN3.items():
        m = P.round_rect(OW, OH, x0, y0, x1, y1, 8)
        face, hi = ("green0", "green") if key == "green" else (("red2", "red4") if clown else ("red0", "red1"))
        cv.fill(m, face)
        cv.fill(m & (yy < y0 + 3), hi)
        cv.fill(m & (yy >= y1 - 3), "bg00")
        cv.fill(P.outline_of(m, 1) & (yy >= y1), "bg00")
        cv.fill(P.outline_of(m, 1) & (yy < y0 + 2), "gray0")
    ring = ((xx - OW // 2) ** 2) / 17 ** 2 + ((yy - 318) ** 2) / 14 ** 2 <= 1
    cv.fill(ring, "gray0")
    cv.fill(ring & (yy < 308), "gray1")
    cv.fill(((xx - OW // 2) ** 2) / 12 ** 2 + ((yy - 318) ** 2) / 9 ** 2 <= 1, "navy0")
    cv.fill(((xx - OW // 2) ** 2) / 6 ** 2 + ((yy - 318) ** 2) / 4 ** 2 <= 1, "gray1")
    # 키패드 첫 줄 윗부분만 살짝(잘린 느낌): 둥근 키 3개 위 끝
    for c in range(3):
        kx = 26 + c * 54
        k = P.round_rect(OW, OH + 10, kx, 340, kx + 40, 364, 7)[:OH]
        cv.fill(k, "navy0")
        cv.fill(k & (yy < 343), "navy2")
    # 긁힘
    seg(cv, (OW - 30, 40), (OW - 18, 52), "gray0", lid & ~glass)
    seg(cv, (16, 300), (22, 306), "gray0", base)
    if clown:
        keep = glass | scr | ((xx >= sx0 - 8) & (xx < sx1 + 8) & (yy >= sy0 - 10) & (yy < sy1 + 10))
        P.recolor_red(cv, keep)
        # 테이프: 뚜껑 왼쪽 위 모서리를 대각선으로(유리판에 안 닿게)
        d = xx + yy
        band = lid & (d >= 18) & (d < 36) & ~glass
        cv.fill(band, "cream0")
        cv.fill(band & ((d == 18) | (d == 35)), "gray1")
        cv.fill(band & (d == 24) & (xx % 3 == 0), "cream2")
        # 몸체 오른쪽 위 깨진 자국(계단) + 경첩 오른쪽 덮개 빠짐
        chip = base & ((OW - xx) + (yy - (HINGE3[1] - 8)) < 16)
        cv.mask[chip] = False
        cv.fill(P.outline_of(chip, 1) & base & ~chip, "bg00")
        cap = hg & (xx >= OW - 34) & (xx < OW - 14)
        cv.fill(cap, "bg00")
        cv.fill(cap & (yy < h0 + 3), "red0")
    return cv


def phone_crack_v3(pal):
    """화면 유리 금(투명, 200×354) — make_art_phone.phone_crack과 같은 금을 새 화면 자리로."""
    old = P.SCREEN
    P.SCREEN = SCREEN3
    try:
        return P.phone_crack(pal)
    finally:
        P.SCREEN = old


# ================= 모니터 =================
# 지금 좌표(320×180). 화면 칸은 make_art_monitor 시안 A와 같음(게임 자리 = 이 좌표 + (0, 38)).
SCR = (115, 81, 209, 131)
NEXT = (190, 101, 204, 125)
TICKER, BASE = 93, 112
M_LEFT, M_TOP, M_RIGHT, M_BEZEL_B, M_BOTTOM = 108, 74, 216, 138, 142     # 새 테두리(전: 112, 78, 212, 134, 140)
OLD_LEFT, OLD_TOP = 112, 78


def crt(pal, style):
    W, H = (M_RIGHT - M_LEFT) * 2, (M_BOTTOM - M_TOP) * 2
    cv = Canvas(pal, W, H)
    yy, xx = np.mgrid[0:H, 0:W]

    def R(x0, y0, x1, y1, r=0):
        return P.round_rect(W, H, (x0 - M_LEFT) * 2, (y0 - M_TOP) * 2, (x1 - M_LEFT) * 2, (y1 - M_TOP) * 2, r)
    body, light, dark, deep = ("gray1", "cream0", "gray0", "bg00") if style == "A" else ("cream0", "cream", "gray1", "gray0")
    # 받침(낮은 받침대) — 테두리 아래
    stand = R(140, M_BEZEL_B - 1, 184, M_BOTTOM, 3)
    cv.fill(stand, dark)
    cv.fill(stand & (yy < (M_BEZEL_B + 1 - M_TOP) * 2), deep)
    cv.fill(stand & (yy >= H - 2), "bg00")
    # 테두리(앞판)
    bez = R(M_LEFT, M_TOP, M_RIGHT, M_BEZEL_B, 6)
    cv.fill(bez, body)
    sparse(cv, bez, dark, np.clip((yy / H) * 0.5 + (xx / W) * 0.25 - 0.25, 0, 0.5))      # 아래·오른쪽으로 조금 어둡게
    edge = P.outline_of(~bez, 1) & bez
    cv.fill(edge, deep)
    cv.fill(P.outline_of(~bez, 2) & bez & ~edge & (yy < 6), light)                    # 위 빛
    cv.fill(P.outline_of(~bez, 2) & bez & ~edge & (xx < 6), light)                    # 왼쪽 빛
    cv.fill(P.outline_of(~bez, 3) & bez & ~P.outline_of(~bez, 1) & (yy > H - 16) & (yy < (M_BEZEL_B - M_TOP) * 2), dark)
    # 화면 둘레 움푹한 홈(안쪽 그늘): 바깥 1줄 light(아래·오른쪽), dark 띠, 화면 바로 둘레 deep
    well = R(SCR[0] - 2, SCR[1] - 2, SCR[2] + 2, SCR[3] + 2, 5)
    cv.fill(well, dark)
    cv.fill(P.outline_of(well, 1) & (yy > (SCR[3] - M_TOP) * 2), light)
    cv.fill(P.outline_of(well, 1) & (xx > (SCR[2] - M_LEFT) * 2), light)
    cv.fill(P.outline_of(well, 1) & (yy < (SCR[1] - M_TOP) * 2 - 2), deep)
    inner = R(SCR[0] - 1, SCR[1] - 1, SCR[2] + 1, SCR[3] + 1, 4)
    cv.fill(inner, "bg00")
    # 화면: 지금과 같은 바탕·주사선·차트 칸(make_art_monitor와 같은 규칙)
    sx0, sy0 = (SCR[0] - M_LEFT) * 2, (SCR[1] - M_TOP) * 2
    sx1, sy1 = (SCR[2] - M_LEFT) * 2, (SCR[3] - M_TOP) * 2
    scr = (xx >= sx0) & (xx < sx1) & (yy >= sy0) & (yy < sy1)
    cv.fill(scr, "bg0")
    cv.fill(scr & ((yy - sy0) % 2 == 1), "bg00")
    ty = (TICKER - M_TOP) * 2
    cv.fill((slice(ty, ty + 1), slice(sx0 + 2, sx1 - 2)), "navy0")
    cx0, cx1 = sx0 + 4, (NEXT[0] - M_LEFT) * 2 - 4
    cy0, cy1 = ty + 3, sy1 - 3
    for gx in range(cx0 + 10, cx1, 24):
        for y in range(cy0, cy1, 4):
            cv.fill((y, gx), "navy0")
    for gy in range(cy0 + 6, cy1, 12):
        for x in range(cx0, cx1, 4):
            cv.fill((gy, x), "navy0")
    by = (BASE - M_TOP) * 2
    for x in range(cx0, cx1, 3):
        cv.fill((slice(by, by + 1), slice(x, x + 2)), "gold0")
    for gy in range(cy0, cy1, 6):
        cv.fill((slice(gy, gy + 1), slice(cx1 - 4, cx1)), "gray0")
    cv.fill((slice(by, by + 1), slice(cx1 - 6, cx1)), "gold1")
    nx0, ny0, nx1, ny1 = [(NEXT[0] - M_LEFT) * 2, (NEXT[1] - M_TOP) * 2, (NEXT[2] - M_LEFT) * 2, (NEXT[3] - M_TOP) * 2]
    for s in ((slice(ny0, ny0 + 2), slice(nx0, nx1)), (slice(ny1 - 2, ny1), slice(nx0, nx1)),
              (slice(ny0, ny1), slice(nx0, nx0 + 2)), (slice(ny0, ny1), slice(nx1 - 2, nx1))):
        cv.fill(s, "navy")
    cv.fill((slice(ny0, ny0 + 1), slice(nx0, nx1)), "navy2")
    # CRT 둥근 화면 모서리(그늘) + 유리 사선 반사(바탕 칸에만 — 글자·캔들은 위에 따로 그려짐)
    cv.fill(scr & ~R(SCR[0], SCR[1], SCR[2], SCR[3], 6), "bg00")
    plain = scr & ((cv.idx == cv.c("bg0")) | (cv.idx == cv.c("bg00")))
    for k, (a, w) in enumerate(((14, 3), (22, 1), (150, 2))):
        d = (xx - sx0) + (yy - sy0)
        streak = plain & (d >= a) & (d < a + w) & ((yy - sy0) < 60)
        cv.fill(streak & ((xx + yy) % 2 == 0), "navy0" if k < 2 else "navy0")
    # 아래 띠: 버튼 4개 + 전원 LED + 통풍 홈(왼쪽)
    bot_y = (SCR[3] + 3 - M_TOP) * 2
    for i in range(4):
        bx = (182 - M_LEFT) * 2 + i * 7
        cv.fill((slice(bot_y, bot_y + 3), slice(bx, bx + 5)), dark)
        cv.fill((slice(bot_y, bot_y + 1), slice(bx, bx + 5)), light)
    lx = (M_RIGHT - 6 - M_LEFT) * 2
    cv.fill((slice(bot_y, bot_y + 3), slice(lx, lx + 3)), "bg00")
    cv.fill((slice(bot_y, bot_y + 2), slice(lx, lx + 2)), "green2")
    for i in range(6):
        vx = (SCR[0] + 2 - M_LEFT) * 2 + i * 4
        cv.fill((slice(bot_y, bot_y + 3), slice(vx, vx + 2)), deep)
    # 긁힘·때 약간(테두리 왼쪽 위·오른쪽 아래)
    bez_only = bez & ~well
    seg(cv, (6, 10), (14, 6), dark, bez_only)
    seg(cv, (W - 16, H - 22), (W - 8, H - 26), dark, bez_only)
    return cv


def crt_outline(pal):
    """열 수 있을 때 금색 테두리: 새 테두리 둘레 1칸 바깥(둥근 모서리)."""
    W, H = (M_RIGHT - M_LEFT + 2) * 2, (M_BEZEL_B - M_TOP + 2) * 2
    cv = Canvas(pal, W, H)
    inner = P.round_rect(W, H, 2, 2, W - 2, H - 2, 6)
    ring = P.outline_of(inner, 2)
    cv.fill(ring, "gold1")
    yy = np.mgrid[0:H, 0:W][0]
    cv.fill(ring & (yy < 2), "gold2")
    return cv


def crt_back(pal, style):
    """광대 쪽 모니터 뒷면(160×80 = 지금 80×40): 앞판 테두리(뒤에서 보임) + 가운데 깊은 몸통(통풍 홈) + 받침."""
    W, H = 160, 80
    cv = Canvas(pal, W, H)
    yy, xx = np.mgrid[0:H, 0:W]
    body, light, dark, deep = ("gray1", "cream0", "gray0", "bg00") if style == "A" else ("cream0", "cream", "gray1", "gray0")
    front = P.round_rect(W, H, 0, 2, W, H - 12, 6)
    cv.fill(front, dark)
    cv.fill(P.outline_of(~front, 1) & front, deep)
    cv.fill(front & (yy < 5), body)
    cv.fill((slice(0, 2), slice(8, W - 8)), "navy2")                     # 위로 새는 화면 빛
    # 깊은 몸통(뒤로 튀어나온 상자, 가운데) — 위가 밝고 아래가 어두움
    box = P.round_rect(W, H, 22, 8, W - 22, H - 14, 8)
    cv.fill(box, body)
    sparse(cv, box, dark, np.clip((yy - 8) / (H - 22) * 0.8, 0, 0.8))
    cv.fill(P.outline_of(~box, 1) & box, deep)
    cv.fill(P.outline_of(~box, 2) & box & ~P.outline_of(~box, 1) & (yy < 14), light)
    # 통풍 홈(몸통 위쪽 가로줄 2묶음)
    for y in range(14, 40, 5):
        for x0 in (34, W - 74):
            cv.fill((slice(y, y + 2), slice(x0, x0 + 40)), deep)
            cv.fill((slice(y + 2, y + 3), slice(x0, x0 + 40)), light)
    # 작은 상표 판(글자 없음) + 전원선
    cv.fill((slice(46, 54), slice(W // 2 - 14, W // 2 + 14)), dark)
    cv.fill((slice(46, 47), slice(W // 2 - 14, W // 2 + 14)), light)
    cv.fill((slice(49, 51), slice(W // 2 - 6, W // 2 + 6)), "gold0")
    cv.fill((slice(H - 16, H - 12), slice(W - 40, W - 36)), deep)
    # 받침(목 + 바닥)
    cv.fill((slice(H - 12, H - 6), slice(W // 2 - 12, W // 2 + 12)), deep)
    cv.fill((slice(H - 6, H), slice(W // 2 - 32, W // 2 + 32)), dark)
    cv.fill((slice(H - 6, H - 5), slice(W // 2 - 32, W // 2 + 32)), light)
    cv.fill((slice(H - 1, H), slice(W // 2 - 32, W // 2 + 32)), "bg00")
    return cv


# ================= 합성 미리보기 =================
def paste(dst, cv_rgba, x, y, keep=None, light=1.0):
    """light = 게임 조명 배율(미리보기용 어림): 캡처에서 잰 값 — 내 모니터 자리 약 0.55, 광대 모니터 약 0.8, HUD(폰)는 조명 없음 1.0."""
    h, w = cv_rgba.shape[:2]
    a = cv_rgba[..., 3] > 0
    if keep is not None:
        a &= ~keep
    sub = dst[y:y + h, x:x + w]
    rgb = sub[..., :3]
    rgb[a] = (cv_rgba[..., :3][a] * light).astype(np.uint8)
    if sub.shape[-1] == 4:
        sub[..., 3][a] = 255


def composite(base, parts, mon, back, pal):
    img = base.copy()
    # 광대 모니터: 월드 (-40, 22) → 지금 좌표 (120, 68)
    paste(img, back.rgba(), 240, 136, light=0.8)
    # 내 모니터: 지금 좌표 (M_LEFT, M_TOP + 38). 화면 칸 안쪽은 캡처(글자 GE코인·? 있음)를 두고, 새 그림의 모서리 그늘·반사만
    mx, my = M_LEFT * 2, (M_TOP + 38) * 2
    r = mon.rgba()
    h, w = r.shape[:2]
    yy, xx = np.mgrid[0:h, 0:w]
    sx0, sy0, sx1, sy1 = [(SCR[0] - M_LEFT) * 2, (SCR[1] - M_TOP) * 2, (SCR[2] - M_LEFT) * 2, (SCR[3] - M_TOP) * 2]
    inside = (xx >= sx0) & (xx < sx1) & (yy >= sy0) & (yy < sy1)
    cap = img[my:my + h, mx:mx + w]
    dark_cap = cap.sum(-1) < 90                                             # 캡처의 어두운 화면 바탕만 새 그림으로
    paste(img, r, mx, my, keep=inside & ~dark_cap, light=0.55)
    # 폰: 내 폰 Image (218, -147) → (436, 294), 광대 폰 (216, -102) → (432, 204)
    paste(img, parts["phone-table-clown-2"].rgba(), 432, 204)
    paste(img, parts["phone-table-mine-2"].rgba(), 436, 294)
    return img


def make_draft(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    base = read_png(CAPTURE)[::2, ::2].copy()
    files, comps, zooms = [], [base], []
    for style in ("A", "B"):
        parts = phone_parts(pal, style)
        mon, back = crt(pal, style), crt_back(pal, style)
        d = OUT / style
        d.mkdir(exist_ok=True)
        for name, cv in list(parts.items()) + [("terminal", mon), ("clown-monitor", back), ("monitor-outline", crt_outline(pal))]:
            p = d / f"{name}.png"
            write_png(p, cv.rgba())
            files.append(p)
        comps.append(composite(base, parts, mon, back, pal))
        # 펼친 폰(내 것 | 광대 것 + 키 1·2 불빛) 미리보기
        mine, clown = parts["phone-open"].rgba().copy(), parts["phone-open-clown"].rgba().copy()
        for i in (0, 1):
            x0, y0, _, _ = P.key_rect(i)
            paste(mine, parts[f"phone-key-lit-{i + 1}"].rgba(), x0, y0)
        gap = np.zeros((P.OH, 8, 4), np.uint8)
        zooms.append(np.concatenate([mine, gap, clown], 1))
    # 게임 화면 비교: 지금 | A | B (위에서 아래로) + 폰·모니터 부분 확대(×2)
    sep = np.full((6, 640, 3), 40, np.uint8)
    write_png(OUT / "compare-game.png", np.dstack([np.concatenate([comps[0], sep, comps[1], sep, comps[2]], 0),
                                                   np.full((360 * 3 + 12, 640), 255, np.uint8)]))
    crops = [c[180:360, 200:560] for c in comps]
    vsep = np.full((180, 4, 3), 40, np.uint8)
    row = np.concatenate([crops[0], vsep, crops[1], vsep, crops[2]], 1)
    write_png(OUT / "compare-zoom-x2.png", np.dstack([row, np.full(row.shape[:2], 255, np.uint8)]).repeat(2, 0).repeat(2, 1))
    # 광대 쪽(먼 곳) 확대: 지금 | A | B
    crops2 = [c[120:260, 220:540] for c in comps]
    vs2 = np.full((140, 4, 3), 40, np.uint8)
    row2 = np.concatenate([crops2[0], vs2, crops2[1], vs2, crops2[2]], 1)
    write_png(OUT / "compare-far-x2.png", np.dstack([row2, np.full(row2.shape[:2], 255, np.uint8)]).repeat(2, 0).repeat(2, 1))
    zr = np.concatenate([zooms[0], np.zeros((P.OH, 16, 4), np.uint8), zooms[1]], 1)
    zr[..., 3] = 255
    write_png(OUT / "compare-open-x2.png", zr.repeat(2, 0).repeat(2, 1))
    print("팔레트 검사 (32색):")
    return check32(files, pal)


if __name__ == "__main__":
    cmd = sys.argv[1] if len(sys.argv) > 1 else ""
    if cmd == "draft":
        sys.exit(0 if make_draft(load_pal()) else 1)
    if cmd == "open3":   # 펼친 폰 v3(아래 잘림) → .scratch/art/drafts/device/A3/
        pal = load_pal()
        d = OUT / "A3"
        d.mkdir(parents=True, exist_ok=True)
        parts = {"phone-open": phone_open_v3(pal), "phone-open-clown": phone_open_v3(pal, clown=True), "phone-crack": phone_crack_v3(pal)}
        files = []
        for name, cv in parts.items():
            write_png(d / f"{name}.png", cv.rgba())
            files.append(d / f"{name}.png")
        pv = [parts["phone-open"].rgba(), np.zeros((P.OH, 12, 4), np.uint8), parts["phone-open-clown"].rgba().copy()]
        paste(pv[2], parts["phone-crack"].rgba(), 0, 0)
        row = np.concatenate(pv, 1)
        row[..., 3] = 255
        write_png(d / "preview-x2.png", row.repeat(2, 0).repeat(2, 1))
        print("팔레트 검사 (32색):")
        sys.exit(0 if check32(files, pal) else 1)
    print(__doc__)

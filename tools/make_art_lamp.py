"""Falling Knife 매달린 전등·빛줄기 (AI 작성, lighting Ticket 49).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art_lamp.py draft   # 전등·빛줄기 시안 + 팔레트 검사 → .scratch/art/drafts/hd/lamp/

- 전등 lamp.png 80×24 새 도트: 위 8줄 = 줄(화면 위 끝에서 내려옴), 아래 = 납작한 원뿔 갓(어두운 금속 navy0,
  왼쪽 위 빛 navy2, 테두리 gray0) + 갓 안쪽이 보이는 타원(gold1 → gold2 → 가운데 전구 gold3·cream2).
  게임에서 기준점 = 위 가운데, 자리 (0, 90) → 갓 아래 끝 약 y 78(광대 머리카락 꼭대기 약 y 80과 살짝 겹침, Spec 제안 1).
- 빛줄기 beam.png 440×236 새 도트: 위 72도트 → 아래 440도트로 퍼지는 사다리꼴. gold2 점을 바둑판(Bayer 4×4)으로
  듬성듬성 — 위는 촘촘(약 45%), 아래로 갈수록·양옆 가장자리로 갈수록 성기게(Spec 제안 3).
  게임에서 반투명(SpriteRenderer 색 알파 약 0.2)·조명 안 받는 재질, 기준점 위 가운데 (0, 79).
- 32색, 무작위 없음.
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import write_png  # noqa: E402
from make_art_hd import BAYER4, Canvas, check32, ellipse, load_pal, polygon  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "hd" / "lamp"

LW, LH = 80, 24      # 전등
CORD = 8             # 줄 길이(줄 수)
BW, BH = 440, 236    # 빛줄기
BTOP = 72            # 빛줄기 위 너비


def draw_lamp(pal):
    cv = Canvas(pal, LW, LH)
    cx = LW // 2
    # 줄: 2도트 굵기, 왼쪽 빛·오른쪽 그늘
    cv.fill((slice(0, CORD), slice(cx - 1, cx)), "gray0")
    cv.fill((slice(0, CORD), slice(cx, cx + 1)), "navy0")
    # 갓: 위 24도트 → 아래 80도트 원뿔
    top, bot = CORD, LH - 3
    shade = polygon([(cx - 12, top), (cx + 12, top), (LW, bot), (0, bot)], LW, LH)
    cv.fill(shade, "navy0")
    yy, xx = np.mgrid[0:LH, 0:LW]
    # 왼쪽 위 빛(갓 윗면 왼쪽 절반 위쪽 줄들), 위 테두리
    cv.fill(shade & (xx < cx) & (yy < top + 5) & ((xx + yy) % 2 == 0), "navy2")
    cv.fill(shade & (yy == top), "gray0")
    # 갓 아래 테두리(rim)
    cv.fill((slice(bot, bot + 1), slice(0, LW)), "gray0")
    # 갓 안쪽(아래에서 보이는 타원): 바깥 gold1 → gold2 → 전구 gold3 → 가운데 cream2
    for rx, ry, col in ((38, 2.6, "gold1"), (24, 2.2, "gold2"), (10, 1.8, "gold3"), (4, 1.4, "cream2")):
        cv.fill(ellipse(cx - 0.5, bot + 1.5, rx, ry, LW, LH) & (yy > bot), col)
    return cv


BEAM_TOP, BEAM_BOTTOM = 0.22, 0.05   # 점 밀도(위 → 아래). 처음 0.45 → 0.12는 너무 빽빽해 눈이 아프다는 사용자 의견으로 절반쯤
BH_TITLE = 274                       # 타이틀용: 전등 아래(y 79)에서 테이블 모서리(y −58)까지 = 137칸 = 274 새 도트(빛이 중간에 끊기지 않게)


def draw_beam(pal, BH=BH):
    cv = Canvas(pal, BW, BH)
    yy, xx = np.mgrid[0:BH, 0:BW] + 0.5
    t = yy / BH                                   # 0 = 위, 1 = 아래
    half = (BTOP + (BW - BTOP) * t) / 2           # 그 줄의 반 너비
    side = np.clip(1 - np.abs(xx - BW / 2) / half, 0, 1)
    level = (BEAM_TOP - (BEAM_TOP - BEAM_BOTTOM) * t) * np.clip(side * 2.5, 0, 1)   # 위 → 아래로 옅게, 양옆 가장자리에서 0으로
    thr = BAYER4[(yy - 0.5).astype(int) % 4, (xx - 0.5).astype(int) % 4]
    cv.fill((level > thr) & (side > 0), "gold2")
    return cv


PW, PH = 600, 110   # 테이블 위 빛 웅덩이(lighting 이어서, 사용자 "조명 빛이 테이블 위에까지 쬐는 이펙트")


def draw_pool(pal):
    """테이블 윗면에 떨어진 빛: 위 가운데(빛줄기가 닿는 테이블 뒤 모서리)를 중심으로 한 반타원, 가운데 촘촘(약 42%) → 가장자리 0.
    빛줄기와 같은 gold2 바둑판 무늬. 게임에서 기준점 = 위 가운데, 테이블 뒤 모서리에 붙임.
    (시안 2 "테이블 정중앙에서 퍼지는 온전한 타원"도 만들어 봤으나 사용자가 이 시안 1로 되돌림)"""
    cv = Canvas(pal, PW, PH)
    yy, xx = np.mgrid[0:PH, 0:PW] + 0.5
    d = np.sqrt(((xx - PW / 2) / (PW / 2)) ** 2 + (yy / PH) ** 2)   # 0 = 위 가운데, 1 = 타원 가장자리
    level = 0.22 * np.clip(1 - d, 0, 1) ** 1.2   # 처음 0.42는 빽빽해 눈이 아픔(사용자) → 0.22
    thr = BAYER4[(yy - 0.5).astype(int) % 4, (xx - 0.5).astype(int) % 4]
    cv.fill((level > thr) & (d < 1), "gold2")
    return cv


def make_pool(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    p = OUT / "pool.png"
    write_png(p, draw_pool(pal).rgba())
    # 빛줄기도 같은 밀도로 다시: 게임용(236줄)·타이틀용(274줄, 테이블 모서리까지)
    b = OUT / "beam.png"
    write_png(b, draw_beam(pal).rgba())
    bt = OUT / "beam-title.png"
    write_png(bt, draw_beam(pal, BH_TITLE).rgba())
    print("팔레트 검사 (32색):")
    return check32([p, b, bt], pal)


def make_draft(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    files = []
    for name, cv in (("lamp", draw_lamp(pal)), ("beam", draw_beam(pal))):
        p = OUT / f"{name}.png"
        write_png(p, cv.rgba())
        files.append(p)
    lamp = draw_lamp(pal).rgba()
    write_png(OUT / "lamp-x6.png", lamp.repeat(6, 0).repeat(6, 1))
    print("팔레트 검사 (32색):")
    return check32(files, pal)


if __name__ == "__main__":
    cmd = sys.argv[1] if len(sys.argv) > 1 else ""
    if cmd == "draft":
        sys.exit(0 if make_draft(load_pal()) else 1)
    if cmd == "pool":
        sys.exit(0 if make_pool(load_pal()) else 1)
    print(__doc__)

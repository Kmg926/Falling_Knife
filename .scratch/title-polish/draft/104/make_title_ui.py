"""title-polish 104 — 타이틀 화면 꾸밈 그림 3장 (레퍼런스: Halo 선택 띠 · Hollow Knight 장식·로고 빛).

    python tools/make_title_ui.py          # → .scratch/art/drafts/title/ (미리보기 포함)

- title-select-bar.png  : 고른 메뉴 뒤 금색 띠. 왼쪽은 진하고 오른쪽으로 갈수록 바둑판(Bayer) 섞기로 사라짐.
- title-knife.png       : 고른 줄 왼쪽에 다는 작은 칼(끝이 아래 — "떨어지는 칼").
- title-logo-glow.png   : 로고 뒤 은은한 붉은 빛(타원, 가운데 진하고 밖으로 갈수록 성기게).
모두 32색 팔레트만 쓴다(반투명 대신 점을 성기게 찍어 흐려 보이게 — 광대 조명 빛과 같은 방법).
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import write_png  # noqa: E402
from make_art_hd import BAYER4, Canvas, check32, load_pal  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "title"


def sparse(cv, region, name, level):
    """level(0~1, 배열 가능)만큼만 점을 찍는다 — 나머지는 투명(바둑판 순서라 고르게 성김)."""
    yy, xx = np.mgrid[0:cv.h, 0:cv.w]
    thr = BAYER4[yy % 4, xx % 4]
    lvl = np.broadcast_to(level, (cv.h, cv.w))
    cv.fill(region & (lvl > thr), name)


def select_bar(pal, w=300, h=26):
    """고른 줄 뒤 띠(300x26 = 화면 150x13칸). 몸통 gold0, 위·아래 한 줄 gold1, 왼쪽 끝 2줄 gold2."""
    cv = Canvas(pal, w, h)
    xx = np.mgrid[0:h, 0:w][1]
    fade = np.clip(1.0 - xx / (w * 0.9), 0, 1) ** 0.8              # 왼쪽 1 → 오른쪽 0
    all_ = np.ones((h, w), bool)
    body = all_.copy(); body[:2, :] = False; body[-2:, :] = False
    sparse(cv, body, "gold0", fade)
    edge_ = np.zeros((h, w), bool); edge_[:2, :] = True; edge_[-2:, :] = True
    sparse(cv, edge_, "gold1", fade * 0.9)
    cv.fill((slice(0, h), slice(0, 4)), "gold2")                     # 왼쪽 끝 기둥
    return cv


def knife(pal, w=12, h=26):
    """작은 칼(12x26 = 6x13칸), 끝이 아래. 위에서부터 손잡이(남색) → 날밑(금색) → 날(크림, 오른쪽 그늘) → 끝."""
    cv = Canvas(pal, w, h)
    cx = w // 2
    cv.fill((slice(0, 8), slice(cx - 2, cx + 2)), "navy")            # 손잡이
    cv.fill((slice(0, 8), slice(cx - 2, cx - 1)), "navy2")
    cv.fill((slice(0, 1), slice(cx - 2, cx + 2)), "gold1")           # 손잡이 끝 고리
    cv.fill((slice(8, 10), slice(1, w - 1)), "gold1")                # 날밑
    cv.fill((slice(8, 9), slice(1, w - 1)), "gold2")
    for y in range(10, h):                                           # 날: 아래로 갈수록 좁아짐
        half = max(0, int(round(2.5 * (h - 1 - y) / (h - 11))))
        if half == 0:
            cv.fill((slice(y, y + 1), slice(cx - 1, cx)), "cream")
            continue
        cv.fill((slice(y, y + 1), slice(cx - half, cx + half)), "cream")
        cv.fill((slice(y, y + 1), slice(cx, cx + half)), "cream0")   # 오른쪽 그늘
        cv.fill((slice(y, y + 1), slice(cx - half, cx - half + 1)), "cream2")   # 왼쪽 날 빛
    return cv


def logo_glow(pal, w=420, h=90):
    """로고 뒤 빛(420x90 = 210x45칸). 타원 안에서 가운데 진하게(red0), 더 가운데는 red1 몇 점."""
    cv = Canvas(pal, w, h)
    yy, xx = np.mgrid[0:h, 0:w]
    d = np.hypot((xx - w / 2) / (w / 2), (yy - h / 2) / (h / 2))
    level = np.clip(1.0 - d, 0, 1) ** 1.3 * 0.55
    sparse(cv, d < 1, "red0", level)
    sparse(cv, d < 0.5, "red1", np.clip(0.5 - d, 0, 1) * 0.35)
    return cv


def main():
    pal = load_pal()
    OUT.mkdir(parents=True, exist_ok=True)
    pics = {"title-select-bar": select_bar(pal), "title-knife": knife(pal), "title-logo-glow": logo_glow(pal)}
    files = []
    for name, cv in pics.items():
        f = OUT / f"{name}.png"
        write_png(f, cv.rgba())
        files.append(f)
    # 미리보기(×3): 어두운 바탕 위에 띠 + 칼 + 빛
    sheet = np.tile(pal["bg0"], (100, 440, 1)).astype(np.uint8)
    for (cv, x, y) in ((pics["title-logo-glow"], 10, 5), (pics["title-select-bar"], 30, 60), (pics["title-knife"], 12, 60)):
        rgba = cv.rgba()
        hh, ww = rgba.shape[:2]
        m = rgba[..., 3] > 0
        sheet[y:y + hh, x:x + ww][m] = rgba[..., :3][m]
    write_png(OUT / "title-preview-x3.png", sheet.repeat(3, 0).repeat(3, 1))
    print("팔레트 검사 (32색):")
    return check32(files, pal)


if __name__ == "__main__":
    sys.exit(0 if main() else 1)

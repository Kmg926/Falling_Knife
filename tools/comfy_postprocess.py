"""Falling Knife 생성 그림 후처리 (AI 작성, comfy-local Ticket 51, Spec 제안 4).

사용법 (C:\\Falling_Knife 에서):
    python tools/comfy_postprocess.py .scratch/art/drafts/comfy/title-bg-101.png [...]

그림마다 4가지를 .scratch/art/drafts/comfy/post/ 에 만들고, 모두 모은 비교 그림 sheet.png 도 만든다.
- A   : 16:9로 가운데 자르기 → 640×360으로 면적 평균 줄이기 → 32색 팔레트에서 가장 가까운 색
- A-d : A와 같되 팔레트 맞추기 전에 바둑판(Bayer 4×4) 무늬 섞기 → 색 사이 단계를 점무늬로
- B   : 320×180으로 줄여 32색 → 2배(Nearest)로 640×360(굵은 도트)
- B-d : B + 무늬 섞기
- 32색(tools/palette.txt), 무작위 없음. 마지막에 팔레트 검사(check32).
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import read_png, write_png  # noqa: E402
from make_art_hd import BAYER4, check32, load_pal  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "comfy" / "post"
DITHER = 28   # 무늬 섞기 세기(RGB 0~255 기준 ±14)


def crop169(img):
    h, w = img.shape[:2]
    th = round(w * 9 / 16)
    if th <= h:
        y0 = (h - th) // 2
        return img[y0:y0 + th]
    tw = round(h * 16 / 9)
    x0 = (w - tw) // 2
    return img[:, x0:x0 + tw]


def area_resize(img, w, h):
    """면적 평균 줄이기: 결과 한 칸 = 원본에서 그 칸이 덮는 구역의 평균(누적합 사용)."""
    src = img.astype(np.float64)
    sh, sw = src.shape[:2]
    ii = np.zeros((sh + 1, sw + 1, src.shape[2]))
    ii[1:, 1:] = src.cumsum(0).cumsum(1)

    def edges(n, m):
        return np.linspace(0, m, n + 1)

    ys, xs = edges(h, sh), edges(w, sw)

    def at(y, x):  # 실수 좌표의 누적합(쌍선형)
        y0, x0 = np.floor(y).astype(int), np.floor(x).astype(int)
        y1, x1 = np.minimum(y0 + 1, sh), np.minimum(x0 + 1, sw)
        fy, fx = (y - y0)[:, None, None], (x - x0)[None, :, None]
        a = ii[y0][:, x0]; b = ii[y0][:, x1]; c = ii[y1][:, x0]; d = ii[y1][:, x1]
        return a * (1 - fy) * (1 - fx) + b * (1 - fy) * fx + c * fy * (1 - fx) + d * fy * fx

    s = at(ys, xs)
    area = (np.diff(ys)[:, None] * np.diff(xs)[None, :])[..., None]
    out = (s[1:, 1:] - s[1:, :-1] - s[:-1, 1:] + s[:-1, :-1]) / area
    return np.clip(out, 0, 255)


def to_palette(img, pal, dither):
    cols = np.array(list(pal.values()), np.float64)
    x = img.astype(np.float64)
    if dither:
        h, w = x.shape[:2]
        yy, xx = np.mgrid[0:h, 0:w]
        x = x + ((BAYER4[yy % 4, xx % 4] - 0.5) * DITHER)[..., None]
    w8 = np.array([0.30, 0.59, 0.11])   # 사람 눈 밝기 무게
    d = (((x[..., None, :] - cols[None, None]) ** 2) * w8).sum(-1)
    return cols[d.argmin(-1)].astype(np.uint8)


def rgba(rgb):
    out = np.full(rgb.shape[:2] + (4,), 255, np.uint8)
    out[..., :3] = rgb
    return out


def process(path, pal):
    src = read_png(path)[..., :3]
    c = crop169(src)
    a_src = area_resize(c, 640, 360)
    b_src = area_resize(c, 320, 180)
    res = {
        "A": to_palette(a_src, pal, False),
        "A-d": to_palette(a_src, pal, True),
        "B": to_palette(b_src, pal, False).repeat(2, 0).repeat(2, 1),
        "B-d": to_palette(b_src, pal, True).repeat(2, 0).repeat(2, 1),
    }
    files = []
    for k, v in res.items():
        p = OUT / f"{Path(path).stem}-{k}.png"
        write_png(p, rgba(v))
        files.append(p)
    return res, files


def main(paths):
    pal = load_pal()
    OUT.mkdir(parents=True, exist_ok=True)
    rows, files = [], []
    for p in paths:
        res, f = process(p, pal)
        rows.append(list(res.values()))
        files += f
    gap = 6
    sheet = np.full((len(rows) * (360 + gap), 4 * (640 + gap), 3), 40, np.uint8)
    for r, row in enumerate(rows):
        for c, img in enumerate(row):
            sheet[r * (360 + gap):r * (360 + gap) + 360, c * (640 + gap):c * (640 + gap) + 640] = img
    write_png(OUT / "sheet.png", rgba(sheet))
    print("팔레트 검사 (32색):")
    return check32(files, pal)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    sys.exit(0 if main(sys.argv[1:]) else 1)

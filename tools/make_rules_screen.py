"""rules-card 105 — 규칙 1쪽 "화면" 그림: 게임 캡처 위에 번호 배지(레퍼런스: 지도 위 번호 + 옆 이름표).

    python tools/make_rules_screen.py      # 입력 .scratch/art/drafts/rules/game-capture-1280.png
                                           # 출력 .scratch/art/drafts/rules/rules-screen.png (640x360) + 미리보기 x2

- 입력은 Unity 게임 화면 1280x720 캡처(MCP, 내 차례 첫 화면). 그림이 640 격자라 2칸마다 1칸 골라 640x360으로 줄여도 깨지지 않음.
- 배지: 금색(gold2) 네모 23x23 + 남색 테두리 + 3x5 숫자를 3배(9x15)로(그림이 화면에 1:1이라 작으면 안 보임). 숫자 자리는 아래 SPOTS(640 기준 왼쪽 위).
- 캡처 자체는 이 게임 화면이라 32색 검사는 배지에만 해당(캡처는 조명 때문에 팔레트 밖 색이 섞임).
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import write_png  # noqa: E402
from make_art_hd import load_pal  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
DIR = ROOT / ".scratch" / "art" / "drafts" / "rules"

# 3x5 숫자(1 = 칠함)
DIGITS = {
    "1": ["010", "110", "010", "010", "111"],
    "2": ["111", "001", "111", "100", "111"],
    "3": ["111", "001", "111", "001", "111"],
    "4": ["101", "101", "111", "001", "001"],
    "5": ["111", "100", "111", "001", "111"],
    "6": ["111", "100", "111", "101", "111"],
    "7": ["111", "001", "010", "010", "010"],
}

# 번호: 이름(TitleMenu 이름표와 같은 순서) · 배지 왼쪽 위(640x360 기준)
SPOTS = [
    ("1", "모니터", (190, 226)),   # device-art 108: 모니터 테두리가 4칸 넓어져 왼쪽으로(전 (198, 232))
    ("2", "레버리지", (490, 140)),
    ("3", "내 잔고", (3, 238)),   # balance-board 112: 잔고 전광판 아래 줄 왼쪽(전 (4, 284) 매트)
    ("4", "광대 잔고", (3, 196)),   # balance-board 112: 전광판 위 줄 왼쪽(전 (36, 192) 매트)
    ("5", "내 폰", (530, 282)),
    ("6", "광대 폰", (404, 192)),
    ("7", "덱", (214, 138)),
]


def read_png(path):
    import zlib
    data = path.read_bytes()
    assert data[:8] == b"\x89PNG\r\n\x1a\n"
    pos, idat, w = 8, b"", 0
    while pos < len(data):
        n = int.from_bytes(data[pos:pos + 4], "big")
        kind = data[pos + 4:pos + 8]
        body = data[pos + 8:pos + 8 + n]
        if kind == b"IHDR":
            w, h, depth, ctype = body[0:4], body[4:8], body[8], body[9]
            w, h = int.from_bytes(w, "big"), int.from_bytes(h, "big")
            ch = {2: 3, 6: 4}[ctype]
            assert depth == 8
        elif kind == b"IDAT":
            idat += body
        pos += 12 + n
    raw = zlib.decompress(idat)
    stride = w * ch
    out = np.zeros((h, stride), np.uint8)
    prev = np.zeros(stride, np.int32)
    i = 0
    for y in range(h):
        f = raw[i]; line = np.frombuffer(raw[i + 1:i + 1 + stride], np.uint8).astype(np.int32); i += 1 + stride
        cur = np.zeros(stride, np.int32)
        for x in range(stride):
            a = cur[x - ch] if x >= ch else 0
            b = prev[x]
            c = prev[x - ch] if x >= ch else 0
            if f == 0: p = 0
            elif f == 1: p = a
            elif f == 2: p = b
            elif f == 3: p = (a + b) // 2
            else:
                pa, pb, pc = abs(b - c), abs(a - c), abs(a + b - 2 * c)
                p = a if pa <= pb and pa <= pc else (b if pb <= pc else c)
            cur[x] = (line[x] + p) & 255
        out[y] = cur
        prev = cur
    img = out.reshape(h, w, ch)
    return img[..., :3]


def badge(img, pal, x, y, d):
    s = 23
    img[y:y + s, x:x + s] = pal["navy"]
    img[y + 2:y + s - 2, x + 2:x + s - 2] = pal["gold2"]
    rows = DIGITS[d]
    ox, oy = x + (s - 9) // 2, y + (s - 15) // 2
    for r, row in enumerate(rows):
        for c, v in enumerate(row):
            if v == "1":
                img[oy + r * 3:oy + r * 3 + 3, ox + c * 3:ox + c * 3 + 3] = pal["navy"]


def main():
    pal = load_pal()
    src = read_png(DIR / "game-capture-1280.png")
    assert src.shape[:2] == (720, 1280), src.shape
    img = src[::2, ::2].copy()                      # 640x360 (그림이 640 격자라 2칸마다 1칸)
    for d, _name, (x, y) in SPOTS:
        badge(img, pal, x, y, d)
    rgba = np.dstack([img, np.full(img.shape[:2], 255, np.uint8)])
    write_png(DIR / "rules-screen.png", rgba)
    write_png(DIR / "rules-screen-preview-x2.png", rgba.repeat(2, 0).repeat(2, 1))
    print("rules-screen.png 640x360, 배지", len(SPOTS))
    return True


if __name__ == "__main__":
    sys.exit(0 if main() else 1)

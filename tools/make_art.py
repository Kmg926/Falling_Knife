"""Falling Knife 도트 그림 만들기 (AI 작성, ADR 0003 / formal-ui Ticket 24).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art.py clown     # 광대 시안 A·B·C + 미리보기 + 팔레트 검사
    python tools/make_art.py check <png...>   # 팔레트 검사만

- 입력: tools/palette.txt (16색), .scratch/art/reference/sketch-play-320.png
  (GDD 화면 스케치 v2 플레이 화면에서 게임 화면 부분만 잘라 320x180으로 줄인 것. 머리글(이름) 없음)
- 출력: .scratch/art/drafts/ (Assets 밖. 사용자가 고른 것만 Assets/_Project/Sprites/ 로 옮김)
- 외부 라이브러리는 numpy만 쓴다(PIL 없음). 무작위 없음 → 같은 입력이면 같은 결과.
"""
import struct
import sys
import zlib
from pathlib import Path

import numpy as np

ROOT = Path(__file__).resolve().parent.parent
PALETTE_FILE = ROOT / "tools" / "palette.txt"
REF_PLAY = ROOT / ".scratch" / "art" / "reference" / "sketch-play-320.png"
DRAFTS = ROOT / ".scratch" / "art" / "drafts"

# 광대 캔버스: 스케치 320x180에서 x 112~207, y 4~99 (96x96). 맨 아래 줄 = 테이블 선.
CLOWN_BOX = (112, 4, 96, 96)      # x, y, 너비, 높이
CLOWN_SEED = (160, 40)            # 얼굴 한가운데(스케치 좌표) — 여기서 이어진 부분만 광대로 봄


# ---------- PNG 읽기·쓰기 ----------
def read_png(path):
    d = Path(path).read_bytes()
    i, idat, w, h, ct = 8, b"", 0, 0, 2
    while i < len(d):
        n = struct.unpack(">I", d[i:i + 4])[0]
        t, c = d[i + 4:i + 8], d[i + 8:i + 8 + n]
        if t == b"IHDR":
            w, h, _, ct = struct.unpack(">IIBB", c[:10])
        elif t == b"IDAT":
            idat += c
        i += 12 + n
    comps = {0: 1, 2: 3, 6: 4}[ct]
    raw, stride = zlib.decompress(idat), w * comps
    out = np.zeros((h, stride), np.int32)
    prev = np.zeros(stride, np.int32)
    for y in range(h):
        f = raw[y * (stride + 1)]
        line = np.frombuffer(raw[y * (stride + 1) + 1:(y + 1) * (stride + 1)], np.uint8).astype(np.int32)
        if f == 0:
            cur = line
        elif f == 2:
            cur = (line + prev) & 255
        else:
            cur = line.copy()
            for x in range(stride):
                a = cur[x - comps] if x >= comps else 0
                b = prev[x]
                c = prev[x - comps] if x >= comps else 0
                if f == 1:
                    cur[x] = (line[x] + a) & 255
                elif f == 3:
                    cur[x] = (line[x] + (a + b) // 2) & 255
                else:
                    p = a + b - c
                    pa, pb, pc = abs(p - a), abs(p - b), abs(p - c)
                    cur[x] = (line[x] + (a if pa <= pb and pa <= pc else b if pb <= pc else c)) & 255
        out[y], prev = cur, cur
    return out.reshape(h, w, comps).astype(np.uint8)


def write_png(path, arr):
    arr = np.ascontiguousarray(arr, np.uint8)
    h, w = arr.shape[:2]
    comps = arr.shape[2] if arr.ndim == 3 else 1
    ct = {1: 0, 3: 2, 4: 6}[comps]
    rows = b"".join(b"\x00" + arr[y].tobytes() for y in range(h))

    def chunk(t, c):
        return struct.pack(">I", len(c)) + t + c + struct.pack(">I", zlib.crc32(t + c) & 0xFFFFFFFF)

    Path(path).write_bytes(b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, ct, 0, 0, 0))
                           + chunk(b"IDAT", zlib.compress(rows, 9)) + chunk(b"IEND", b""))


# ---------- 팔레트 ----------
def load_palette():
    pal = {}
    for line in PALETTE_FILE.read_text(encoding="utf-8").splitlines():
        if not line.strip() or line.startswith("#"):
            continue
        name, hexcode = line.split()[:2]
        pal[name] = np.array([int(hexcode[i:i + 2], 16) for i in (1, 3, 5)])
    return pal


def quantize(rgb, pal):
    """각 픽셀을 팔레트에서 가장 가까운 색의 번호로."""
    cols = np.array(list(pal.values()))
    d = ((rgb[..., None, :].astype(int) - cols[None, None]) ** 2).sum(-1)
    return d.argmin(-1)


def check(paths, pal):
    """투명하지 않은 픽셀이 모두 팔레트 색인지, 색 수가 16 이하인지 (Spec C1)."""
    allowed = {tuple(c) for c in pal.values()}
    ok = True
    for p in paths:
        a = read_png(p)
        px = a[..., :3][a[..., 3] > 0] if a.shape[2] == 4 else a.reshape(-1, 3)
        used = {tuple(c) for c in np.unique(px, axis=0)}
        bad = used - allowed
        print(f"  {Path(p).name}: 색 {len(used)}개, 팔레트 밖 {len(bad)}개 {'OK' if not bad and len(used) <= 16 else 'FAIL'}")
        ok &= not bad and len(used) <= 16
    return ok


# ---------- 광대 ----------
def flood(mask, seed):
    """seed에서 상하좌우로 이어진 True 영역만 남긴다(배경 위 빨간 차트 선·주석 번호 제거)."""
    h, w = mask.shape
    keep = np.zeros_like(mask)
    stack = [seed]
    while stack:
        x, y = stack.pop()
        if 0 <= x < w and 0 <= y < h and mask[y, x] and not keep[y, x]:
            keep[y, x] = True
            stack += [(x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)]
    return keep


LIGHTER = {"red0": "red1", "red1": "red2", "red2": "red3", "navy": "gray1", "gold0": "gold1", "gray1": "cream"}
DARKER = {"red3": "red2", "red2": "red1", "red1": "red0", "cream": "gray1", "gray1": "gray0", "gold1": "gold0"}


def rim_light(idx, mask, names, depth=2):
    """위쪽 조명: 위로 depth칸 안에 빈칸이 있는 가장자리를 한 단계 밝게(테이블 위 조명 하나).
    1도트 디테일(눈·입·점무늬)은 가장자리가 아니라서 그대로 남는다."""
    out = idx.copy()
    h, w = idx.shape
    for y in range(h):
        for x in range(w):
            if mask[y, x] and any(y - k < 0 or not mask[y - k, x] for k in range(1, depth + 1)):
                n = names[idx[y, x]]
                if n in LIGHTER:
                    out[y, x] = names.index(LIGHTER[n])
    return out


def bottom_shade(idx, mask, names, rows=14):
    """아래 그림자: 광대 맨 아래 rows줄(테이블 가까운 몸)을 한 단계 어둡게 — 빛이 위에서만 온다."""
    out = idx.copy()
    bottom = np.where(mask.any(1))[0].max()
    for y in range(bottom - rows + 1, bottom + 1):
        for x in range(idx.shape[1]):
            n = names[idx[y, x]]
            if mask[y, x] and n in DARKER:
                out[y, x] = names.index(DARKER[n])
    return out


def bald_top(idx, mask, names, cx=48.5, rx=18, top=12, mid=40, gap=2, crown=17):
    """가운데가 빈 머리(사용자 요청 2026-11-16): 얼굴 피부를 위로 이어 둥근 정수리를 만들고,
    정수리 위·양옆 gap칸 안의 머리카락은 지워 양옆 머리 뭉치만 남긴다.
    정수리 = 가로 반지름 rx, 위 끝 top, 가장 넓은 줄 mid인 타원의 윗부분."""
    idx, mask = idx.copy(), mask.copy()
    hair = {names.index(n) for n in ("red0", "red1", "red2")}
    skin, shade, edge = names.index("cream"), names.index("gray1"), names.index("red0")
    ry = mid - top
    h, w = idx.shape
    new_skin = np.zeros_like(mask)
    for y in range(mid):
        for x in range(w):
            dx, dy = (x + 0.5 - cx) / rx, (y + 0.5 - mid) / ry
            inside = dx * dx + dy * dy <= 1
            if inside and (idx[y, x] in hair or not mask[y, x]):
                idx[y, x] = skin                      # 정수리 피부
                mask[y, x] = True
                new_skin[y, x] = True
            elif not inside and idx[y, x] in hair and (abs(x + 0.5 - cx) <= rx + gap or y < crown):
                mask[y, x] = False                    # 정수리 위·옆 머리카락 지움 → 양옆 뭉치만
    # 새로 만든 정수리의 오른쪽 가장자리 2칸 = 그늘(빛은 위·왼쪽). 원래 얼굴(눈 주변)은 건드리지 않음
    for y in range(mid):
        for x in range(w):
            if new_skin[y, x] and x > cx and (x + 2 >= w or not mask[y, x + 2] or idx[y, x + 2] != skin):
                idx[y, x] = shade
    # 남은 머리 뭉치의 잘린 안쪽 = 어두운 빨강
    for y in range(mid):
        for x in range(w):
            if mask[y, x] and idx[y, x] in hair:
                near = [(y + dy, x + dx) for dy in (-1, 0, 1) for dx in (-1, 0, 1)]
                if any(0 <= yy < h and 0 <= xx < w and (not mask[yy, xx])
                       and (abs(xx + 0.5 - cx) <= rx + gap or yy < crown) for yy, xx in near):
                    idx[y, x] = edge
    return idx, mask


def to_rgba(idx, mask, pal):
    cols = np.array(list(pal.values()), np.uint8)
    rgba = np.zeros(idx.shape + (4,), np.uint8)
    rgba[..., :3] = cols[idx]
    rgba[..., 3] = np.where(mask, 255, 0)
    return rgba


def make_clown(pal):
    names = list(pal.keys())
    ref = read_png(REF_PLAY)[..., :3]
    idx_full = quantize(ref, pal)
    background = np.isin(idx_full, [names.index("bg0"), names.index("bg1")])
    mask_full = flood(~background, CLOWN_SEED)

    x, y, w, h = CLOWN_BOX
    idx = idx_full[y:y + h, x:x + w]
    mask = mask_full[y:y + h, x:x + w].copy()
    # 단말기 위 테두리(가로로 길게 이어진 줄)부터 아래는 광대가 아니다 → 그 줄부터 잘라 냄
    wide = np.where(mask.sum(1) > 0.9 * w)[0]
    if len(wide):
        mask[wide.min():] = False
    mask = flood(mask, (CLOWN_SEED[0] - x, CLOWN_SEED[1] - y))   # 잘라 낸 뒤 떨어진 점(테두리 모서리) 제거
    print(f"광대 영역: {mask.sum()}픽셀, 몸 높이 {np.ptp(np.where(mask.any(1))[0]) + 1}도트, "
          f"너비 {np.ptp(np.where(mask.any(0))[0]) + 1}도트")

    # 11주차 "한 가지씩 고치기": 시안마다 A에서 한 가지만 바꾼다
    # 두 머리 모양 × 시안 3개. bald = 가운데가 빈 머리(사용자 요청)
    DRAFTS.mkdir(parents=True, exist_ok=True)
    files = []
    for suffix, (base, m) in (("", (idx, mask)), ("-bald", bald_top(idx, mask, names))):
        a = base                              # A: 스케치를 팔레트로만 맞춤
        b = rim_light(a, m, names)            # B: A + 위쪽 조명 테두리(2도트)
        c = bottom_shade(a, m, names)         # C: A + 아래 그림자(14줄)
        for tag, im in (("A", a), ("B", b), ("C", c)):
            p = DRAFTS / f"clown-{tag}{suffix}.png"
            write_png(p, to_rgba(im, m, pal))
            files.append(p)

    # 미리보기: 윗줄 원래 머리, 아랫줄 가운데 빈 머리. 어두운 바탕 위 4배 (사용자가 보고 고르는 용도)
    bg = pal["bg0"].astype(np.uint8)
    sheet = np.tile(bg, (h * 2 + 8, w * 3 + 16, 1))
    for i, p in enumerate(files):
        im = read_png(p)
        ox, oy = (i % 3) * (w + 8), (i // 3) * (h + 8)
        region = sheet[oy:oy + h, ox:ox + w]
        region[im[..., 3] > 0] = im[..., :3][im[..., 3] > 0]
    write_png(DRAFTS / "clown-preview-x4.png", sheet.repeat(4, 0).repeat(4, 1))
    print("팔레트 검사 (C1):")
    check(files, pal)


if __name__ == "__main__":
    palette = load_palette()
    if len(sys.argv) > 1 and sys.argv[1] == "check":
        sys.exit(0 if check(sys.argv[2:], palette) else 1)
    make_clown(palette)

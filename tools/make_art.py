"""Falling Knife 도트 그림 만들기 (AI 작성, ADR 0003 / formal-ui Ticket 24).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art.py clown     # 광대 시안 A·B·C + 미리보기 + 팔레트 검사
    python tools/make_art.py play      # Ticket 25 플레이 화면 그림 + 합친 미리보기
    python tools/make_art.py hud       # Ticket 26 정식 UI 그림(틀·버튼·슬롯·동전·아이콘) + 미리보기
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


def fill_holes(mask):
    """광대 안쪽에 갇힌 빈칸(눈동자처럼 바탕색과 같은 어두운 칸)을 채운다.
    바깥 테두리에서 이어진 빈칸만 진짜 바깥 → 나머지 빈칸은 그림의 일부(불투명)."""
    h, w = mask.shape
    outside = np.zeros_like(mask)
    for sx, sy in [(x, 0) for x in range(w)] + [(x, h - 1) for x in range(w)] + \
                  [(0, y) for y in range(h)] + [(w - 1, y) for y in range(h)]:
        if not mask[sy, sx] and not outside[sy, sx]:
            outside |= flood(~mask & ~outside, (sx, sy))
    return ~outside


def clown_base(pal):
    """스케치에서 광대만 떼어 낸 (색 번호, 불투명 여부). 시안 A와 '생각 중' 그림의 공통 바탕."""
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
    mask = fill_holes(mask)                                        # 눈동자(어두운 칸)가 투명 구멍이 되지 않게
    return idx, mask


def make_clown(pal):
    names = list(pal.keys())
    w, h = CLOWN_BOX[2], CLOWN_BOX[3]
    idx, mask = clown_base(pal)
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


# ---------- 플레이 화면 그림 (Ticket 25) ----------
# 위치·크기는 스케치 320x180에서 잰 값(아트 메모 7절). (x, y) = 그림 왼쪽 위, 화면 위쪽이 y 0.
TERMINAL = (114, 97, 92, 42)      # GE코인 단말기 틀
QBOX = (182, 111, 19, 23)         # 단말기 안 "다음 캔들" 칸(? 글자는 Canvas)
LEVER_BOX = (214, 102, 70, 40)    # 레버리지 상자 틀
TABLE = (0, 100, 320, 41)         # 테이블(사다리꼴), 위 끝 x 30~290 → 아래 끝 x 0~319
CANDLE = (5, 18)                  # 캔들 1개(너비, 높이): 심지 3 + 몸통 13 + 심지 2
FLAME = (6, 9)                    # 불꽃 1개


def sprite(w, h):
    """빈(투명) 그림: (색 번호, 불투명 여부)."""
    return np.zeros((h, w), int), np.zeros((h, w), bool)


def put(idx, mask, ys, xs, color, names):
    idx[ys, xs] = names.index(color)
    mask[ys, xs] = True


def frame(w, h, border, inside, names):
    """1도트 테두리 틀. 안쪽은 inside 색(없으면 투명)."""
    idx, mask = sprite(w, h)
    if inside:
        put(idx, mask, slice(None), slice(None), inside, names)
    for ys, xs in ((0, slice(None)), (h - 1, slice(None)), (slice(None), 0), (slice(None), w - 1)):
        put(idx, mask, ys, xs, border, names)
    return idx, mask


def room(pal):
    """방 배경 320x180(불투명): bg0 + 스케치의 붉은 차트 선(red0). 광대·글자 칸·테이블 자리는 바탕색."""
    names = list(pal.keys())
    q = quantize(read_png(REF_PLAY)[..., :3], pal)
    idx = np.full((180, 320), names.index("bg0"))
    _, cmask = clown_base(pal)
    clown = np.zeros((180, 320), bool)
    x, y, w, h = CLOWN_BOX
    clown[y:y + h, x:x + w] = cmask
    # 스케치의 차트 선은 아주 어두운 갈색-빨강이라 팔레트로 맞추면 wood·bg1·red0 중 하나가 된다
    curve = np.isin(q, [names.index(n) for n in ("wood", "bg1", "red0", "red1")]) & ~clown
    curve[TABLE[1]:] = False                         # 테이블부터 아래는 다른 그림
    curve[:36, :76] = False                          # 왼쪽 위 '광대 잔고' 글자 칸
    curve[:36, 248:] = False                         # 오른쪽 위 'ROUND' 글자 칸
    # 떨어진 한 점(스케치 주석 번호·잡티)은 선이 아니다 → 주변 8칸에 선이 하나도 없으면 지움
    pad = np.pad(curve, 1)
    neighbours = sum(pad[1 + dy:181 + dy, 1 + dx:321 + dx] for dy in (-1, 0, 1) for dx in (-1, 0, 1)) - curve
    curve &= neighbours > 0
    idx[curve] = names.index("red0")
    return idx, np.ones((180, 320), bool)


def table(pal, grain):
    """테이블 사다리꼴. 위 끝 1줄 = 어두운 금색 모서리. grain=True면 원근 나뭇결 선 4개."""
    names = list(pal.keys())
    w, h = TABLE[2], TABLE[3]
    idx, mask = sprite(w, h)
    for r in range(h):
        left = round(30 - r * 30 / (h - 1))
        right = round(290 + r * 29 / (h - 1))
        put(idx, mask, r, slice(left, right + 1), "gold0" if r == 0 else "wood", names)
    if grain:
        for top_x in (80, 130, 190, 240):            # 위 끝 점에서 화면 가운데(160) 반대쪽으로 벌어지는 선
            for r in range(2, h, 1):
                gx = round(top_x + (top_x - 160) * r / 60)
                if 0 <= gx < w and mask[r, gx]:
                    put(idx, mask, r, gx, "bg1", names)
    return idx, mask


def candle(pal, color):
    names = list(pal.keys())
    w, h = CANDLE
    idx, mask = sprite(w, h)
    put(idx, mask, slice(0, 3), w // 2, color, names)          # 위 심지
    put(idx, mask, slice(3, 16), slice(None), color, names)    # 몸통
    put(idx, mask, slice(16, 18), w // 2, color, names)        # 아래 심지
    return idx, mask


def flame(pal, lit):
    """불꽃: 켜짐 = 주황 덩어리 + 밝은 노랑 심, 꺼짐 = 주황 테두리만."""
    names = list(pal.keys())
    shape = ["..#...",
             "..##..",
             ".###..",
             ".####.",
             "######",
             "######",
             "######",
             ".####.",
             "..##.."]
    idx, mask = sprite(*FLAME)
    for r, row in enumerate(shape):
        for c, ch in enumerate(row):
            if ch == "#":
                edge = any(not (0 <= r + dr < len(shape) and 0 <= c + dc < len(row)) or shape[r + dr][c + dc] != "#"
                           for dr, dc in ((1, 0), (-1, 0), (0, 1), (0, -1)))
                if lit or edge:
                    put(idx, mask, r, c, "orange", names)
    if lit:
        put(idx, mask, slice(5, 7), slice(2, 4), "gold2", names)   # 밝은 심
    return idx, mask


def clown_thinking(pal, up=2):
    """광대 '생각 중': 평소 그림(A)에서 눈동자(가운데 어두운 세로 칸)만 위로 up칸 올린다(위를 보며 생각)."""
    names = list(pal.keys())
    idx, mask = clown_base(pal)
    idx = idx.copy()
    dark = names.index("bg0")
    skin = names.index("cream")
    for px in (152 - CLOWN_BOX[0], 168 - CLOWN_BOX[0]):            # 두 눈동자 열(스케치 x 152, 168)
        col = [yy for yy in range(30, 50) if idx[yy, px] == dark and mask[yy, px]]
        for yy in col:
            idx[yy, px] = skin
        for yy in col:
            idx[yy - up, px] = dark
    return idx, mask


def make_play(pal):
    """Ticket 25 그림 시안 + 스케치 배치대로 합친 미리보기(4배) + 팔레트 검사."""
    names = list(pal.keys())
    DRAFTS.mkdir(parents=True, exist_ok=True)
    parts = {
        "room": room(pal),
        "table-A": table(pal, grain=False),
        "table-B": table(pal, grain=True),
        "terminal": frame(TERMINAL[2], TERMINAL[3], "navy", "bg0", names),
        "terminal-qbox": frame(QBOX[2], QBOX[3], "gold1", None, names),
        "leverage-box": frame(LEVER_BOX[2], LEVER_BOX[3], "orange", "bg1", names),
        "candle-up": candle(pal, "green"),
        "candle-down": candle(pal, "red2"),
        "flame-on": flame(pal, True),
        "flame-off": flame(pal, False),
        "clown-thinking": clown_thinking(pal),
        "clown-idle": clown_base(pal),
    }
    files = []
    for name, (idx, mask) in parts.items():
        p = DRAFTS / f"{name}.png"
        write_png(p, to_rgba(idx, mask, pal))
        files.append(p)

    def scene(table_name, clown_name):
        canvas = np.zeros((180, 320, 3), np.uint8)
        cols = np.array(list(pal.values()), np.uint8)

        def paste(name, x, y):
            idx, mask = parts[name]
            h, w = idx.shape
            reg = canvas[y:y + h, x:x + w]
            reg[mask] = cols[idx][mask]

        paste("room", 0, 0)
        paste(clown_name, CLOWN_BOX[0], CLOWN_BOX[1])
        paste(table_name, TABLE[0], TABLE[1])
        paste("terminal", TERMINAL[0], TERMINAL[1])
        paste("terminal-qbox", QBOX[0], QBOX[1])
        paste("candle-up", 122, 108)
        paste("candle-down", 132, 108)
        paste("leverage-box", LEVER_BOX[0], LEVER_BOX[1])
        for i, lit in enumerate((True, True, False)):
            paste("flame-on" if lit else "flame-off", 218 + i * 8, 118)
        return canvas

    gap = np.full((180, 4, 3), 255, np.uint8)
    top = np.concatenate([scene("table-A", "clown-idle"), gap, scene("table-B", "clown-thinking")], 1)
    write_png(DRAFTS / "play-preview-x2.png", top.repeat(2, 0).repeat(2, 1))
    write_png(DRAFTS / "play-preview-A-x4.png", scene("table-A", "clown-idle").repeat(4, 0).repeat(4, 1))
    print("팔레트 검사 (C1):")
    check(files, pal)


# ---------- 정식 UI 그림 (Ticket 26) ----------
# 위치·크기는 스케치 320x180에서 잰 값(아트 메모 7-3절). 글자(잔고 숫자·ROUND·버튼 이름·슬롯 번호)는 Canvas.
HUD_BOX = (66, 26)          # 오른쪽 위 'ROUND' 틀
HUD_BOX_WIDE = (88, 26)     # 왼쪽 위 '광대 잔고' 틀: 동전 10개(8도트 간격 = 80)가 들어가게 스케치(66)보다 넓힘
BUTTON = (64, 30)           # 보유·매도 버튼
SLOT = (19, 25)             # 아이템 슬롯
COIN = (7, 7)               # 동전(잔고 10개가 들어가게 스케치 9x9보다 작게, 제안 8)
ICON_REGIONS = {            # 스케치 슬롯 안 아이콘 자리(번호 글자 아래). (x, y, w, h)
    "item-chart": (233, 161, 17, 13),
    "item-rumor": (254, 161, 17, 13),
    "item-halt": (275, 161, 17, 13),
}


def coin(pal, kind):
    """동전 7x7. on = 금색, danger = 빨강(내 잔고 3 이하), off = 회색 점선 고리(잃은 잔고)."""
    names = list(pal.keys())
    shape = ["..###..",
             ".#####.",
             "###@###",
             "#######",
             "#######",
             ".#####.",
             "..###.."]
    rim, body, hi = {"on": ("gold0", "gold1", "gold2"), "danger": ("red0", "red2", "red3")}.get(kind, (None, None, None))
    idx, mask = sprite(*COIN)
    for r, row in enumerate(shape):
        for c, ch in enumerate(row):
            if ch == ".":
                continue
            edge = r in (0, 6) or c in (0, 6) or shape[r][c - 1] == "." or shape[r][c + 1 if c < 6 else c] == "."                 or shape[r - 1 if r else r][c] == "." or shape[r + 1 if r < 6 else r][c] == "."
            if kind == "off":
                if edge and (r + c) % 2 == 0:
                    put(idx, mask, r, c, "gray0", names)
            else:
                put(idx, mask, r, c, rim if edge else (hi if (r, c) in ((2, 2), (2, 3), (3, 2)) else body), names)
    return idx, mask


def button(pal, kind):
    """버튼 64x30. normal = 남색 1도트 테두리, selected = 금색 2도트 테두리 + 나무색 안, disabled = 회색 테두리."""
    names = list(pal.keys())
    w, h = BUTTON
    if kind == "selected":
        idx, mask = frame(w, h, "gold1", "wood", names)
        put(idx, mask, 1, slice(1, w - 1), "gold1", names)
        put(idx, mask, h - 2, slice(1, w - 1), "gold1", names)
        put(idx, mask, slice(1, h - 1), 1, "gold1", names)
        put(idx, mask, slice(1, h - 1), w - 2, "gold1", names)
        return idx, mask
    return frame(w, h, "gray0" if kind == "disabled" else "navy", "bg0", names)


def sketch_icon(pal, region):
    """스케치 슬롯 안의 아이콘(사용자 스케치)을 팔레트로 맞춰 떼어 낸다. 바탕(bg0·bg1)은 투명."""
    names = list(pal.keys())
    q = quantize(read_png(REF_PLAY)[..., :3], pal)
    x, y, w, h = region
    idx = q[y:y + h, x:x + w].copy()
    mask = ~np.isin(idx, [names.index("bg0"), names.index("bg1")])
    return idx, mask


def lottery_icon(pal):
    """복권(스케치 4번 칸은 비어 있어 새로 그림): 크림색 표 + 양옆 홈 + 가운데 빨간 별 점."""
    names = list(pal.keys())
    shape = ["..............",
             "..............",
             ".############.",
             ".#..........#.",
             "##...*..*...##",
             ".#....**....#.",
             ".#....**....#.",
             "##...*..*...##",
             ".#..........#.",
             ".############.",
             "..............",
             ".............."]
    idx, mask = sprite(17, 13)
    for r, row in enumerate(shape):
        for c, ch in enumerate(row):
            if ch == "#":
                put(idx, mask, r, c + 1, "gold1", names)
            elif ch == "*":
                put(idx, mask, r, c + 1, "red3", names)
            elif ch == "." and 2 < r < 9 and 1 < c < 12:
                put(idx, mask, r, c + 1, "cream", names)
    return idx, mask


def make_hud(pal):
    """Ticket 26 그림 + 플레이 화면 위에 합친 미리보기(4배) + 팔레트 검사."""
    names = list(pal.keys())
    parts = {
        "hud-box": frame(*HUD_BOX, "navy", "bg0", names),
        "hud-box-wide": frame(*HUD_BOX_WIDE, "navy", "bg0", names),
        "hud-bar": (lambda: (lambda i, m: (put(i, m, slice(None), slice(None), "bg0", names), put(i, m, 0, slice(None), "wood", names), (i, m))[-1])(*sprite(320, 36)))(),
        "button-normal": button(pal, "normal"),
        "button-selected": button(pal, "selected"),
        "button-disabled": button(pal, "disabled"),
        "slot-normal": frame(*SLOT, "navy", None, names),
        "slot-selected": frame(*SLOT, "gold2", None, names),
        "coin-on": coin(pal, "on"),
        "coin-danger": coin(pal, "danger"),
        "coin-off": coin(pal, "off"),
        "item-lottery": lottery_icon(pal),
    }
    for name, region in ICON_REGIONS.items():
        parts[name] = sketch_icon(pal, region)
    files = []
    for name, (idx, mask) in parts.items():
        p = DRAFTS / f"{name}.png"
        write_png(p, to_rgba(idx, mask, pal))
        files.append(p)

    # 미리보기: 25의 플레이 화면 + HUD (글자 없이 그림만, 위치는 스케치 그대로)
    cols = np.array(list(pal.values()), np.uint8)
    canvas = np.zeros((180, 320, 3), np.uint8)
    play = {n: read_png(DRAFTS / f"{n}.png") for n in ("room", "clown-idle", "table-A", "terminal", "terminal-qbox", "leverage-box")}

    def paste_png(im, x, y):
        h, w = im.shape[:2]
        reg = canvas[y:y + h, x:x + w]
        a = im[..., 3] > 0
        reg[a] = im[..., :3][a]

    def paste(name, x, y):
        idx, mask = parts[name]
        h, w = idx.shape
        reg = canvas[y:y + h, x:x + w]
        reg[mask] = cols[idx][mask]

    paste_png(play["room"], 0, 0)
    paste_png(play["clown-idle"], CLOWN_BOX[0], CLOWN_BOX[1])
    paste_png(play["table-A"], TABLE[0], TABLE[1])
    paste_png(play["terminal"], TERMINAL[0], TERMINAL[1])
    paste_png(play["terminal-qbox"], QBOX[0], QBOX[1])
    paste_png(play["leverage-box"], LEVER_BOX[0], LEVER_BOX[1])
    paste("hud-box-wide", 6, 6)                 # 광대 잔고
    for i in range(10):                         # 광대 동전 10개: 7개 남음
        paste("coin-on" if i < 7 else "coin-off", 9 + i * 8, 20)
    for i, n in enumerate(("item-chart", "item-lottery")):   # 광대 아이템(틀 아래)
        paste(n, 6 + i * 19, 34)
    paste("hud-box", 250, 6)                    # ROUND
    paste("hud-bar", 0, 144)
    for i in range(10):                         # 내 동전: 3개(위험) 남음
        paste("coin-danger" if i < 3 else "coin-off", 6 + i * 8, 165)
    paste("button-selected", 88, 147)
    paste("button-normal", 158, 147)
    for i, n in enumerate(("item-chart", "item-rumor", "item-halt", "item-lottery")):
        paste("slot-selected" if i == 0 else "slot-normal", 232 + i * 21, 150)
        paste(n, 233 + i * 21, 161)
    write_png(DRAFTS / "hud-preview-x4.png", canvas.repeat(4, 0).repeat(4, 1))
    print("팔레트 검사 (C1):")
    check(files, pal)


if __name__ == "__main__":
    palette = load_palette()
    if len(sys.argv) > 1 and sys.argv[1] == "hud":
        make_hud(palette)
        sys.exit(0)
    if len(sys.argv) > 1 and sys.argv[1] == "check":
        sys.exit(0 if check(sys.argv[2:], palette) else 1)
    if len(sys.argv) > 1 and sys.argv[1] == "play":
        make_play(palette)
    else:
        make_clown(palette)

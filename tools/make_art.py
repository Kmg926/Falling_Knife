"""Falling Knife 도트 그림 만들기 (AI 작성, ADR 0003 / formal-ui Ticket 24).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art.py clown     # 광대 시안 A·B·C + 미리보기 + 팔레트 검사
    python tools/make_art.py play      # Ticket 25 플레이 화면 그림 + 합친 미리보기
    python tools/make_art.py hud       # Ticket 26 정식 UI 그림(틀·버튼·슬롯·동전·아이콘) + 미리보기
    python tools/make_art.py closeup   # Ticket 27 아이템 확인 패널 그림 + 미리보기
    python tools/make_art.py result    # Ticket 28 결과 화면 그림 + 미리보기
    python tools/make_art.py title     # Ticket 29 타이틀·일시정지 그림 + 미리보기
    python tools/make_art.py revamp30  # UI 개편 30: 한 단계 어두운 광대·턴 띠
    python tools/make_art.py revamp31  # UI 개편 31: 캔들 뒷면
    python tools/make_art.py revamp32  # UI 개편 32: 아이템 회전 그림 4x8 + 그림자
    python tools/make_art.py clowntell # clown-tell 80: 말풍선 꼬리
    python tools/make_art.py skipkey   # intro-story 86: 건너뛰기 키캡 SPACE(평소·찬 것)
    python tools/make_art.py gear      # game-settings 91: 설정 톱니(평소 회색·올림 금색)
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


# ---------- 아이템 확인 패널 (Ticket 27) ----------
# 스케치 화면 3을 기준으로, 아래 띠(y 144~)와 선택 슬롯이 가려지지 않게 패널을 y 30~139로 올림(아트 메모 7-4절).
CLOSEUP_PANEL = (86, 30, 168, 110)   # x, y, w, h
ICON_BOX = (42, 42)
CATEGORY_TAG = (30, 12)


def big_icon(idx, mask):
    """작은 아이콘(17x13)을 2배로 키운 큰 아이콘(34x26). 확인 패널 전용 — 도트 하나가 2x2가 된다(스케치 화면 3과 같은 굵기)."""
    return idx.repeat(2, 0).repeat(2, 1), mask.repeat(2, 0).repeat(2, 1)


def dim_dither(names, w=320, h=144):
    """배경 어둡게: 한 칸 걸러 bg0 점(바둑판). 반투명 대신 점으로 덮어 팔레트 밖 색이 생기지 않는다."""
    idx, mask = sprite(w, h)
    yy, xx = np.mgrid[0:h, 0:w]
    on = (yy + xx) % 2 == 0
    idx[on] = names.index("bg0")
    mask[on] = True
    return idx, mask


def make_closeup(pal):
    """Ticket 27 그림: 패널 틀, 큰 아이콘 칸, 분류 표, 큰 아이콘 4개 + 플레이 화면 위 미리보기(4배) + 팔레트 검사."""
    names = list(pal.keys())
    icon_box_idx, icon_box_mask = frame(*ICON_BOX, "navy", "wood", names)
    tag_idx, tag_mask = frame(*CATEGORY_TAG, "navy", "navy", names)
    parts = {
        "closeup-panel": frame(CLOSEUP_PANEL[2], CLOSEUP_PANEL[3], "gold1", "bg0", names),
        "closeup-iconbox": (icon_box_idx, icon_box_mask),
        "closeup-tag": (tag_idx, tag_mask),
        "closeup-dim": dim_dither(names),
    }
    small = {"item-chart": sketch_icon(pal, ICON_REGIONS["item-chart"]),
             "item-rumor": sketch_icon(pal, ICON_REGIONS["item-rumor"]),
             "item-halt": sketch_icon(pal, ICON_REGIONS["item-halt"]),
             "item-lottery": lottery_icon(pal)}
    for name, (i, m) in small.items():
        parts[name + "-big"] = big_icon(i, m)
    files = []
    for name, (idx, mask) in parts.items():
        p = DRAFTS / f"{name}.png"
        write_png(p, to_rgba(idx, mask, pal))
        files.append(p)

    # 미리보기: HUD 미리보기 위에 어둡게(바둑판 무늬로 bg0를 반만 덮어 '어둡게' 흉내) + 패널
    base = read_png(DRAFTS / "hud-preview-x4.png")[::4, ::4, :3].copy()
    cols = np.array(list(pal.values()), np.uint8)
    yy, xx = np.mgrid[0:144, 0:320]
    base[:144][(yy + xx) % 2 == 0] = pal["bg0"].astype(np.uint8)

    def paste(name, x, y):
        idx, mask = parts[name]
        h, w = idx.shape
        reg = base[y:y + h, x:x + w]
        reg[mask] = cols[idx][mask]

    px, py, pw, ph = CLOSEUP_PANEL
    paste("closeup-panel", px, py)
    bx = px + (pw - ICON_BOX[0]) // 2
    paste("closeup-iconbox", bx, py + 5)
    paste("item-chart-big", bx + 4, py + 5 + 8)
    paste("closeup-tag", px + (pw - CATEGORY_TAG[0]) // 2, py + 62)
    write_png(DRAFTS / "closeup-preview-x4.png", base.repeat(4, 0).repeat(4, 1))
    print("팔레트 검사 (C1):")
    check(files, pal)


# ---------- 결과 화면 (Ticket 28) ----------
# 스케치 화면 4(파산)·5(생존)에서 잰 배치(아트 메모 7-5절). 글자(파산·생존·원인·기록·버튼 이름)는 Canvas.
RESULT_BOX = (200, 57)       # 원인/결정타·기록 상자, (20, 84)
RETRY_BUTTON = (72, 21)      # 다시하기, (20, 150)
TITLE_BUTTON = (64, 21)      # 타이틀, (98, 150)


def result_background(names, bankrupt):
    """결과 화면 바탕 320x180(불투명). 파산 = bg0 위에 bg1 사선 줄무늬(붉은 어둠), 생존 = bg0만."""
    idx = np.full((180, 320), names.index("bg0"))
    if bankrupt:
        yy, xx = np.mgrid[0:180, 0:320]
        idx[(xx + yy) % 6 == 0] = names.index("bg1")
    return idx, np.ones((180, 320), bool)


def result_box(names, border):
    """원인·기록 상자: 1도트 테두리 + 안 bg0 + 가운데 가로 구분선(wood, 위 원인 / 아래 기록)."""
    idx, mask = frame(*RESULT_BOX, border, "bg0", names)
    put(idx, mask, 30, slice(4, RESULT_BOX[0] - 4), "wood", names)
    return idx, mask


def clown_lights_off(pal):
    """생존 화면 광대: 평소 그림을 두 단계 어둡게(스케치 화면 5 '조명이 꺼져 어둡게 남음')."""
    names = list(pal.keys())
    idx, mask = clown_base(pal)
    darker = dict(DARKER)
    darker.update({"navy": "bg1", "gray0": "bg1", "red0": "bg1", "wood": "bg1", "gold0": "wood"})
    out = idx.copy()
    for _ in range(2):
        for y in range(out.shape[0]):
            for x in range(out.shape[1]):
                n = names[out[y, x]]
                if mask[y, x] and n in darker:
                    out[y, x] = names.index(darker[n])
    return out, mask


def make_result(pal):
    """Ticket 28 그림 + 파산·생존 미리보기(4배) + 팔레트 검사."""
    names = list(pal.keys())
    retry = frame(*RETRY_BUTTON, "gold1", "wood", names)
    parts = {
        "result-bg-bankrupt": result_background(names, True),
        "result-bg-survive": result_background(names, False),
        "result-box-bankrupt": result_box(names, "red0"),
        "result-box-survive": result_box(names, "gray0"),
        "result-button-retry": retry,
        "result-button-title": frame(*TITLE_BUTTON, "navy", "bg0", names),
        "clown-dark": clown_lights_off(pal),
    }
    files = []
    for name, (idx, mask) in parts.items():
        p = DRAFTS / f"{name}.png"
        write_png(p, to_rgba(idx, mask, pal))
        files.append(p)

    cols = np.array(list(pal.values()), np.uint8)

    def screen(bankrupt):
        canvas = np.zeros((180, 320, 3), np.uint8)

        def paste(name, x, y, src=None):
            idx, mask = src if src is not None else parts[name]
            h, w = idx.shape
            reg = canvas[y:y + h, x:x + w]
            reg[mask] = cols[idx][mask]

        paste("result-bg-bankrupt" if bankrupt else "result-bg-survive", 0, 0)
        paste("result-box-bankrupt" if bankrupt else "result-box-survive", 20, 84)
        paste("result-button-retry", 20, 150)
        paste("result-button-title", 98, 150)
        paste(None, 216, 52, clown_base(pal) if bankrupt else parts["clown-dark"])
        return canvas

    gap = np.full((180, 4, 3), 255, np.uint8)
    both = np.concatenate([screen(True), gap, screen(False)], 1)
    write_png(DRAFTS / "result-preview-x2.png", both.repeat(2, 0).repeat(2, 1))
    print("팔레트 검사 (C1):")
    check(files, pal)


# ---------- 타이틀·일시정지 (Ticket 29) ----------
# 스케치 화면 1(타이틀)에서 잰 배치(아트 메모 7-6절). 로고·부제·목표·조작 글·버튼 이름은 Canvas.
REF_TITLE = ROOT / ".scratch" / "art" / "reference" / "sketch-title-320.png"
TITLE_GRAPH_BOX = (14, 18, 172, 30)     # 떨어지는 빨간 그래프 자리 (x, y, w, h)
CONTROLS_BOX = (140, 36)                # 조작 요약 상자, (22, 122)
START_BUTTON = (70, 15)                 # Space 시작, (22, 162)
PAUSE_PANEL = (120, 72)                 # 일시정지 패널, (100, 50)


def title_graph(pal):
    """사용자 스케치의 떨어지는 빨간 그래프(화살표 포함)를 팔레트로 맞춰 떼어 낸다. 빨강 계열만 남김."""
    names = list(pal.keys())
    q = quantize(read_png(REF_TITLE)[..., :3], pal)
    x, y, w, h = TITLE_GRAPH_BOX
    idx = q[y:y + h, x:x + w].copy()
    mask = np.isin(idx, [names.index(n) for n in ("red0", "red1", "red2", "red3")])
    idx[mask] = names.index("red2")
    return idx, mask


def make_title(pal):
    """Ticket 29 그림 + 타이틀·일시정지 미리보기(2배) + 팔레트 검사."""
    names = list(pal.keys())
    parts = {
        "title-graph": title_graph(pal),
        "title-controls-box": frame(*CONTROLS_BOX, "navy", "bg0", names),
        "title-start-button": frame(*START_BUTTON, "gold1", "wood", names),
        "pause-dim": dim_dither(names, 320, 180),
        "pause-panel": frame(*PAUSE_PANEL, "gold1", "bg0", names),
    }
    files = []
    for name, (idx, mask) in parts.items():
        p = DRAFTS / f"{name}.png"
        write_png(p, to_rgba(idx, mask, pal))
        files.append(p)

    cols = np.array(list(pal.values()), np.uint8)

    def paste(canvas, src, x, y):
        idx, mask = src
        h, w = idx.shape
        reg = canvas[y:y + h, x:x + w]
        reg[mask] = cols[idx][mask]

    title = np.tile(pal["bg0"].astype(np.uint8), (180, 320, 1))
    paste(title, parts["title-graph"], TITLE_GRAPH_BOX[0], TITLE_GRAPH_BOX[1])
    paste(title, parts["title-controls-box"], 22, 122)
    paste(title, parts["title-start-button"], 22, 162)
    paste(title, clown_base(pal), 205, 52)

    pause = read_png(DRAFTS / "hud-preview-x4.png")[::4, ::4, :3].copy()
    paste(pause, parts["pause-dim"], 0, 0)
    paste(pause, parts["pause-panel"], 100, 50)
    retry = read_png(DRAFTS / "result-button-retry.png")
    titleb = read_png(DRAFTS / "result-button-title.png")
    a = retry[..., 3] > 0
    pause[70:70 + retry.shape[0], 124:124 + retry.shape[1]][a] = retry[..., :3][a]
    b = titleb[..., 3] > 0
    pause[96:96 + titleb.shape[0], 128:128 + titleb.shape[1]][b] = titleb[..., :3][b]

    gap = np.full((180, 4, 3), 255, np.uint8)
    write_png(DRAFTS / "title-pause-preview-x2.png", np.concatenate([title, gap, pause], 1).repeat(2, 0).repeat(2, 1))
    print("팔레트 검사 (C1):")
    check(files, pal)


# ---------- UI 개편 30: 조명·턴 띠 ----------
def clown_dim(pal, steps=1):
    """내 차례의 광대: 평소 그림을 steps 단계 어둡게(조명이 광대에게서 비켜남, ui-revamp 제안 4)."""
    names = list(pal.keys())
    idx, mask = clown_base(pal)
    darker = dict(DARKER)
    darker.update({"navy": "bg1", "gray0": "bg1", "red0": "bg1", "wood": "bg1", "gold0": "wood"})
    out = idx.copy()
    for _ in range(steps):
        for y in range(out.shape[0]):
            for x in range(out.shape[1]):
                n = names[out[y, x]]
                if mask[y, x] and n in darker:
                    out[y, x] = names.index(darker[n])
    return out, mask


def make_revamp30(pal):
    """ui-revamp Ticket 30 그림: 한 단계 어두운 광대, 턴 띠(바둑판 320x16) + 팔레트 검사."""
    names = list(pal.keys())
    parts = {"clown-dim": clown_dim(pal, 1), "turn-banner": dim_dither(names, 320, 16)}
    files = []
    for name, (idx, mask) in parts.items():
        p = DRAFTS / f"{name}.png"
        write_png(p, to_rgba(idx, mask, pal))
        files.append(p)
    print("팔레트 검사 (C1):")
    check(files, pal)


# ---------- UI 개편 31: 캔들 뒷면 ----------
def candle_back(pal):
    """덱 공개 연출에서 뒤집힌 캔들 5x18: 남색 몸통 + 회색 테두리 + 가운데 금색 점(어느 쪽인지 모름)."""
    names = list(pal.keys())
    w, h = CANDLE
    idx, mask = frame(w, h, "gray0", "navy", names)
    put(idx, mask, slice(8, 10), 2, "gold0", names)
    return idx, mask


def make_revamp31(pal):
    files = []
    for name, part in {"candle-back": candle_back(pal)}.items():
        p = DRAFTS / f"{name}.png"
        write_png(p, to_rgba(*part, pal))
        files.append(p)
    print("팔레트 검사 (C1):")
    check(files, pal)


# ---------- UI 개편 32: 아이템 회전 그림(유사 3D) ----------
SPIN_FRAMES = 8          # 45° 간격 한 바퀴
SPIN_THICK = 3           # 아이콘 두께(도트)
SPIN_W = 20              # 회전 그림 캔버스 너비(아이콘 17 + 두께 여유)


def spin_frames(pal, idx, mask):
    """아이콘을 두께 SPIN_THICK 도트짜리 물체로 세우고 Y축으로 돌린 SPIN_FRAMES장(정사영, 무작위 없음).
    앞을 향한 면 = 원래 색, 옆으로 보이는 면 = 한 단계 어두운 색(DARKER) → 두께가 느껴진다."""
    names = list(pal.keys())
    h, w = idx.shape
    cx = w // 2                                           # 정수 중심(0°에서 도트가 반 칸에 걸리지 않게)
    frames = []
    for f in range(SPIN_FRAMES):
        a = np.radians(f * 360 / SPIN_FRAMES)
        ca, sa = np.cos(a), np.sin(a)
        out_i, out_m = sprite(SPIN_W, h)
        depth = np.full((h, SPIN_W), -1e9)
        front = 1 if ca >= 0 else -1                       # 지금 카메라를 향한 판
        for y in range(h):
            for x in range(w):
                if not mask[y, x]:
                    continue
                for z in range(-(SPIN_THICK // 2), SPIN_THICK // 2 + 1):
                    px = (x - cx) * ca - z * sa             # 회전 뒤 가로 위치
                    pz = (x - cx) * sa + z * ca             # 회전 뒤 깊이(클수록 앞)
                    sx = int(np.floor(px + 0.5)) + SPIN_W // 2   # 반올림을 한 방향으로(빈 줄무늬 방지)
                    if not 0 <= sx < SPIN_W or pz <= depth[y, sx]:
                        continue
                    depth[y, sx] = pz
                    n = names[idx[y, x]]
                    facing = (z == front * (SPIN_THICK // 2)) and abs(ca) > 0.35
                    color = n if facing else DARKER.get(n, n)
                    out_i[y, sx] = names.index(color)
                    out_m[y, sx] = True
        frames.append((out_i, out_m))
    return frames


def make_revamp32(pal):
    """아이템 4종 x 회전 8장(2배 = 40x26) + 그림자 + 미리보기 시트 + 팔레트 검사."""
    names = list(pal.keys())
    icons = {"chart": sketch_icon(pal, ICON_REGIONS["item-chart"]),
             "rumor": sketch_icon(pal, ICON_REGIONS["item-rumor"]),
             "halt": sketch_icon(pal, ICON_REGIONS["item-halt"]),
             "lottery": lottery_icon(pal)}
    files = []
    sheet_rows = []
    for name, (i, m) in icons.items():
        row = []
        for k, (fi, fm) in enumerate(spin_frames(pal, i, m)):
            bi, bm = big_icon(fi, fm)
            p = DRAFTS / f"spin-{name}-{k}.png"
            write_png(p, to_rgba(bi, bm, pal))
            files.append(p)
            row.append((bi, bm))
        sheet_rows.append(row)
    shadow = dim_dither(names, 32, 3)
    write_png(DRAFTS / "spin-shadow.png", to_rgba(*shadow, pal))
    files.append(DRAFTS / "spin-shadow.png")

    cols = np.array(list(pal.values()), np.uint8)
    H, W = 26, 40
    sheet = np.tile(pal["wood"].astype(np.uint8), (len(sheet_rows) * (H + 4), SPIN_FRAMES * (W + 4), 1))
    for r, row in enumerate(sheet_rows):
        for c, (bi, bm) in enumerate(row):
            reg = sheet[r * (H + 4):r * (H + 4) + H, c * (W + 4):c * (W + 4) + W]
            reg[bm] = cols[bi][bm]
    write_png(DRAFTS / "spin-preview-x3.png", sheet.repeat(3, 0).repeat(3, 1))
    print("팔레트 검사 (C5):")
    check(files, pal)


def speech_tail(names):
    """clown-tell 80: 말풍선 꼬리(오른쪽 = 광대 쪽을 가리킴), 4x7.
    왼쪽 줄은 창 안쪽 색(bg0)이라 창의 오른쪽 테두리 위에 겹쳐 놓으면 틀과 꼬리가 한 덩어리로 이어진다.
    줄 r의 길이 e = 3 - |r - 3| → 0..e-1 = bg0, e = gold1(테두리, 창 테두리와 같은 색)."""
    idx, mask = sprite(4, 7)
    for r in range(7):
        e = 3 - abs(r - 3)
        if e > 0:
            put(idx, mask, r, slice(0, e), "bg0", names)
        put(idx, mask, r, e, "gold1", names)
    return idx, mask


def make_clowntell(pal):
    """clown-tell 80 그림: 말풍선 꼬리 + 미리보기(창 테두리와 이어지는지, 8배)."""
    names = list(pal.keys())
    idx, mask = speech_tail(names)
    p = DRAFTS / "speech-tail.png"
    write_png(p, to_rgba(idx, mask, pal))
    # 미리보기: 20x9 창(1도트 금색 테두리) 오른쪽 테두리 위에 꼬리를 겹침
    box_i, box_m = frame(20, 9, "gold1", "bg0", names)
    canvas_i, canvas_m = sprite(24, 9)
    canvas_i[:, :20], canvas_m[:, :20] = box_i, box_m
    canvas_i[1:8, 19:23][mask] = idx[mask]
    canvas_m[1:8, 19:23] |= mask
    rgba = to_rgba(canvas_i, canvas_m, pal)
    write_png(DRAFTS / "speech-tail-preview-x8.png", rgba.repeat(8, 0).repeat(8, 1))
    print("팔레트 검사:")
    check([p], pal)


# intro-story 86: 건너뛰기 키캡 "SPACE"(3×5 글자) — 평소 그림 + 금색으로 찬 그림(게임에서 왼→오 채움으로 겹침)
LETTERS35 = {
    "S": ["111", "100", "111", "001", "111"], "P": ["111", "101", "111", "100", "100"],
    "A": ["010", "101", "111", "101", "101"], "C": ["111", "100", "100", "100", "111"],
    "E": ["111", "100", "111", "100", "111"],
}


def skip_key(names, filled):
    """27×11 키캡: 윗면(0~8줄) + 앞 옆면(9~10줄, 눌리는 키 두께). 모서리 1칸 깎음.
    평소: 테두리 gray0·윗면 bg0·위 빛 gray0 한 줄·글자 cream / 찬 것: 윗면 gold1·위 빛 gold2·글자 bg00·옆면 goldD."""
    w, h = 27, 11
    idx, mask = sprite(w, h)
    put(idx, mask, slice(0, 9), slice(None), "gold1" if filled else "bg0", names)
    put(idx, mask, slice(9, 11), slice(None), "goldD" if filled else "gray0", names)     # 앞 옆면
    put(idx, mask, 10, slice(None), "gold0" if filled else "bg00", names)
    put(idx, mask, 1, slice(1, w - 1), "gold2" if filled else "gray0", names)            # 위 빛
    for ys, xs in ((0, slice(None)), (slice(0, 9), 0), (slice(0, 9), w - 1)):           # 테두리
        put(idx, mask, ys, xs, "gold2" if filled else "gray0", names)
    for y, x in ((0, 0), (0, w - 1), (10, 0), (10, w - 1)):                              # 모서리 깎기
        mask[y, x] = False
    x = 4
    for ch in "SPACE":
        for r, row in enumerate(LETTERS35[ch]):
            for c, v in enumerate(row):
                if v == "1":
                    put(idx, mask, 3 + r, x + c, "bg00" if filled else "cream", names)
        x += 4
    return idx, mask


def make_skipkey(pal):
    names = list(pal.keys())
    files = []
    for name, filled in (("skip-key", False), ("skip-key-fill", True)):
        idx, mask = skip_key(names, filled)
        p = DRAFTS / f"{name}.png"
        write_png(p, to_rgba(idx, mask, pal))
        files.append(p)
    # 미리보기(8배): 평소 / 반쯤 참 / 다 참
    a, b = [to_rgba(*skip_key(names, f), pal) for f in (False, True)]
    half = a.copy()
    half[:, :14] = b[:, :14]
    gap = np.zeros((11, 3, 4), np.uint8)
    pv = np.concatenate([a, gap, half, gap, b], 1)
    bgc = np.zeros_like(pv); bgc[..., :3] = pal["bg0"]; bgc[..., 3] = 255
    al = pv[..., 3:4] / 255.0
    bgc[..., :3] = (pv[..., :3] * al + bgc[..., :3] * (1 - al)).astype(np.uint8)
    write_png(DRAFTS / "skip-key-preview-x8.png", bgc.repeat(8, 0).repeat(8, 1))
    print("팔레트 검사:")
    check(files, pal)


def gear_icon(names, hover):
    """game-settings 91: 설정 톱니 22×22(지금 좌표 11×11). 이빨 8개 + 몸통 링 + 가운데 구멍.
    평소 = 회색(gray1 몸통·cream0 위 빛·gray0 테두리), 올림(hover) = 금색(gold1·gold2·gold0). 밝은 바탕이 없어 어두운 테두리로 떼어 냄."""
    n = 22
    idx, mask = sprite(n, n)

    def paint(m, color):          # 2D 참/거짓 칸에 한 색(put은 줄·칸을 따로 받아서)
        idx[m] = names.index(color)
        mask[m] = True
    yy, xx = np.mgrid[0:n, 0:n] + 0.5
    cx = cy = n / 2
    r = np.hypot(xx - cx, yy - cy)
    ang = np.arctan2(yy - cy, xx - cx)
    tooth = (np.cos(8 * ang) > 0.35) & (r <= 10.2)              # 이빨: 각도마다 바깥 반지름이 커짐(8개)
    body = (r <= 7.6) | tooth
    hole = r <= 3.0
    shape = body & ~hole
    base, light, edge_c = ("gold1", "gold2", "gold0") if hover else ("gray1", "cream0", "gray0")
    paint(shape, base)
    paint(shape & (xx + yy < n - 4), light)      # 왼쪽 위 빛
    # 테두리: 모양 바깥 1칸 + 구멍 둘레
    m = shape.copy()
    out = np.zeros_like(m)
    out[1:] |= m[:-1]; out[:-1] |= m[1:]; out[:, 1:] |= m[:, :-1]; out[:, :-1] |= m[:, 1:]
    rim = (out & ~m & ~hole) | (shape & (r <= 3.9))
    paint(rim, edge_c)
    return idx, mask


def make_gear(pal):
    names = list(pal.keys())
    files = []
    for name, hover in (("gear", False), ("gear-hover", True)):
        idx, mask = gear_icon(names, hover)
        p = DRAFTS / f"{name}.png"
        write_png(p, to_rgba(idx, mask, pal))
        files.append(p)
    a, b = [to_rgba(*gear_icon(names, h), pal) for h in (False, True)]
    pv = np.concatenate([a, np.zeros((22, 4, 4), np.uint8), b], 1)
    bgc = np.zeros_like(pv); bgc[..., :3] = pal["bg0"]; bgc[..., 3] = 255
    al = pv[..., 3:4] / 255.0
    bgc[..., :3] = (pv[..., :3] * al + bgc[..., :3] * (1 - al)).astype(np.uint8)
    write_png(DRAFTS / "gear-preview-x8.png", bgc.repeat(8, 0).repeat(8, 1))
    print("팔레트 검사:")
    check(files, pal)


if __name__ == "__main__":
    palette = load_palette()
    if len(sys.argv) > 1 and sys.argv[1] == "gear":
        make_gear(palette)
        sys.exit(0)
    if len(sys.argv) > 1 and sys.argv[1] == "skipkey":
        make_skipkey(palette)
        sys.exit(0)
    if len(sys.argv) > 1 and sys.argv[1] == "clowntell":
        make_clowntell(palette)
        sys.exit(0)
    if len(sys.argv) > 1 and sys.argv[1] == "revamp32":
        make_revamp32(palette)
        sys.exit(0)
    if len(sys.argv) > 1 and sys.argv[1] == "revamp31":
        make_revamp31(palette)
        sys.exit(0)
    if len(sys.argv) > 1 and sys.argv[1] == "revamp30":
        make_revamp30(palette)
        sys.exit(0)
    if len(sys.argv) > 1 and sys.argv[1] == "title":
        make_title(palette)
        sys.exit(0)
    if len(sys.argv) > 1 and sys.argv[1] == "result":
        make_result(palette)
        sys.exit(0)
    if len(sys.argv) > 1 and sys.argv[1] == "closeup":
        make_closeup(palette)
        sys.exit(0)
    if len(sys.argv) > 1 and sys.argv[1] == "hud":
        make_hud(palette)
        sys.exit(0)
    if len(sys.argv) > 1 and sys.argv[1] == "check":
        sys.exit(0 if check(sys.argv[2:], palette) else 1)
    if len(sys.argv) > 1 and sys.argv[1] == "play":
        make_play(palette)
    else:
        make_clown(palette)

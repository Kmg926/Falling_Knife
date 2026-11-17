"""Falling Knife 광대 2배 그림 (AI 작성, clown-idle Ticket 45).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art_clown_hd.py draft   # 지금 광대 4종(1배) → 2배 시안 + 비교 그림 + 팔레트 검사
  (사용자가 시안을 고른 뒤 시안 png 4장을 같은 이름으로 Assets에 복사해 씀 — 2026-11-18. 이후 다시 돌리면 이미 2배인 그림을 또 늘리므로 입력은 git의 1배 판)

- 입력: Assets/_Project/Sprites/Clown/clown-idle·thinking·dim.png, Result/clown-dark.png (지금 쓰는 1배 96x96)
- 방법: ① Scale2x(EPX)로 192x192 — 새 색을 만들지 않고 대각선만 매끄럽게, 윤곽은 그대로
        ② 넓은 색 구역(머리카락·얼굴·옷)에만 빛(왼쪽 위 가장자리 = 한 단계 밝게)·그늘(오른쪽 아래 가장자리 = 한 단계 어둡게)
        ③ (머리카락 결은 1차 시안에서 뜨개질처럼 보여 끔 — HAIR_MIN)
        눈·코·나비넥타이 같은 작은 부분은 그대로(표정 유지, Spec 제안 5).
- 32색, 무작위 없음. 출력: .scratch/art/drafts/hd/clown/
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import read_png, write_png  # noqa: E402
from make_art_hd import Canvas, check32, load_pal  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
SPR = ROOT / "Assets" / "_Project" / "Sprites"
OUT = ROOT / ".scratch" / "art" / "drafts" / "hd" / "clown"
SOURCES = {"clown-idle": "Clown/clown-idle", "clown-thinking": "Clown/clown-thinking",
           "clown-dim": "Clown/clown-dim", "clown-dark": "Result/clown-dark"}

LIGHT = {"cream": "cream2", "cream0": "cream", "gray0": "gray1", "gray1": "cream0",
         "red0": "red1", "red1": "red2", "red2": "red3", "navy0": "navy", "navy": "navy2",
         "bg1": "red0", "bg0": "navy0"}
DARK = {"cream2": "cream", "cream": "cream0", "cream0": "gray1", "gray1": "gray0", "gray0": "bg0",
        "red3": "red2", "red2": "red1", "red1": "red0", "red0": "bg1", "navy2": "navy", "navy": "navy0",
        "bg1": "bg00", "navy0": "bg00"}
BIG = 600            # 2배에서 이만큼 넘는 색 구역만 명암(작은 부분 = 표정은 그대로)
HAIR_MIN = 10 ** 9   # 머리카락 결은 끔: 옷 빨간 줄무늬가 머리카락과 같은 색이라 같이 무늬가 생기고, 뜨개질처럼 보였음(시안 1차)


def to_index(rgba, pal):
    """그림 → 팔레트 번호(투명 = -1)."""
    cols = np.array(list(pal.values()), int)
    idx = np.full(rgba.shape[:2], -1)
    m = rgba[..., 3] > 0
    d = ((rgba[..., :3][m][:, None, :].astype(int) - cols[None]) ** 2).sum(-1)
    idx[m] = d.argmin(-1)
    return idx


def scale2x(idx):
    """EPX/Scale2x: 픽셀 하나 → 2x2. 이웃이 같은 색으로 이어지는 대각선만 채워 계단을 부드럽게."""
    p = np.pad(idx, 1, constant_values=-1)
    P = p[1:-1, 1:-1]
    A, B, C, D = p[:-2, 1:-1], p[1:-1, 2:], p[1:-1, :-2], p[2:, 1:-1]   # 위·오른쪽·왼쪽·아래
    e0 = np.where((C == A) & (C != D) & (A != B), A, P)
    e1 = np.where((A == B) & (A != C) & (B != D), B, P)
    e2 = np.where((D == C) & (D != B) & (C != A), C, P)
    e3 = np.where((B == D) & (B != A) & (D != C), D, P)
    h, w = idx.shape
    out = np.empty((h * 2, w * 2), int)
    out[0::2, 0::2], out[0::2, 1::2], out[1::2, 0::2], out[1::2, 1::2] = e0, e1, e2, e3
    return out


def shifted(a, dy, dx, fill=-2):
    out = np.full_like(a, fill)
    h, w = a.shape
    ys, yd = slice(max(-dy, 0), h - max(dy, 0)), slice(max(dy, 0), h - max(-dy, 0))
    xs, xd = slice(max(-dx, 0), w - max(dx, 0)), slice(max(dx, 0), w - max(-dx, 0))
    out[yd, xd] = a[ys, xs]
    return out


def detail(idx, names):
    """넓은 구역에 빛·그늘 + 머리카락 결. 원본 idx는 안 바꾸고 새 배열을 돌려줌."""
    out = idx.copy()
    vals, cnt = np.unique(idx[idx >= 0], return_counts=True)
    big = {int(v) for v, c in zip(vals, cnt) if c >= BIG}
    up, left = shifted(idx, 1, 0), shifted(idx, 0, 1)            # 위·왼쪽 이웃 값
    down, right = shifted(idx, -1, 0), shifted(idx, 0, -1)       # 아래·오른쪽 이웃 값
    for v in big:
        n = names[v]
        region = idx == v
        lit = region & ((up != v) | (left != v))
        shade = region & ((down != v) | (right != v)) & ~lit
        if n in LIGHT:
            out[lit] = names.index(LIGHT[n])
        if n in DARK:
            out[shade] = names.index(DARK[n])
    # 머리카락 결: 가장 넓은 구역 안쪽(가장자리 제외)에 가는 사선, 위쪽으로 갈수록 성기게
    v = int(vals[cnt.argmax()])
    if cnt.max() >= HAIR_MIN and names[v] in DARK:
        yy, xx = np.mgrid[0:idx.shape[0], 0:idx.shape[1]]
        inner = (idx == v) & (up == v) & (left == v) & (down == v) & (right == v)
        strand = ((xx * 2 + yy) % 13 == 0) | ((xx * 2 - yy) % 17 == 0)
        out[inner & strand & ((yy // 2) % 5 != 0)] = names.index(DARK[names[v]])
    return out


def to_canvas(idx, pal):
    cv = Canvas(pal, idx.shape[1], idx.shape[0])
    m = idx >= 0
    cv.idx[m] = idx[m]
    cv.mask[:] = m
    return cv


def make_draft(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    names = list(pal)
    files, row_old, row_new = [], [], []
    for key, src in SOURCES.items():
        old = read_png(SPR / f"{src}.png")
        idx = detail(scale2x(to_index(old, pal)), names)
        cv = to_canvas(idx, pal)
        p = OUT / f"{key}.png"
        write_png(p, cv.rgba())
        files.append(p)
        row_old.append(old)
        row_new.append(cv.rgba())
    # 비교 그림: 위 = 지금(1배를 2배로 그냥 늘림), 아래 = 새 2배. 바탕은 방 색
    bg = pal["bg0"].astype(np.uint8)
    sheet = np.tile(bg, (2 * 196 + 8, 4 * 196, 1))
    for i, (o, n) in enumerate(zip(row_old, row_new)):
        big_old = o.repeat(2, 0).repeat(2, 1)
        for img, y in ((big_old, 2), (n, 198 + 6)):
            m = img[..., 3] > 0
            reg = sheet[y:y + 192, i * 196 + 2:i * 196 + 194]
            reg[m] = img[..., :3][m]
    write_png(OUT / "clown-compare-x2.png", sheet.repeat(2, 0).repeat(2, 1))
    print("팔레트 검사 (32색):")
    return check32(files, pal)


# ---------- Ticket 46: 숨쉬기·눈 그림 ----------
CHIN_Y = 136                 # 이 줄부터 아래(턱 아래 = 목·나비넥타이·몸)가 숨쉬기로 내려감(2배 그림 기준)
EYES = [(73, 63, 90, 87), (105, 63, 122, 87)]   # 왼쪽·오른쪽 눈 마름모 상자(x0, y0, x1, y1), 눈동자 2x6이 가운데
PUPIL_DROP = 5               # 힐끗: 눈동자를 아래(플레이어 쪽)로 5도트


def breathe(idx, k):
    """턱 아래를 k도트 내림: 위쪽 빈 줄은 CHIN_Y 줄을 늘여 채움(맨 아래는 모니터 뒤라 잘려도 안 보임)."""
    out = idx.copy()
    if k > 0:
        out[CHIN_Y + k:] = idx[CHIN_Y:-k]
        out[CHIN_Y:CHIN_Y + k] = idx[CHIN_Y]
    return out


def eye_interior(idx, names, box):
    """눈 상자 안에서 남색 테두리로 둘러싸인 안쪽(눈동자 포함) — 가운데에서 남색이 아닌 칸으로 번짐."""
    x0, y0, x1, y1 = box
    sub = idx[y0:y1, x0:x1]
    wall = np.isin(sub, [names.index(n) for n in ("navy", "navy0", "navy2", "gray0", "bg00")]) | (sub < 0)
    seen = np.zeros_like(wall)
    stack = [((y1 - y0) // 2, (x1 - x0) // 2)]
    while stack:
        y, x = stack.pop()
        if not (0 <= y < sub.shape[0] and 0 <= x < sub.shape[1]) or seen[y, x] or wall[y, x]:
            continue
        seen[y, x] = True
        stack += [(y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)]
    full = np.zeros(idx.shape, bool)
    full[y0:y1, x0:x1] = seen
    return full


FOREHEAD = (96, 52)          # 이마 가운데(x, y): 얼굴 바탕색을 여기서 읽음(밝은 = cream, 어두운 = gray1)


def eye_overlays(idx, names, inners):
    """눈 덮개 2장(깜빡임·힐끗): 눈 안쪽만 칠한 투명 그림. 색은 그 그림(밝은·어두운)에서 가져옴.
    inners = 밝은 그림에서 찾은 눈 안쪽 모양(어두운 그림은 색만 다르고 모양이 같아 그대로 씀 —
    어두운 그림의 테두리 색으로 다시 찾으면 테두리를 넘어 번졌음, 시안 1차)."""
    blink = np.full(idx.shape, -1)
    glance = np.full(idx.shape, -1)
    pupil = names.index("bg0")
    face = int(idx[FOREHEAD[1], FOREHEAD[0]])
    for inner in inners:
        lid = names.index(DARK.get(names[face], names[face]))
        line = names.index(DARK.get(names[lid], names[lid]))
        ys, xs = np.nonzero(inner)
        mid = (ys.min() + ys.max()) // 2
        blink[inner] = lid                                      # 감은 눈꺼풀
        blink[inner & (np.arange(idx.shape[0])[:, None] == mid)] = line   # 가운데 감은 줄
        glance[inner] = face                                    # 눈동자 지우고
        pys, pxs = np.nonzero(inner & (idx == pupil))
        for y, x in zip(pys, pxs):                              # 같은 모양 눈동자를 아래로
            if inner[min(y + PUPIL_DROP, idx.shape[0] - 1), x]:
                glance[y + PUPIL_DROP, x] = pupil
    return blink, glance


def make_idle(pal):
    """사용자가 고른 2배 광대(Assets에 있는 것)로 숨쉬기 둘째 장(1도트 내림)과 눈 덮개(밝은·어두운) + 비교 그림."""
    OUT.mkdir(parents=True, exist_ok=True)
    names = list(pal)
    files = []
    rows = []
    lit = to_index(read_png(SPR / "Clown/clown-idle.png"), pal)
    inners = [eye_interior(lit, names, box) for box in EYES]   # 눈 안쪽 모양은 밝은 그림에서 한 번
    for key, src in (("clown-idle", "Clown/clown-idle"), ("clown-thinking", "Clown/clown-thinking"), ("clown-dim", "Clown/clown-dim")):
        idx = to_index(read_png(SPR / f"{src}.png"), pal)
        b = breathe(idx, 1)
        p = OUT / f"{key}-b.png"
        write_png(p, to_canvas(b, pal).rgba())
        files.append(p)
        row = [idx, b]
        if key != "clown-thinking":
            blink, glance = eye_overlays(idx, names, inners)
            tag = "" if key == "clown-idle" else "-dim"
            for nm, ov in (("eyes-blink", blink), ("eyes-glance", glance)):
                q = OUT / f"{nm}{tag}.png"
                write_png(q, to_canvas(ov, pal).rgba())
                files.append(q)
                comp = idx.copy()
                comp[ov >= 0] = ov[ov >= 0]
                row.append(comp)
        rows.append(row)
    # 비교 그림(얼굴·어깨 부분만 잘라 3배): 줄 = 평소·생각 중·어둡게, 칸 = 숨 0·숨 1·깜빡임·힐끗
    crop = (slice(40, 180), slice(16, 176))
    bg = pal["bg0"].astype(np.uint8)
    ch, cw = 140, 160
    sheet = np.tile(bg, (len(rows) * (ch + 4), 4 * (cw + 4), 1))
    for r, row in enumerate(rows):
        for c, img in enumerate(row):
            rgba = to_canvas(img, pal).rgba()[crop]
            m = rgba[..., 3] > 0
            reg = sheet[r * (ch + 4):r * (ch + 4) + ch, c * (cw + 4):c * (cw + 4) + cw]
            reg[m] = rgba[..., :3][m]
    write_png(OUT / "clown-idle-sheet-x3.png", sheet.repeat(3, 0).repeat(3, 1))
    print("팔레트 검사 (32색):")
    return check32(files, pal)


if __name__ == "__main__":
    cmd = sys.argv[1] if len(sys.argv) > 1 else ""
    if cmd == "draft":
        sys.exit(0 if make_draft(load_pal()) else 1)
    if cmd == "idle":
        sys.exit(0 if make_idle(load_pal()) else 1)
    print(__doc__)

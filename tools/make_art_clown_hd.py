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


# ---------- Ticket 48: 눈에 보이는 숨쉬기 ----------
def breathe2(idx, body, head):
    """숨쉬기 한 장: 그림 전체(머리)를 head도트, 턱 아래(몸)는 모두 합쳐 body도트 내림.
    위쪽에 생기는 빈 줄은 투명, 턱 아래 빈 줄은 턱 줄을 늘여 채움(46과 같음)."""
    # 음수 = 위로(들이쉼 끝, 48 "위·아래 2픽셀 더"): 위쪽 여백 12줄이라 머리가 올라가도 안 잘림
    out = np.full_like(idx, -1)
    if head > 0:
        out[head:] = idx[:-head]
    elif head < 0:
        out[:head] = idx[-head:]
    else:
        out[:] = idx
    k = body - head
    if k != 0:
        out = _breathe_at(out, CHIN_Y + head, k)
    return out


def _breathe_at(idx, chin, k):
    """턱 줄(chin)부터 아래를 k도트 내림(k < 0이면 올림: 몸이 턱 아래 끝을 덮고, 맨 아래 빈 줄은 마지막 줄을 늘임 — 모니터 뒤)."""
    out = idx.copy()
    if k > 0:
        out[chin + k:] = idx[chin:-k]
        out[chin:chin + k] = idx[chin]
    else:
        k = -k
        out[chin - k:-k] = idx[chin:]
        out[-k:] = idx[-1]
    return out


BREATH = [("u", -2, -1), ("a", 0, 0), ("b", 2, 1), ("c", 4, 2), ("d", 6, 3), ("e", 8, 4)]   # (이름, 몸 도트, 머리 도트, 아래가 +)
# 순서 u(맨 위, 0.3초 쉼) → a → b → c → d → e(맨 아래, 0.3초 쉼) → d → c → b → a → u, 한 장 0.25초
# (사용자 "위, 아래 2픽셀 더 가게, 피크 찍으면 0.3초 쉬었다가" — u·e 추가. 그 전: a~d, 0.5초씩 a → d → a)
# (48 첫판: 몸 1·2, 머리 0·1, a → b → c → b — 사용자 "3배는 더 크게"로 3배)


def make_breath(pal):
    """2배 광대 3상태 × 숨 b·c장(a는 원래 그림) + 미리보기(비교 그림·움직이는 HTML)."""
    OUT.mkdir(parents=True, exist_ok=True)
    files, rows = [], []
    for key, src in (("clown-idle", "Clown/clown-idle"), ("clown-thinking", "Clown/clown-thinking"), ("clown-dim", "Clown/clown-dim")):
        idx = to_index(read_png(SPR / f"{src}.png"), pal)
        row = []
        for name, body, head in BREATH:
            f = breathe2(idx, body, head)
            row.append(f)
            if name != "a":
                p = OUT / f"{key}-{name}.png"
                write_png(p, to_canvas(f, pal).rgba())
                files.append(p)
        rows.append(row)
    crop = (slice(40, 192), slice(16, 176))
    bg = pal["bg0"].astype(np.uint8)
    ch, cw = 152, 160
    sheet = np.tile(bg, (len(rows) * (ch + 4), len(BREATH) * (cw + 4), 1))
    for r, row in enumerate(rows):
        for c, img in enumerate(row):
            rgba = to_canvas(img, pal).rgba()[crop]
            m = rgba[..., 3] > 0
            reg = sheet[r * (ch + 4):r * (ch + 4) + ch, c * (cw + 4):c * (cw + 4) + cw]
            reg[m] = rgba[..., :3][m]
    write_png(OUT / "clown-breath-sheet-x3.png", sheet.repeat(3, 0).repeat(3, 1))
    # 움직이는 미리보기: 같은 폴더 그림을 a → b → c → d → c → b(각 0.5초, 한 바퀴 3초)로 넘김
    for key, _ in (("clown-idle", 0), ("clown-thinking", 0), ("clown-dim", 0)):
        write_png(OUT / f"{key}-a.png", read_png(SPR / f"Clown/{key}.png"))
    html = """<!doctype html><meta charset="utf-8"><title>숨쉬기 미리보기</title>
<body style="background:#111;color:#ddd;font-family:sans-serif">
<p>u(0.3초 쉼) → a → b → c → d → e(0.3초 쉼) → d → c → b → a, 한 장 0.25초 (3배 확대). 왼쪽부터 평소·생각 중·어둡게</p>
<div id="w"></div><script>
const keys=["clown-idle","clown-thinking","clown-dim"],seq=["u","a","b","c","d","e","d","c","b","a"],ms=[550,250,250,250,250,550,250,250,250,250];
const imgs=keys.map(k=>{const i=document.createElement("img");i.style="width:576px;image-rendering:pixelated;margin:4px";document.getElementById("w").appendChild(i);return [k,i];});
let n=0;function tick(){const j=n%seq.length;imgs.forEach(([k,i])=>i.src=k+"-"+seq[j]+".png");n++;setTimeout(tick,ms[j]);}tick();
</script>"""
    (OUT / "breath-preview.html").write_text(html, encoding="utf-8")
    print("팔레트 검사 (32색):")
    return check32(files, pal)


# ---------- 타이틀 광대 눈 발작(사용자 2026-11-19 "발작형, 2도트") ----------
def twitch_overlays(idx, names, inners):
    """눈 덮개 3장(힐끗과 같은 방식: 눈 안쪽을 얼굴색으로 지우고 눈동자를 다시 그림).
    pin = 눈동자가 바늘처럼 작아짐(가운데 2×2), cross = 두 눈이 바깥으로 갈라짐(왼눈 왼쪽·오른눈 오른쪽 3도트),
    up = 눈동자가 위로 쏠림(5도트)."""
    pupil = names.index("bg0")
    face = int(idx[FOREHEAD[1], FOREHEAD[0]])
    out = {k: np.full(idx.shape, -1) for k in ("pin", "cross", "up")}
    for side, inner in enumerate(inners):
        pys, pxs = np.nonzero(inner & (idx == pupil))
        cy, cx = int(round(pys.mean())), int(round(pxs.mean()))
        for k in out:
            out[k][inner] = face
        # 바늘 눈동자: 가운데 2×2
        for y in (cy - 1, cy):
            for x in (cx - 1, cx):
                if inner[y, x]:
                    out["pin"][y, x] = pupil
        # 갈라진 눈·위로 쏠린 눈: 같은 모양 눈동자를 옮김(눈 안쪽을 벗어나는 칸은 버림)
        dx = -3 if side == 0 else 3
        for y, x in zip(pys, pxs):
            if 0 <= x + dx < idx.shape[1] and inner[y, x + dx]:
                out["cross"][y, x + dx] = pupil
            if inner[max(y - PUPIL_DROP, 0), x]:
                out["up"][y - PUPIL_DROP, x] = pupil
    return out


def make_twitch(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    names = list(pal)
    lit = to_index(read_png(SPR / "Clown/clown-idle.png"), pal)
    inners = [eye_interior(lit, names, box) for box in EYES]
    ovs = twitch_overlays(lit, names, inners)
    files = []
    comps = [lit]
    for k, ov in ovs.items():
        p = OUT / f"eyes-twitch-{k}.png"
        write_png(p, to_canvas(ov, pal).rgba())
        files.append(p)
        comp = lit.copy()
        comp[ov >= 0] = ov[ov >= 0]
        comps.append(comp)
    # 사용자(2026-11-19): "눈동자만 움직여야 하는데 흰 배경도 움직여서 파란 선을 넘어가" →
    # 흔들 때 쓰는 그림을 나눔: 흰 바탕(제자리) / 눈동자만(흔들림) / 눈 안쪽 가리개(눈동자가 테두리 밖으로 안 나가게)
    face = int(lit[FOREHEAD[1], FOREHEAD[0]])
    inner_all = np.zeros(lit.shape, bool)
    for inner in inners:
        inner_all |= inner
    fill = np.full(lit.shape, -1)
    fill[inner_all] = face
    mask = np.full(lit.shape, -1)
    mask[inner_all] = names.index("cream2")       # 가리개 모양만 쓰임(색은 안 보임)
    for nm, arr in (("eyes-twitch-fill", fill), ("eyes-twitch-mask", mask)):
        p = OUT / f"{nm}.png"
        write_png(p, to_canvas(arr, pal).rgba())
        files.append(p)
    pupil = names.index("bg0")
    # 조용히 한쪽 보기(사용자 "발작만 말고 그냥 아래나 위, 오른쪽만 보는 것도"): 눈동자만 아래 5·오른쪽 3도트(두 눈 같은 쪽).
    # 위는 발작의 "up"과 같은 그림을 씀
    looks = {"down": (PUPIL_DROP, 0), "right": (0, 3)}
    for k, (dy, dx) in looks.items():
        arr = np.full(lit.shape, -1)
        for inner in inners:
            pys, pxs = np.nonzero(inner & (lit == pupil))
            for y, x in zip(pys, pxs):
                ny, nx = y + dy, x + dx
                if 0 <= ny < lit.shape[0] and 0 <= nx < lit.shape[1] and inner[ny, nx]:
                    arr[ny, nx] = pupil
        p = OUT / f"eyes-look-{k}-pupil.png"
        write_png(p, to_canvas(arr, pal).rgba())
        files.append(p)
    for k, ov in ovs.items():
        only = np.where(ov == pupil, pupil, -1)
        p = OUT / f"eyes-twitch-{k}-pupil.png"
        write_png(p, to_canvas(only, pal).rgba())
        files.append(p)
    # 비교 그림(얼굴만 4배): 평소 · 바늘 · 갈라짐 · 위로
    crop = (slice(40, 110), slice(56, 140))
    sheet = np.concatenate([to_canvas(c, pal).rgba()[crop] for c in comps], 1)
    write_png(OUT / "eyes-twitch-sheet-x4.png", sheet.repeat(4, 0).repeat(4, 1))
    print("팔레트 검사 (32색):")
    return check32(files, pal)


if __name__ == "__main__":
    cmd = sys.argv[1] if len(sys.argv) > 1 else ""
    if cmd == "twitch":   # 타이틀 광대 눈 발작 덮개 3장
        sys.exit(0 if make_twitch(load_pal()) else 1)
    if cmd == "breath":
        sys.exit(0 if make_breath(load_pal()) else 1)
    if cmd == "draft":
        sys.exit(0 if make_draft(load_pal()) else 1)
    if cmd == "idle":
        sys.exit(0 if make_idle(load_pal()) else 1)
    print(__doc__)

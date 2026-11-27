"""balance-board 112 — 잔고 전광판 그림 (AI 작성).

    python tools/make_art_board.py draft     # 시안 A·B → .scratch/art/drafts/board/{A,B}/ + 테이블(매트 없음)
    python tools/make_art_board.py tablecheck  # 지금 게임 table.png가 make_art_trade.wide_table()과 같은지

테이블 왼쪽에 세운 LED 기계 하나: 위 줄 = 광대, 아래 줄 = 나. 줄마다 7세그먼트 숫자 2자리(꺼진 획은 흐리게 "88"이 비침) + 칸 막대.
글자(이름·"정지"·"▼n")와 켜진 숫자·칸은 게임에서 겹쳐 그림(Image 바꿔 끼움·TMP). 그래서 몸통 그림엔 꺼진 "88"만 넣는다.
- 시안 A: 어두운 베이지 금속 몸통(CRT 모니터와 한 세트) + 초록 LED
- 시안 B: 검은 몸통 + 호박색(금색) LED
레퍼런스(벅샷 룰렛 제세동기 충전 표시)는 "기계가 정확한 숫자를 보여 준다"는 방식만 참고 — 모양·번개 아이콘은 쓰지 않음.
32색 팔레트만, 무작위 없음. 크기는 HUD(320×180) 칸 × 2 = 그림 도트.
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
import make_art_phone as P  # noqa: E402  (round_rect·outline_of)
from make_art import write_png  # noqa: E402
from make_art_hd import BAYER4, Canvas, check32, load_pal  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "board"

# 칸(HUD) 단위 배치 — 그림 왼쪽 위 기준. 게임 자리는 HUD (14, 90)
# 사용자(2026-11-27) "모니터 옆을 차지하지 않고 테이블 위에 있는 오브젝트처럼" → 처음 96×64 판에서
# 테이블 왼쪽 뒤에 놓인 작은 입체 기계로: 앞면(화면) + 윗면·오른쪽 옆면(안쪽으로 물러남) + 받침 다리 + 그림자.
# 앞면은 정면 그대로(숫자가 기울지 않게), 줄마다 위 = 이름·숫자·▼n, 아래 = 칸 막대.
BODY = (58, 60)
FRONT = (0, 5, 50, 56)             # 앞면 x0, y0, x1, y1
DEPTH = (6, 5)                      # 윗면·옆면이 뒤로 물러나는 만큼(오른쪽, 위)
SCREEN = (3, 8, 47, 52)            # 검은 화면
ROW_Y = (10, 31)                    # 줄 맨 위 y(광대, 나)
DIGIT = (6, 10)                     # 숫자 칸 크기
DIGIT_X = (21, 28)                  # 십의 자리·일의 자리 x
CELL = (3, 6)                       # 칸 크기
CELL_X0, CELL_STEP, CELL_DY = 5, 4, 12   # 칸 x 시작·간격, 줄 맨 위에서 칸 위까지

STYLES = {
    "A": dict(body="gray0", light="gray1", hi="cream0", dark="bg00", lit="green2", core="green", glint="cream2", ghost="bg0",
              cell_on=("green2", "green"), cell_off=("mat0", "mat1")),
    "B": dict(body="bg0", light="gray0", hi="gray1", dark="bg00", lit="gold2", core="gold1", glint="gold3", ghost="gold0",
              cell_on=("gold2", "gold1"), cell_off=("bg00", "gold0")),
}

# 7세그먼트: a(위) b(오른쪽 위) c(오른쪽 아래) d(아래) e(왼쪽 아래) f(왼쪽 위) g(가운데)
SEGS = {"0": "abcdef", "1": "bc", "2": "abged", "3": "abgcd", "4": "fgbc", "5": "afgcd",
        "6": "afgedc", "7": "abc", "8": "abcdefg", "9": "abcdfg"}


def seg_masks(w, h, t=3):
    """숫자 칸(w×h 도트) 안의 획 7개 mask. 획 끝은 1도트씩 깎아 LED처럼."""
    yy, xx = np.mgrid[0:h, 0:w]
    mid = h // 2
    m = {}
    hz = lambda y0: (yy >= y0) & (yy < y0 + t) & (xx >= t - 1) & (xx < w - t + 1)
    vt = lambda x0, y0, y1: (xx >= x0) & (xx < x0 + t) & (yy >= y0) & (yy < y1)
    m["a"] = hz(0)
    m["g"] = hz(mid - t // 2 - 1)
    m["d"] = hz(h - t)
    m["f"] = vt(0, t - 1, mid)
    m["b"] = vt(w - t, t - 1, mid)
    m["e"] = vt(0, mid, h - t + 1)
    m["c"] = vt(w - t, mid, h - t + 1)
    for k in m:   # 획 끝 모서리 깎기(가로 획은 양 끝 위·아래, 세로 획은 위·아래 끝 양옆)
        s = m[k]
        ys, xs = np.nonzero(s)
        if k in "agd":
            for x in (xs.min(), xs.max()):
                s[ys.min(), x] = s[ys.max(), x] = False
        else:
            for y in (ys.min(), ys.max()):
                s[y, xs.min()] = s[y, xs.max()] = False
    return m


def digit_sprite(pal, ch, col, edge):
    """켜진 숫자: 획 4도트 굵기, 가장자리 1도트는 어두운 색(edge), 안쪽은 밝은 색(col) — LED 관처럼.
    (처음엔 가운데 1줄만 흰색이라 숫자가 속 빈 글씨처럼 보였음)"""
    w, h = DIGIT[0] * 2, DIGIT[1] * 2
    cv = Canvas(pal, w, h)
    m = seg_masks(w, h, t=3)   # 작은 숫자(12×20도트)라 획 3도트: 가장자리 1 + 안쪽 1
    for k in SEGS[ch]:
        cv.fill(m[k], edge)
        cv.fill(m[k] & ~P.outline_of(~m[k], 1), col)
    return cv


def cell_sprite(pal, face, edge, core=None):
    w, h = CELL[0] * 2, CELL[1] * 2
    cv = Canvas(pal, w, h)
    box = P.round_rect(w, h, 0, 0, w, h, 1)
    cv.fill(box, edge)
    cv.fill(P.round_rect(w, h, 1, 1, w - 1, h - 1, 1), face)
    if core:
        cv.fill((slice(2, h - 2), slice(2, 3)), core)
    return cv


def body_sprite(pal, s):
    """테이블 위 작은 기계(위에서 내려다본 3/4): 그림자 → 받침 다리 → 오른쪽 옆면·윗면(뒤로 물러남) → 앞면(화면)."""
    w, h = BODY[0] * 2, BODY[1] * 2
    cv = Canvas(pal, w, h)
    yy, xx = np.mgrid[0:h, 0:w]
    fx0, fy0, fx1, fy1 = [v * 2 for v in FRONT]
    dx, dy = DEPTH[0] * 2, DEPTH[1] * 2
    sparse = lambda region, name, level: cv.fill(region & (np.broadcast_to(level, (h, w)) > BAYER4[yy % 4, xx % 4]), name)
    # 테이블 위 그림자(전등이 가운데 위 → 그림자는 왼쪽 아래로 조금, 바둑판으로 옅게)
    shadow = (yy >= fy1 - 2) & (yy < h) & (xx >= 0) & (xx < fx1 + dx - 6)
    sparse(shadow, 'bg00', 0.55)
    # 받침 다리 2개
    for lx in (8, fx1 - 18):
        cv.fill((slice(fy1, fy1 + 6), slice(lx, lx + 10)), s['dark'])
        cv.fill((slice(fy1, fy1 + 1), slice(lx, lx + 10)), s['body'])
    # 윗면: 앞면 위 끝에서 오른쪽 위로 물러난 평행사변형 / 오른쪽 옆면: 앞면 오른쪽 끝에서 같은 만큼
    top = (yy >= fy0 - dy) & (yy < fy0) & (xx >= fx0 + (fy0 - yy)) & (xx < fx1 + (fy0 - yy))
    side = (xx >= fx1) & (xx < fx1 + dx) & (yy >= fy0 - (xx - fx1)) & (yy < fy1 - (xx - fx1))
    cv.fill(top, s['light'])
    cv.fill(top & (yy == fy0 - dy), s['hi'])
    cv.fill(side, s['dark'])
    sparse(side, s['body'], 0.35)
    cv.fill(top & ((xx - (fy0 - yy)) >= fx1 - 1), s['body'])          # 윗면·옆면이 만나는 모서리
    # 앞면(몸통): 둥근 모서리 판, 위·왼쪽 빛 / 아래·오른쪽 그늘
    front = P.round_rect(w, h, fx0, fy0, fx1, fy1, 3)
    cv.fill(front, s['body'])
    sparse(front, s['dark'], np.clip(((yy - fy0) / (fy1 - fy0) - 0.6) * 0.9, 0, 0.45))
    edge = P.outline_of(~front, 1) & front
    cv.fill(edge, s['dark'])
    cv.fill(P.outline_of(~front, 2) & front & ~edge & ((yy < fy0 + 3) | (xx < fx0 + 3)), s['hi'])
    # 화면(움푹): 둘레 그늘 → 검은 유리 + 옅은 주사선
    sx0, sy0, sx1, sy1 = [v * 2 for v in SCREEN]
    well = P.round_rect(w, h, sx0 - 2, sy0 - 2, sx1 + 2, sy1 + 2, 3)
    cv.fill(well, s['dark'])
    cv.fill(P.outline_of(well, 1) & (yy > sy1) & front, s['hi'])
    scr = P.round_rect(w, h, sx0, sy0, sx1, sy1, 2)
    cv.fill(scr, 'bg00')
    cv.fill(scr & ((yy - sy0) % 3 == 2), 'bg0')
    # 두 줄 사이 구분선
    mid = (ROW_Y[1] * 2 + (ROW_Y[0] + CELL_DY + CELL[1]) * 2) // 2
    cv.fill(scr & (yy == mid) & (xx > sx0 + 2) & (xx < sx1 - 2), s['body'])
    # 꺼진 "88" 비침 — 켜진 숫자 그림이 그 위에 겹침
    dw, dh = DIGIT[0] * 2, DIGIT[1] * 2
    ghost = seg_masks(dw, dh, t=3)
    for ry in ROW_Y:
        for gx in DIGIT_X:
            x0, y0 = gx * 2, ry * 2
            for k in 'abcdefg':
                sub = np.zeros((h, w), bool)
                sub[y0:y0 + dh, x0:x0 + dw] = ghost[k]
                cv.fill(sub, s['ghost'])
    # 앞면 아래 턱: 전원 LED + 나사 2개
    cv.fill((slice(fy1 - 5, fy1 - 3), slice(fx1 - 9, fx1 - 7)), s['lit'])
    for sxx in (fx0 + 4, fx1 - 16):
        cv.fill((slice(fy1 - 6, fy1 - 3), slice(sxx, sxx + 3)), s['dark'])
    # 옆면 통풍 홈 + 뒤로 나가는 전선 한 가닥(옆면 아래에서 오른쪽 아래로)
    for i in range(4):
        y = fy0 + 10 + i * 6
        cv.fill(side & (yy >= y - (xx - fx1)) & (yy < y + 2 - (xx - fx1)) & (xx > fx1 + 2), 'bg00')
    P.line(cv, (fx1 + dx - 2, fy1 - dx - 2), (w - 1, fy1 + 4), 'bg00', None)
    # 긁힘
    P.line(cv, (fx0 + 4, fy0 + 12), (fx0 + 8, fy0 + 9), s['dark'], front & ~well)
    return cv


def parts(pal, style):
    s = STYLES[style]
    out = {"board-body": body_sprite(pal, s)}
    for d in "0123456789":
        out[f"board-digit-{d}"] = digit_sprite(pal, d, s["lit"], s["core"])
        out[f"board-digit-red-{d}"] = digit_sprite(pal, d, "red4", "red2")
    out["board-cell-on"] = cell_sprite(pal, s["cell_on"][0], s["cell_on"][1], s["glint"])
    out["board-cell-off"] = cell_sprite(pal, s["cell_off"][0], s["cell_off"][1])
    out["board-cell-hit"] = cell_sprite(pal, "red3", "red2", "red4")
    out["board-cell-heal"] = cell_sprite(pal, "cream2", s["cell_on"][0])
    return out


def table_nomat(pal):
    """지금 테이블(make_art_trade.wide_table)에서 매트 2장만 빼고 그림."""
    import make_art_trade as T
    keep = T.draw_mat
    T.draw_mat = lambda *a, **k: None
    try:
        return T.wide_table(pal)
    finally:
        T.draw_mat = keep


def preview(pal, p, style):
    """몸통 + 숫자(광대 06, 나 03 빨강) + 칸(광대 6/7, 나 3/7, 잃은 칸 1개 빨강) ×4."""
    b = p['board-body'].rgba().copy()

    def put(img, x, y):
        a = img[..., 3] > 0
        b[y:y + img.shape[0], x:x + img.shape[1]][a] = img[a]
    for ry, val, red, cap in ((ROW_Y[0], 6, False, 7), (ROW_Y[1], 3, True, 7)):
        put(p[f"board-digit-{'red-' if red else ''}{val}"].rgba(), DIGIT_X[1] * 2, ry * 2)
        for i in range(cap):
            name = 'board-cell-on' if i < val else ('board-cell-hit' if i == val else 'board-cell-off')
            put(p[name].rgba(), int((CELL_X0 + i * CELL_STEP) * 2), (ry + CELL_DY) * 2)
    bg = np.zeros_like(b); bg[..., :3] = (43, 33, 28); bg[..., 3] = 255   # 테이블 색 위에
    a = b[..., 3] > 0
    bg[a] = b[a]
    return bg.repeat(4, 0).repeat(4, 1)


def main(cmd):
    pal = load_pal()
    if cmd == "tablecheck":
        from make_rules_screen import read_png
        cur = read_png(ROOT / "Assets/_Project/Sprites/Play/table.png")
        import make_art_trade as T
        new = T.wide_table(pal).rgba()[..., :3]
        same = cur.shape == new.shape and (cur == new).all()
        print("table.png == wide_table():", same, cur.shape, new.shape)
        return same
    files = []
    for style in ("A", "B"):
        d = OUT / style
        d.mkdir(parents=True, exist_ok=True)
        p = parts(pal, style)
        for name, cv in p.items():
            write_png(d / f"{name}.png", cv.rgba())
            files.append(d / f"{name}.png")
        write_png(OUT / f"preview-{style}-x3.png", preview(pal, p, style))
    t = table_nomat(pal)
    write_png(OUT / "table.png", t.rgba())
    files.append(OUT / "table.png")
    print("팔레트 검사 (32색):")
    return check32(files, pal)


if __name__ == "__main__":
    sys.exit(0 if main(sys.argv[1] if len(sys.argv) > 1 else "draft") else 1)

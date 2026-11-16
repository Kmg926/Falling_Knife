"""Falling Knife 2배 세밀(640x360 도트) 그림 (AI 작성, art-hd).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art_hd.py draft <캡처폴더>   # Ticket 34: 테이블·동전 탑 시안 A·B·C + 색표 + 팔레트 검사

- 팔레트: tools/palette32.txt (지금 16색 + 새 16색 제안판)
- 캡처폴더: 게임 화면을 검정 바탕(X_k.png)·흰 바탕(X_w.png)으로 1280x720 찍은 것(방·테이블·동전·아래 띠는 숨김).
  두 장의 차이로 글자·광대·단말기 같은 '앞 물건'의 투명도를 얻어 새 테이블 위에 겹친다(자리 표시용, 그림 아님).
- 출력: .scratch/art/drafts/hd/ (Assets 밖). 외부 라이브러리는 numpy만. 무작위 없음.
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import read_png, write_png  # noqa: E402  PNG 읽기·쓰기는 기존 도구 그대로

ROOT = Path(__file__).resolve().parent.parent
PAL32 = ROOT / "tools" / "palette32.txt"
OUT = ROOT / ".scratch" / "art" / "drafts" / "hd"
W, H = 640, 360

BAYER4 = np.array([[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]) / 16 + 1 / 32


def load_pal():
    pal = {}
    for line in PAL32.read_text(encoding="utf-8").splitlines():
        if line.strip() and not line.startswith("#"):
            n, h = line.split()[:2]
            pal[n] = np.array([int(h[i:i + 2], 16) for i in (1, 3, 5)], np.uint8)
    return pal


class Canvas:
    """색 이름 번호로 그리는 도화지. mask=False인 곳은 투명."""

    def __init__(self, pal, w=W, h=H):
        self.pal, self.names = pal, list(pal)
        self.idx = np.zeros((h, w), np.int32)
        self.mask = np.zeros((h, w), bool)
        self.h, self.w = h, w

    def c(self, name):
        return self.names.index(name)

    def fill(self, region, name):
        self.idx[region] = self.c(name)
        self.mask[region] = True

    def dither(self, region, dark, light, level):
        """level(0~1, 배열 가능)만큼 light, 나머지 dark — 바둑판(Bayer) 섞기로 단계 사이를 메운다(팔레트 유지)."""
        yy, xx = np.mgrid[0:self.h, 0:self.w]
        thr = BAYER4[yy % 4, xx % 4]
        lvl = np.broadcast_to(level, (self.h, self.w))
        pick = np.where(lvl > thr, self.c(light), self.c(dark))
        self.idx[region] = pick[region]
        self.mask[region] = True

    def rgba(self):
        cols = np.array(list(self.pal.values()), np.uint8)
        out = np.zeros((self.h, self.w, 4), np.uint8)
        out[..., :3] = cols[self.idx]
        out[..., 3] = self.mask * 255
        return out


def polygon(pts, w=W, h=H):
    """점 목록(시계 방향 아무거나)으로 채운 다각형 mask. 픽셀 가운데 기준 짝홀 규칙."""
    yy, xx = np.mgrid[0:h, 0:w] + 0.5
    inside = np.zeros((h, w), bool)
    n = len(pts)
    for i in range(n):
        (x1, y1), (x2, y2) = pts[i], pts[(i + 1) % n]
        if y1 == y2:
            continue
        cond = (yy >= min(y1, y2)) & (yy < max(y1, y2))
        xcross = x1 + (yy - y1) * (x2 - x1) / (y2 - y1)
        inside ^= cond & (xx < xcross)
    return inside


def edge(mask, dy, dx):
    """mask 안쪽이면서 (dy, dx) 쪽 이웃이 바깥인 칸 = 그쪽 가장자리."""
    sh = np.zeros_like(mask)
    ys = slice(max(dy, 0), mask.shape[0] + min(dy, 0))
    yd = slice(max(-dy, 0), mask.shape[0] + min(-dy, 0))
    xs = slice(max(dx, 0), mask.shape[1] + min(dx, 0))
    xd = slice(max(-dx, 0), mask.shape[1] + min(-dx, 0))
    sh[yd, xd] = mask[ys, xs]
    return mask & ~sh


# ---------- 테이블 ----------
VP = (320, -116)          # 원근 소실점: 지금 테이블 옆선 (60,200)→(0,273)을 늘인 곳
TOP_Y, FRONT_Y = 200, 274  # 테이블 윗면 먼 끝 / 가까운 끝
FACE = 8                   # 앞 모서리 두께(도트)


def toward_vp(x, y_from, y_to):
    """y_from 높이의 x를 소실점 쪽으로 y_to 높이까지 옮긴 x (원근 평행선)."""
    return VP[0] + (x - VP[0]) * (y_to - VP[1]) / (y_from - VP[1])


def table_surface(corner=None):
    """윗면 다각형. corner=(x끝, y끝)이면 왼쪽 앞 모서리가 아래 띠까지 내려온 판(시안 B)."""
    if corner is None:
        return [(60, TOP_Y), (580, TOP_Y), (640, FRONT_Y), (0, FRONT_Y)]
    cx, cy = corner
    return [(60, TOP_Y), (580, TOP_Y), (640, FRONT_Y), (cx + 14, FRONT_Y), (cx, FRONT_Y + 14), (cx, cy), (0, cy), (0, FRONT_Y)]


def draw_table(cv, corner=None, divider=False):
    surf = polygon(table_surface(corner))
    yy, xx = np.mgrid[0:H, 0:W] + 0.5
    # 판자: 소실점으로 모이는 이음새로 나눈 띠. 판자마다 한 가지 바탕색(점 섞기 없음 → 깨끗한 면)
    #   광대 위 램프 빛 → 가운데 판자는 밝은 나무(wood2), 바깥 판자는 wood
    PLANK = 46
    top_x = VP[0] + (xx - VP[0]) * (TOP_Y - VP[1]) / (yy - VP[1])     # 이 칸을 먼 끝 높이로 옮긴 x = 어느 판자인지
    plank = np.floor((top_x - 36) / PLANK).astype(int)
    lit = np.abs(top_x - 320) < 120
    cv.fill(surf & lit, "wood2")
    cv.fill(surf & ~lit, "wood")
    # 결: 판자 안 가는 줄 2개(판자 바탕보다 한 단계 어둡게), 일정한 규칙으로 끊김 → 나무 무늬
    pos = (top_x - 36) % PLANK
    for off, phase in ((13, 0), (30, 5)):
        line = surf & (np.abs(pos - off) < 0.55 * (yy - VP[1]) / (TOP_Y - VP[1]))
        line &= ((yy.astype(int) + plank * 7 + phase) % 23) < 15
        cv.fill(line & lit, "wood")
        cv.fill(line & ~lit, "wood0")
    # 이음새(어두운 줄)
    seam = surf & (np.abs(pos) < 0.5 * (yy - VP[1]) / (TOP_Y - VP[1]))
    cv.fill(seam, "wood0")
    # 램프 빛 반사: 먼 가운데 위쪽에 짧은 밝은 결
    sheen = surf & lit & (yy < TOP_Y + 22) & (np.abs(pos - 22) < 1) & ((yy.astype(int) % 5) < 3)
    cv.fill(sheen, "wood3")
    # 테두리: 먼 끝·옆선 빛(wood3), 앞 모서리 옆면
    cv.fill(edge(surf, -1, 0), "wood3")
    cv.fill(edge(surf, 0, -1) & (yy < FRONT_Y), "wood3")
    cv.fill(edge(surf, 0, 1) & (yy < FRONT_Y), "wood3")
    front = np.zeros_like(surf)
    for d in range(1, FACE + 1):
        sh = np.zeros_like(surf)
        sh[d:] = surf[:-d]
        front |= sh & ~surf
    cv.fill(front, "wood0")
    top_row = edge(front, -1, 0)
    cv.fill(top_row, "wood3")                                   # 앞 모서리 위쪽 빛
    groove = np.zeros_like(front)
    groove[3:] = top_row[:-3]
    cv.fill(groove & front, "bg00")                             # 모서리 홈 한 줄
    cv.fill(edge(front, 1, 0), "bg00")
    if divider:                                                  # 시안 C: 테이블 가운데 금색 상감 선 = 두 사람 자리 경계
        y_div = 240
        row = surf[y_div]
        cv.fill((np.full(row.sum(), y_div), np.nonzero(row)[0]), "gold0")
        cv.fill((np.full(row.sum(), y_div - 1), np.nonzero(row)[0]), "wood0")
    return surf | front


def draw_bar(cv, x0=0):
    """아래 띠(지금 hud-bar 2배): 위 2줄 wood, 나머지 bg0. x0부터."""
    cv.fill((slice(288, 360), slice(x0, W)), "bg0")
    cv.fill((slice(288, 290), slice(x0, W)), "wood")


def draw_mat(cv, front_y, back_y, x_left, x_right, name="mat0"):
    """가죽 매트: 앞 끝 x_left~x_right, 뒤 끝은 소실점 쪽으로 좁아짐. 테두리·바느질·아래 그림자."""
    bl, br = toward_vp(x_left, front_y, back_y), toward_vp(x_right, front_y, back_y)
    m = polygon([(bl, back_y), (br, back_y), (x_right, front_y), (x_left, front_y)])
    shadow = np.zeros_like(m)
    shadow[2:, 1:] = m[:-2, :-1]
    cv.fill(shadow & ~m, "bg00")
    cv.fill(m, name)
    cv.fill(edge(m, -1, 0) | edge(m, 0, -1), "mat1")           # 빛 받는 위·왼쪽 테두리
    cv.fill(edge(m, 1, 0) | edge(m, 0, 1), "bg00")             # 아래·오른쪽 어두운 테두리
    inner = polygon([(toward_vp(x_left + 5, front_y, back_y + 4), back_y + 4),
                     (toward_vp(x_right - 5, front_y, back_y + 4), back_y + 4),
                     (x_right - 5, front_y - 4), (x_left + 5, front_y - 4)])
    stitch = edge(inner, -1, 0) | edge(inner, 1, 0) | edge(inner, 0, -1) | edge(inner, 0, 1)
    yy, xx = np.mgrid[0:H, 0:W]
    cv.fill(stitch & ((xx + yy) % 4 < 2), "gray0")            # 바느질 점선
    return m


# ---------- 동전 탑 ----------
GOLD = {"top": "gold1", "rim": "gold2", "shine": "gold3", "side": "goldD", "gap": "gold0", "mark": "goldD"}
RED = {"top": "red2", "rim": "red3", "shine": "red4", "side": "red1", "gap": "red0", "mark": "red1"}


def ellipse(cx, cy, rx, ry, w=W, h=H):
    yy, xx = np.mgrid[0:h, 0:w]
    return ((xx - cx) / (rx + 0.5)) ** 2 + ((yy - cy) / (ry + 0.5)) ** 2 <= 1


def draw_coin(cv, cx, cy, rx, ry, th, col):
    """동전 1개: 윗면 타원 중심 (cx, cy), 옆면 두께 th(아래로).
    옆면 = 톱니 무늬(세로 줄 번갈아) + 맨 아래 틈 한 줄, 왼쪽 끝 빛·오른쪽 끝 그늘. 윗면 = 테두리·안쪽 원·왼쪽 위 반짝임."""
    yy, xx = np.mgrid[0:H, 0:W]
    side = np.zeros((H, W), bool)
    for d in range(1, th + 1):
        side |= ellipse(cx, cy + d, rx, ry)
    top = ellipse(cx, cy, rx, ry)
    band = side & ~top
    cv.fill(band, col["side"])
    cv.fill(band & (xx >= cx - rx + 1) & (xx <= cx - rx // 3), col["top"])           # 왼쪽 1/3 빛(램프 쪽)
    cv.fill(band & (xx <= cx - rx + 1), col["rim"])                                  # 왼쪽 끝 가장 밝게
    cv.fill(band & (xx >= cx + rx - 1), col["gap"])                                  # 오른쪽 끝 그늘
    cv.fill(edge(side, 1, 0), col["gap"])                                            # 동전 사이 틈
    cv.fill(top, col["top"])
    cv.fill(edge(top, 1, 0) | edge(top, 0, 1), col["side"])                          # 윗면 아래·오른쪽 테두리
    cv.fill(edge(top, -1, 0) | edge(top, 0, -1), col["rim"])                         # 윗면 위·왼쪽 테두리
    if rx >= 6 and ry >= 2:
        inner = ellipse(cx, cy, rx - 3, ry - 1) & ~ellipse(cx, cy, rx - 4, max(ry - 2, 0))
        cv.fill(inner & (yy >= cy), col["side"])                                     # 안쪽 원(새김) 아래 반
        cv.fill(inner & (yy < cy), col["rim"])
    cv.fill(top & (xx >= cx - rx + 2) & (xx <= cx - rx + 4) & (yy == cy - ry + 1), col["shine"])


def draw_spot(cv, cx, by, rx, ry):
    """빈 탑 자리: 매트 위 흐린 점선 타원(잃은 동전 자리)."""
    e = ellipse(cx, by, rx, ry)
    ring = e & ~ellipse(cx, by, rx - 1, max(ry - 1, 0))
    yy, xx = np.mgrid[0:H, 0:W]
    cv.fill(ring & ((xx // 2) % 2 == 0), "gray0")


def draw_stacks(cv, x, by, n, rx, ry, th, danger=False, gap=3):
    """동전 탑 2개(왼쪽부터 5개씩 채움). (x, by) = 왼쪽 탑 맨 아래 동전 윗면 중심."""
    col = RED if danger else GOLD
    xs = [x, x + 2 * rx + 1 + gap]
    for cx in xs:                                              # 그림자(오른쪽 아래)·빈 자리
        sh = ellipse(cx + 2, by + th + 1, rx, ry)
        cv.dither(sh & cv.mask, "bg00", "mat0", 0.25)
        draw_spot(cv, cx, by + th, rx, ry)
    for t, cx in enumerate(xs):
        k = max(0, min(5, n - 5 * t))
        for i in range(k):                                     # 아래부터 위로 그림(위 동전이 아래 동전 윗면을 가림)
            draw_coin(cv, cx, by - i * th, rx, ry, th, col)


# ---------- 앞 물건(글자·광대·단말기) 겹치기 ----------
def foreground(cap_dir, name):
    """검정·흰 바탕 캡처 두 장 → 640x360 색·투명도(2x2 평균)."""
    k = read_png(cap_dir / f"{name}_k.png")[..., :3].astype(float)
    w = read_png(cap_dir / f"{name}_w.png")[..., :3].astype(float)
    k = k.reshape(H, 2, W, 2, 3).mean((1, 3))
    w = w.reshape(H, 2, W, 2, 3).mean((1, 3))
    a = np.clip(1 - (w - k).mean(-1) / 255, 0, 1)
    col = np.where(a[..., None] > 0.01, k / np.maximum(a[..., None], 0.01), 0)
    return np.clip(col, 0, 255), a


def composite(cv, fg):
    base = cv.rgba()[..., :3].astype(float)
    base[~cv.mask] = cv.pal["bg0"]
    col, a = fg
    return np.clip(col * a[..., None] + base * (1 - a[..., None]), 0, 255).astype(np.uint8)


def overlaps(region, fg, thr=0.5):
    return int((region & (fg[1] > thr)).sum())


# ---------- 시안 3개 ----------
# 좌표는 640x360 도트. 글자 자리는 캡처한 화면 그대로(시안마다 글자 위치만 옮겨 찍음).
VARIANTS = {
    "A": dict(title="앞뒤 매트", corner=None, divider=False,
              clown_mat=(236, 210, 56, 222), clown_stack=(76, 224),
              me_mat=(272, 244, 6, 200), me_stack=(26, 260)),
    "B": dict(title="모서리", corner=(176, 356), divider=False,
              clown_mat=(238, 206, 52, 222), clown_stack=(74, 226),
              me_mat=(350, 298, 6, 168), me_stack=(28, 336)),
    "C": dict(title="상감 선", corner=None, divider=True,
              clown_mat=None, clown_stack=(76, 226),
              me_mat=None, me_stack=(26, 262)),
}
CLOWN_COIN = (7, 2, 2)     # 먼 쪽(작게): rx, ry, 두께
ME_COIN = (8, 3, 3)


def scene(pal, v, me=10, clown=10):
    cv = Canvas(pal)
    cv.fill((slice(0, H), slice(0, W)), "bg0")
    draw_table(cv, v["corner"], v["divider"])
    draw_bar(cv, x0=v["corner"][0] if v["corner"] else 0)
    regions = {}
    if v["clown_mat"]:
        regions["clown_mat"] = draw_mat(cv, *v["clown_mat"])
    if v["me_mat"]:
        regions["me_mat"] = draw_mat(cv, *v["me_mat"])
    before = cv.mask.copy(), cv.idx.copy()
    draw_stacks(cv, *v["clown_stack"], clown, *CLOWN_COIN)
    draw_stacks(cv, *v["me_stack"], me, *ME_COIN, danger=me <= 3)
    regions["stacks"] = cv.idx != before[1]
    return cv, regions


def swatch(pal):
    names = list(pal)
    cell = 40
    img = np.zeros((2 * cell, 16 * cell, 3), np.uint8)
    for i, n in enumerate(names):
        r, c = divmod(i, 16)
        img[r * cell:(r + 1) * cell, c * cell:(c + 1) * cell] = pal[n]
    return img


def check32(paths, pal):
    allowed = {tuple(int(v) for v in c) for c in pal.values()}
    ok = True
    for p in paths:
        a = read_png(p)
        px = a[..., :3][a[..., 3] > 0] if a.shape[2] == 4 else a.reshape(-1, 3)
        used = {tuple(int(v) for v in c) for c in np.unique(px, axis=0)}
        bad = used - allowed
        good = not bad and len(used) <= 32
        print(f"  {Path(p).name}: 색 {len(used)}개, 팔레트 밖 {len(bad)}개 {'OK' if good else 'FAIL'}")
        ok &= good
    return ok


def make_draft(cap_dir):
    pal = load_pal()
    assert len(pal) == 32, len(pal)
    OUT.mkdir(parents=True, exist_ok=True)
    write_png(OUT / "palette32-swatch.png", swatch(pal))
    art_files = []
    for key, v in VARIANTS.items():
        cv, regions = scene(pal, v)
        art = OUT / f"draft-{key}-art.png"                      # 새 그림만(팔레트 검사 대상)
        write_png(art, cv.rgba())
        art_files.append(art)
        if cap_dir is None:                                     # 캡처 없이 그림만 볼 때
            write_png(OUT / f"draft-{key}-art-x2.png", cv.rgba().repeat(2, 0).repeat(2, 1))
            continue
        fg = foreground(cap_dir, key)
        full = composite(cv, fg)
        write_png(OUT / f"draft-{key}-x2.png", full.repeat(2, 0).repeat(2, 1))
        print(f"시안 {key} ({v['title']}): 글자·앞 물건과 겹침 — 동전 탑 {overlaps(regions['stacks'], fg)}칸")
        # 잔고 상태 비교: 내 10/3/0 · 광대 10/7/2 (왼쪽 테이블만 잘라 2배, 글자 없음)
        crops = []
        for me, cl in ((10, 10), (3, 7), (0, 2)):
            c2, _ = scene(pal, v, me, cl)
            rgb = c2.rgba()[..., :3]
            crops.append(rgb[190:360, 0:240])
            crops.append(np.full((170, 4, 3), 255, np.uint8))
        write_png(OUT / f"draft-{key}-states-x2.png", np.hstack(crops[:-1]).repeat(2, 0).repeat(2, 1))
    print("팔레트 검사 (C2, 32색):")
    return check32(art_files, pal)


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "draft":
        sys.exit(0 if make_draft(Path(sys.argv[2]) if len(sys.argv) > 2 else None) else 1)
    print(__doc__)

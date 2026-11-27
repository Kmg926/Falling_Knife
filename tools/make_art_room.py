"""room-dress 113·114 — 배경(지하 불법 증권 거래실)·테이블 꾸미기 (AI 작성).

    python tools/make_art_room.py draft    # → .scratch/art/drafts/room/{A,B}/room.png + table.png + room-leds-a/b.png + 미리보기

레퍼런스(사용자 2026-11-27): 벅샷 룰렛 타이틀(무대 조명·양옆 장비·테이블 선), 지하 통제실(녹슨 철판·배관·낡은 컴퓨터), 벽 화면 3개.
원작 디자인(해골 얼굴·같은 조명 모양)은 그리지 않음 — 분위기·구성만 참고.
- 배경 room.png 640×360(HUD ×2): 천장 배관 2줄 + 늘어진 전선 + 리벳 철판 벽(녹물) + 가운데 큰 시세판(차트 선이 이 안에서 흐름)
  + 무대 조명 2개(어둡게, 빛줄기 아주 옅게) + 왼쪽 서버 선반 + 오른쪽 서류 캐비닛·벽 단말.
  시안 A = 녹슨 철판(붉은 갈색), B = 콘크리트(어두운 초록 회색).
- table.png 640×160: 지금 테이블(매트 없음) + 닳은 페인트 선(가운데 선·광대 자리·내 자리 — 폰이 놓인 곳) + 소품(영수증·커피잔·재떨이·계산기).
- room-leds-a/b.png: LED만 있는 겹침 그림 2장(게임에서 번갈아 켬 — RoomFlicker).
꾸밈은 어둡고 낮은 대비, 금색(gold1·gold2·gold3) 안 씀 — 누를 수 있는 것 전용(Grilling Q8). 32색, 무작위 없음.
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
import make_art_phone as P  # noqa: E402  (round_rect·outline_of·line)
import make_art_hd as HD  # noqa: E402
from make_art import write_png  # noqa: E402
from make_art_hd import BAYER4, Canvas, check32, load_pal  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "room"
W, H = 640, 360

# 시세판(차트 선이 흐르는 화면) — 640 격자. 게임 ChartLines: 폭 448·높이 152, 왼쪽 위 = HUD (48, 20)
BOARD = (88, 34, 552, 198)          # 바깥 틀
BOARD_IN = (96, 40, 544, 192)       # 안쪽 화면(= ChartLines 텍스처 자리)

WALL = {
    "A": dict(base="bg0", plate="wood0", seam="bg00", rivet="gray0", rust="red0", rust2="wood"),
    "B": dict(base="bg0", plate="mat0", seam="bg00", rivet="gray0", rust="wood0", rust2="mat1"),
}


def yx(h=H, w=W):
    return np.mgrid[0:h, 0:w]


def sparse(cv, region, name, level):
    yy, xx = yx(cv.h, cv.w)
    lvl = np.broadcast_to(level, (cv.h, cv.w))
    cv.fill(region & (lvl > BAYER4[yy % 4, xx % 4]), name)


def rect(cv, x0, y0, x1, y1):
    m = np.zeros((cv.h, cv.w), bool)
    m[max(0, y0):max(0, y1), max(0, x0):max(0, x1)] = True
    return m


def thick_line(cv, p0, p1, col, t=2, allow=None):
    for k in range(t):
        P.line(cv, (p0[0], p0[1] + k), (p1[0], p1[1] + k), col, allow)


def sag(cv, x0, x1, y, depth, col, t=2):
    """늘어진 전선: 두 점 사이 포물선."""
    pts = []
    for x in range(x0, x1 + 1, 2):
        u = (x - x0) / max(1, x1 - x0)
        pts.append((x, int(round(y + depth * 4 * u * (1 - u)))))
    for a, b in zip(pts, pts[1:]):
        thick_line(cv, a, b, col, t)


# ---------------- 배경 ----------------
def wall(cv, s):
    yy, xx = yx()
    cv.fill(rect(cv, 0, 0, W, H), s["base"])
    # 철판: 80도트 폭 세로 판, 판마다 밝기 조금 다르게(바둑판), 위·아래 두 단
    for i, x0 in enumerate(range(0, W, 80)):
        for j, (y0, y1) in enumerate(((28, 118), (118, 210))):
            plate = rect(cv, x0 + 2, y0 + 2, x0 + 78, y1 - 2)
            sparse(cv, plate, s["plate"], 0.18 + 0.08 * ((i + j) % 3))
            # 판 테두리 홈 + 리벳(10도트마다)
            cv.fill(rect(cv, x0, y0, x0 + 2, y1) | rect(cv, x0, y0, x0 + 80, y0 + 2), s["seam"])
            for ry in range(y0 + 6, y1 - 2, 12):
                cv.fill((slice(ry, ry + 2), slice(x0 + 4, x0 + 6)), s["rivet"])
                cv.fill((slice(ry, ry + 2), slice(x0 + 74, x0 + 76)), s["rivet"])
            # 녹물: 위 리벳에서 아래로 흘러내린 줄(판마다 자리 다르게, 무작위 없음)
            for k, rx in enumerate((x0 + 5, x0 + 30 + (i * 13) % 30, x0 + 75)):
                length = 20 + ((i * 7 + j * 5 + k * 11) % 40)
                drip = rect(cv, rx, y0 + 8, rx + 2, y0 + 8 + length)
                fade = np.clip(1 - (yy - (y0 + 8)) / max(1, length), 0, 1) * 0.8
                sparse(cv, drip, s["rust"], fade)
            sparse(cv, rect(cv, x0 + 8, y1 - 14, x0 + 72, y1 - 2), s["rust2"], 0.12)   # 판 아래쪽 때
    # 천장(위 28도트) 더 어둡게
    cv.fill(rect(cv, 0, 0, W, 28), "bg00")


def pipes(cv):
    yy, xx = yx()
    for y0 in (6, 17):
        body = rect(cv, 0, y0, W, y0 + 9)
        cv.fill(body, "gray0")
        cv.fill(rect(cv, 0, y0, W, y0 + 2), "gray1")
        cv.fill(rect(cv, 0, y0 + 7, W, y0 + 9), "bg00")
        sparse(cv, body, "red0", 0.15 + 0.15 * (np.sin(xx / 37.0 + y0) > 0.6))     # 녹 얼룩
        for fx in range(40 + y0 * 3, W, 130):                                       # 이음 고리(플랜지)
            cv.fill(rect(cv, fx, y0 - 2, fx + 6, y0 + 11), "gray0")
            cv.fill(rect(cv, fx, y0 - 2, fx + 1, y0 + 11), "gray1")
            cv.fill(rect(cv, fx + 5, y0 - 2, fx + 6, y0 + 11), "bg00")
    # 늘어진 전선 3가닥 + 한 가닥은 아래로 늘어짐
    sag(cv, 20, 200, 27, 26, "bg00")
    sag(cv, 260, 380, 27, 12, "bg00")
    sag(cv, 430, 630, 27, 34, "bg00")
    thick_line(cv, (612, 27), (606, 70), "bg00")
    cv.fill(rect(cv, 603, 70, 609, 76), "gray0")          # 끝 플러그


def ticker_board(cv):
    x0, y0, x1, y1 = BOARD
    ix0, iy0, ix1, iy1 = BOARD_IN
    yy, xx = yx()
    frame = rect(cv, x0, y0, x1, y1) & ~rect(cv, ix0, iy0, ix1, iy1)
    cv.fill(frame, "gray0")
    cv.fill(rect(cv, x0, y0, x1, y0 + 2), "gray1")
    cv.fill(rect(cv, x0, y0, x0 + 2, y1), "gray1")
    cv.fill(rect(cv, x0, y1 - 2, x1, y1) | rect(cv, x1 - 2, y0, x1, y1), "bg00")
    cv.fill(P.outline_of(rect(cv, ix0, iy0, ix1, iy1), 1) & frame, "bg00")
    sparse(cv, frame, "red0", 0.12)
    for bx, by in ((x0 + 3, y0 + 3), (x1 - 6, y0 + 3), (x0 + 3, y1 - 6), (x1 - 6, y1 - 6)):   # 모서리 볼트
        cv.fill(rect(cv, bx, by, bx + 3, by + 3), "gray1")
        cv.fill(rect(cv, bx + 1, by + 1, bx + 3, by + 3), "bg00")
    # 화면: 어두운 바탕 + 옅은 격자 + 주사선 (차트 선은 게임 코드가 이 위에)
    scr = rect(cv, ix0, iy0, ix1, iy1)
    cv.fill(scr, "bg00")
    cv.fill(scr & ((yy - iy0) % 3 == 1), "bg0")
    for gx in range(ix0 + 28, ix1, 56):
        cv.fill(scr & (xx == gx) & ((yy % 4) == 0), "navy0")
    for gy in range(iy0 + 19, iy1, 38):
        cv.fill(scr & (yy == gy) & ((xx % 4) == 0), "navy0")
    # 위 테두리 위: 매달린 쇠줄 2개(배관에서)
    for cx in (x0 + 40, x1 - 40):
        thick_line(cv, (cx, 26), (cx, y0), "bg00", 1)
        cv.fill(rect(cv, cx - 2, y0 - 3, cx + 3, y0), "gray0")


def spotlight(cv, cx, flip):
    """천장 배관에 매달린 무대 조명(꺼진 듯 어둡게): 광대 쪽 아래를 향한 원통 + 앞 렌즈(옆에서 본 납작한 타원) + 빛가리개 2장.
    처음엔 렌즈가 정면 동그라미 + 빛줄기라 벽에 커다란 얼굴(눈·웃는 입)처럼 보였음 → 빛줄기 없앰, 렌즈를 옆으로 돌림."""
    yy, xx = yx()
    thick_line(cv, (cx, 26), (cx, 38), 'gray0', 2)                       # 걸이
    c = np.array([cx, 52.0])
    d = np.array([320.0, 170.0]) - c
    d /= np.linalg.norm(d)
    n = np.array([-d[1], d[0]])
    L, Rr = 15.0, 11.0
    back, front = c - d * L, c + d * L
    body = HD.polygon([tuple(back + n * Rr), tuple(front + n * Rr), tuple(front - n * Rr), tuple(back - n * Rr)])
    cv.fill(body, 'bg0')
    rel = np.stack([xx - c[0], yy - c[1]], -1)
    side = rel @ n                                                       # 원통 옆 좌표(위쪽 = 빛 받음)
    cv.fill(body & (side * (1 if n[1] < 0 else -1) > 6), 'gray0')
    cv.fill(P.outline_of(~body, 1) & body, 'bg00')
    back_cap = ((rel - (back - c)) @ d) ** 2 / 4 ** 2 + ((rel - (back - c)) @ n) ** 2 / Rr ** 2 <= 1
    cv.fill(back_cap, 'gray0')
    lr = rel - (front - c)
    lens = (lr @ d) ** 2 / 4 ** 2 + (lr @ n) ** 2 / (Rr - 1) ** 2 <= 1
    cv.fill(lens, 'gray0')
    cv.fill(lens & ((lr @ d) ** 2 / 2.5 ** 2 + (lr @ n) ** 2 / (Rr - 4) ** 2 <= 1), 'bg00')
    for sgn in (1, -1):                                                  # 빛가리개(앞으로 벌어진 판 2장)
        p0 = front + n * sgn * Rr
        p1 = p0 + d * 9 + n * sgn * 5
        thick_line(cv, tuple(int(v) for v in p0), tuple(int(v) for v in p1), 'bg00', 2)
    cv.fill(rect(cv, cx - 3, 36, cx + 4, 40), 'gray0')                   # 걸이 고정쇠


def server_rack(cv):
    yy, xx = yx()
    x0, y0, x1, y1 = 4, 62, 78, 204
    cab = rect(cv, x0, y0, x1, y1)
    cv.fill(cab, "bg0")
    cv.fill(rect(cv, x0, y0, x1, y0 + 2) | rect(cv, x0, y0, x0 + 2, y1), "gray0")
    cv.fill(rect(cv, x1 - 2, y0, x1, y1), "bg00")
    # 테이프 장치: 릴 2개
    for rx in (x0 + 20, x0 + 52):
        reel = (xx - rx) ** 2 + (yy - (y0 + 22)) ** 2 <= 13 ** 2
        cv.fill(reel, "gray0")
        cv.fill(reel & ((xx - rx) ** 2 + (yy - (y0 + 22)) ** 2 <= 10 ** 2), "bg00")
        cv.fill(reel & ((xx - rx) ** 2 + (yy - (y0 + 22)) ** 2 <= 4 ** 2), "gray1")
        for k in range(3):
            a = k * 2.094
            P.line(cv, (rx, y0 + 22), (int(rx + 9 * np.cos(a)), int(y0 + 22 + 9 * np.sin(a))), "gray0")
    # 장비 칸 4개(앞판 + 통풍 홈 + LED 구멍)
    for k, uy in enumerate(range(y0 + 42, y1 - 10, 24)):
        u = rect(cv, x0 + 4, uy, x1 - 4, uy + 20)
        cv.fill(u, "gray0")
        sparse(cv, u, "bg0", 0.35)
        cv.fill(rect(cv, x0 + 4, uy, x1 - 4, uy + 1), "gray1")
        for vx in range(x0 + 30, x1 - 8, 4):
            cv.fill(rect(cv, vx, uy + 5, vx + 2, uy + 15), "bg00")
        for lx in range(x0 + 8, x0 + 26, 5):
            cv.fill(rect(cv, lx, uy + 8, lx + 3, uy + 11), "bg00")          # LED 자리(겹침 그림이 켬)


def cabinet(cv):
    yy, xx = yx()
    # 벽 단말(작은 화면) — 초록 글자 줄(실제 글자 아님)
    tx0, ty0, tx1, ty1 = 572, 50, 632, 96
    cv.fill(rect(cv, tx0, ty0, tx1, ty1), "gray0")
    cv.fill(rect(cv, tx0, ty0, tx1, ty0 + 1), "gray1")
    cv.fill(rect(cv, tx0 + 4, ty0 + 4, tx1 - 4, ty1 - 8), "bg00")
    for k, n in enumerate((30, 22, 38, 16, 26)):
        cv.fill(rect(cv, tx0 + 7, ty0 + 7 + k * 6, tx0 + 7 + n, ty0 + 9 + k * 6), "green0")
    cv.fill(rect(cv, tx0 + 4, ty1 - 6, tx1 - 4, ty1 - 4), "bg0")
    thick_line(cv, (tx1 - 8, ty1), (tx1 - 2, 206), "bg00", 2)            # 전선
    # 서류 캐비닛(서랍 3개) + 삐져나온 영수증
    x0, y0, x1, y1 = 566, 112, 636, 206
    cv.fill(rect(cv, x0, y0, x1, y1), "gray0")
    sparse(cv, rect(cv, x0, y0, x1, y1), "bg0", 0.3)
    cv.fill(rect(cv, x0, y0, x1, y0 + 2), "gray1")
    cv.fill(rect(cv, x1 - 2, y0, x1, y1), "bg00")
    for k, dy in enumerate((y0 + 6, y0 + 36, y0 + 66)):
        cv.fill(rect(cv, x0 + 4, dy, x1 - 6, dy + 26), "bg0")
        cv.fill(rect(cv, x0 + 4, dy, x1 - 6, dy + 1), "gray0")
        cv.fill(rect(cv, x0 + 26, dy + 10, x0 + 44, dy + 13), "gray1")      # 손잡이
        cv.fill(rect(cv, x0 + 30, dy + 4, x0 + 40, dy + 8), "bg00")        # 이름표 칸
    for (px, py, w, h) in ((x0 + 8, y0 + 31, 18, 6), (x0 + 40, y0 + 29, 14, 8), (x0 + 12, y0 + 61, 22, 5)):
        cv.fill(rect(cv, px, py, px + w, py + h), "cream0")
        cv.fill(rect(cv, px, py + h - 1, px + w, py + h), "gray1")
    # 캐비닛 위 서류 더미
    cv.fill(rect(cv, x0 + 6, y0 - 8, x0 + 40, y0), "cream0")
    cv.fill(rect(cv, x0 + 6, y0 - 3, x0 + 40, y0 - 2), "gray1")
    cv.fill(rect(cv, x0 + 10, y0 - 12, x0 + 36, y0 - 8), "cream")


def room(pal, style):
    cv = Canvas(pal, W, H)
    wall(cv, WALL[style])
    ticker_board(cv)
    pipes(cv)
    spotlight(cv, 150, flip=False)
    spotlight(cv, 490, flip=True)
    server_rack(cv)
    cabinet(cv)
    return cv


def leds(pal, phase):
    """LED만 있는 겹침 그림(투명). phase 0/1 = 번갈아 켜는 두 무리."""
    cv = Canvas(pal, W, H)
    x0, y0 = 4, 62
    for k, uy in enumerate(range(y0 + 42, 204 - 10, 24)):
        for j, lx in enumerate(range(x0 + 8, x0 + 26, 5)):
            if (j + k + phase) % 2 == 0:
                cv.fill((slice(uy + 8, uy + 11), slice(lx, lx + 3)), "green2" if (j + k) % 3 else "red3")
    # 벽 단말 커서(깜빡)
    if phase == 0:
        cv.fill((slice(57 + 5 * 6, 59 + 5 * 6), slice(579, 583)), "green2")
    # 시세판 위 작은 상태등
    cv.fill((slice(36, 38), slice(BOARD[2] - 14 + 4 * phase, BOARD[2] - 12 + 4 * phase)), "red3")
    return cv


# ---------------- 테이블 ----------------
def table_base(pal):
    import make_art_trade as T
    keep = T.draw_mat
    T.draw_mat = lambda *a, **k: None
    try:
        return T.wide_table(pal)        # 640×160 = 640 격자 y 200~360
    finally:
        T.draw_mat = keep


def quad(near_y, x_l, x_r, far_y, oy=200):
    """테이블 위 사각형(원근): 가까운 끝 near_y의 x_l~x_r, 먼 끝 far_y는 소실점 쪽으로 좁힘. 반환 = 꼭짓점(그림 좌표)."""
    fl, fr = HD.toward_vp(x_l, near_y, far_y), HD.toward_vp(x_r, near_y, far_y)
    return [(fl, far_y - oy), (fr, far_y - oy), (x_r, near_y - oy), (x_l, near_y - oy)]


def paint_quad(cv, pts, t=2):
    """닳은 페인트 선: 꼭짓점 사이 굵기 t, 바둑판으로 70%만(닳음)."""
    m = np.zeros((cv.h, cv.w), bool)
    sub = Canvas(cv.pal, cv.w, cv.h)
    for a, b in zip(pts, pts[1:] + pts[:1]):
        for k in range(t):
            P.line(sub, (int(a[0]), int(a[1]) + k), (int(b[0]), int(b[1]) + k), "cream0")
    m = sub.mask
    sparse(cv, m, "cream0", 0.55)
    sparse(cv, m, "gray1", 0.85)


def table(pal):
    cv = table_base(pal)
    yy, xx = yx(cv.h, cv.w)
    on_table = cv.mask.copy()
    # 페인트 선: 가운데 가로선(광대 쪽·내 쪽 나눔), 광대 자리·내 자리 사각형(폰 자리 둘레)
    sub = Canvas(pal, cv.w, cv.h)
    gy = 272 - 200
    P.line(sub, (0, gy), (cv.w - 1, gy), "cream0")
    P.line(sub, (0, gy + 1), (cv.w - 1, gy + 1), "cream0")
    sparse(cv, sub.mask & on_table, "gray1", 0.8)
    sparse(cv, sub.mask & on_table, "cream0", 0.4)
    paint_quad(cv, quad(258, 446, 548, 204))      # 광대 자리(광대 폰 둘레)
    paint_quad(cv, quad(356, 450, 568, 288))      # 내 자리(내 폰 둘레, 앞 끝은 화면 밖)
    # 소품 — 왼쪽 앞: 영수증 뭉치, 커피잔, 재떨이 / 오른쪽 뒤: 계산기 (그림자 먼저)
    oy = 200

    def R(x0, y0, x1, y1):
        return rect(cv, x0, y0 - oy, x1, y1 - oy)
    # 영수증 뭉치(겹친 종이 3장, 살짝 어긋나게)
    sparse(cv, R(30, 336, 98, 350), "bg00", 0.6)
    for k, (dx, dy) in enumerate(((0, 0), (4, -3), (-3, -6))):
        cv.fill(R(30 + dx, 322 + dy, 88 + dx, 342 + dy), "cream0" if k < 2 else "cream")
        for ly in range(326 + dy, 340 + dy, 4):
            cv.fill(R(36 + dx, ly, 72 + dx, ly + 1), "gray1")
        cv.fill(R(30 + dx, 341 + dy, 88 + dx, 342 + dy), "gray1")
    # 커피잔(옆에서 본 원통 + 손잡이 + 위 커피 타원)
    mx, my = 118, 304
    sparse(cv, R(mx - 4, my + 30, mx + 30, my + 36), "bg00", 0.6)
    cv.fill(R(mx, my + 4, mx + 22, my + 32), "gray1")
    cv.fill(R(mx + 16, my + 4, mx + 22, my + 32), "gray0")
    cv.fill(R(mx, my + 4, mx + 3, my + 32), "cream0")
    top = ((xx - (mx + 11)) ** 2 / 11 ** 2 + (yy - (my + 4 - oy)) ** 2 / 4 ** 2 <= 1)
    cv.fill(top, "gray1")
    cv.fill(top & ((xx - (mx + 11)) ** 2 / 9 ** 2 + (yy - (my + 4 - oy)) ** 2 / 3 ** 2 <= 1), "wood0")
    handle = R(mx + 22, my + 10, mx + 30, my + 24) & ~R(mx + 22, my + 13, mx + 27, my + 21)
    cv.fill(handle, "gray0")
    cv.fill(R(mx + 4, my + 14, mx + 14, my + 18), "red0")     # 잔에 붙은 빨간 띠(무늬)
    # 재떨이(낮은 타원 접시 + 꽁초 2개 + 재)
    ax, ay = 172, 338
    dish = ((xx - ax) ** 2 / 20 ** 2 + (yy - (ay - oy)) ** 2 / 7 ** 2 <= 1)
    sparse(cv, ((xx - ax - 3) ** 2 / 22 ** 2 + (yy - (ay + 4 - oy)) ** 2 / 8 ** 2 <= 1), "bg00", 0.6)
    cv.fill(dish, "gray0")
    cv.fill(dish & ((xx - ax) ** 2 / 15 ** 2 + (yy - (ay - 1 - oy)) ** 2 / 4 ** 2 <= 1), "bg0")
    sparse(cv, dish & ((xx - ax) ** 2 / 12 ** 2 + (yy - (ay - 1 - oy)) ** 2 / 3 ** 2 <= 1), "gray1", 0.35)
    for (bx, by, bx2, by2) in ((ax - 8, ay - 2, ax + 2, ay - 5), (ax + 2, ay, ax + 14, ay - 1)):
        thick_line(cv, (bx, by - oy), (bx2, by2 - oy), "cream0", 2)
        cv.fill(R(bx2 - 1, by2 - 1, bx2 + 2, by2 + 1), "orange")           # 탄 끝
    # 계산기(오른쪽 뒤, 테이블에 눕힘 — 위에서 비스듬히 본 판)
    cx0, cy0 = 572, 214
    calc = R(cx0, cy0, cx0 + 36, cy0 + 22)
    sparse(cv, R(cx0 + 2, cy0 + 20, cx0 + 40, cy0 + 25), "bg00", 0.6)
    cv.fill(calc, "gray0")
    cv.fill(R(cx0, cy0, cx0 + 36, cy0 + 1), "gray1")
    cv.fill(R(cx0 + 4, cy0 + 3, cx0 + 32, cy0 + 8), "bg00")
    cv.fill(R(cx0 + 18, cy0 + 5, cx0 + 30, cy0 + 7), "green0")             # 화면 숫자(흐리게)
    for ky in range(cy0 + 10, cy0 + 20, 4):
        for kx in range(cx0 + 4, cx0 + 32, 6):
            cv.fill(R(kx, ky, kx + 4, ky + 2), "bg0")
    return cv


def main():
    pal = load_pal()
    files = []
    tbl = table(pal)
    for style in ("A", "B"):
        d = OUT / style
        d.mkdir(parents=True, exist_ok=True)
        r = room(pal, style)
        write_png(d / "room.png", r.rgba())
        files.append(d / "room.png")
        # 미리보기: 배경 + 테이블(y 200) + LED a
        pv = r.rgba().copy()
        t = tbl.rgba()
        a = t[..., 3] > 0
        pv[200:360][a] = t[a]
        la = leds(pal, 0).rgba()
        a = la[..., 3] > 0
        pv[a] = la[a]
        pv[..., 3] = 255
        write_png(OUT / f"preview-{style}-x2.png", pv.repeat(2, 0).repeat(2, 1))
    write_png(OUT / "table.png", tbl.rgba())
    files.append(OUT / "table.png")
    for ph in (0, 1):
        write_png(OUT / f"room-leds-{'ab'[ph]}.png", leds(pal, ph).rgba())
        files.append(OUT / f"room-leds-{'ab'[ph]}.png")
    print("팔레트 검사 (32색):")
    return check32(files, pal)


if __name__ == "__main__":
    sys.exit(0 if main() else 1)

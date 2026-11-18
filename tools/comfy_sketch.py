"""Falling Knife img2img 구도 밑그림 (AI 작성, comfy-local Ticket 51 3차).

사용법 (C:\\Falling_Knife 에서):
    python tools/comfy_sketch.py   # → tools/comfy/title-bg-sketch.png (1344×768)

1·2차는 글(프롬프트)만으로 구도를 정하다 실패(너무 밝음·왼쪽 안 빔·물건 많음). 3차는 이 밑그림을 바탕으로
그리게 해서(img2img) 구도를 고정한다. 사용자 결정(2026-11-18): 배경 선(차트) 움직임은 게임 코드가 맡고,
Comfy는 **배경만** → 밑그림에도 차트 없음.
- 왼쪽 45%: 거의 검정(제목 글자 자리)
- 뒤 벽(위 62%): 아주 어두운 남색, 아래는 더 어두운 바닥
- 오른쪽(가로 약 72%): 천장에서 내려오는 줄 + 전등갓, 아래로 퍼지는 노란 빛 원뿔(부드럽게), 그 아래 작은 테이블
- 부드러운 그라데이션만(도트 아님) — 모델이 질감을 입힘. 무작위 없음.
"""
from pathlib import Path

import numpy as np

import sys
sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import write_png  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "tools" / "comfy" / "title-bg-sketch.png"
W, H = 1344, 768


def main():
    yy, xx = np.mgrid[0:H, 0:W].astype(np.float64)
    img = np.zeros((H, W, 3))
    wall = np.array([18, 22, 38.0])
    floor = np.array([12, 10, 10.0])
    horizon = 0.62 * H
    img[:] = np.where((yy < horizon)[..., None], wall, floor)
    # 왼쪽으로 갈수록 검정(제목 자리)
    left = np.clip((xx / W - 0.05) / 0.45, 0, 1)[..., None]
    img *= 0.35 + 0.65 * left
    lx, top, shade_y = 0.72 * W, 0.0, 0.18 * H
    # 노란 빛 원뿔(전등 아래 → 테이블)
    depth = np.clip((yy - shade_y) / (0.62 * H), 0, 1)
    half = 40 + depth * 260
    cone = np.clip(1 - np.abs(xx - lx) / half, 0, 1) * (yy > shade_y) * (1 - 0.5 * depth)
    img += cone[..., None] ** 1.5 * np.array([150, 115, 45.0])
    # 테이블 위 밝은 웅덩이
    pool = np.exp(-(((xx - lx) / 230) ** 2 + ((yy - 0.66 * H) / 45) ** 2))
    img += pool[..., None] * np.array([170, 130, 55.0])
    # 테이블(어두운 나무 윗면 + 다리)
    tbl = (np.abs(xx - lx) < 170) & (yy > 0.64 * H) & (yy < 0.69 * H)
    img[tbl] = img[tbl] * 0.4 + np.array([90, 60, 30.0])
    legs = ((np.abs(xx - (lx - 150)) < 10) | (np.abs(xx - (lx + 150)) < 10)) & (yy >= 0.69 * H) & (yy < 0.84 * H)
    img[legs] = np.array([30, 20, 14.0])
    # 줄과 전등갓
    cord = (np.abs(xx - lx) < 3) & (yy < shade_y - 30)
    img[cord] = np.array([40, 40, 45.0])
    shade = (yy >= shade_y - 32) & (yy < shade_y) & (np.abs(xx - lx) < 30 + (yy - (shade_y - 32)) * 2.2)
    img[shade] = np.array([30, 36, 60.0])
    bulb = ((xx - lx) / 26) ** 2 + ((yy - shade_y) / 9) ** 2 < 1
    img[bulb] = np.array([255, 235, 170.0])
    out = np.full((H, W, 4), 255, np.uint8)
    out[..., :3] = np.clip(img, 0, 255).astype(np.uint8)
    OUT.parent.mkdir(parents=True, exist_ok=True)
    write_png(OUT, out)
    print(OUT)


# ---------- 4차: 우리 그림으로 밑그림 ----------
# 3차 밑그림(부드러운 그라데이션)은 우리 그림과 달라 결과도 달랐음(사용자 "우리가 만든 아트워크랑 너무 다름").
# → 게임에 실제로 쓰는 스프라이트(방·테이블·전등·빛줄기)를 640 격자 그대로 겹치고, 조명(전체 0.35·남색 + 노란 원뿔,
#   lighting 49 값)을 흉내 낸 뒤 정확히 2배(1280×720)로 키움. 생성 뒤 후처리에서 정확히 1/2로 줄이면 도트 자리가 맞음.
SPR = ROOT / "Assets" / "_Project" / "Sprites" / "Play"


def place(canvas, img, col, row, alpha=1.0):
    h, w = img.shape[:2]
    c0, r0 = max(col, 0), max(row, 0)
    c1, r1 = min(col + w, canvas.shape[1]), min(row + h, canvas.shape[0])
    sub = img[r0 - row:r1 - row, c0 - col:c1 - col].astype(np.float64)
    a = (sub[..., 3:4] / 255.0) * alpha
    canvas[r0:r1, c0:c1] = canvas[r0:r1, c0:c1] * (1 - a) + sub[..., :3] * a


def main_ours():
    from make_art import read_png
    room = read_png(SPR / "room.png")
    cv = np.zeros((360, 640, 3))
    place(cv, room, 0, 0)
    place(cv, read_png(SPR / "table.png"), 0, 200)             # Table: 월드 y −10 ~ −90 → 줄 200 ~ 360
    # 조명 흉내(Light 2D 값): 전체 0.35 × (0.75, 0.8, 1) + 전등(줄 24, 가운데 320)에서 아래로 노란 원뿔
    yy, xx = np.mgrid[0:360, 0:640].astype(np.float64)
    dx, dy = xx - 320, yy - 24
    r = np.hypot(dx, dy)
    ang = np.degrees(np.arctan2(np.abs(dx), np.maximum(dy, 1e-6)))
    spot = np.clip((50 - ang) / 30, 0, 1) * np.clip(1 - (r - 120) / 260, 0, 1) * (dy > 0)
    light = 0.35 * np.array([0.75, 0.8, 1.0]) + (1.3 * spot)[..., None] * np.array([0.85, 0.71, 0.35])
    cv = cv * light
    place(cv, read_png(SPR / "lamp-beam.png"), 100, 22, alpha=0.2)   # 조명 안 받음
    place(cv, read_png(SPR / "lamp.png"), 280, 0)
    out = np.full((720, 1280, 4), 255, np.uint8)
    out[..., :3] = np.clip(cv, 0, 255).astype(np.uint8).repeat(2, 0).repeat(2, 1)
    p = ROOT / "tools" / "comfy" / "title-bg-ours.png"
    write_png(p, out)
    print(p)


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "ours":
        main_ours()
    else:
        main()

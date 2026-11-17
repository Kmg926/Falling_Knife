"""Falling Knife 방 배경(고정 선 없음) (AI 작성, stock-screen Ticket 41).

사용법 (C:\\Falling_Knife 에서):
    python tools/make_art_stock.py room   # 640x360 방 배경 → .scratch/art/drafts/hd/stock/room.png + 팔레트 검사

- 예전 방 그림(320x180)에는 스케치의 빨간 차트 선이 그려져 있었다 → 움직이는 시세 선(ChartLinesView)과 겹쳐 보여서 뺀다.
- 어두운 벽(bg0) + 아주 옅은 세로 판자 이음새(bg00) + 테이블 바로 위 걸레받이 줄. 선·광대·글자를 방해하지 않게 대비를 낮게.
- 32색, 무작위 없음.
"""
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from make_art import write_png  # noqa: E402
from make_art_hd import Canvas, check32, load_pal  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "hd" / "stock"


def room(pal):
    w, h = 640, 360
    cv = Canvas(pal, w, h)
    cv.fill((slice(0, h), slice(0, w)), "bg0")
    for x in range(40, w, 80):                                 # 세로 판자 이음새(새 도트 1줄, 위아래로 끊긴 점선)
        for y in range(0, 196, 1):
            if (y // 3) % 4 != 3:
                cv.fill((y, x), "bg00")
    cv.fill((slice(188, 190), slice(0, w)), "bg00")            # 걸레받이(테이블 먼 끝 y 200 바로 위)
    cv.fill((slice(190, 191), slice(0, w)), "navy0")
    return cv


def make_room(pal):
    OUT.mkdir(parents=True, exist_ok=True)
    p = OUT / "room.png"
    write_png(p, room(pal).rgba())
    print("팔레트 검사 (32색):")
    return check32([p], pal)


if __name__ == "__main__":
    cmd = sys.argv[1] if len(sys.argv) > 1 else ""
    if cmd == "room":
        sys.exit(0 if make_room(load_pal()) else 1)
    print(__doc__)

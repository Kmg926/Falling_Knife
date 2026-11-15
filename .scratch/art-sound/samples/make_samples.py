"""시험 샘플: 도트 아이콘(찌라시·캔들 2종)과 효과음(충격음·초침) 생성. 외부 라이브러리 없이 numpy + zlib."""
import os, sys, zlib, struct, wave
import numpy as np

OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)

# ---------- PNG 쓰기 (RGBA) ----------
def write_png(path, rgba):
    h, w, _ = rgba.shape
    raw = b"".join(b"\x00" + rgba[y].astype(np.uint8).tobytes() for y in range(h))
    def chunk(t, d):
        c = struct.pack(">I", len(d)) + t + d
        return c + struct.pack(">I", zlib.crc32(t + d) & 0xFFFFFFFF)
    png = b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, 6, 0, 0, 0))
    png += chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b"")
    open(path, "wb").write(png)

# 채도를 낮춘 팔레트 (GDD: 16색 이내, 상승 초록·하락 빨강만 색)
PAL = {
    ".": (0, 0, 0, 0),
    "k": (14, 13, 18, 255),     # 거의 검정 (외곽선)
    "d": (46, 44, 52, 255),     # 어두운 회색
    "m": (92, 88, 96, 255),     # 중간 회색
    "l": (156, 150, 140, 255),  # 밝은 회색(종이)
    "w": (214, 206, 188, 255),  # 바랜 흰색
    "r": (168, 58, 52, 255),    # 하락 빨강
    "R": (112, 36, 34, 255),    # 하락 빨강 어둠
    "g": (74, 150, 92, 255),    # 상승 초록
    "G": (44, 96, 58, 255),     # 상승 초록 어둠
}

def sprite(rows):
    return np.array([[PAL[c] for c in row] for row in rows], dtype=np.uint8)

def upscale(img, s):
    return img.repeat(s, axis=0).repeat(s, axis=1)

rumor = sprite([  # 14x14 찌라시: 접힌 종이 + 빨간 하락 화살표
    "..............",
    "..kkkkkkkkk...",
    "..kwwwwwwwkk..",
    "..kwmmmmwwlk..",
    "..kwwwwwwwwk..",
    "..kwmmmmmmwk..",
    "..kwwwwrwwwk..",
    "..kwmmwrwmwk..",
    "..kwwrrrrrwk..",
    "..kwwwrrrwwk..",
    "..kwwwwrwwwk..",
    "..kwmmmmmmwk..",
    "..kkkkkkkkkk..",
    "..............",
])
candle_up = sprite([  # 5x16 상승 캔들
    "..k..", "..G..", "..G..", "kgggk", "kgggk", "kgggk", "kgggk", "kgggk",
    "kgggk", "kgggk", "kgggk", "kGGGk", "..G..", "..G..", "..k..", ".....",
])
candle_down = sprite([  # 5x16 하락 캔들
    "..k..", "..R..", "..R..", "krrrk", "krrrk", "krrrk", "krrrk", "krrrk",
    "krrrk", "krrrk", "krrrk", "kRRRk", "..R..", "..R..", "..k..", ".....",
])

write_png(os.path.join(OUT, "icon_rumor_14.png"), rumor)
# 미리보기: 8배 확대, 어두운 배경 위에 나란히
bg = np.zeros((16 * 8, (14 + 2 + 5 + 2 + 5) * 8, 4), dtype=np.uint8)
bg[:, :] = (20, 18, 24, 255)
def paste(dst, img, x, y):
    a = img[..., 3:4] / 255.0
    region = dst[y:y + img.shape[0], x:x + img.shape[1]]
    region[...] = (img * a + region * (1 - a)).astype(np.uint8)
paste(bg, upscale(rumor, 8), 0, 8)
paste(bg, upscale(candle_up, 8), (14 + 2) * 8, 0)
paste(bg, upscale(candle_down, 8), (14 + 2 + 5 + 2) * 8, 0)
write_png(os.path.join(OUT, "preview_x8.png"), bg)

# ---------- WAV 쓰기 ----------
SR = 44100
def write_wav(path, x, peak_db=-3.0):
    x = x / (np.max(np.abs(x)) + 1e-9) * (10 ** (peak_db / 20))
    data = (x * 32767).astype("<i2").tobytes()
    with wave.open(path, "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes(data)

rng = np.random.default_rng(7)

# 둔탁한 충격음 0.35초: 120→45Hz로 떨어지는 사인 + 짧은 잡음, 빠른 감쇠, 앞 무음 없음
t = np.arange(int(SR * 0.35)) / SR
freq = 45 + 75 * np.exp(-t * 18)
phase = 2 * np.pi * np.cumsum(freq) / SR
body = np.sin(phase) * np.exp(-t * 9)
noise = rng.standard_normal(len(t))
noise = np.convolve(noise, np.ones(40) / 40, mode="same") * np.exp(-t * 60) * 0.6
thud = body + noise
fade = np.ones_like(thud); fade[-400:] = np.linspace(1, 0, 400)
write_wav(os.path.join(OUT, "sfx_impact.wav"), thud * fade)

# 시계 초침 1회 0.06초: 높은 대역 짧은 클릭 두 겹
t2 = np.arange(int(SR * 0.06)) / SR
click = (np.sin(2 * np.pi * 3200 * t2) * 0.5 + np.sin(2 * np.pi * 5400 * t2) * 0.3) * np.exp(-t2 * 160)
click += rng.standard_normal(len(t2)) * np.exp(-t2 * 400) * 0.3
write_wav(os.path.join(OUT, "sfx_tick.wav"), click, peak_db=-6.0)

print("ok", os.listdir(OUT))

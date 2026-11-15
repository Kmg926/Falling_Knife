"""Falling Knife 효과음 후보 만들기 (AI 작성 도구, ADR 0003 / sound-feel Grilling Q3·Q7).

외부 소리 파일·모델 없이 numpy로 파형을 계산해 합성한다.
소리마다 후보 A·B·C(톤·감쇠만 다름)를 만들고, 길이·앞 무음·최대 음량을 측정한다.
고르는 것은 사람이 한다(AI는 측정만).

사용법:
    python tools/make_sfx.py <출력 폴더>
    예) python tools/make_sfx.py .scratch/art-sound/candidates
"""
import os
import sys
import wave

import numpy as np

SR = 44100          # 44.1kHz
SEED = 20261115     # 같은 코드면 같은 소리가 나오게 고정


# ---------- 공통 ----------
def t_axis(seconds):
    return np.arange(int(SR * seconds)) / SR


def smooth_noise(rng, n, width):
    """잡음을 이동평균으로 부드럽게(높은 소리 줄이기)."""
    x = rng.standard_normal(n)
    if width > 1:
        x = np.convolve(x, np.ones(width) / width, mode="same")
    return x


def fade_out(x, ms=5):
    n = min(len(x), int(SR * ms / 1000))
    x = x.copy()
    x[-n:] *= np.linspace(1, 0, n)
    return x


def normalize(x, peak_db):
    return x / (np.max(np.abs(x)) + 1e-12) * (10 ** (peak_db / 20))


def write_wav(path, x):
    data = (np.clip(x, -1, 1) * 32767).astype("<i2").tobytes()
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data)


def measure(x):
    """길이(초), 앞 무음(초, -50 dBFS 미만), 최대 음량(dBFS)."""
    level = np.abs(x)
    above = np.nonzero(level > 10 ** (-50 / 20))[0]
    lead = above[0] / SR if len(above) else len(x) / SR
    peak = 20 * np.log10(np.max(level) + 1e-12)
    return len(x) / SR, lead, peak


# ---------- 충격음 (0.3~0.4초): 하락으로 잔고가 줄 때 ----------
def impact(rng, variant):
    if variant == "A":   # 둔탁: 110→45Hz로 떨어지는 사인 + 짧은 잡음
        t = t_axis(0.35)
        f = 45 + 65 * np.exp(-t * 18)
        body = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t * 9)
        hit = smooth_noise(rng, len(t), 40) * np.exp(-t * 60) * 0.6
        x = body + hit
    elif variant == "B":  # 더 낮고 무거움: 38Hz 바닥 + 날카로운 첫 타격
        t = t_axis(0.4)
        f = 38 + 50 * np.exp(-t * 14)
        body = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t * 7)
        sub = np.sin(2 * np.pi * 30 * t) * np.exp(-t * 10) * 0.5
        hit = smooth_noise(rng, len(t), 8) * np.exp(-t * 120) * 0.8
        x = body + sub + hit
    else:                 # C: 책상을 내리치는 느낌(나무 공명 + 잡음)
        t = t_axis(0.3)
        f = 70 + 90 * np.exp(-t * 30)
        body = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t * 14)
        wood = np.sin(2 * np.pi * 180 * t) * np.exp(-t * 35) * 0.35
        hit = smooth_noise(rng, len(t), 20) * np.exp(-t * 80) * 0.7
        x = body + wood + hit
    return normalize(fade_out(x), -3.0)


# ---------- 초침 (0.05~0.08초): 캔들 공개 ----------
def tick(rng, variant):
    if variant == "A":   # 높은 시계 초침
        t = t_axis(0.06)
        x = (np.sin(2 * np.pi * 3200 * t) * 0.5 + np.sin(2 * np.pi * 5400 * t) * 0.3) * np.exp(-t * 160)
        x += rng.standard_normal(len(t)) * np.exp(-t * 400) * 0.3
    elif variant == "B":  # 낮은 나무 "톡"
        t = t_axis(0.07)
        x = (np.sin(2 * np.pi * 1200 * t) + np.sin(2 * np.pi * 1900 * t) * 0.4) * np.exp(-t * 110)
        x += smooth_noise(rng, len(t), 4) * np.exp(-t * 300) * 0.25
    else:                 # C: 기계식 "틱-" (짧은 두 겹)
        t = t_axis(0.08)
        first = np.sin(2 * np.pi * 2600 * t) * np.exp(-t * 220)
        second = np.where(t > 0.018, np.sin(2 * np.pi * 2200 * (t - 0.018)) * np.exp(-(t - 0.018) * 260) * 0.6, 0)
        x = first + second + rng.standard_normal(len(t)) * np.exp(-t * 500) * 0.2
    return normalize(fade_out(x, 3), -9.0)


# ---------- 클릭 (0.03~0.05초): 선택 이동, 아이템 확인·취소 ----------
def click(rng, variant):
    if variant == "A":   # 부드러운 버튼
        t = t_axis(0.04)
        x = np.sin(2 * np.pi * 900 * t) * np.exp(-t * 180)
        x += smooth_noise(rng, len(t), 6) * np.exp(-t * 500) * 0.3
    elif variant == "B":  # 또렷한 스위치
        t = t_axis(0.03)
        x = np.sin(2 * np.pi * 2000 * t) * np.exp(-t * 300)
        x += rng.standard_normal(len(t)) * np.exp(-t * 900) * 0.4
    else:                 # C: 낮고 먹먹한 키
        t = t_axis(0.05)
        x = np.sin(2 * np.pi * 420 * t) * np.exp(-t * 140)
        x += smooth_noise(rng, len(t), 12) * np.exp(-t * 400) * 0.35
    return normalize(fade_out(x, 3), -12.0)


# ---------- 셔플 (0.5~0.8초): 덱 재장전 ----------
def shuffle(rng, variant):
    if variant == "A":
        seconds, n_flicks, width, decay = 0.6, 14, 6, 90
    elif variant == "B":
        seconds, n_flicks, width, decay = 0.7, 22, 10, 120
    else:
        seconds, n_flicks, width, decay = 0.55, 10, 4, 70
    t = t_axis(seconds)
    x = np.zeros(len(t))
    # 카드가 촤라락 넘어가는 작은 소리들: 앞쪽은 빽빽하고 뒤로 갈수록 성기게
    starts = np.sort((np.linspace(0, 1, n_flicks) ** 1.4) * (seconds - 0.08))
    for i, s in enumerate(starts):
        idx = int(s * SR)
        n = int(0.04 * SR)
        seg_t = np.arange(n) / SR
        flick = smooth_noise(rng, n, width) * np.exp(-seg_t * decay) * (0.6 + 0.4 * rng.random())
        end = min(len(x), idx + n)
        x[idx:end] += flick[: end - idx]
    if variant == "C":   # 마지막에 덱을 탁 정리하는 소리
        idx = int((seconds - 0.07) * SR)
        n = len(x) - idx
        seg_t = np.arange(n) / SR
        x[idx:] += np.sin(2 * np.pi * 160 * seg_t) * np.exp(-seg_t * 60) * 1.2
    return normalize(fade_out(x, 8), -6.0)


SOUNDS = [
    ("impact", impact, (0.3, 0.4)),
    ("tick", tick, (0.05, 0.08)),
    ("click", click, (0.03, 0.05)),
    ("shuffle", shuffle, (0.5, 0.8)),
]


def main():
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(".scratch", "art-sound", "candidates")
    os.makedirs(out, exist_ok=True)
    rng = np.random.default_rng(SEED)
    rows = []
    for name, make, (lo, hi) in SOUNDS:
        for v in "ABC":
            x = make(rng, v)
            path = os.path.join(out, f"{name}-{v}.wav")
            write_wav(path, x)
            length, lead, peak = measure(x)
            ok = "OK" if lo <= length + 1e-9 and length <= hi + 1e-9 else "범위 밖"
            rows.append(f"| {name}-{v}.wav | {length:.3f} | {lo}~{hi} | {ok} | {lead * 1000:.1f} | {peak:.1f} |")
    table = "\n".join([
        "| 파일 | 길이(초) | 정한 범위 | 범위 | 앞 무음(ms, -50dBFS 미만) | 최대(dBFS) |",
        "|---|---|---|---|---|---|",
        *rows,
    ])
    with open(os.path.join(out, "measurements.md"), "w", encoding="utf-8") as f:
        f.write("# 효과음 후보 측정표 (tools/make_sfx.py)\n\n" + table + "\n")
    print(table)


if __name__ == "__main__":
    main()

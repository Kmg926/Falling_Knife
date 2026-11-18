"""Falling Knife 로컬 ComfyUI 생성 (AI 작성, comfy-local Ticket 51).

사용법 (C:\\Falling_Knife 에서, ComfyUI Desktop 실행 중):
    python tools/comfy_generate.py tools/comfy/title-bg.json 101 202 303 404
    python tools/comfy_generate.py tools/comfy/title-bg.json 101 --fresh   # 캐시를 비우고 다시(재현 확인 C2)

- 워크플로 JSON의 {{SEED}}·{{PROMPT}}·{{NEGATIVE}}를 채워 127.0.0.1:8188/prompt 로 보내고, 끝나면 그림을 받아
  .scratch/art/drafts/comfy/<워크플로이름>-<시드>.png 로 저장. 같은 이름 .json에 재현 정보
  (프롬프트·네거티브·시드·모델·LoRA·VAE·크기·걸음 수·샘플러·걸린 시간)를 남긴다(Spec 제안 6).
- --fresh: 생성 전에 /free 로 ComfyUI 캐시·모델을 비움 → 같은 시드를 처음부터 다시 계산.
- 표준 라이브러리만 씀. 무작위는 시드로만.
"""
import json
import sys
import time
import urllib.parse
import urllib.request
from pathlib import Path

URL = "http://127.0.0.1:8188"
ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / ".scratch" / "art" / "drafts" / "comfy"


def post(path, body):
    req = urllib.request.Request(URL + path, data=json.dumps(body).encode(), headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=30) as r:
        data = r.read()
    return json.loads(data) if data else {}


def get(path):
    with urllib.request.urlopen(URL + path, timeout=30) as r:
        return r.read()


def upload(path):
    """img2img 밑그림을 ComfyUI input 폴더로 올림(/upload/image, multipart). 돌려받은 이름을 LoadImage에 씀."""
    data = Path(path).read_bytes()
    b = "----fkboundary7d1"
    body = (f"--{b}\r\nContent-Disposition: form-data; name=\"image\"; filename=\"{Path(path).name}\"\r\n"
            f"Content-Type: image/png\r\n\r\n").encode() + data + (
            f"\r\n--{b}\r\nContent-Disposition: form-data; name=\"overwrite\"\r\n\r\ntrue\r\n--{b}--\r\n").encode()
    req = urllib.request.Request(URL + "/upload/image", data=body, headers={"Content-Type": f"multipart/form-data; boundary={b}"})
    with urllib.request.urlopen(req, timeout=60) as r:
        return json.loads(r.read())["name"]


def fill(graph, seed, prompt, negative, image=""):
    text = json.dumps(graph).replace("{{PROMPT}}", prompt.replace('"', '\\"')).replace("{{NEGATIVE}}", negative.replace('"', '\\"'))
    text = text.replace("{{INPUT_IMAGE}}", image)
    text = text.replace('"{{SEED}}"', str(seed))
    return json.loads(text)


def generate(wf_path, seed, fresh=False):
    wf = json.loads(Path(wf_path).read_text(encoding="utf-8"))
    image = upload(ROOT / wf["input_image"]) if wf.get("input_image") else ""   # 3차: img2img 밑그림
    graph = fill(wf["graph"], seed, wf["prompt"], wf["negative"], image)
    if fresh:
        post("/free", {"unload_models": True, "free_memory": True})
        time.sleep(2)
    t0 = time.time()
    pid = post("/prompt", {"prompt": graph})["prompt_id"]
    while True:
        h = json.loads(get("/history/" + pid))
        if pid in h:
            st = h[pid].get("status", {})
            if st.get("status_str") == "error":
                raise RuntimeError(json.dumps(st)[:800])
            if h[pid].get("outputs"):
                break
        time.sleep(1)
    secs = time.time() - t0
    save = [n for n, v in graph.items() if v["class_type"] == "SaveImage"][0]
    img = h[pid]["outputs"][save]["images"][0]
    q = urllib.parse.urlencode({"filename": img["filename"], "subfolder": img["subfolder"], "type": img["type"]})
    OUT.mkdir(parents=True, exist_ok=True)
    stem = f"{Path(wf_path).stem}-{seed}" + ("-fresh" if fresh else "")
    (OUT / f"{stem}.png").write_bytes(get("/view?" + q))
    ks = [v for v in graph.values() if v["class_type"] == "KSampler"][0]["inputs"]
    lats = [v for v in graph.values() if v["class_type"] == "EmptyLatentImage"]
    lat = lats[0]["inputs"] if lats else {"width": wf.get("size", [0, 0])[0], "height": wf.get("size", [0, 0])[1]}
    info = {
        "workflow": str(Path(wf_path).as_posix()), "seed": seed, "prompt": wf["prompt"], "negative": wf["negative"],
        "checkpoint": graph["1"]["inputs"]["ckpt_name"], "lora": graph["2"]["inputs"]["lora_name"],
        "lora_strength": graph["2"]["inputs"]["strength_model"], "vae": graph["3"]["inputs"]["vae_name"],
        "size": [lat["width"], lat["height"]], "steps": ks["steps"], "cfg": ks["cfg"],
        "sampler": ks["sampler_name"], "scheduler": ks["scheduler"], "denoise": ks.get("denoise", 1.0),
        "input_image": wf.get("input_image", ""), "seconds": round(secs, 1),
        "tool": "ComfyUI (로컬, Comfy Desktop) — 이 파일은 tools/comfy_generate.py가 씀",
    }
    (OUT / f"{stem}.json").write_text(json.dumps(info, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"{stem}: {secs:.1f}s")
    return OUT / f"{stem}.png"


if __name__ == "__main__":
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(1)
    fresh = "--fresh" in sys.argv
    args = [a for a in sys.argv[1:] if a != "--fresh"]
    for s in args[1:]:
        generate(args[0], int(s), fresh)

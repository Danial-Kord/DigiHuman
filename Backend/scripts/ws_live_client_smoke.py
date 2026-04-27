"""
Smoke test for /ws/live_mocap (requires server: python server.py).

  Backend/venv/Scripts/python scripts/ws_live_client_smoke.py

Stops after 30 frames or on error. Close with Ctrl+C to end early.
"""

from __future__ import annotations

import json
import sys

try:
    import websocket
except ImportError:
    print("Install websocket-client: pip install websocket-client", file=sys.stderr)
    sys.exit(1)


def main():
    url = "ws://127.0.0.1:5000/ws/live_mocap"
    max_frames = 30
    n = 0
    ws = websocket.WebSocket()
    ws.connect(url)
    ws.send(json.dumps({"cmd": "start", "camera_id": 0}))
    try:
        while n < max_frames:
            raw = ws.recv()
            if isinstance(raw, bytes):
                raw = raw.decode("utf-8")
            data = json.loads(raw)
            if data.get("error"):
                print("error:", data)
                break
            n += 1
            blends = data.get("faceData", {}).get("blendShapes")
            nb = len(blends) if blends is not None else 0
            preds = len(data.get("bodyPose", {}).get("predictions") or [])
            print(f"frame {n} body_pts={preds} blendShapes={nb}")
    finally:
        try:
            ws.send(json.dumps({"cmd": "stop"}))
        except Exception:
            pass
        ws.close()
    print("done, frames:", n)


if __name__ == "__main__":
    main()

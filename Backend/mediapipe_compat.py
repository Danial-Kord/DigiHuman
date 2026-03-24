"""Helpers for MediaPipe Tasks API (Python 3.13+): legacy mp.solutions was removed."""

from __future__ import annotations

import os
import urllib.request

import cv2

from mediapipe.tasks.python.core import base_options as base_options_module
from mediapipe.tasks.python.vision.core import image as image_module
from mediapipe.tasks.python.vision.core import vision_task_running_mode as running_mode_module

_BaseOptions = base_options_module.BaseOptions
_Image = image_module.Image
_ImageFormat = image_module.ImageFormat
_RunningMode = running_mode_module.VisionTaskRunningMode

_MODEL_DIR = os.path.join(os.path.dirname(__file__), "mediapipe_models")

_MODEL_URLS = {
    "face_landmarker.task": (
        "https://storage.googleapis.com/mediapipe-models/face_landmarker/"
        "face_landmarker/float16/1/face_landmarker.task"
    ),
    "holistic_landmarker.task": (
        "https://storage.googleapis.com/mediapipe-models/holistic_landmarker/"
        "holistic_landmarker/float16/latest/holistic_landmarker.task"
    ),
    "pose_landmarker_full.task": (
        "https://storage.googleapis.com/mediapipe-models/pose_landmarker/"
        "pose_landmarker_full/float16/latest/pose_landmarker_full.task"
    ),
    "hand_landmarker.task": (
        "https://storage.googleapis.com/mediapipe-models/hand_landmarker/"
        "hand_landmarker/float16/latest/hand_landmarker.task"
    ),
}


def model_path(filename: str) -> str:
    """Return absolute path to a bundled MediaPipe model, downloading once if needed."""
    if filename not in _MODEL_URLS:
        raise ValueError(f"Unknown model file: {filename}")
    os.makedirs(_MODEL_DIR, exist_ok=True)
    path = os.path.join(_MODEL_DIR, filename)
    if os.path.isfile(path) and os.path.getsize(path) > 0:
        return path
    url = _MODEL_URLS[filename]
    urllib.request.urlretrieve(url, path)
    return path


def numpy_rgb_to_mp_image(image_rgb: "object") -> _Image:
    """Build MediaPipe Image from a contiguous HxWx3 uint8 RGB ndarray."""
    import numpy as np

    if not image_rgb.flags["C_CONTIGUOUS"]:
        image_rgb = np.ascontiguousarray(image_rgb)
    return _Image(image_format=_ImageFormat.SRGB, data=image_rgb)


class LegacyLandmarkList:
    """Mimics the old protobuf NormalizedLandmarkList (`.landmark` iterable)."""

    __slots__ = ("landmark",)

    def __init__(self, landmarks):
        self.landmark = landmarks


def frame_timestamp_ms(cap, frame_index: int) -> int:
    """Monotonic-ish timestamp in ms for MediaPipe video mode."""
    t = cap.get(cv2.CAP_PROP_POS_MSEC)
    if t and t > 0:
        return int(t)
    return int(frame_index * (1000.0 / 30.0))

"""Live webcam capture: holistic pose + hands + Unity-order blendshapes (one JSON dict per frame)."""

import cv2
import numpy as np
from mediapipe.tasks.python.vision import holistic_landmarker as holistic_landmarker_module

from blendshapes.blendshape_calculator import BlendshapeCalculator
from blendshapes.facedata import FaceData
from face_geometry import PCF, get_metric_landmarks
from mediaPipeFace import blendshapes_for_unity
from mediapipe_compat import frame_timestamp_ms, numpy_rgb_to_mp_image
from pose_estimator import default_holistic_landmarker_options, holistic_result_to_full_pose_dict

_HolisticLandmarker = holistic_landmarker_module.HolisticLandmarker


def iter_live_mocap_frames(camera_id: int = 0):
    """
    Yield one JSON-serializable dict per captured frame:
    bodyPose, handsPose, frame (full pose), plus faceData { blendShapes (40), frame, time }.

    On fatal open failure, yields a single dict with key "error" then returns.
    """
    cap = cv2.VideoCapture(camera_id)
    if not cap.isOpened():
        yield {"error": "could_not_open_camera", "camera_id": camera_id}
        return

    blendshape_calculator = BlendshapeCalculator()
    face_data = FaceData(filter_size=4)
    pcf = None
    options = default_holistic_landmarker_options()
    zero_blend = [0.0] * 40

    try:
        with _HolisticLandmarker.create_from_options(options) as holistic:
            frame_index = 0
            while cap.isOpened():
                success, image = cap.read()
                frame_index += 1
                if not success:
                    continue

                rows, cols = image.shape[:2]
                if pcf is None:
                    focal = float(cols)
                    pcf = PCF(
                        near=1,
                        far=10000,
                        frame_height=float(rows),
                        frame_width=float(cols),
                        fy=focal,
                    )

                frame = int(cap.get(cv2.CAP_PROP_POS_FRAMES))
                image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
                mp_image = numpy_rgb_to_mp_image(image_rgb)
                ts = frame_timestamp_ms(cap, frame_index)
                results = holistic.detect_for_video(mp_image, ts)

                payload = holistic_result_to_full_pose_dict(results, frame, rows, cols)
                time_msec = float(cap.get(cv2.CAP_PROP_POS_MSEC) or 0.0)

                if results.face_landmarks and len(results.face_landmarks) > 0:
                    lm_list = results.face_landmarks[0]
                    lm468 = lm_list[:468]
                    landmarks = np.array([(lm.x, lm.y, lm.z) for lm in lm468])
                    landmarks = landmarks.T
                    metric_landmarks, _ = get_metric_landmarks(landmarks.copy(), pcf)
                    blendshape_calculator.calculate_blendshapes(
                        face_data,
                        metric_landmarks[0:3].T,
                        lm468,
                    )
                    blends = blendshapes_for_unity(face_data)
                else:
                    blends = list(zero_blend)

                payload["faceData"] = {
                    "blendShapes": blends,
                    "frame": frame,
                    "time": time_msec,
                }
                yield payload
    finally:
        cap.release()

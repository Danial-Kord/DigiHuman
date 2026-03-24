import json

import cv2
from mediapipe.tasks.python.core import base_options as base_options_module
from mediapipe.tasks.python.vision import drawing_utils as mp_drawing
from mediapipe.tasks.python.vision import face_landmarker as face_landmarker_module
from mediapipe.tasks.python.vision import hand_landmarker as hand_landmarker_module
from mediapipe.tasks.python.vision import holistic_landmarker as holistic_landmarker_module
from mediapipe.tasks.python.vision import pose_landmarker as pose_landmarker_module
from mediapipe.tasks.python.vision.core import vision_task_running_mode as running_mode_module

from mediapipe_compat import frame_timestamp_ms, model_path, numpy_rgb_to_mp_image

_BaseOptions = base_options_module.BaseOptions
_RunningMode = running_mode_module.VisionTaskRunningMode
_PoseLandmarker = pose_landmarker_module.PoseLandmarker
_PoseLandmarkerOptions = pose_landmarker_module.PoseLandmarkerOptions
_PoseLandmark = pose_landmarker_module.PoseLandmark
_PoseLandmarksConnections = pose_landmarker_module.PoseLandmarksConnections
_HolisticLandmarker = holistic_landmarker_module.HolisticLandmarker
_HolisticLandmarkerOptions = holistic_landmarker_module.HolisticLandmarkerOptions
_HandLandmarker = hand_landmarker_module.HandLandmarker
_HandLandmarkerOptions = hand_landmarker_module.HandLandmarkerOptions
_HandLandmarksConnections = hand_landmarker_module.HandLandmarksConnections
_FaceLandmarksConnections = face_landmarker_module.FaceLandmarksConnections


def _landmark_sequence(landmarks):
    if landmarks is None:
        return []
    if hasattr(landmarks, "landmark"):
        return landmarks.landmark
    return landmarks


def add_extra_points(landmark_list):
    left_shoulder = landmark_list[11]
    right_shoulder = landmark_list[12]
    left_hip = landmark_list[23]
    right_hip = landmark_list[24]

    hip = {
        "x": (left_hip["x"] + right_hip["x"]) / 2.0,
        "y": (left_hip["y"] + right_hip["y"]) / 2.0,
        "z": (left_hip["z"] + right_hip["z"]) / 2.0,
        "visibility": (left_hip["visibility"] + right_hip["visibility"]) / 2.0,
    }
    landmark_list.append(hip)

    spine = {
        "x": (left_hip["x"] + right_hip["x"] + right_shoulder["x"] + left_shoulder["x"]) / 4.0,
        "y": (left_hip["y"] + right_hip["y"] + right_shoulder["y"] + left_shoulder["y"]) / 4.0,
        "z": (left_hip["z"] + right_hip["z"] + right_shoulder["z"] + left_shoulder["z"]) / 4.0,
        "visibility": (
            left_hip["visibility"]
            + right_hip["visibility"]
            + right_shoulder["visibility"]
            + left_shoulder["visibility"]
        )
        / 4.0,
    }
    landmark_list.append(spine)

    left_mouth = landmark_list[9]
    right_mouth = landmark_list[10]
    nose = landmark_list[0]
    left_ear = landmark_list[7]
    right_ear = landmark_list[8]
    neck = {
        "x": (left_mouth["x"] + right_mouth["x"] + right_shoulder["x"] + left_shoulder["x"]) / 4.0,
        "y": (left_mouth["y"] + right_mouth["y"] + right_shoulder["y"] + left_shoulder["y"]) / 4.0,
        "z": (left_mouth["z"] + right_mouth["z"] + right_shoulder["z"] + left_shoulder["z"]) / 4.0,
        "visibility": (
            left_mouth["visibility"]
            + right_mouth["visibility"]
            + right_shoulder["visibility"]
            + left_shoulder["visibility"]
        )
        / 4.0,
    }
    landmark_list.append(neck)

    head = {
        "x": (nose["x"] + left_ear["x"] + right_ear["x"]) / 3.0,
        "y": (nose["y"] + left_ear["y"] + right_ear["y"]) / 3.0,
        "z": (nose["z"] + left_ear["z"] + right_ear["z"]) / 3.0,
        "visibility": (
            nose["visibility"] + left_ear["visibility"] + right_ear["visibility"]
        )
        / 3.0,
    }
    landmark_list.append(head)


def world_landmarks_list_to_array(landmark_list, image_shape):
    rows, cols, _ = image_shape
    array = []
    for lmk in _landmark_sequence(landmark_list):
        vis = getattr(lmk, "visibility", None)
        if vis is None:
            vis = 1.0
        new_row = {
            "x": lmk.x * cols,
            "y": lmk.y * rows,
            "z": lmk.z * cols,
            "visibility": vis,
        }
        array.append(new_row)
    return array


def landmarks_list_to_array(landmark_list):
    array = []
    for lmk in _landmark_sequence(landmark_list):
        vis = getattr(lmk, "visibility", None)
        if vis is None:
            vis = 1.0
        new_row = {
            "x": lmk.x,
            "y": lmk.y,
            "z": lmk.z,
            "visibility": vis,
        }
        array.append(new_row)
    return array


def Save_Json(path, index, dump_data):
    json_path = path + "" + str(index) + ".json"
    with open(json_path, "w") as fl:
        fl.write(json.dumps(dump_data, indent=2, separators=(",", ": ")))


def Pose_Images():
    """Static image pose demo (legacy paths in this function may not exist on your machine)."""
    IMAGE_FILES = [
        "C:\\Danial\\Projects\\Clone\\3DModelGeneratorTest\\pifuhd\\sample_images\\5.jpg"
    ]
    options = _PoseLandmarkerOptions(
        base_options=_BaseOptions(model_asset_path=model_path("pose_landmarker_full.task")),
        running_mode=_RunningMode.IMAGE,
        min_pose_detection_confidence=0.0,
        output_segmentation_masks=False,
    )
    pose_spec = mp_drawing.DrawingSpec(color=mp_drawing.WHITE_COLOR, thickness=2)
    with _PoseLandmarker.create_from_options(options) as pose:
        for idx, file in enumerate(IMAGE_FILES):
            image = cv2.imread(file)
            if image is None:
                continue
            image_height, image_width, _ = image.shape
            image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            mp_image = numpy_rgb_to_mp_image(image_rgb)
            results = pose.detect(mp_image)

            if not results.pose_landmarks or not results.pose_landmarks[0]:
                continue
            plm = results.pose_landmarks[0]
            print(
                f"Nose coordinates: ("
                f"{plm[_PoseLandmark.NOSE].x * image_width}, "
                f"{plm[_PoseLandmark.NOSE].y * image_height})"
            )

            annotated_image = image.copy()
            mp_drawing.draw_landmarks(
                annotated_image,
                plm,
                _PoseLandmarksConnections.POSE_LANDMARKS,
                landmark_drawing_spec=pose_spec,
            )
            cv2.imwrite(
                "C:/Danial/Projects/Danial/DigiHuman/Backend/output" + str(idx) + ".png",
                annotated_image,
            )

            world_lm = results.pose_world_landmarks[0] if results.pose_world_landmarks else []
            new_pose = world_landmarks_list_to_array(world_lm, image.shape)
            pose_landmarks = landmarks_list_to_array(plm)
            print(plm)
            print(pose_landmarks)

            json_path = "C:/Danial/Projects/Danial/DigiHuman/Backend/json/"
            with open(json_path, "w") as fl:
                dump_data = {"predictions": pose_landmarks, "predictions_world": new_pose}
                fl.write(json.dumps(dump_data, indent=2, separators=(",", ": ")))


def Pose_Video(video_path, debug=False):
    print("pose estimator started...")
    cap = cv2.VideoCapture(video_path)
    frame = 0

    if debug:
        frame_width = int(cap.get(3))
        frame_height = int(cap.get(4))
        size = (frame_width, frame_height)
        result = cv2.VideoWriter(
            "debug.avi", cv2.VideoWriter_fourcc(*"MJPG"), 10, size
        )

    options = _PoseLandmarkerOptions(
        base_options=_BaseOptions(model_asset_path=model_path("pose_landmarker_full.task")),
        running_mode=_RunningMode.VIDEO,
        min_pose_detection_confidence=0.5,
        min_pose_presence_confidence=0.5,
        min_tracking_confidence=0.8,
    )
    pose_conn = _PoseLandmarksConnections.POSE_LANDMARKS
    pose_spec = mp_drawing.DrawingSpec(color=mp_drawing.WHITE_COLOR, thickness=2)

    with _PoseLandmarker.create_from_options(options) as pose:
        frame_index = 0
        while cap.isOpened():
            success, image = cap.read()
            frame_index += 1
            frame = cap.get(cv2.CAP_PROP_POS_FRAMES)
            if not success:
                break

            image.flags.writeable = False
            image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            mp_image = numpy_rgb_to_mp_image(image_rgb)
            ts = frame_timestamp_ms(cap, frame_index)
            results = pose.detect_for_video(mp_image, ts)

            try:
                if not results.pose_world_landmarks or not results.pose_world_landmarks[0]:
                    raise ValueError("no pose")
                pose_landmarks = landmarks_list_to_array(results.pose_world_landmarks[0])
                rows, cols, _ = image_rgb.shape
                add_extra_points(pose_landmarks)
                json_data = {
                    "predictions": pose_landmarks,
                    "frame": frame,
                    "height": rows,
                    "width": cols,
                }
                yield json_data
                if debug:
                    json_path = "C:/Danial/Projects/Danial/DigiHuman/Backend/json/"
                    Save_Json(json_path, frame, json_data)
            except Exception:
                print("wtf")
                continue

            if debug and results.pose_landmarks and results.pose_landmarks[0]:
                image_bgr = cv2.cvtColor(image_rgb, cv2.COLOR_RGB2BGR)
                mp_drawing.draw_landmarks(
                    image_bgr,
                    results.pose_landmarks[0],
                    pose_conn,
                    landmark_drawing_spec=pose_spec,
                )
                cv2.imshow("MediaPipe Pose", cv2.flip(image_bgr, 1))
                result.write(image_bgr)
                if cv2.waitKey(5) & 0xFF == 27:
                    break

    cap.release()


def Hands_Full(video_path, debug=False):
    cap = cv2.VideoCapture(video_path)
    options = _HolisticLandmarkerOptions(
        base_options=_BaseOptions(model_asset_path=model_path("holistic_landmarker.task")),
        running_mode=_RunningMode.VIDEO,
        min_face_detection_confidence=0.9,
        min_face_landmarks_confidence=0.9,
        min_pose_detection_confidence=0.9,
        min_pose_landmarks_confidence=0.9,
        min_hand_landmarks_confidence=0.9,
    )
    hand_conn = _HandLandmarksConnections.HAND_CONNECTIONS
    hand_spec = mp_drawing.DrawingSpec(color=mp_drawing.GREEN_COLOR, thickness=2)

    with _HolisticLandmarker.create_from_options(options) as holistic:
        frame_index = 0
        while cap.isOpened():
            success, image = cap.read()
            frame_index += 1
            frame = cap.get(cv2.CAP_PROP_POS_FRAMES)
            if not success:
                break

            image.flags.writeable = True
            image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            mp_image = numpy_rgb_to_mp_image(image_rgb)
            ts = frame_timestamp_ms(cap, frame_index)
            results = holistic.detect_for_video(mp_image, ts)

            rows, cols, _ = image_rgb.shape
            try:
                if results.pose_world_landmarks:
                    pose_landmarks = landmarks_list_to_array(results.pose_world_landmarks)
                    add_extra_points(pose_landmarks)
                    body_pose = {
                        "predictions": pose_landmarks,
                        "frame": frame,
                        "height": rows,
                        "width": cols,
                    }
                else:
                    raise ValueError("no pose")
            except Exception:
                body_pose = {
                    "predictions": [],
                    "frame": frame,
                    "height": rows,
                    "width": cols,
                }

            hands_array_R = []
            hands_array_L = []
            if results.left_hand_landmarks:
                hands_array_L = landmarks_list_to_array(results.left_hand_landmarks)
            if results.right_hand_landmarks:
                hands_array_R = landmarks_list_to_array(results.right_hand_landmarks)
            hands_pose = {"handsR": hands_array_R, "handsL": hands_array_L, "frame": frame}
            yield hands_pose

            if debug:
                image_bgr = cv2.cvtColor(image_rgb, cv2.COLOR_RGB2BGR)
                if results.right_hand_landmarks:
                    mp_drawing.draw_landmarks(
                        image_bgr,
                        results.right_hand_landmarks,
                        hand_conn,
                        landmark_drawing_spec=hand_spec,
                    )
                if results.left_hand_landmarks:
                    mp_drawing.draw_landmarks(
                        image_bgr,
                        results.left_hand_landmarks,
                        hand_conn,
                        landmark_drawing_spec=hand_spec,
                    )
                cv2.imshow("MediaPipe Holistic", cv2.flip(image_bgr, 1))
                if cv2.waitKey(5) & 0xFF == 27:
                    break
    cap.release()


def Complete_pose_Video(video_path, debug=False):
    cap = cv2.VideoCapture(video_path)
    options = _HolisticLandmarkerOptions(
        base_options=_BaseOptions(model_asset_path=model_path("holistic_landmarker.task")),
        running_mode=_RunningMode.VIDEO,
        min_face_detection_confidence=0.5,
        min_face_landmarks_confidence=0.8,
        min_pose_detection_confidence=0.5,
        min_pose_landmarks_confidence=0.8,
        min_hand_landmarks_confidence=0.8,
    )
    face_contours = _FaceLandmarksConnections.FACE_LANDMARKS_CONTOURS
    pose_conn = _PoseLandmarksConnections.POSE_LANDMARKS
    hand_conn = _HandLandmarksConnections.HAND_CONNECTIONS
    face_spec = mp_drawing.DrawingSpec(color=mp_drawing.WHITE_COLOR, thickness=1)
    pose_spec = mp_drawing.DrawingSpec(color=mp_drawing.WHITE_COLOR, thickness=2)
    hand_spec = mp_drawing.DrawingSpec(color=mp_drawing.GREEN_COLOR, thickness=2)

    with _HolisticLandmarker.create_from_options(options) as holistic:
        frame_index = 0
        while cap.isOpened():
            success, image = cap.read()
            frame_index += 1
            frame = cap.get(cv2.CAP_PROP_POS_FRAMES)
            if not success:
                break

            image.flags.writeable = True
            image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            mp_image = numpy_rgb_to_mp_image(image_rgb)
            ts = frame_timestamp_ms(cap, frame_index)
            results = holistic.detect_for_video(mp_image, ts)

            rows, cols, _ = image_rgb.shape
            try:
                if results.pose_world_landmarks:
                    pose_landmarks = landmarks_list_to_array(results.pose_world_landmarks)
                    add_extra_points(pose_landmarks)
                    body_pose = {
                        "predictions": pose_landmarks,
                        "frame": frame,
                        "height": rows,
                        "width": cols,
                    }
                else:
                    raise ValueError("no pose")
            except Exception:
                body_pose = {
                    "predictions": [],
                    "frame": frame,
                    "height": rows,
                    "width": cols,
                }

            hands_array_R = []
            hands_array_L = []
            if results.left_hand_landmarks:
                hands_array_L = landmarks_list_to_array(results.left_hand_landmarks)
            if results.right_hand_landmarks:
                hands_array_R = landmarks_list_to_array(results.right_hand_landmarks)
            hands_pose = {"handsR": hands_array_R, "handsL": hands_array_L, "frame": frame}

            json_data = {"bodyPose": body_pose, "handsPose": hands_pose, "frame": frame}
            yield json_data

            if debug:
                image_bgr = cv2.cvtColor(image_rgb, cv2.COLOR_RGB2BGR)
                if results.face_landmarks:
                    mp_drawing.draw_landmarks(
                        image_bgr,
                        results.face_landmarks,
                        face_contours,
                        landmark_drawing_spec=None,
                        connection_drawing_spec=face_spec,
                    )
                if results.pose_landmarks:
                    mp_drawing.draw_landmarks(
                        image_bgr,
                        results.pose_landmarks,
                        pose_conn,
                        landmark_drawing_spec=pose_spec,
                    )
                if results.right_hand_landmarks:
                    mp_drawing.draw_landmarks(
                        image_bgr,
                        results.right_hand_landmarks,
                        hand_conn,
                        landmark_drawing_spec=hand_spec,
                    )
                if results.left_hand_landmarks:
                    mp_drawing.draw_landmarks(
                        image_bgr,
                        results.left_hand_landmarks,
                        hand_conn,
                        landmark_drawing_spec=hand_spec,
                    )
                cv2.imshow("MediaPipe Holistic", cv2.flip(image_bgr, 1))
                if cv2.waitKey(5) & 0xFF == 27:
                    break
    cap.release()


def Hand_pose_video(video_path, debug=False):
    cap = cv2.VideoCapture(video_path)
    frame = 0
    options = _HandLandmarkerOptions(
        base_options=_BaseOptions(model_asset_path=model_path("hand_landmarker.task")),
        running_mode=_RunningMode.VIDEO,
        num_hands=2,
        min_hand_detection_confidence=0.6,
        min_hand_presence_confidence=0.5,
        min_tracking_confidence=0.8,
    )
    hand_conn = _HandLandmarksConnections.HAND_CONNECTIONS
    hand_spec = mp_drawing.DrawingSpec(color=mp_drawing.GREEN_COLOR, thickness=2)

    with _HandLandmarker.create_from_options(options) as hands:
        frame_index = 0
        while cap.isOpened():
            success, image = cap.read()
            frame_index += 1
            frame = cap.get(cv2.CAP_PROP_POS_FRAMES)

            if not success:
                break

            image.flags.writeable = False
            image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            mp_image = numpy_rgb_to_mp_image(image_rgb)
            ts = frame_timestamp_ms(cap, frame_index)
            results = hands.detect_for_video(mp_image, ts)

            image_bgr = cv2.cvtColor(image_rgb, cv2.COLOR_RGB2BGR)
            hands_array_R = []
            hands_array_L = []

            if results.hand_landmarks:
                for i, hand_lm in enumerate(results.hand_landmarks):
                    mp_drawing.draw_landmarks(
                        image_bgr,
                        hand_lm,
                        hand_conn,
                        landmark_drawing_spec=hand_spec,
                    )
                    label = None
                    if results.handedness and i < len(results.handedness):
                        cats = results.handedness[i]
                        if cats:
                            label = (cats[0].category_name or cats[0].display_name or "").lower()
                    if label == "left":
                        hands_array_L = landmarks_list_to_array(hand_lm)
                    elif label == "right":
                        hands_array_R = landmarks_list_to_array(hand_lm)

            json_data = {"handsR": hands_array_R, "handsL": hands_array_L, "frame": frame}
            yield json_data
            if debug:
                json_path = "C:/Danial/Projects/Danial/DigiHuman/Backend/hand_json/"
                Save_Json(json_path, frame, json_data)
            cv2.imshow("MediaPipe Hands", cv2.flip(image_bgr, 1))
            if cv2.waitKey(5) & 0xFF == 27:
                break
    cap.release()

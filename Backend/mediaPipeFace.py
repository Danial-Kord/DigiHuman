import cv2
import numpy as np
from blendshapes.blendshape_calculator import BlendshapeCalculator
from blendshapes.facedata import FaceData, FaceBlendShape
from face_geometry import PCF, get_metric_landmarks
from mediapipe.tasks.python.core import base_options as base_options_module
from mediapipe.tasks.python.vision import drawing_utils as mp_drawing
from mediapipe.tasks.python.vision import face_landmarker as face_landmarker_module
from mediapipe.tasks.python.vision import hand_landmarker as hand_landmarker_module
from mediapipe.tasks.python.vision import holistic_landmarker as holistic_landmarker_module
from mediapipe.tasks.python.vision import pose_landmarker as pose_landmarker_module
from mediapipe.tasks.python.vision.core import vision_task_running_mode as running_mode_module

from mediapipe_compat import frame_timestamp_ms, model_path, numpy_rgb_to_mp_image

_BaseOptions = base_options_module.BaseOptions
_FaceLandmarker = face_landmarker_module.FaceLandmarker
_FaceLandmarkerOptions = face_landmarker_module.FaceLandmarkerOptions
_FaceLandmarksConnections = face_landmarker_module.FaceLandmarksConnections
_RunningMode = running_mode_module.VisionTaskRunningMode
_HolisticLandmarker = holistic_landmarker_module.HolisticLandmarker
_HolisticLandmarkerOptions = holistic_landmarker_module.HolisticLandmarkerOptions


def Show_Frame_Landmarks(image, face_result):
    """Draw face mesh debug (BGR output). `image` is RGB uint8; `face_result` is FaceLandmarkerResult."""
    image = image.copy()
    image.flags.writeable = True
    image_bgr = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
    if not face_result.face_landmarks:
        cv2.imshow("MediaPipe Face Mesh", cv2.flip(image_bgr, 1))
        return image_bgr
    tesselation = _FaceLandmarksConnections.FACE_LANDMARKS_TESSELATION
    contours = _FaceLandmarksConnections.FACE_LANDMARKS_CONTOURS
    iris = (
        _FaceLandmarksConnections.FACE_LANDMARKS_LEFT_IRIS
        + _FaceLandmarksConnections.FACE_LANDMARKS_RIGHT_IRIS
    )
    spec_tess = mp_drawing.DrawingSpec(color=mp_drawing.GREEN_COLOR, thickness=1)
    spec_conn = mp_drawing.DrawingSpec(color=mp_drawing.WHITE_COLOR, thickness=1)
    for lm in face_result.face_landmarks:
        mp_drawing.draw_landmarks(
            image_bgr,
            lm,
            connections=tesselation,
            landmark_drawing_spec=None,
            connection_drawing_spec=spec_tess,
        )
        mp_drawing.draw_landmarks(
            image_bgr,
            lm,
            connections=contours,
            landmark_drawing_spec=None,
            connection_drawing_spec=spec_conn,
        )
        mp_drawing.draw_landmarks(
            image_bgr,
            lm,
            connections=iris,
            landmark_drawing_spec=None,
            connection_drawing_spec=spec_conn,
        )
    cv2.imshow("MediaPipe Face Mesh", cv2.flip(image_bgr, 1))
    return image_bgr


def Calculate_Face_Mocap(path=None, debug=False):
    if path is None:
        cap = cv2.VideoCapture(0)
        image_height, image_width, channels = (480, 640, 3)
    else:
        cap = cv2.VideoCapture(path)
        image_height, image_width, channels = (
            cap.get(cv2.CAP_PROP_FRAME_HEIGHT),
            cap.get(cv2.CAP_PROP_FRAME_WIDTH),
            3,
        )

    if debug:
        frame_width = int(cap.get(3))
        frame_height = int(cap.get(4))
        size = (frame_width, frame_height)
        result = cv2.VideoWriter(
            "debug.avi", cv2.VideoWriter_fourcc(*"MJPG"), 10, size
        )

    blendshape_calulator = BlendshapeCalculator()
    face_data = FaceData(filter_size=4)

    focal_length = image_width
    center = (image_width / 2, image_height / 2)
    camera_matrix = np.array(
        [[focal_length, 0, center[0]], [0, focal_length, center[1]], [0, 0, 1]],
        dtype="double",
    )
    pcf = PCF(
        near=1,
        far=10000,
        frame_height=image_height,
        frame_width=image_width,
        fy=camera_matrix[1, 1],
    )
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, image_width)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, image_height)

    options = _FaceLandmarkerOptions(
        base_options=_BaseOptions(model_asset_path=model_path("face_landmarker.task")),
        running_mode=_RunningMode.VIDEO,
        num_faces=1,
        min_face_detection_confidence=0.2,
        min_face_presence_confidence=0.5,
        min_tracking_confidence=0.9,
    )
    with _FaceLandmarker.create_from_options(options) as face_landmarker:
        frame_index = 0
        while cap.isOpened():
            success, image = cap.read()
            frame_index += 1

            if not success:
                print("Ignoring empty camera frame.")
                if path is not None:
                    break
                continue

            image.flags.writeable = False
            image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            mp_image = numpy_rgb_to_mp_image(image_rgb)
            ts = frame_timestamp_ms(cap, frame_index)
            face_result = face_landmarker.detect_for_video(mp_image, ts)

            if face_result.face_landmarks:
                for lm_list in face_result.face_landmarks:
                    lm468 = lm_list[:468]
                    landmarks = np.array([(lm.x, lm.y, lm.z) for lm in lm468])
                    landmarks = landmarks.T
                    metric_landmarks, pose_transform_mat = get_metric_landmarks(
                        landmarks.copy(), pcf
                    )
                    blendshape_calulator.calculate_blendshapes(
                        face_data,
                        metric_landmarks[0:3].T,
                        lm468,
                    )

                    blends = []
                    blends.append(face_data.get_blendshape(FaceBlendShape.EyeBlinkLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.EyeBlinkRight))
                    blends.append(face_data.get_blendshape(FaceBlendShape.EyeSquintLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.EyeSquintRight))
                    blends.append(face_data.get_blendshape(FaceBlendShape.EyeWideLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.EyeWideRight))

                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthSmileRight))
                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthSmileLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthDimpleLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthDimpleRight))

                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthFrownRight))
                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthFrownLeft))

                    blends.append(face_data.get_blendshape(FaceBlendShape.LipLowerDownLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.LipLowerDownRight))
                    blends.append(face_data.get_blendshape(FaceBlendShape.LipUpperUpLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.LipUpperUpRight))

                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthRight))
                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthStretchLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthStretchRight))

                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthLowerDownRight))
                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthLowerDownLeft))

                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthPressLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthPressRight))

                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthOpen))
                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthPucker))

                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthShrugUpper))

                    blends.append(face_data.get_blendshape(FaceBlendShape.JawOpen))
                    blends.append(face_data.get_blendshape(FaceBlendShape.JawLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.JawRight))

                    blends.append(face_data.get_blendshape(FaceBlendShape.BrowDownLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.BrowOuterUpLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.BrowDownRight))
                    blends.append(face_data.get_blendshape(FaceBlendShape.BrowOuterUpRight))

                    blends.append(face_data.get_blendshape(FaceBlendShape.CheekSquintRight))
                    blends.append(face_data.get_blendshape(FaceBlendShape.CheekSquintLeft))

                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthRollLower))
                    blends.append(face_data.get_blendshape(FaceBlendShape.MouthRollUpper))

                    blends.append(face_data.get_blendshape(FaceBlendShape.NoseSneerLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.NoseSneerRight))

                    frame = cap.get(cv2.CAP_PROP_POS_FRAMES)
                    currentTime = cap.get(cv2.CAP_PROP_POS_MSEC)
                    json_data = {"blendShapes": blends, "frame": frame, "time": currentTime}

                    yield json_data

            if debug:
                dbg_bgr = Show_Frame_Landmarks(image_rgb, face_result)
                result.write(dbg_bgr)
            if cv2.waitKey(5) & 0xFF == 27:
                break
    cap.release()


def face_holistic(video_path, debug=False):
    if video_path is None:
        cap = cv2.VideoCapture(0)
    else:
        cap = cv2.VideoCapture(video_path)

    if debug:
        frame_width = int(cap.get(3))
        frame_height = int(cap.get(4))
        size = (frame_width, frame_height)
        result = cv2.VideoWriter(
            "debug.mp4", cv2.VideoWriter_fourcc(*"MJPG"), 10, size
        )

    blendshape_calulator = BlendshapeCalculator()
    face_data = FaceData(filter_size=4)
    image_height, image_width, channels = (480, 640, 3)
    focal_length = image_width
    center = (image_width / 2, image_height / 2)
    camera_matrix = np.array(
        [[focal_length, 0, center[0]], [0, focal_length, center[1]], [0, 0, 1]],
        dtype="double",
    )
    pcf = PCF(
        near=1,
        far=10000,
        frame_height=image_height,
        frame_width=image_width,
        fy=camera_matrix[1, 1],
    )
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, image_width)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, image_height)

    options = _HolisticLandmarkerOptions(
        base_options=_BaseOptions(model_asset_path=model_path("holistic_landmarker.task")),
        running_mode=_RunningMode.VIDEO,
        min_face_detection_confidence=0.5,
        min_face_landmarks_confidence=0.8,
        min_pose_detection_confidence=0.5,
        min_pose_landmarks_confidence=0.8,
        min_hand_landmarks_confidence=0.8,
    )
    pose_conn = pose_landmarker_module.PoseLandmarksConnections.POSE_LANDMARKS
    hand_conn = hand_landmarker_module.HandLandmarksConnections.HAND_CONNECTIONS
    face_contours = _FaceLandmarksConnections.FACE_LANDMARKS_CONTOURS
    hand_spec = mp_drawing.DrawingSpec(color=mp_drawing.GREEN_COLOR, thickness=2)
    pose_spec = mp_drawing.DrawingSpec(color=mp_drawing.WHITE_COLOR, thickness=2)

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

            if results.face_landmarks:
                lm468 = results.face_landmarks[:468]
                landmarks = np.array([(lm.x, lm.y, lm.z) for lm in lm468])
                landmarks = landmarks.T

                metric_landmarks, pose_transform_mat = get_metric_landmarks(
                    landmarks.copy(), pcf
                )
                blendshape_calulator.calculate_blendshapes(
                    face_data, metric_landmarks[0:3].T, lm468
                )

                blends = []
                blends.append(face_data.get_blendshape(FaceBlendShape.EyeBlinkLeft))
                blends.append(face_data.get_blendshape(FaceBlendShape.EyeBlinkRight))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthSmileRight))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthSmileLeft))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthFrownRight))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthFrownLeft))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthLeft))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthRight))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthLowerDownRight))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthLowerDownLeft))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthPressLeft))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthPressRight))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthClose))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthPucker))
                blends.append(face_data.get_blendshape(FaceBlendShape.MouthShrugUpper))
                blends.append(face_data.get_blendshape(FaceBlendShape.JawOpen))
                blends.append(face_data.get_blendshape(FaceBlendShape.JawLeft))
                blends.append(face_data.get_blendshape(FaceBlendShape.JawRight))
                blends.append(face_data.get_blendshape(FaceBlendShape.BrowDownLeft))
                blends.append(face_data.get_blendshape(FaceBlendShape.BrowOuterUpLeft))
                blends.append(face_data.get_blendshape(FaceBlendShape.BrowDownRight))
                blends.append(face_data.get_blendshape(FaceBlendShape.BrowOuterUpRight))
                blends.append(face_data.get_blendshape(FaceBlendShape.CheekSquintRight))
                blends.append(face_data.get_blendshape(FaceBlendShape.CheekSquintLeft))

                frame = cap.get(cv2.CAP_PROP_POS_FRAMES)
                currentTime = cap.get(cv2.CAP_PROP_POS_MSEC)
                json_data = {"blendShapes": blends, "frame": frame, "time": currentTime}

                yield json_data

            if debug:
                image_bgr = cv2.cvtColor(image_rgb, cv2.COLOR_RGB2BGR)
                if results.face_landmarks:
                    mp_drawing.draw_landmarks(
                        image_bgr,
                        results.face_landmarks,
                        face_contours,
                        landmark_drawing_spec=None,
                        connection_drawing_spec=mp_drawing.DrawingSpec(
                            color=mp_drawing.WHITE_COLOR, thickness=1
                        ),
                    )
                if results.pose_landmarks:
                    mp_drawing.draw_landmarks(
                        image_bgr,
                        results.pose_landmarks,
                        pose_conn,
                        landmark_drawing_spec=pose_spec,
                    )
                if results.left_hand_landmarks:
                    mp_drawing.draw_landmarks(
                        image_bgr,
                        results.left_hand_landmarks,
                        hand_conn,
                        landmark_drawing_spec=hand_spec,
                    )
                if results.right_hand_landmarks:
                    mp_drawing.draw_landmarks(
                        image_bgr,
                        results.right_hand_landmarks,
                        hand_conn,
                        landmark_drawing_spec=hand_spec,
                    )
                cv2.imshow("MediaPipe Holistic", cv2.flip(image_bgr, 1))
                result.write(image_bgr)
                if cv2.waitKey(5) & 0xFF == 27:
                    break
    cap.release()


if __name__ == "__main__":
    print("dd")
    path = "D:\\pose\\New\\final\\1.mp4"
    for i in Calculate_Face_Mocap(path, True):
        continue

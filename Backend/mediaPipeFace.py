import cv2
import mediapipe as mp
import numpy as np
from blendshapes.blendshape_calculator import BlendshapeCalculator
from blendshapes.facedata import FaceData, FaceBlendShape
from face_geometry import (
    PCF,
    get_metric_landmarks,
    procrustes_landmark_basis,
)

mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_face_mesh = mp.solutions.face_mesh


# Draw the face mesh annotations on the image.
def Show_Frame_Landmarks(image,results):
    image.flags.writeable = True
    image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
    if results.multi_face_landmarks:
        for face_landmarks in results.multi_face_landmarks:
            mp_drawing.draw_landmarks(
                image=image,
                landmark_list=face_landmarks,
                connections=mp_face_mesh.FACEMESH_TESSELATION,
                landmark_drawing_spec=None,
                connection_drawing_spec=mp_drawing_styles
                    .get_default_face_mesh_tesselation_style())
            mp_drawing.draw_landmarks(
                image=image,
                landmark_list=face_landmarks,
                connections=mp_face_mesh.FACEMESH_CONTOURS,
                landmark_drawing_spec=None,
                connection_drawing_spec=mp_drawing_styles
                    .get_default_face_mesh_contours_style())
            mp_drawing.draw_landmarks(
                image=image,
                landmark_list=face_landmarks,
                connections=mp_face_mesh.FACEMESH_IRISES,
                landmark_drawing_spec=None,
                connection_drawing_spec=mp_drawing_styles
                    .get_default_face_mesh_iris_connections_style())
    # Flip the image horizontally for a selfie-view display.
    cv2.imshow('MediaPipe Face Mesh', cv2.flip(image, 1))

def Calculate_Face_Mocap(path=None,debug=False):
    # For webcam input:
    drawing_spec = mp_drawing.DrawingSpec(thickness=1, circle_radius=1)
    if path is None:
        cap = cv2.VideoCapture(0)
    else:
        cap = cv2.VideoCapture(path)

    blendshape_calulator = BlendshapeCalculator()
    face_data = FaceData(filter_size=4)
    image_height, image_width, channels = (480, 640, 3)
    # pseudo camera internals
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
    with mp_face_mesh.FaceMesh(
            max_num_faces=1,
            refine_landmarks=True,
            min_detection_confidence=0.5,
            min_tracking_confidence=0.5) as face_mesh:
        while cap.isOpened():
            success, image = cap.read()
            if not success:
                print("Ignoring empty camera frame.")
                # If loading a video, use 'break' instead of 'continue'.
                if path is not None:
                    break
                continue

            # To improve performance, optionally mark the image as not writeable to
            # pass by reference.
            image.flags.writeable = False
            image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            results = face_mesh.process(image)

            if results.multi_face_landmarks:
                for face_landmarks in results.multi_face_landmarks:
                    landmarks = np.array(
                        [(lm.x, lm.y, lm.z) for lm in face_landmarks.landmark[:468]]

                    )
                    landmarks = landmarks.T

                    metric_landmarks, pose_transform_mat = get_metric_landmarks(
                        landmarks.copy(), pcf
                    )
                    # calculate and set all the blendshapes
                    blendshape_calulator.calculate_blendshapes(face_data,metric_landmarks[0:3].T,face_landmarks.landmark)
                    # blends = live_link_face.get_all_blendshapes()

                    json_data = {
                        #Eye
                        'EyeBlinkLeft': face_data.get_blendshape(FaceBlendShape.EyeBlinkLeft),
                        'EyeBlinkRight': face_data.get_blendshape(FaceBlendShape.EyeBlinkRight),

                        #mouth
                        'MouthSmileRight': face_data.get_blendshape(FaceBlendShape.MouthSmileRight),
                        'MouthSmileLeft': face_data.get_blendshape(FaceBlendShape.MouthSmileLeft),

                        'MouthFrownRight': face_data.get_blendshape(FaceBlendShape.MouthFrownRight),
                        'MouthFrownLeft': face_data.get_blendshape(FaceBlendShape.MouthFrownLeft),

                        'MouthLeft': face_data.get_blendshape(FaceBlendShape.MouthLeft),
                        'MouthRight': face_data.get_blendshape(FaceBlendShape.MouthRight),
                        'MouthLowerDownRight': face_data.get_blendshape(FaceBlendShape.MouthLowerDownRight),
                        'MouthLowerDownLeft': face_data.get_blendshape(FaceBlendShape.MouthLowerDownLeft),

                        'MouthPressLeft': face_data.get_blendshape(FaceBlendShape.MouthPressLeft),
                        'MouthPressRight': face_data.get_blendshape(FaceBlendShape.MouthPressRight),

                        'MouthOpen': face_data.get_blendshape(FaceBlendShape.MouthOpen),
                        'MouthPucker': face_data.get_blendshape(FaceBlendShape.MouthPucker),
                        'MouthShrugUpper': face_data.get_blendshape(FaceBlendShape.MouthShrugUpper),

                        #Jaw
                        'JawOpen': face_data.get_blendshape(FaceBlendShape.JawOpen),
                        'JawLeft': face_data.get_blendshape(FaceBlendShape.JawLeft),
                        'JawRight': face_data.get_blendshape(FaceBlendShape.JawRight),

                        #Brow
                        'BrowDownLeft': face_data.get_blendshape(FaceBlendShape.BrowDownLeft),
                        'BrowOuterUpLeft': face_data.get_blendshape(FaceBlendShape.BrowOuterUpLeft),
                        'BrowDownRight': face_data.get_blendshape(FaceBlendShape.BrowDownRight),
                        'BrowOuterUpRight': face_data.get_blendshape(FaceBlendShape.BrowOuterUpRight),

                        #Cheek
                        'CheekSquintRight': face_data.get_blendshape(FaceBlendShape.CheekSquintRight),
                        'CheekSquintLeft': face_data.get_blendshape(FaceBlendShape.CheekSquintLeft),
                    }
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
                    json_data = {
                        "blendShapes" : blends,
                        "frame" : frame,
                        "time" : currentTime
                    }

                    yield json_data

            if debug:
                Show_Frame_Landmarks(image,results)

            if cv2.waitKey(5) & 0xFF == 27:
                break
    cap.release()

# path = "D:\\pose\\New\\2022-07-14\\C2824.MP4"


if __name__ == '__main__':
    print("dd")
    for i in Calculate_Face_Mocap(None,True):
        continue
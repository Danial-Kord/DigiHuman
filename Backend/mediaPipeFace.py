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
    return image

def Calculate_Face_Mocap(path=None,debug=False):
    # For webcam input:
    drawing_spec = mp_drawing.DrawingSpec(thickness=1, circle_radius=1)
    if path is None:
        cap = cv2.VideoCapture(0)
        image_height, image_width, channels = (480, 640, 3)
    else:
        cap = cv2.VideoCapture(path)
        image_height, image_width, channels = (cap.get(cv2.CAP_PROP_FRAME_HEIGHT),cap.get(cv2.CAP_PROP_FRAME_WIDTH), 3)

    if debug:
        frame_width = int(cap.get(3))
        frame_height = int(cap.get(4))

        size = (frame_width, frame_height)

        result = cv2.VideoWriter('debug.avi',
                                 cv2.VideoWriter_fourcc(*'MJPG'),
                                 10, size)

    blendshape_calulator = BlendshapeCalculator()
    face_data = FaceData(filter_size=4)

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
            min_detection_confidence=0.2,
            min_tracking_confidence=0.9) as face_mesh:
        while cap.isOpened():
            success, image = cap.read()

            # resize image TODO optional
            # dim = (image_width, image_height)
            # image = cv2.resize(image, dim, interpolation=cv2.INTER_AREA)

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

                    #Lips
                    blends.append(face_data.get_blendshape(FaceBlendShape.LipLowerDownLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.LipLowerDownRight))
                    blends.append(face_data.get_blendshape(FaceBlendShape.LipUpperUpLeft))
                    blends.append(face_data.get_blendshape(FaceBlendShape.LipUpperUpRight))

                    #not good for now
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

                    #should test
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
                    json_data = {
                        "blendShapes" : blends,
                        "frame" : frame,
                        "time" : currentTime
                    }

                    yield json_data

            if debug:
                image = Show_Frame_Landmarks(image,results)
                result.write(image)
            if cv2.waitKey(5) & 0xFF == 27:
                break
    cap.release()




def face_holistic(video_path,debug=False):
    mp_drawing = mp.solutions.drawing_utils
    mp_drawing_styles = mp.solutions.drawing_styles
    mp_holistic = mp.solutions.holistic
    mp_hands = mp.solutions.hands
    json_path = "TestPath"
    # cap = cv2.VideoCapture(video_path)
    # cap = cv2.VideoCapture(0)

    # For webcam input:
    drawing_spec = mp_drawing.DrawingSpec(thickness=1, circle_radius=1)
    if video_path is None:
        cap = cv2.VideoCapture(0)
    else:
        cap = cv2.VideoCapture(video_path)

    if debug:
        frame_width = int(cap.get(3))
        frame_height = int(cap.get(4))

        size = (frame_width, frame_height)

        result = cv2.VideoWriter('debug.mp4',
                                 cv2.VideoWriter_fourcc(*'MJPG'),
                                 10, size)

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
    with mp_holistic.Holistic(
        min_detection_confidence=0.5,
        min_tracking_confidence=0.8,
        model_complexity=2) as holistic:
      while cap.isOpened():
        success, image = cap.read()
        # current_frame
        frame = cap.get(cv2.CAP_PROP_POS_FRAMES)
        if not success:
          break

        # To improve performance, optionally mark the image as not writeable to
        # pass by reference.
        image.flags.writeable = True
        image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        results = holistic.process(image)

        if results.face_landmarks:
            face_landmarks = results.face_landmarks
            landmarks = np.array(
                [(lm.x, lm.y, lm.z) for lm in face_landmarks.landmark[:468]]

            )
            landmarks = landmarks.T

            metric_landmarks, pose_transform_mat = get_metric_landmarks(
                landmarks.copy(), pcf
            )
            # calculate and set all the blendshapes
            blendshape_calulator.calculate_blendshapes(face_data, metric_landmarks[0:3].T, face_landmarks.landmark)
            # blends = live_link_face.get_all_blendshapes()

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
                "blendShapes": blends,
                "frame": frame,
                "time": currentTime
            }

            yield json_data

        if debug:
            # Draw landmark annotation on the image.
            image.flags.writeable = True
            image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
            mp_drawing.draw_landmarks(
                image,
                results.face_landmarks,
                mp_holistic.FACEMESH_CONTOURS,
                landmark_drawing_spec=None,
                connection_drawing_spec=mp_drawing_styles
                .get_default_face_mesh_contours_style())
            mp_drawing.draw_landmarks(
                image,
                results.pose_landmarks,
                mp_holistic.POSE_CONNECTIONS,
                landmark_drawing_spec=mp_drawing_styles
                .get_default_pose_landmarks_style())
            if results.right_hand_landmarks:
                mp_drawing.draw_landmarks(
                    image,
                    results.right_hand_landmarks,
                    mp_hands.HAND_CONNECTIONS,
                    mp_drawing_styles.get_default_hand_landmarks_style(),
                    mp_drawing_styles.get_default_hand_connections_style())

            # Flip the image horizontally for a selfie-view display.
            cv2.imshow('MediaPipe Holistic', cv2.flip(image, 1))
            result.write(image)
            if cv2.waitKey(5) & 0xFF == 27:
              break
    cap.release()


if __name__ == '__main__':
    print("dd")
    path = "D:\\pose\\New\\final\\1.mp4"
    for i in Calculate_Face_Mocap(path,True):
        continue
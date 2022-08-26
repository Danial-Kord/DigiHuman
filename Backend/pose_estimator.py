import json

import cv2
import mediapipe as mp
import numpy as np
# import mocap



# For adding new landmarks based on default predicted landmarks
def add_extra_points(landmark_list):
    left_shoulder = landmark_list[11]
    right_shoulder = landmark_list[12]
    left_hip = landmark_list[23]
    right_hip = landmark_list[24]

    # Calculating hip position and visibility
    hip = {
          'x': (left_hip['x'] + right_hip['x']) / 2.0,
          'y': (left_hip['y'] + right_hip['y']) / 2.0,
          'z': (left_hip['z'] + right_hip['z']) / 2.0,
          'visibility': (left_hip['visibility'] + right_hip['visibility']) / 2.0
        }
    landmark_list.append(hip)

    # Calculating spine position and visibility
    spine = {
          'x': (left_hip['x'] + right_hip['x'] + right_shoulder['x'] + left_shoulder['x']) / 4.0,
          'y': (left_hip['y'] + right_hip['y'] + right_shoulder['y'] + left_shoulder['y']) / 4.0,
          'z': (left_hip['z'] + right_hip['z'] + right_shoulder['z'] + left_shoulder['z']) / 4.0,
          'visibility': (left_hip['visibility'] + right_hip['visibility'] + right_shoulder['visibility'] + left_shoulder['visibility']) / 4.0
        }
    landmark_list.append(spine)

    left_mouth = landmark_list[9]
    right_mouth = landmark_list[10]
    nose = landmark_list[0]
    left_ear = landmark_list[7]
    right_ear = landmark_list[8]
    # Calculating neck position and visibility
    neck = {
          'x': (left_mouth['x'] + right_mouth['x'] + right_shoulder['x'] + left_shoulder['x']) / 4.0,
          'y': (left_mouth['y'] + right_mouth['y'] + right_shoulder['y'] + left_shoulder['y']) / 4.0,
          'z': (left_mouth['z'] + right_mouth['z'] + right_shoulder['z'] + left_shoulder['z']) / 4.0,
          'visibility': (left_mouth['visibility'] + right_mouth['visibility'] + right_shoulder['visibility'] + left_shoulder['visibility']) / 4.0
        }
    landmark_list.append(neck)

    # Calculating head position and visibility
    head = {
          'x': (nose['x'] + left_ear['x'] + right_ear['x']) / 3.0,
          'y': (nose['y'] + left_ear['y'] + right_ear['y']) / 3.0,
          'z': (nose['z'] + left_ear['z'] + right_ear['z']) / 3.0,
          'visibility': (nose['visibility'] + left_ear['visibility'] + right_ear['visibility']) / 3.0,
        }
    landmark_list.append(head)


def world_landmarks_list_to_array(landmark_list, image_shape):
    rows, cols, _ = image_shape
    array = []
    for lmk in landmark_list.landmark:
        new_row = {
          'x': lmk.x * cols,
          'y': lmk.y * rows,
          'z': lmk.z * cols,
          'visibility': lmk.visibility
        }
        array.append(new_row)
    return array
    return np.asarray([(lmk.x * cols, lmk.y * rows, lmk.z * cols,lmk.visibility)
                       for lmk in landmark_list.landmark])


def landmarks_list_to_array(landmark_list):

    array = []
    for lmk in landmark_list.landmark:
        new_row = {
          'x': lmk.x,
          'y': lmk.y,
          'z': lmk.z,
          'visibility': lmk.visibility
        }
        array.append(new_row)
    return array
    return np.asarray([(lmk.x, lmk.y, lmk.z, lmk.visibility)
                       for lmk in landmark_list.landmark])





def Save_Json(path, index,dump_data):
    json_path = path + "" + str(index) + ".json"
    with open(json_path, 'w') as fl:
        # np.around(pose_landmarks, 4).tolist()
        fl.write(json.dumps(dump_data, indent=2, separators=(',', ': ')))
        fl.close()



def Pose_Images():
    mp_drawing = mp.solutions.drawing_utils
    mp_drawing_styles = mp.solutions.drawing_styles
    mp_pose = mp.solutions.pose
    # For static images:
    IMAGE_FILES = ["C:\\Danial\\Projects\\Clone\\3DModelGeneratorTest\\pifuhd\\sample_images\\5.jpg"]
    BG_COLOR = (192, 192, 192) # gray
    with mp_pose.Pose(
        static_image_mode=True,
        model_complexity=2,
        enable_segmentation=True,
        min_detection_confidence=0) as pose:
      for idx, file in enumerate(IMAGE_FILES):
        image = cv2.imread(file)
        image_height, image_width, _ = image.shape
        # Convert the BGR image to RGB before processing.
        results = pose.process(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))

        if not results.pose_landmarks:
          continue
        print(
            f'Nose coordinates: ('
            f'{results.pose_landmarks.landmark[mp_pose.PoseLandmark.NOSE].x * image_width}, '
            f'{results.pose_landmarks.landmark[mp_pose.PoseLandmark.NOSE].y * image_height})'
        )

        annotated_image = image.copy()
        # Draw segmentation on the image.
        # To improve segmentation around boundaries, consider applying a joint
        # bilateral filter to "results.segmentation_mask" with "image".
        condition = np.stack((results.segmentation_mask,) * 3, axis=-1) > 0.1
        bg_image = np.zeros(image.shape, dtype=np.uint8)
        bg_image[:] = BG_COLOR
        annotated_image = np.where(condition, annotated_image, bg_image)
        # Draw pose landmarks on the image.
        mp_drawing.draw_landmarks(
            annotated_image,
            results.pose_landmarks,
            mp_pose.POSE_CONNECTIONS,
            landmark_drawing_spec=mp_drawing_styles.get_default_pose_landmarks_style())
        cv2.imwrite('C:/Danial/Projects/Danial/DigiHuman/Backend/output' + str(idx) + '.png', annotated_image)
        # Plot pose world landmarks.
        mp_drawing.plot_landmarks(
            results.pose_world_landmarks, mp_pose.POSE_CONNECTIONS)

        new_pose = world_landmarks_list_to_array(
                results.pose_world_landmarks)
        pose_landmarks = landmarks_list_to_array(results.pose_landmarks,
                                                       image.shape)
        print(results.pose_landmarks)
        print(pose_landmarks)

        # Dump actual JSON.
        json_path = "C:/Danial/Projects/Danial/DigiHuman/Backend/json/"
        with open(json_path, 'w') as fl:
          dump_data = {
              'predictions': pose_landmarks,
              'predictions_world': new_pose
          }
          #np.around(pose_landmarks, 4).tolist()
          fl.write(json.dumps(dump_data, indent=2, separators=(',', ': ')))


# For video input:
def Pose_Video(video_path,debug = False):
    print("pose estimator started...")
    mp_drawing = mp.solutions.drawing_utils
    mp_drawing_styles = mp.solutions.drawing_styles
    mp_pose = mp.solutions.pose
    cap = cv2.VideoCapture(video_path)
    frame = 0
    out_put = []

    if debug:
        frame_width = int(cap.get(3))
        frame_height = int(cap.get(4))

        size = (frame_width, frame_height)

        result = cv2.VideoWriter('debug.avi',
                                 cv2.VideoWriter_fourcc(*'MJPG'),
                                 10, size)

    with mp_pose.Pose(
        min_detection_confidence=0.5,
        min_tracking_confidence=0.8) as pose:
      while cap.isOpened():
        success, image = cap.read()


        # current_frame
        frame = cap.get(cv2.CAP_PROP_POS_FRAMES)
        if not success:
          #print("Some probelm with video!")
          # If loading a video, use 'break' instead of 'continue'.
          break

        # To improve performance, optionally mark the image as not writeable to
        # pass by reference.
        image.flags.writeable = False
        image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        results = pose.process(image)


        try:

            pose_landmarks = landmarks_list_to_array(results.pose_world_landmarks) #also can use results.pose_landmarks
           # world_pose_landmarks = world_landmarks_list_to_array(results.pose_world_landmarks, image.shape)

            rows, cols, _ = image.shape
            add_extra_points(pose_landmarks)
            # add_extra_points(world_pose_landmarks)

            json_data = {
                'predictions': pose_landmarks,
                'frame': frame,
                'height': rows,
                'width': cols
                }
            # out_put.append(json_data)
            # print(json_data)
            yield json_data
            if debug:
                json_path = "C:/Danial/Projects/Danial/DigiHuman/Backend/json/"
                Save_Json(json_path,frame,json_data)
        except:
            print("wtf")
            continue

        if debug:
            # Draw the pose annotation on the image.
            image.flags.writeable = True
            image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
            mp_drawing.draw_landmarks(
                image,
                results.pose_landmarks,
                mp_pose.POSE_CONNECTIONS,
                landmark_drawing_spec=mp_drawing_styles.get_default_pose_landmarks_style())
            # Flip the image horizontally for a selfie-view display.
            cv2.imshow('MediaPipe Pose', cv2.flip(image, 1))
            result.write(image)
            if cv2.waitKey(5) & 0xFF == 27:
              break

    cap.release()






def Hands_Full(video_path,debug=False):
    mp_drawing = mp.solutions.drawing_utils
    mp_drawing_styles = mp.solutions.drawing_styles
    mp_holistic = mp.solutions.holistic
    mp_hands = mp.solutions.hands
    json_path = "TestPath"
    cap = cv2.VideoCapture(video_path)
    # cap = cv2.VideoCapture(0)
    with mp_holistic.Holistic(
            smooth_landmarks=True,
        min_detection_confidence=0.9,
        min_tracking_confidence=0.9,
        model_complexity=2) as holistic:
      while cap.isOpened():
        success, image = cap.read()
        # current_frame
        frame = cap.get(cv2.CAP_PROP_POS_FRAMES)
        # if(frame < 3600):
        #     continue
        if not success:
          break

        # To improve performance, optionally mark the image as not writeable to
        # pass by reference.
        image.flags.writeable = True
        image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        results = holistic.process(image)

        # ---- Body pose ----
        rows, cols, _ = image.shape
        try:
            pose_landmarks = landmarks_list_to_array(results.pose_world_landmarks)  # also can use results.pose_landmarks
            # world_pose_landmarks = world_landmarks_list_to_array(results.pose_world_landmarks, image.shape)
            add_extra_points(pose_landmarks)
            # add_extra_points(world_pose_landmarks)
            body_pose = {
                'predictions': pose_landmarks,
                'frame': frame,
                'height': rows,
                'width': cols
            }
        except:
            body_pose = {
                'predictions': [],
                'frame': frame,
                'height': rows,
                'width': cols
            }
        # ---- Hands ----
        hands_array_R = []
        hands_array_L = []
        if results.left_hand_landmarks:
            hands_array_L = landmarks_list_to_array(results.left_hand_landmarks)
        if results.right_hand_landmarks:
            hands_array_R = landmarks_list_to_array(results.right_hand_landmarks)
        hands_pose = {
            'handsR': hands_array_R,
            'handsL': hands_array_L,
            'frame': frame
        }
        yield hands_pose

        if debug:
            # Draw landmark annotation on the image.
            image.flags.writeable = True
            image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
            # mp_drawing.draw_landmarks(
            #     image,
            #     results.face_landmarks,
            #     mp_holistic.FACEMESH_CONTOURS,
            #     landmark_drawing_spec=None,
            #     connection_drawing_spec=mp_drawing_styles
            #     .get_default_face_mesh_contours_style())
            # mp_drawing.draw_landmarks(
            #     image,
            #     results.pose_landmarks,
            #     mp_holistic.POSE_CONNECTIONS,
            #     landmark_drawing_spec=mp_drawing_styles
            #     .get_default_pose_landmarks_style())
            if results.right_hand_landmarks:
                mp_drawing.draw_landmarks(
                    image,
                    results.right_hand_landmarks,
                    mp_hands.HAND_CONNECTIONS,
                    mp_drawing_styles.get_default_hand_landmarks_style(),
                    mp_drawing_styles.get_default_hand_connections_style())
            if results.left_hand_landmarks:
                mp_drawing.draw_landmarks(
                    image,
                    results.left_hand_landmarks,
                    mp_hands.HAND_CONNECTIONS,
                    mp_drawing_styles.get_default_hand_landmarks_style(),
                    mp_drawing_styles.get_default_hand_connections_style())

            # Flip the image horizontally for a selfie-view display.
            cv2.imshow('MediaPipe Holistic', cv2.flip(image, 1))
            if cv2.waitKey(5) & 0xFF == 27:
              break
    cap.release()


def Complete_pose_Video(video_path,debug=False):
    mp_drawing = mp.solutions.drawing_utils
    mp_drawing_styles = mp.solutions.drawing_styles
    mp_holistic = mp.solutions.holistic
    mp_hands = mp.solutions.hands
    json_path = "TestPath"
    cap = cv2.VideoCapture(video_path)
    # cap = cv2.VideoCapture(0)
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

        # ---- Body pose ----
        rows, cols, _ = image.shape
        try:
            pose_landmarks = landmarks_list_to_array(results.pose_world_landmarks)  # also can use results.pose_landmarks
            # world_pose_landmarks = world_landmarks_list_to_array(results.pose_world_landmarks, image.shape)
            add_extra_points(pose_landmarks)
            # add_extra_points(world_pose_landmarks)
            body_pose = {
                'predictions': pose_landmarks,
                'frame': frame,
                'height': rows,
                'width': cols
            }
        except:
            body_pose = {
                'predictions': [],
                'frame': frame,
                'height': rows,
                'width': cols
            }
        # ---- Hands ----
        hands_array_R = []
        hands_array_L = []
        if results.left_hand_landmarks:
            hands_array_L = landmarks_list_to_array(results.left_hand_landmarks)
        if results.right_hand_landmarks:
            hands_array_R = landmarks_list_to_array(results.right_hand_landmarks)
        hands_pose = {
            'handsR': hands_array_R,
            'handsL': hands_array_L,
            'frame': frame
        }

        # ---- Face ----

        # facial_expression = mocap.get_frame_facial_mocap(image,frame)
        # if facial_expression is None:
        #     facial_expression = {
        #     'leftEyeWid': -1,
        #     'rightEyeWid': -1,
        #     'mouthWid': -1,
        #     'mouthLen': -1,
        #     'frame': frame
        #     }

        json_data = {
            'bodyPose': body_pose,
            'handsPose': hands_pose,
            'frame': frame
        }
        # print(json_data)
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
            if cv2.waitKey(5) & 0xFF == 27:
              break
    cap.release()


#add_extra_points([])

def Hand_pose_video(video_path, debug=False):
    mp_drawing = mp.solutions.drawing_utils
    mp_drawing_styles = mp.solutions.drawing_styles
    mp_hands = mp.solutions.hands
    # cap = cv2.VideoCapture(0)
    cap = cv2.VideoCapture(video_path)
    # cframe = cap.get(cv2.CV_CAP_PROP_POS_FRAMES)  # retrieves the current frame number
    # tframe = cap.get(cv2.CAP_PROP_FRAME_COUNT)  # get total frame count
    # print(tframe)
    # fps = cap.get(CV_CAP_PROP_FPS)  # get the FPS of the videos
    frame = 0
    with mp_hands.Hands(
            model_complexity=1,
            max_num_hands=2,
            min_detection_confidence=0.6,
            min_tracking_confidence=0.8) as hands:
        while cap.isOpened():
            success, image = cap.read()
            #current_frame
            frame = cap.get(cv2.CAP_PROP_POS_FRAMES)

            if not success:
                # print("Ignoring empty camera frame.")
                # If loading a video, use 'break' instead of 'continue'.
                break
                # continue

            # To improve performance, optionally mark the image as not writeable to
            # pass by reference.
            image.flags.writeable = False
            image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            results = hands.process(image)

            # Draw the hand annotations on the image.
            image.flags.writeable = True
            image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
            hands_array_R = []
            hands_array_L = []


            if results.multi_hand_landmarks:
                index = 0
                for count, hand_landmarks in enumerate(results.multi_hand_landmarks):
                    index += 1
                    mp_drawing.draw_landmarks(
                        image,
                        hand_landmarks,
                        mp_hands.HAND_CONNECTIONS,
                        mp_drawing_styles.get_default_hand_landmarks_style(),
                        mp_drawing_styles.get_default_hand_connections_style())

                    if results.multi_handedness[count].classification[0].label == "Left":
                        hands_array_L = landmarks_list_to_array(hand_landmarks)
                        # print("L")
                    elif results.multi_handedness[count].classification[0].label == "Right":
                        hands_array_R = landmarks_list_to_array(hand_landmarks)
                        # print("R")


            json_data = {
                'handsR': hands_array_R,
                'handsL': hands_array_L,
                'frame': frame
            }
            yield json_data
            if debug:
                json_path = "C:/Danial/Projects/Danial/DigiHuman/Backend/hand_json/"
                Save_Json(json_path,frame,json_data)
            # Flip the image horizontally for a selfie-view display.
            cv2.imshow('MediaPipe Hands', cv2.flip(image, 1))
            if cv2.waitKey(5) & 0xFF == 27:
                break
    cap.release()


# if __name__ == '__main__':
    # for i in Complete_pose_Video(video_path="D:\\pose\\New\\2022-07-14\\C2828.MP4",debug=True):
    #     continue
    # for i in Hands_Full(video_path="C:\Danial\Projects\Danial\DigiHuman\Backend\Video\WIN_20220414_23_51_39_Pro.mp4"):
    #     continue

    # for i in Pose_Video(video_path="D:\\pose\\New\\2022-07-14\\C2831.MP4", debug=True):
    #     continue
#
#     print("finished")

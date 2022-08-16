import math
import numpy as np
from .facedata import FaceData, FaceBlendShape
from google.protobuf.internal.containers import RepeatedCompositeFieldContainer
from .blendshape_config import BlendShapeConfig

#Big Thanks to https://github.com/JimWest/MeFaMo for his great repository

class BlendshapeCalculator():
    """ BlendshapeCalculator class

    This class calculates the blendshapes from the given landmarks.
    """

    def __init__(self) -> None:
        self.blend_shape_config = BlendShapeConfig()

    def calculate_blendshapes(self, face_data: FaceData, metric_landmarks: np.ndarray,
                              normalized_landmarks: RepeatedCompositeFieldContainer) -> None:
        """ Calculate the blendshapes from the given landmarks.

        This function calculates the blendshapes from the given landmarks and stores them in the given live_link_face.

        Parameters
        ----------
        face_data : FaceData
            Index of the BlendShape to get the value from.
        metric_landmarks: np.ndarray
            The metric landmarks of the face in 3d.
        normalized_landmarks: RepeatedCompositeFieldContainer
            Output from the mediapipe process function for each face.

        Returns
        ----------
        None
        """

        self._face_data = face_data
        self._metric_landmarks = metric_landmarks
        self._normalized_landmarks = normalized_landmarks

        self._calculate_mouth_landmarks()
        self._calculate_eye_landmarks()

    def _get_landmark(self, index: int, use_normalized: bool = False) -> np.array:
        """ Get the stored landmark from the given index.

        This function converts both the metric and normalized landmarks to a numpy array.

        Parameters
        ----------
        index : int
            Index of the point to get the landmark from.
        use_normalized: bool
            If true, the normalized landmarks are used. Otherwise the metric landmarks are used.

        Returns
        ----------
        np.array
            The landmark in a 3d numpy array.
        """

        landmarks = self._metric_landmarks
        if use_normalized:
            landmarks = self._normalized_landmarks

        if type(landmarks) == np.ndarray:
            # is a 3d landmark
            x = landmarks[index][0]
            y = landmarks[index][1]
            z = landmarks[index][2]
            return np.array([x, y, z])
        else:
            # is a normalized landmark
            x = landmarks[index].x  # * self.image_width
            y = landmarks[index].y  # * self.image_height
            z = landmarks[index].z  # * self.image_height
            return np.array([x, y, z])

            #  clamp value to 0 - 1 using the min and max values of the config

    def _remap(self, value, min, max):
        return (np.clip(value, min, max) - min) / (max - min)

    def _remap_blendshape(self, index: FaceBlendShape, value: float):
        min, max = self.blend_shape_config.config.get(index)
        return self._remap(value, min, max)

    def dist(self,p, q):
        return math.sqrt(sum((px - qx) ** 2.0 for px, qx in zip(p, q)))
    def dist2(self,p, q,o):
        return math.sqrt(sum((px - ox) ** 2.0 + (px - qx) ** 2.0 for px, qx,ox in zip(p, q,o)))
    def _calculate_mouth_landmarks(self):
        upper_lip = self._get_landmark(self.blend_shape_config.CanonicalPoints.upper_lip)
        upper_outer_lip = self._get_landmark(self.blend_shape_config.CanonicalPoints.upper_outer_lip)
        lower_lip = self._get_landmark(self.blend_shape_config.CanonicalPoints.lower_lip)

        mouth_corner_left = self._get_landmark(self.blend_shape_config.CanonicalPoints.mouth_corner_left)
        mouth_corner_right = self._get_landmark(self.blend_shape_config.CanonicalPoints.mouth_corner_right)
        lowest_chin = self._get_landmark(self.blend_shape_config.CanonicalPoints.lowest_chin)
        nose_tip = self._get_landmark(self.blend_shape_config.CanonicalPoints.nose_tip)
        upper_head = self._get_landmark(self.blend_shape_config.CanonicalPoints.upper_head)

        mouth_width = self.dist(mouth_corner_left, mouth_corner_right)
        mouth_center = (upper_lip + lower_lip) / 2
        mouth_open_dist = self.dist(upper_lip, lower_lip)
        mouth_center_nose_dist = self.dist(mouth_center, nose_tip)

        jaw_nose_dist = self.dist(lowest_chin, nose_tip)
        head_height = self.dist(upper_head, lowest_chin)
        jaw_open_ratio = jaw_nose_dist / head_height

        #Jaw Open
        jaw_open = self._remap_blendshape(FaceBlendShape.JawOpen, jaw_open_ratio)
        self._face_data.set_blendshape(FaceBlendShape.JawOpen, jaw_open)

        #mouth open/close
        mouth_close = self._remap_blendshape(FaceBlendShape.MouthClose, mouth_center_nose_dist - mouth_open_dist)
        self._face_data.set_blendshape(FaceBlendShape.MouthClose, mouth_close)

        mouth_open = self._remap_blendshape(FaceBlendShape.MouthOpen, mouth_open_dist/mouth_width) #mouth aspect ratio
        self._face_data.set_blendshape(FaceBlendShape.MouthOpen, mouth_open)

        #Simle
        mouth_smile_left,mouth_smile_right = self.detect_smile(upper_lip,mouth_corner_left,mouth_corner_right)

        #mouth frown
        self.detect_mouth_frown(mouth_corner_left,mouth_corner_right)

        #mouth is stretched left or right
        mouth_left, mouth_right = self.detect_mouth_Stretch(mouth_center,mouth_corner_left,mouth_corner_right,mouth_smile_left,mouth_smile_right)




        uppest_lip = self._get_landmark(0)
        lowest_lip = self._get_landmark(self.blend_shape_config.CanonicalPoints.lowest_lip)
        under_lip = self._get_landmark(self.blend_shape_config.CanonicalPoints.under_lip)

        #Jaw left right
        self.detect_Jaw_direction(nose_tip,lowest_chin)

        #mouth roll
        self.detect_Mouth_Roll(lower_lip,upper_lip,upper_outer_lip,lowest_lip)

        #mouth pucker
        self.detect_mouth_pucker(mouth_width,mouth_open_dist)

        #Mouth shrug
        self.detect_mouth_shrug(nose_tip,uppest_lip,lowest_lip)

        #When whole mouth is lower down left or right
        self.detect_mouth_lower_direction(mouth_open_dist)

        #detect corners lip direction
        # self.detect_lip_direction(mouth_open,mouth_left,mouth_right)
        #lower lip
        lower_down_left = self.dist(self._get_landmark(
            321), self._get_landmark(395))
        lower_down_left2 = self.dist(self._get_landmark(
            321), self._get_landmark(364))

        lower_down_right = self.dist(self._get_landmark(
            91), self._get_landmark(170))
        lower_down_right2 = self.dist(self._get_landmark(
            91), self._get_landmark(135))


        lower_down_left_lip = 1 - self._remap_blendshape(FaceBlendShape.LipLowerDownLeft, (lower_down_left + lower_down_left2)/2.0)
        lower_down_right_final = 1 - self._remap_blendshape(FaceBlendShape.LipLowerDownRight,(lower_down_right + lower_down_right2)/2.0)
        self._face_data.set_blendshape(FaceBlendShape.LipLowerDownLeft, lower_down_left_lip)
        self._face_data.set_blendshape(FaceBlendShape.LipLowerDownRight, lower_down_right_final)

        # upper lip
        upper_up_left = self.dist(self._get_landmark(
            270), self._get_landmark(425))
        upper_up_left2 = self.dist(self._get_landmark(
            270), self._get_landmark(266))

        upper_up_right = self.dist(self._get_landmark(
            40), self._get_landmark(205))
        upper_up_right2 = self.dist(self._get_landmark(
            40), self._get_landmark(36))

        upper_up_left_lip = 1 - self._remap_blendshape(FaceBlendShape.LipUpperUpLeft, (upper_up_left + upper_up_left2)/2.0)
        upper_up_right_final = 1 - self._remap_blendshape(FaceBlendShape.LipUpperUpRight,(upper_up_right + upper_up_right2)/2.0)
        self._face_data.set_blendshape(FaceBlendShape.LipUpperUpLeft, upper_up_left_lip)
        self._face_data.set_blendshape(FaceBlendShape.LipUpperUpRight, upper_up_right_final)

        #Mouth press
        self.detect_mouth_press()


        # really hard to do this, mediapipe is not really moving here
        # right_under_eye = self._get_landmark(350)
        # nose_sneer_right_dist = self.dist(nose_tip, right_under_eye)
        # print(nose_sneer_right_dist)
        # same with cheek puff


    def detect_smile(self,upper_lip,mouth_corner_left,mouth_corner_right):
        # Smile
        # TODO mouth open but teeth closed
        smile_left = upper_lip[1] - mouth_corner_left[1]
        smile_right = upper_lip[1] - mouth_corner_right[1]

        mouth_smile_left = 1 - \
                           self._remap_blendshape(FaceBlendShape.MouthSmileLeft, smile_left)
        mouth_smile_right = 1 - \
                            self._remap_blendshape(FaceBlendShape.MouthSmileRight, smile_right)

        # print(mouth_smile_right)
        self._face_data.set_blendshape(
            FaceBlendShape.MouthSmileLeft, mouth_smile_left)
        self._face_data.set_blendshape(
            FaceBlendShape.MouthSmileRight, mouth_smile_right)
        # ------------------------------------------------

        #Extra
        self._face_data.set_blendshape(
            FaceBlendShape.MouthDimpleLeft, mouth_smile_left / 2)
        self._face_data.set_blendshape(
            FaceBlendShape.MouthDimpleRight, mouth_smile_right / 2)

        return mouth_smile_left,mouth_smile_right

    def detect_mouth_frown(self,mouth_corner_left,mouth_corner_right):
        #mouth frown
        mouth_frown_left = \
        (mouth_corner_left - self._get_landmark(self.blend_shape_config.CanonicalPoints.mouth_frown_left))[1]
        mouth_frown_right = \
        (mouth_corner_right - self._get_landmark(self.blend_shape_config.CanonicalPoints.mouth_frown_right))[1]

        mouth_frown_left_final = 1 - self._remap_blendshape(FaceBlendShape.MouthFrownLeft, mouth_frown_left)
        self._face_data.set_blendshape(
            FaceBlendShape.MouthFrownLeft, mouth_frown_left_final)

        mouth_frown_right_final = 1 - self._remap_blendshape(FaceBlendShape.MouthFrownRight, mouth_frown_right)
        self._face_data.set_blendshape(
            FaceBlendShape.MouthFrownRight,
            mouth_frown_right_final)
        #-------------------------------------------------

    def detect_mouth_Stretch(self,mouth_center,mouth_corner_left,mouth_corner_right,mouth_smile_left,mouth_smile_right):
        #mouth is stretched left or right

        # todo: also strech when laughing, need to be fixed


        upper_nose = self._get_landmark(self.blend_shape_config.CanonicalPoints.upper_nose)

        # only interested in the axis coordinates here

        # mouth_center_left_stretch = mouth_center[0] - mouth_left_stretch_point[0]
        # mouth_center_right_stretch = mouth_center[0] - mouth_right_stretch_point[0]

        stretch = mouth_center[0] - upper_nose[0]

        mouth_left = self._remap_blendshape(
            FaceBlendShape.MouthLeft, stretch)
        mouth_right = 1 - \
                      self._remap_blendshape(FaceBlendShape.MouthRight,
                                             stretch)
        self._face_data.set_blendshape(
            FaceBlendShape.MouthLeft, mouth_left)
        self._face_data.set_blendshape(
            FaceBlendShape.MouthRight, mouth_right)

        print(mouth_left)

        # self._live_link_face.set_blendshape(ARKitFace.MouthRight, 1 - remap(mouth_left_right, -1.5, 0.0))
        #-------------------------------------------------------

        #Extra
        mouth_left_stretch_point = self._get_landmark(self.blend_shape_config.CanonicalPoints.mouth_left_stretch)
        mouth_right_stretch_point = self._get_landmark(self.blend_shape_config.CanonicalPoints.mouth_right_stretch)

        # only interested in the axis coordinates here
        mouth_left_stretch = mouth_corner_left[0] - mouth_left_stretch_point[0]
        mouth_right_stretch = mouth_right_stretch_point[0] - mouth_corner_right[0]

        stretch_normal_left = -0.7 + \
                              (0.42 * mouth_smile_left) + (0.36 * mouth_left)
        stretch_max_left = -0.45 + \
                           (0.45 * mouth_smile_left) + (0.36 * mouth_left)

        stretch_normal_right = -0.7 + 0.42 * \
                               mouth_smile_right + (0.36 * mouth_right)
        stretch_max_right = -0.45 + \
                            (0.45 * mouth_smile_right) + (0.36 * mouth_right)


        mouth_left_stretch_final = self._remap(mouth_left_stretch, stretch_normal_left, stretch_max_left)
        self._face_data.set_blendshape(FaceBlendShape.MouthStretchLeft, mouth_left_stretch_final)

        mouth_right_stretch_final = self._remap(mouth_right_stretch, stretch_normal_right, stretch_max_right)
        self._face_data.set_blendshape(FaceBlendShape.MouthStretchRight, mouth_right_stretch_final)

        return mouth_left, mouth_right

    # when your mouth roll and shrink this will be near to 1
    def detect_Mouth_Roll(self,lower_lip,upper_lip,upper_outer_lip,lowest_lip):


        outer_lip_dist = self.dist(lower_lip, lowest_lip)
        upper_lip_dist = self.dist(upper_lip, upper_outer_lip)
        self._face_data.set_blendshape(
            FaceBlendShape.MouthRollLower, 1 - self._remap_blendshape(FaceBlendShape.MouthRollLower, outer_lip_dist))
        self._face_data.set_blendshape(
            FaceBlendShape.MouthRollUpper, 1 - self._remap_blendshape(FaceBlendShape.MouthRollUpper, upper_lip_dist))


    def detect_Jaw_direction(self,nose_tip,lowest_chin):
        #Jaw left right
        # jaw only interesting on x yxis
        jaw_right_left = nose_tip[0] - lowest_chin[0]

        # TODO: this is not face rotation resistant
        jaw_left = 1 - self._remap_blendshape(FaceBlendShape.JawLeft, jaw_right_left)
        self._face_data.set_blendshape(
            FaceBlendShape.JawLeft, jaw_left)

        jaw_right = self._remap_blendshape(FaceBlendShape.JawRight, jaw_right_left)
        self._face_data.set_blendshape(FaceBlendShape.JawRight, jaw_right)
        #-------------------------------

    # will appear in the time of mouth press(when you hide your lips by pulling it inside your mouth!)
    def detect_mouth_press(self):
        left_upper_press = self.dist(
            self._get_landmark(self.blend_shape_config.CanonicalPoints.left_upper_press[0]),
            self._get_landmark(self.blend_shape_config.CanonicalPoints.left_upper_press[1])
        )
        left_lower_press = self.dist(
            self._get_landmark(self.blend_shape_config.CanonicalPoints.left_lower_press[0]),
            self._get_landmark(self.blend_shape_config.CanonicalPoints.left_lower_press[1])
        )
        mouth_press_left = (left_upper_press + left_lower_press) / 2

        right_upper_press = self.dist(
            self._get_landmark(self.blend_shape_config.CanonicalPoints.right_upper_press[0]),
            self._get_landmark(self.blend_shape_config.CanonicalPoints.right_upper_press[1])
        )
        right_lower_press = self.dist(
            self._get_landmark(self.blend_shape_config.CanonicalPoints.right_lower_press[0]),
            self._get_landmark(self.blend_shape_config.CanonicalPoints.right_lower_press[1])
        )
        mouth_press_right = (right_upper_press + right_lower_press) / 2

        mouth_press_left_final = 1 - self._remap_blendshape(FaceBlendShape.MouthPressLeft, mouth_press_left)
        mouth_press_right_final = 1 - self._remap_blendshape(FaceBlendShape.MouthPressRight, mouth_press_right)

        # print(mouth_press_left_final)
        self._face_data.set_blendshape(
            FaceBlendShape.MouthPressLeft, mouth_press_left_final)
        self._face_data.set_blendshape(
            FaceBlendShape.MouthPressRight,
            mouth_press_right_final
            )

    #will appear when your whole mouth goes to corner left down or right direction
    def detect_mouth_lower_direction(self,mouth_open_dist):
        lower_down_left = self.dist(self._get_landmark(
            424), self._get_landmark(319)) + mouth_open_dist * 0.5
        lower_down_right = self.dist(self._get_landmark(
            204), self._get_landmark(89)) + mouth_open_dist * 0.5

        lower_down_left_final = 1 - self._remap_blendshape(FaceBlendShape.MouthLowerDownLeft, lower_down_left)
        lower_down_right_final = 1 -self._remap_blendshape(FaceBlendShape.MouthLowerDownRight,lower_down_right)
        # print(self.dist(self._get_landmark(
        #     424), self._get_landmark(319)))
        self._face_data.set_blendshape(FaceBlendShape.MouthLowerDownLeft, lower_down_left_final)
        self._face_data.set_blendshape(FaceBlendShape.MouthLowerDownRight, lower_down_right_final)

    #will appear when your quarter of your mouth(left_up_lip, left _down_lip,right_up_lip,right_down_lip) turns more to its corner
    def detect_lip_direction(self,mouth_open_dist,mouth_left,mouth_right):
        lower_down_left = self.dist(self._get_landmark(
            424), self._get_landmark(319))
        lower_down_right = self.dist(self._get_landmark(
            204), self._get_landmark(89))

        lower_down_left_lip = 1 - self._remap_blendshape(FaceBlendShape.LipLowerDownLeft, lower_down_left)
        lower_down_right_final = 1 -self._remap_blendshape(FaceBlendShape.LipLowerDownRight,lower_down_right)
        # print(lower_down_left + (mouth_open_dist - mouth_left))
        self._face_data.set_blendshape(FaceBlendShape.LipLowerDownLeft, lower_down_left_lip)
        self._face_data.set_blendshape(FaceBlendShape.LipLowerDownRight, lower_down_right_final)

    def detect_mouth_shrug(self,nose_tip,uppest_lip,lowest_lip):
        #mouth shrug up will be near 1 if upper mouth is near nose!
        upper_lip_nose_dist = nose_tip[1] - uppest_lip[1]
        mouth_shrug_upper = 1 - self._remap_blendshape(FaceBlendShape.MouthShrugUpper, upper_lip_nose_dist)
        self._face_data.set_blendshape(FaceBlendShape.MouthShrugUpper,mouth_shrug_upper)

        over_upper_lip = self._get_landmark(self.blend_shape_config.CanonicalPoints.over_upper_lip)
        mouth_shrug_lower = self.dist(lowest_lip, over_upper_lip)

        #not good
        mouth_shrug_lower_final = 1 - self._remap_blendshape(FaceBlendShape.MouthShrugLower, mouth_shrug_lower)
        self._face_data.set_blendshape(
            FaceBlendShape.MouthShrugLower,mouth_shrug_lower_final)

        #------------------------------------------------------------------

    def detect_mouth_pucker(self,mouth_width,mouth_open_dist):
        mouth_pucker = self._remap_blendshape(
            FaceBlendShape.MouthPucker, mouth_open_dist/mouth_width)
        # self._face_data.set_blendshape(
        #     FaceBlendShape.MouthPucker, 1 - mouth_pucker)
        self._face_data.set_blendshape(
            FaceBlendShape.MouthPucker, mouth_pucker)

        # print(mouth_pucker)

        # mouth funnel only can be seen if mouth pucker is really small
        if self._face_data.get_blendshape(FaceBlendShape.MouthPucker) < 0.5:
            self._face_data.set_blendshape(
                FaceBlendShape.MouthFunnel, 1 - self._remap_blendshape(FaceBlendShape.MouthFunnel, mouth_width))
        else:
            self._face_data.set_blendshape(FaceBlendShape.MouthFunnel, 0)

    def _eye_lid_distance(self, eye_points):
        eye_width = self.dist(self._get_landmark(
            eye_points[0]), self._get_landmark(eye_points[1]))
        eye_outer_lid = self.dist(self._get_landmark(
            eye_points[2]), self._get_landmark(eye_points[5]))
        eye_mid_lid = self.dist(self._get_landmark(
            eye_points[3]), self._get_landmark(eye_points[6]))
        eye_inner_lid = self.dist(self._get_landmark(
            eye_points[4]), self._get_landmark(eye_points[7]))
        eye_lid_avg = (eye_outer_lid + eye_mid_lid + eye_inner_lid) / 3
        ratio = eye_lid_avg / eye_width
        return ratio

    def _calculate_eye_landmarks(self):
        # Using EAR(Eye Aspect Ratio) for detecting blinks
        def get_eye_open_ration(points):
            eye_distance = self._eye_lid_distance(points)
            max_ratio = 0.285
            ratio = np.clip(eye_distance / max_ratio, 0, 2)
            return ratio

        #Blinks
        self.detect_blinks(get_eye_open_ration)

        squint_left = self.dist(
            self._get_landmark(self.blend_shape_config.CanonicalPoints.squint_left[0]),
            self._get_landmark(self.blend_shape_config.CanonicalPoints.squint_left[1])
        )
        self._face_data.set_blendshape(
            FaceBlendShape.EyeSquintLeft, self._remap_blendshape(FaceBlendShape.EyeSquintLeft, squint_left))

        squint_right = self.dist(
            self._get_landmark(self.blend_shape_config.CanonicalPoints.squint_right[0]),
            self._get_landmark(self.blend_shape_config.CanonicalPoints.squint_right[1])
        )
        self._face_data.set_blendshape(
            FaceBlendShape.EyeSquintRight, self._remap_blendshape(FaceBlendShape.EyeSquintRight, squint_right))

        # print(self._remap_blendshape(FaceBlendShape.EyeSquintRight, squint_right))

        #Brow
        self.detect_brow_actions()

        #Cheek
        self.detect_cheek()


    def detect_blinks(self,get_eye_open_ration):
        # Eye Blink ---------------
        eye_open_ratio_left = get_eye_open_ration(self.blend_shape_config.CanonicalPoints.eye_left)
        eye_open_ratio_right = get_eye_open_ration(self.blend_shape_config.CanonicalPoints.eye_right)

        blink_left = 1 - \
                     self._remap_blendshape(
                         FaceBlendShape.EyeBlinkLeft, eye_open_ratio_left)
        blink_right = 1 - \
                      self._remap_blendshape(
                          FaceBlendShape.EyeBlinkRight, eye_open_ratio_right)

        self._face_data.set_blendshape(FaceBlendShape.EyeBlinkLeft, blink_left, True)
        self._face_data.set_blendshape(FaceBlendShape.EyeBlinkRight, blink_right, True)

        self._face_data.set_blendshape(FaceBlendShape.EyeWideLeft, self._remap_blendshape(
            FaceBlendShape.EyeWideLeft, eye_open_ratio_left))
        self._face_data.set_blendshape(FaceBlendShape.EyeWideRight, self._remap_blendshape(
            FaceBlendShape.EyeWideRight, eye_open_ratio_right))

        # ----------------------------------------

    def detect_brow_actions(self):
        #Brow up down

        right_brow_lower = (
                                   self._get_landmark(self.blend_shape_config.CanonicalPoints.right_brow_lower[0]) +
                                   self._get_landmark(self.blend_shape_config.CanonicalPoints.right_brow_lower[1]) +
                                   self._get_landmark(self.blend_shape_config.CanonicalPoints.right_brow_lower[2])
                           ) / 3
        right_brow_dist = self.dist(self._get_landmark(self.blend_shape_config.CanonicalPoints.right_brow),
                                    right_brow_lower)

        left_brow_lower = (
                                  self._get_landmark(self.blend_shape_config.CanonicalPoints.left_brow_lower[0]) +
                                  self._get_landmark(self.blend_shape_config.CanonicalPoints.left_brow_lower[1]) +
                                  self._get_landmark(self.blend_shape_config.CanonicalPoints.left_brow_lower[2])
                          ) / 3
        left_brow_dist = self.dist(self._get_landmark(self.blend_shape_config.CanonicalPoints.left_brow),
                                   left_brow_lower)

        brow_down_left = 1 - self._remap_blendshape(FaceBlendShape.BrowDownLeft, left_brow_dist)
        self._face_data.set_blendshape(FaceBlendShape.BrowDownLeft, brow_down_left)

        brow_outer_up_left = self._remap_blendshape(FaceBlendShape.BrowOuterUpLeft, left_brow_dist)
        self._face_data.set_blendshape(FaceBlendShape.BrowOuterUpLeft, brow_outer_up_left)

        brow_down_right = 1 - self._remap_blendshape(FaceBlendShape.BrowDownRight, right_brow_dist)
        self._face_data.set_blendshape(FaceBlendShape.BrowDownRight, brow_down_right)

        brow_outer_up_right = self._remap_blendshape(FaceBlendShape.BrowOuterUpRight, right_brow_dist)
        self._face_data.set_blendshape(FaceBlendShape.BrowOuterUpRight, brow_outer_up_right)
        # print(brow_outer_up_right)
        #-------------------------------------------------

        #Extra
        inner_brow = self._get_landmark(self.blend_shape_config.CanonicalPoints.inner_brow)
        upper_nose = self._get_landmark(self.blend_shape_config.CanonicalPoints.upper_nose)
        inner_brow_dist = self.dist(upper_nose, inner_brow)

        brow_inner_up = self._remap_blendshape(FaceBlendShape.BrowInnerUp, inner_brow_dist)
        self._face_data.set_blendshape(FaceBlendShape.BrowInnerUp, brow_inner_up)
        # print(brow_inner_up)


    def detect_cheek(self):
        # Cheek is turned over left or right (will be higher when goes right up or left up) (will be 1 when nose also turn over)

        cheek_squint_left = self.dist(
            self._get_landmark(self.blend_shape_config.CanonicalPoints.cheek_squint_left[0]),
            self._get_landmark(self.blend_shape_config.CanonicalPoints.cheek_squint_left[1])
        )

        cheek_squint_right = self.dist(
            self._get_landmark(self.blend_shape_config.CanonicalPoints.cheek_squint_right[0]),
            self._get_landmark(self.blend_shape_config.CanonicalPoints.cheek_squint_right[1])
        )

        cheek_squint_left_final = 1 - self._remap_blendshape(FaceBlendShape.CheekSquintLeft, cheek_squint_left)
        self._face_data.set_blendshape(FaceBlendShape.CheekSquintLeft, cheek_squint_left_final)

        cheek_squint_right_final = 1 - self._remap_blendshape(FaceBlendShape.CheekSquintRight, cheek_squint_right)
        self._face_data.set_blendshape(FaceBlendShape.CheekSquintRight, cheek_squint_right_final)

        # print(cheek_squint_right_final)

        # ----------------------------------------------

        # just use the same values for cheeksquint for nose sneer, mediapipe deosn't seem to have a separate value for nose sneer
        self._face_data.set_blendshape(
            FaceBlendShape.NoseSneerLeft, self._face_data.get_blendshape(FaceBlendShape.CheekSquintLeft))
        self._face_data.set_blendshape(
            FaceBlendShape.NoseSneerRight, self._face_data.get_blendshape(FaceBlendShape.CheekSquintRight))
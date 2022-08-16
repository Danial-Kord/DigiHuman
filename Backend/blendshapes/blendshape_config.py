
from .facedata import FaceBlendShape


class BlendShapeConfig:
        class CanonicalPoints:
            # Canonical points mapped from the canonical face model
            # for better understanding of the points, see the canonical face model from mediaPipe
            # https://github.com/google/mediapipe/blob/master/mediapipe/modules/face_geometry/data/canonical_face_model_uv_visualization.png

            eye_right = [33, 133, 160, 159, 158, 144, 145, 153]
            eye_left = [263, 362, 387, 386, 385, 373, 374, 380]
            head = [10, 152]
            nose_tip = 1
            upper_lip = 13
            lower_lip = 14
            upper_outer_lip = 12
            mouth_corner_left = 291
            mouth_corner_right = 61
            lowest_chin = 152
            upper_head = 10
            mouth_frown_left = 422
            mouth_frown_right = 202
            mouth_left_stretch = 287
            mouth_right_stretch = 57
            uppest_lip = 0
            lowest_lip = 17
            under_lip = 18
            over_upper_lip = 164
            left_upper_press = [40, 80]
            left_lower_press = [88, 91]
            right_upper_press = [270, 310]
            right_lower_press = [318, 321]
            squint_left = [253, 450]
            squint_right = [23, 230]            
            right_brow = 27
            right_brow_lower = [53, 52, 65]
            left_brow = 257
            left_brow_lower = [283, 282, 295]
            inner_brow = 9
            upper_nose = 6
            cheek_squint_left = [359, 342]
            cheek_squint_right = [130, 113]

        # blend shape type, min and max value
        config = {
            FaceBlendShape.EyeBlinkLeft : (0.40, 0.70),
            # FaceBlendShape.EyeLookDownLeft : (-0.4, 0.0),
            # FaceBlendShape.EyeLookInLeft : (-0.4, 0.0),
            # FaceBlendShape.EyeLookOutLeft : (-0.4, 0.0),
            # FaceBlendShape.EyeLookUpLeft : (-0.4, 0.0),
            FaceBlendShape.EyeSquintLeft : (0.37, 0.6),
            FaceBlendShape.EyeWideLeft : (0.9, 1.2),
            FaceBlendShape.EyeBlinkRight : (0.40, 0.70),
            # FaceBlendShape.EyeLookDownRight : (-0.4, 0.0),
            # FaceBlendShape.EyeLookInRight : (-0.4, 0.0),
            # FaceBlendShape.EyeLookOutRight : (-0.4, 0.0),
            # FaceBlendShape.EyeLookUpRight : (-0.4, 0.0),
            FaceBlendShape.EyeSquintRight : (0.37, 0.6),
            FaceBlendShape.EyeWideRight : (0.9, 1.2),
            # FaceBlendShape.JawForward : (-0.4, 0.0),
            FaceBlendShape.JawLeft : (-0.4, 0.0),
            FaceBlendShape.JawRight : (0.0, 0.4),
            FaceBlendShape.JawOpen : (0.50, 0.55),
            FaceBlendShape.MouthClose : (3.0, 4.5),
            FaceBlendShape.MouthOpen : (0.02, 0.62),
            FaceBlendShape.MouthFunnel : (4.0, 4.8),
            # FaceBlendShape.MouthPucker : (3.46, 4.92),
            FaceBlendShape.MouthPucker : (0, 1.2),
            # FaceBlendShape.MouthLeft : (-3.4, -2.3),
            # FaceBlendShape.MouthRight : ( 1.5, 3.0),
            FaceBlendShape.MouthLeft: (0.4, 1.2),
            FaceBlendShape.MouthRight: (-1.2, -0.4),
            FaceBlendShape.MouthSmileLeft : (-0.25, 0.0),
            FaceBlendShape.MouthSmileRight : (-0.25, 0.0),
            FaceBlendShape.MouthFrownLeft : (0.4, 0.9),
            FaceBlendShape.MouthFrownRight : (0.4, 0.9),
            # FaceBlendShape.MouthDimpleLeft : (-0.4, 0.0),
            # FaceBlendShape.MouthDimpleRight : (-0.4, 0.0),
            FaceBlendShape.MouthStretchLeft : (-0.4, 0.0),
            FaceBlendShape.MouthStretchRight : (-0.4, 0.0),
            FaceBlendShape.MouthRollLower : (0.4, 0.7),
            FaceBlendShape.MouthRollUpper : (0.31, 0.34),
            FaceBlendShape.MouthShrugLower : (1.9, 2.3),
            FaceBlendShape.MouthShrugUpper : (1.4, 2.4),
            FaceBlendShape.MouthPressLeft : (0.4, 0.5),
            FaceBlendShape.MouthPressRight : (0.4, 0.5),
            FaceBlendShape.MouthLowerDownLeft : (1.7, 2),
            FaceBlendShape.MouthLowerDownRight : (1.7, 2),
            # FaceBlendShape.MouthUpperUpLeft : (-0.4, 0.0),
            # FaceBlendShape.MouthUpperUpRight : (-0.4, 0.0),
            FaceBlendShape.BrowDownLeft : (1.0, 1.2),
            FaceBlendShape.BrowDownRight : (1.0, 1.2),
            FaceBlendShape.BrowInnerUp : (2.2, 2.6),
            FaceBlendShape.BrowOuterUpLeft : (1.25, 1.5),
            FaceBlendShape.BrowOuterUpRight : (1.25, 1.5),
            # FaceBlendShape.CheekPuff : (-0.4, 0.0),
            FaceBlendShape.CheekSquintLeft : (0.55, 0.63),
            FaceBlendShape.CheekSquintRight : (0.55, 0.63),
            # FaceBlendShape.NoseSneerLeft : (-0.4, 0.0),
            # FaceBlendShape.NoseSneerRight : (-0.4, 0.0),
            # FaceBlendShape.TongueOut : (-0.4, 0.0),
            # FaceBlendShape.HeadYaw : (-0.4, 0.0),
            # FaceBlendShape.HeadPitch : (-0.4, 0.0),
            # FaceBlendShape.HeadRoll : (-0.4, 0.0),
            # FaceBlendShape.LeftEyeYaw : (-0.4, 0.0),
            # FaceBlendShape.LeftEyePitch : (-0.4, 0.0),
            # FaceBlendShape.LeftEyeRoll : (-0.4, 0.0),
            # FaceBlendShape.RightEyeYaw : (-0.4, 0.0),
            # FaceBlendShape.RightEyePitch : (-0.4, 0.0),
            # FaceBlendShape.RightEyeRoll : (-0.4, 0.0),
            FaceBlendShape.LipLowerDownLeft: (2.7, 3.15),
            FaceBlendShape.LipLowerDownRight: (2.6, 3.15),
            FaceBlendShape.LipUpperUpLeft: (2.5, 3.05),
            FaceBlendShape.LipUpperUpRight: (2.45, 3.05),
        }

       

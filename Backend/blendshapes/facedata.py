from __future__ import annotations
from collections import deque
from statistics import mean
from enum import Enum
import struct
from typing import Tuple
import datetime
import uuid



class FaceBlendShape(Enum):
    EyeBlinkLeft = 0
    EyeLookDownLeft = 1
    EyeLookInLeft = 2
    EyeLookOutLeft = 3
    EyeLookUpLeft = 4
    EyeSquintLeft = 5
    EyeWideLeft = 6
    EyeBlinkRight = 7
    EyeLookDownRight = 8
    EyeLookInRight = 9
    EyeLookOutRight = 10
    EyeLookUpRight = 11
    EyeSquintRight = 12
    EyeWideRight = 13
    JawForward = 14
    JawLeft = 15
    JawRight = 16
    JawOpen = 17
    MouthOpen = 18
    MouthFunnel = 19
    MouthPucker = 20
    MouthLeft = 21
    MouthRight = 22
    MouthSmileLeft = 23
    MouthSmileRight = 24
    MouthFrownLeft = 25
    MouthFrownRight = 26
    MouthDimpleLeft = 27
    MouthDimpleRight = 28
    MouthStretchLeft = 29
    MouthStretchRight = 30
    MouthRollLower = 31
    MouthRollUpper = 32
    MouthShrugLower = 33
    MouthShrugUpper = 34
    MouthPressLeft = 35
    MouthPressRight = 36
    MouthLowerDownLeft = 37
    MouthLowerDownRight = 38
    MouthUpperUpLeft = 39
    MouthUpperUpRight = 40
    MouthClose = 41
    LipLowerDownLeft = 42
    LipLowerDownRight = 43
    LipUpperUpLeft = 44
    LipUpperUpRight = 45
    CheekPuff = 46
    CheekSquintLeft = 47
    CheekSquintRight = 48
    NoseSneerLeft = 49
    NoseSneerRight = 50
    TongueOut = 51
    HeadYaw = 52
    HeadPitch = 53
    HeadRoll = 54
    LeftEyeYaw = 55
    LeftEyePitch = 56
    LeftEyeRoll = 57
    RightEyeYaw = 58
    RightEyePitch = 59
    RightEyeRoll = 60
    BrowDownLeft = 61
    BrowDownRight = 62
    BrowInnerUp = 63
    BrowOuterUpLeft = 64
    BrowOuterUpRight = 65
class FaceData:

    def __init__(self,filter_size: int = 5) -> None:

        # properties
        self._filter_size = filter_size
        self._blend_shapes = [0.000] * 66
        self._old_blend_shapes = []  # used for filtering
        for i in range(66):
            self._old_blend_shapes.append(deque([0.0], maxlen=self._filter_size))




    def get_blendshape(self, index: FaceBlendShape) -> float:
        """ Get the current value of the blend shape.
        Parameters
        ----------
        index : FaceBlendShape
            Index of the BlendShape to get the value from.
        Returns
        -------
        float
            The value of the BlendShape.
        """
        return self._blend_shapes[index.value]


    def get_all_blendshapes(self) -> list:
        """ Get the current value of the blend shape.
        Parameters
        ----------
        index : FaceBlendShape
            Index of the BlendShape to get the value from.
        Returns
        -------
        list of floats
            array of BlendShape.
        """
        return self._blend_shapes

    def set_blendshape(self, index: FaceBlendShape, value: float,
                       no_filter: bool = False) -> None:
        """ Sets the value of the blendshape.

        The function will use mean to filter between the old and the new
        values, unless `no_filter` is set to True.
        Parameters
        ----------
        index : FaceBlendShape
            Index of the BlendShape to get the value from.
        value: float
            Value to set the BlendShape to, should be in the range of 0 - 1 for
            the blendshapes and between -1 and 1 for the head rotation
            (yaw, pitch, roll).
        no_filter: bool
            If set to True, the blendshape will be set to the value without
            filtering.

        Returns
        ----------
        None
        """
        value *= 100

        if no_filter:
            self._blend_shapes[index.value] = value
        else:
            self._old_blend_shapes[index.value].append(value)
            filterd_value = mean(self._old_blend_shapes[index.value])
            self._blend_shapes[index.value] = filterd_value


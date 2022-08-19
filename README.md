# DigiHuman
Digihuman is a project which aims to automatically generate <b>whole body pose animation + facial animation</b> on 3D Character models based on the camera input.
<br/>
This project is my B.Sc thesis of Computer Engineering at Amikabir University of Technology(AUT).



## About DigiHuman
DigiHuman is a system for bringing automation in animation generation on 3D virtual characters.
It uses Pose estimation and facial landmarks generator models to create entire body and face animation on 3D characters.
<br/>
This project is done with **MediaPipe** and **Unity3D**.
MediaPipe generates 3D landmarks for the human whole body and face, and Unity3D is used to render the final animation after processing the generated landmarks from MediaPipe.


<!-- GETTING STARTED -->
## Installatiom
Follow the instruction to run the program!
### Backend server installtion
1. Install MediaPipe and its dependencies based on your Operating System: [link](https://google.github.io/mediapipe/getting_started/install.html)
2. Go to `backend` directory and install GauGan requirements:
  ```py
   pip install -r requirements.txt
   ```
3. You'll need to [download](https://drive.google.com/file/d/15VSa2m2F6Ch0NpewDR7mkKAcXlMgDi5F/view?usp=sharing) the pretrained generator model for the COCO dataset into `backend/checkpoints/coco_pretrained/`.

### Unity3D Installation
Install Unity3D and its requirements by the following guide lines. (Skip if you have Unity3D Installed)
1. Download and install  [UnityHub](https://unity.com/download)
2. Add a new licence in UnityHub and register it on Unity website
3. Install a Unity Editor inside UnityHub(Better to be `LTS` and a version higher than `2020.3.25f1`.
4. Download [default 3D character](link.com) and place it under `Assets\Models` sdirectory.(build this directory first, if it does not exist)
5. Download and import the following packages into your project for making recording option available with FFmpeg.(Download `.unitypackage` files and drag them to your project)

- [FFmpegOut package] (MIT license)
- [FFmpegOutBinaries package] (GPL)

[FFmpegOut package]: https://github.com/keijiro/FFmpegOut/releases
[FFmpegOutBinaries package]:
    https://github.com/keijiro/FFmpegOutBinaries/releases

# Usage
- Run backend server at `backend` directory with the following command:
  ```
   python server.py
   ```
- Run Unity Project and open the main scene at `Assets\Scenes\Main.unity`
- Test the program by uploading videos to backend from Unity project.

## Adding new 3D characters
You can add your own characters to the project!
Characters should have standard Humanoid rig to show kinematic animations. For rendering face animations, hcaracters should have blendmesh for their face.
Follow these steps to add your character:
1. Drag and drop your 3D charcter model to the Unity Scene(You can download them from websites like [Mixamo](mixamo.com/))
2. Open the character setting and set the rig to humanoid
3. Place your new character under Character slideshow and add it to the slideshow items.
4. Add BlendshapeController and QualityData component to your model
5. Set character skinmesh renderer pointer to BlendshapeController.
6. 


## License & Citation
### DigiHuman Licence
  
### FFmpeg</br>
- FFmpeg is licensed under the [GNU Lesser General Public License (LGPL) version 2.1](http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html) or later. However, FFmpeg incorporates several optional parts and optimizations that are covered by the [GNU General Public License (GPL) version 2](http://www.gnu.org/licenses/old-licenses/gpl-2.0.html) or later. If those parts get used the GPL applies to all of FFmpeg. 
- Unity FFmpeg packages are licenced under [Keijiro Takahashi MIT](https://github.com/keijiro/FFmpegOut/blob/master/LICENSE.md)

### GauGan
- Used [SPADE](https://github.com/NVlabs/SPADE) repository developed by NVIDIA
```
@inproceedings{park2019SPADE,
  title={Semantic Image Synthesis with Spatially-Adaptive Normalization},
  author={Park, Taesung and Liu, Ming-Yu and Wang, Ting-Chun and Zhu, Jun-Yan},
  booktitle={Proceedings of the IEEE Conference on Computer Vision and Pattern Recognition},
  year={2019}
}
```

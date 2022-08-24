# DigiHuman

Digihuman is a project which aims to automatically generate <b>whole body pose animation + facial animation</b> on 3D Character models based on the camera input.
<br/>
This project is my B.Sc thesis of Computer Engineering at Amikabir University of Technology(AUT).



## About DigiHuman
DigiHuman is a system for bringing automation in animation generation on 3D virtual characters.
It uses Pose estimation and facial landmarks generator models to create entire body and face animation on 3D characters.
<br/>
This project is done with [**MediaPipe**](https://github.com/google/mediapipe) and **Unity3D**.
MediaPipe generates 3D landmarks for the human whole body and face, and Unity3D is used to render the final animation after processing the generated landmarks from MediaPipe.



## Sample Outputs of the project
### Hands animations demo

### Full body animation

### Face animation

<!-- GETTING STARTED -->
## Installatiom
Follow the instruction to run the program!
### Backend server installtion
1. Install MediaPipe and its dependencies based on your Operating System: [link](https://google.github.io/mediapipe/getting_started/install.html)
2. Go to `backend` directory and install GauGan requirements:
  ```py
   pip install -r requirements.txt
   ```
3. You'll need to [download](https://drive.google.com/file/d/15VSa2m2F6Ch0NpewDR7mkKAcXlMgDi5F/view?usp=sharing) the pretrained generator model for the COCO dataset and place it into `backend/checkpoints/coco_pretrained/`.

### Unity3D Installation
Install Unity3D and its requirements by the following guide lines(Skip 1-3 Unity3D is already installed).
1. Download and install  [UnityHub](https://unity.com/download)
2. Add a new licence in UnityHub and register it on Unity website
3. Install a Unity Editor inside UnityHub(Better to be `LTS` and a version higher than `2020.3.25f1`.
4. Download [default 3D character](link.com) and place it under `Assets\Models` directory(build this directory first, if it does not exist).
5. Download and import the following packages into your project for making recording option available with FFmpeg(Download `.unitypackage` files and drag them to your project).

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
- Test the program by uploading videos to backend from Unity project. (You can test the application by selecting provided animations from the right side menu!)

## Adding new 3D characters
You can add your own characters to the project!
Characters should have standard Humanoid rig to show kinematic animations. For rendering face animations, characters should have blendmesh in their face.</br>
Follow these steps to add your character:
1. Find a 3D character model from [Unity asset store](http://assetstore.unity.com/) or download a free one. (You can download them from websites like [Mixamo](http://mixamo.com/))
2. Open the character setting and set the rig to humanoid

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/3.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/3.png?raw=true" alt="Logo" width="300" height="150">
  </a>
</div>

3. Drag and drop your 3D charcter model to `CharacterChooser/CharacterSlideshow/Parent` object in Unity main Scene like the image below

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/1.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/1.png?raw=true" alt="Logo" width="300" height="400">
  </a>
</div>

4. Add `BlendShapeController` and `QualityData` components to character object in scene(which is dragged inside Parent object in last step).
5. Set `BlendShapeController` values
- Set character `SkinnedMeshRenderer` component to `BlendShapeController` component.

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/5.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/5.png?raw=true" alt="Logo" >
  </a>
</div>

- Find each blnedshape weight number under `SkinnedMeshRenderer` and set those numbers in `BlendShapes` field inside `BlendShapeController` (for specifying each blendshape value to the `BlendShapeController` component so animation would be shown on character face by modification on these blendhape values)

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/6.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/6.png?raw=true" alt="Logo" width="300" height="400">
  </a>
</div>

6. Open `CharacterSlideshow` Object at `CharacterChooser/CharacterSlideshow` path inside scene hierachy, then add new dragged character to the `nodes` property(all characters should be refrenced inside `nodes`).

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/8.jpg">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/8.jpg?raw=true" alt="Logo" width="500" height="300">
  </a>
</div>

7. Run the application and you can now select your character for showing animation!

# Features
<!-- ROADMAP -->

<!-- ## Available features -->
- [x] Making full body animation
- [x] Animating multiple blendshapes on 3D character (upto 40 blendshape animations is supported currently)
- [x] Supporting any 3D models with Humanoid T-Pose
- [x] Exporting animation in a video file
- [x] Saving animation data and re-rendering it in future usages
- [x] Filtering mediapipe outputs in order to detect and remove noises and better smoothness (Low Pass Filtering is used currently) 

<!-- ## TODO -->

- [ ] Animating character face in high details
    - [ ] Training a regression model with deep learning for generating Blendmesh weights by feeding the output data of mediaPipe FaceMesh(468 points)
    - [ ] Using StyleGan techniques to replace whole character face mesh
- [ ] Automatic rigging for 3D models without humanoid rig (Using deep neural network models like RigNet)
- [ ] Generating complete character mesh automatically using models like PIFuHD (in progress!)
- [ ] Animating 3D character mouth in high details using audio signal or natural language processing methods
- [ ] Generating complete environment in 3D



## Licences & Citations
### DigiHuman Licence
  
### FFmpeg</br>
- FFmpeg is licensed under the [GNU Lesser General Public License (LGPL) version 2.1](http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html) or later. However, FFmpeg incorporates several optional parts and optimizations that are covered by the [GNU General Public License (GPL) version 2](http://www.gnu.org/licenses/old-licenses/gpl-2.0.html) or later. If those parts get used the GPL applies to all of FFmpeg. 
- Unity FFmpeg packages are licenced under [Keijiro Takahashi MIT](https://github.com/keijiro/FFmpegOut/blob/master/LICENSE.md)

### GauGan
- Used [SPADE](https://github.com/NVlabs/SPADE) repository developed by NVIDIA and the customization is addapted from [Smart-Sketch](https://github.com/noyoshi/smart-sketch) with [GNU V 3.0](https://github.com/noyoshi/smart-sketch/blob/master/LICENSE) licence
```
@inproceedings{park2019SPADE,
  title={Semantic Image Synthesis with Spatially-Adaptive Normalization},
  author={Park, Taesung and Liu, Ming-Yu and Wang, Ting-Chun and Zhu, Jun-Yan},
  booktitle={Proceedings of the IEEE Conference on Computer Vision and Pattern Recognition},
  year={2019}
}
```

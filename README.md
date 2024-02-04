# DigiHuman

Digihuman is a project which aims to automatically generate <b>whole body pose animation + facial animation</b> on 3D Character models based on the camera input.
<br/>
This project is my B.Sc thesis of Computer Engineering at Amirkabir University of Technology(AUT).



## About DigiHuman
DigiHuman is a system for bringing automation in animation generation on 3D virtual characters.
It uses Pose estimation and facial landmark generator models to create entire body and face animation on 3D virtual characters.
<br/>
DigiHuman is developed with [**MediaPipe**](https://github.com/google/mediapipe) and **Unity3D**.
MediaPipe generates 3D landmarks for the human whole body and face, and Unity3D is used to render the final animation after processing the generated landmarks from MediaPipe. The diagram below, shows the whole architucture of the application.
<div align="center">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/dataFlow.png?raw=true" alt="Logo">
</div>



## Sample Outputs of the project
<div align="center">
<a href="https://youtu.be/maUUXfe_EcU">Project demo</a> | <a href="https://youtu.be/L62w5AMaFOk">Tutorial</a>
</div>

### Hands animations

<div align="center">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/2828_ok.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/2828_ok.gif?raw=true" alt="Logo">
  </a>
  
  <a href="https://thumbs.gfycat.com/VibrantDearestKomododragon-size_restricted.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/2828_1t05.gif?raw=true" alt="Logo">
  </a>
  
</div>


### Full body animation
<div align="center">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/figure_headphone.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/figure_headphone.gif" alt="Logo">
  </a>
    <a href="https://gfycat.com/braveglumguanaco">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/hands_greek.gif" alt="Logo">
  </a>
</div>


### Face animation

<div align="center">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/mouth_deform_1_japan.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/mouth_deform_1_japan.gif?raw=true" alt="Logo">
  </a>
  
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/blinks_1_japan.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/blinks_1_japan.gif?raw=true" alt="Logo">
  </a>
  
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/mouth_1_japan.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/mouth_1_japan.gif?raw=true" alt="Logo">
  </a>
  
   <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/mouth_dir_1_japan.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/mouth_dir_1_japan.gif?raw=true" alt="Logo">
  </a>
  
</div>


<!-- # Donation
Do you want to support me in this project? :D

<p align="left">
  <a href="https://ko-fi.com/danialkord">
  <img src="https://raw.githubusercontent.com/SMotlaq/LoRa/master/bmc.png" width="200" alt="Buy me a Coffee"/>
  </a>
</p> -->


<!-- GETTING STARTED -->
## Installation
Follow the instructions to run the program!
### Backend server installtion
1. Install MediaPipe python.
  ```py
   pip install mediapipe
   ```
3. Install OpenCV python.
  ```py
   pip install opencv-python
   ```
5. Go to `backend` directory and install other requirements:
  ```py
   pip install -r requirements.txt
   ```
6. You'll need to [download](https://drive.google.com/file/d/15VSa2m2F6Ch0NpewDR7mkKAcXlMgDi5F/view?usp=sharing) the pre-trained generator model for the COCO dataset and place it into `backend/checkpoints/coco_pretrained/`.

### Unity3D Installation
Install Unity3D and its requirements by the following guidelines(Skip 1-3 if Unity3D is already installed).
1. Download and install  [UnityHub](https://unity.com/download)
2. Add a new license in UnityHub and register it
3. Install a Unity Editor inside UnityHub(`LTS` versions and a version higher than `2020.3.25f1` are recommended).
4. In the Unity project setting, allow HTTP connections in the player setting.
 
 <div align="center">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/http.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/http.png?raw=true" alt="Logo">
  </a>
  
</div>
 
5. Download and import the following packages into your project to enable the recording option available with FFmpeg(Download `.unitypackage` files and drag them to your project).

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
- Run Unity Project and open the main scene at `Assets\Scenes\MainScene.unity`
- Test the program by uploading videos to backend from the Unity project(You can test the application by selecting provided animations from the right side menu!).

## Adding new 3D characters
You can add your characters to the project!
Characters should have a standard Humanoid rig to show kinematic animations. For rendering face animations, characters should have a facial rig(Blendmesh).</br>
Follow these steps to add your character:
1. Find a 3D character model from [Unity asset store](http://assetstore.unity.com/) or download a free one(You can download them from websites like [Mixamo](http://mixamo.com/)).
2. Open the character setting and set the rig to humanoid

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/3.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/3.png?raw=true" alt="Logo" width="300" height="150">
  </a>
</div>

3. Drag and drop your 3D character model to `CharacterChooser/CharacterSlideshow/Parent` object in Unity main Scene like the image below

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/1.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/1.png?raw=true" alt="Logo" width="300" height="400">
  </a>
</div>

4. Add `BlendShapeController` and `QualityData` components to the character object in the scene(which is dragged inside the Parent object in the last step).
5. Set `BlendShapeController` values
- Add character `SkinnedMeshRenderer` component to `BlendShapeController` component.

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/5.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/5.png?raw=true" alt="Logo" >
  </a>
</div>

- Find each blnedShape weight number under `SkinnedMeshRenderer` and set those numbers in `BlendShapes` field inside `BlendShapeController` (for specifying each blendshape value to the `BlendShapeController` component so the animation would be shown on character face by modification on these blnedShape values)

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/6.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/6.png?raw=true" alt="Logo" width="300" height="400">
  </a>
</div>

6. Open `CharacterSlideshow` Object on `CharacterChooser/CharacterSlideshow` path inside the scene hierarchy, then add a new dragged character to the `nodes` property(all characters should be referenced inside `nodes`).

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/8.jpg">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/8.jpg?raw=true" alt="Logo" width="500" height="300">
  </a>
</div>

7. Run the application and you can now select your character for rendering animation!

# Features
<!-- ROADMAP -->

<!-- ## Available features -->
- [x] Making full body animation
- [x] Animating multiple blendShapes on 3D character (up to 40 blendshape animations is supported currently)
- [x] Supporting any 3D models with Humanoid T-Pose rig
- [x] Exporting animation in a video file
- [x] Saving animation data and re-rendering it for future usage
- [x] Filtering mediaPipe outputs in order to detect and remove noises and better smoothness (Low Pass Filtering is used currently) 

<!-- ## TODO -->

- [ ] Animating the character's face in great details
    - [ ] Training a regression model to generate Blendmesh weights by feeding the output data of mediaPipe FaceMesh(468 points)
    - [ ] Using StyleGan techniques to replace whole character face mesh
- [ ] Automatic rigging for 3D models without humanoid rig (Using deep neural network models like RigNet)
- [ ] Generating complete character mesh automatically using models like PIFuHD (in progress!)
- [ ] Animating 3D character mouth in great detail using audio signal or natural language processing methods
- [ ] Generating complete environment in 3D


## Resources
- Body Pose Estimation: BlazePose model
  - Paper: [BlazePose: On-device Real-time Body Pose Tracking](https://arxiv.org/abs/2006.10204)
- Hands Pose Estimation: MediaPipe Hands model
  - Paper: [MediaPipe Hands: On-device Real-time Hand Tracking](https://arxiv.org/abs/2006.10214)
- Face Detection: BlazeFace model
  - Paper: [BlazeFace: Sub-millisecond Neural Face Detection on Mobile GPUs](https://arxiv.org/abs/1907.05047)
- Face Landmark Generator: MediaPipe Face Landmark Model 
  - Paper: [Real-time Facial Surface Geometry from Monocular Video on Mobile GPUs](https://arxiv.org/abs/1907.06724)

## Licenses & Citations
### DigiHuman Licence
   Application License: [GPL-3.0 license](https://github.com/Danial-Kord/DigiHuman/blob/main/LICENSE.md)
   Non-commercial use only. If you distribute or communicate copies of the modified or unmodified Program, or any portion thereof, you must provide appropriate credit to Danial Kordmodanlou as the original author of the Program. This attribution should be included in any location where the Program is used or displayed.

### FFmpeg</br>
- FFmpeg is licensed under the [GNU Lesser General Public License (LGPL) version 2.1](http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html) or later. However, FFmpeg incorporates several optional parts and optimizations that are covered by the [GNU General Public License (GPL) version 2](http://www.gnu.org/licenses/old-licenses/gpl-2.0.html) or later. If those parts get used the GPL applies to all of FFmpeg. 
- Unity FFmpeg packages are licensed under [Keijiro Takahashi MIT](https://github.com/keijiro/FFmpegOut/blob/master/LICENSE.md)

### GauGan
- Used [SPADE](https://github.com/NVlabs/SPADE) repository developed by NVIDIA and the customization is addapted from [Smart-Sketch](https://github.com/noyoshi/smart-sketch) with [GPL V 3.0](https://github.com/noyoshi/smart-sketch/blob/master/LICENSE) licence
```
@inproceedings{park2019SPADE,
  title={Semantic Image Synthesis with Spatially-Adaptive Normalization},
  author={Park, Taesung and Liu, Ming-Yu and Wang, Ting-Chun and Zhu, Jun-Yan},
  booktitle={Proceedings of the IEEE Conference on Computer Vision and Pattern Recognition},
  year={2019}
}
```
### 3D Characters
[Unity-chan model](https://unity-chan.com/contents/license_en/) & [mixamo models](https://www.mixamo.com)



<!-- CONTACT -->
# Contact
Danial Kordmodanlou - [kordmodanloo@gmail.com](mailto:kordmodanloo@gmail.com)

Website : [danial-kord.github.io](https://danial-kord.github.io/) 

Project Link: [github.com/Danial-Kord/DigiHuman](https://github.com/Danial-Kord/DigiHuman)

Telegram ID: [@Danial_km](https://t.me/Danial_km)

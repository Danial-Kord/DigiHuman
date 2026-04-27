# DigiHuman

DigiHuman is a project which aims to automatically generate <b>whole body pose animation + facial animation</b> on 3D character models based on camera input.

This project is my B.Sc thesis of Computer Engineering at Amirkabir University of Technology (AUT).

## About DigiHuman

DigiHuman automates animation generation on 3D virtual characters. It uses pose estimation and facial landmark models for full-body and face animation.

DigiHuman is developed with [**MediaPipe**](https://github.com/google/mediapipe) and **Unity**. MediaPipe produces 3D landmarks (body, hands, face), and Unity renders the character after consuming that data over HTTP. The diagram below shows the overall architecture.

<div align="center">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/dataFlow.png?raw=true" alt="DigiHuman data flow diagram">
</div>

## How it works

1. **Upload** — From Unity, the user selects a video. `NetworkManager` uploads it to the Flask backend (`/uploader`, `/handUploader`, full-pose and face upload routes as applicable).
2. **Process** — The server saves the file under `Backend/temp/`, starts a background thread, and runs MediaPipe (`pose_estimator.py` for body/hands/holistic pose, `mediaPipeFace.py` for face / blendshape data). Each frame becomes a JSON object appended to an in-memory list keyed by the temp file path.
3. **Stream** — Unity polls with `POST` and JSON `{ "fileName", "index" }` until the response body is `Done`. Frame endpoints include `/pose` (body), `/hand` (hands), `/holistc` (full body + hands; spelling matches the server route), and `/face` (facial blendshape weights).
4. **Animate** — `FrameReader` aligns body, hand, and face data per frame and drives `Pose3DMapper`, hand preprocessing, and `FacialExpressionHandler` / `BlendShapeController` on a **Humanoid** rig with optional blend shapes.

Default server URL in the sample scene is `http://127.0.0.1:5000`. Ensure the `NetworkManager` URLs in Unity match your host and port.

## Sample outputs

<div align="center">
<a href="https://youtu.be/maUUXfe_EcU">Project demo</a> | <a href="https://youtu.be/L62w5AMaFOk">Tutorial</a>
</div>

### Hands animations

<div align="center">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/2828_ok.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/2828_ok.gif?raw=true" alt="Hands animation sample">
  </a>
  
  <a href="https://thumbs.gfycat.com/VibrantDearestKomododragon-size_restricted.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/2828_1t05.gif?raw=true" alt="Hands animation sample">
  </a>
  
</div>

### Full body animation

<div align="center">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/figure_headphone.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/figure_headphone.gif" alt="Full body sample">
  </a>
    <a href="https://gfycat.com/braveglumguanaco">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/hands_greek.gif" alt="Full body sample">
  </a>
</div>

### Face animation

<div align="center">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/mouth_deform_1_japan.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/mouth_deform_1_japan.gif?raw=true" alt="Face animation sample">
  </a>
  
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/blinks_1_japan.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/blinks_1_japan.gif?raw=true" alt="Blink animation sample">
  </a>
  
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/mouth_1_japan.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/mouth_1_japan.gif?raw=true" alt="Mouth animation sample">
  </a>
  
   <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/mouth_dir_1_japan.gif">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/mouth_dir_1_japan.gif?raw=true" alt="Mouth direction sample">
  </a>
  
</div>

## Installation

### Backend server

Use **Python 3** with `pip`.

1. Install MediaPipe:

   ```bash
   pip install mediapipe
   ```

2. Install OpenCV:

   ```bash
   pip install opencv-python
   ```

3. Open a terminal, change to the **`Backend`** folder (repository root contains `Backend/`, not `backend/`), and install the rest of the dependencies:

   ```bash
   cd Backend
   pip install -r requirements.txt
   ```

4. For the optional GauGAN / SPADE image pipeline used by `/uploader` for **images**, download the [pre-trained COCO generator](https://drive.google.com/file/d/15VSa2m2F6Ch0NpewDR7mkKAcXlMgDi5F/view?usp=sharing) and extract it under `Backend/checkpoints/coco_pretrained/` (create the folders if they are missing). Video mocap does not require this checkpoint. On **Windows**, `server.py` calls the Unix `cp` command in the GauGAN helper; use WSL, Git Bash, or change `copy_file` to a cross-platform copy if you rely on image synthesis.

### Unity

1. Download and install [Unity Hub](https://unity.com/download).
2. Add a license in Unity Hub.
3. Install an Editor (LTS recommended; newer than **2020.3.25f1** is suggested in the original project notes).
4. In **Player Settings**, allow **insecure HTTP** connections so Unity can call `http://127.0.0.1:5000` during development.

 <div align="center">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/http.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/http.png?raw=true" alt="Unity allow HTTP setting">
  </a>
  
</div>

5. Optional recording: import [FFmpegOut (MIT)](https://github.com/keijiro/FFmpegOut/releases) and [FFmpegOutBinaries (GPL)](https://github.com/keijiro/FFmpegOutBinaries/releases) as `.unitypackage` files.

## Usage

1. Start the backend from the **`Backend`** directory:

   ```bash
   cd Backend
   python server.py
   ```

   Flask listens on **port 5000** by default (`http://127.0.0.1:5000`).

2. Open the Unity project and load the main integration scene:

   **`Assets/Scenes/Network Test.unity`**

   (`MainScene.unity` is not present in this repository; `Assets/Scenes/Test.unity` also references `FrameReader` for related tests.)

3. Run Play mode and upload videos from the UI (or use sample clips from the side menu if configured in your scene).

### Real-time webcam landmarks (WebSocket)

The backend can stream **live** holistic body + hands + **40** face blendshape weights (same ordering as offline `/face` / Unity `FaceJson`) from the **machine running `server.py`** (server-side webcam).

- **URL:** `ws://127.0.0.1:5000/ws/live_mocap` (same host/port as Flask; use `ws://` / `wss://` accordingly).
- **Handshake:** After connecting, send one text frame: `{"cmd":"start","camera_id":0}` (`camera_id` optional, default `0`).
- **Stream:** The server sends one JSON text message per captured frame until you send `{"cmd":"stop"}` or disconnect.
- **Concurrency:** Only **one** live session at a time; a second client gets `{"error":"live_stream_already_active"}`.

**Payload shape (each frame):**

- `frame` — integer frame index from the capture device when available.
- `bodyPose` — same structure as offline full pose (`predictions`, `width`, `height`, `frame`).
- `handsPose` — `handsR`, `handsL`, `frame` (normalized hand landmarks).
- `faceData` — `{ "blendShapes": [ 40 floats ], "frame", "time" }` (`time` is capture timestamp in ms when the backend exposes it; otherwise `0.0`). If no face is detected, `blendShapes` are zeros.

**Smoke test** (with the server running):

```bash
cd Backend
python scripts/ws_live_client_smoke.py
```

**Unity client**

1. Add a **`LiveMocapClient`** component to a scene object (e.g. next to `NetworkManager`).
2. Assign the same **`FrameReader`** used by the rest of the app.
3. Set **Web Socket Url** to `ws://127.0.0.1:5000/ws/live_mocap` (adjust host if the Python server runs elsewhere).
4. Optional: assign **Start** / **Stop** UI `Button`s, or call `StartLiveStream()` / `StopLiveStream()` from code (e.g. `NetworkManager` exposes **`StartLiveMocapStream`** / **`StopLiveMocapStream`** if you wire the **`LiveMocapClient`** reference there).
5. Enter Play mode, start the backend, then start the live stream. The character is driven in **`LateUpdate`** from the latest received frame (timeline **`FixedUpdate`** playback is paused while live mode is active).

Requires **.NET** stack with `System.Net.WebSockets` (Unity **2021+** / **Api Compatibility Level** .NET Standard 2.1 or .NET Framework is typical). The webcam used is always on the **Python server** machine, not the Unity machine.

## Adding new 3D characters

Characters should use a **Humanoid** rig for body animation. For face animation they need **blend shapes** on a `SkinnedMeshRenderer`.

1. Obtain a model (e.g. [Unity Asset Store](https://assetstore.unity.com/) or [Mixamo](https://www.mixamo.com/)).
2. In the model Import settings, set **Animation Type** to **Humanoid**.

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/3.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/3.png?raw=true" alt="Humanoid rig settings" width="300" height="150">
  </a>
</div>

3. Drag the character under `CharacterChooser/CharacterSlideshow/Parent` in the scene hierarchy.

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/1.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/1.png?raw=true" alt="Character parent" width="300" height="400">
  </a>
</div>

4. Add **`BlendShapeController`** and **`QualityData`** to the character instance.
5. Configure **`BlendShapeController`**:
   - Assign the face **`SkinnedMeshRenderer`**.
   - Map each relevant **blend shape index** from the mesh to the `BlendShapes` entries on the component.

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/5.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/5.png?raw=true" alt="BlendShapeController SkinnedMeshRenderer" >
  </a>
</div>

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/6.png">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/6.png?raw=true" alt="Blend shape index mapping" width="300" height="400">
  </a>
</div>

6. Select **`CharacterSlideshow`** under `CharacterChooser/CharacterSlideshow` and add the new character to the **`nodes`** list.

<div align="left">
  <a href="https://github.com/Danial-Kord/DigiHuman/blob/images/images/8.jpg">
    <img src="https://github.com/Danial-Kord/DigiHuman/blob/images/images/8.jpg?raw=true" alt="Character slideshow nodes" width="500" height="300">
  </a>
</div>

7. Run the app and choose the character from the UI.

## Features

- [x] Full body animation
- [x] Multiple blend shapes on the character (up to about 40 supported)
- [x] Humanoid T-pose–style models
- [x] Export animation to video (with FFmpegOut)
- [x] Save animation data and replay later
- [x] Smoothed MediaPipe output (e.g. low-pass filtering)

**Roadmap (excerpt)**

- [ ] Finer face detail
    - [ ] Regression model from MediaPipe FaceMesh (468 points) to blendshape weights
    - [ ] StyleGAN-style face replacement
- [ ] Automatic rigging without a humanoid rig (e.g. RigNet-style approaches)
- [ ] Full character mesh from images (e.g. PIFuHD) — in progress
- [ ] Mouth detail from audio or language models
- [ ] Full 3D environment generation

## Resources

- Body pose: BlazePose — [paper](https://arxiv.org/abs/2006.10204)
- Hands: MediaPipe Hands — [paper](https://arxiv.org/abs/2006.10214)
- Face detection: BlazeFace — [paper](https://arxiv.org/abs/1907.05047)
- Face landmarks: MediaPipe Face Landmark model — [paper](https://arxiv.org/abs/1907.06724)

## Licenses & citations

### DigiHuman license

Application license: [GPL-3.0](https://github.com/Danial-Kord/DigiHuman/blob/main/LICENSE.md). Non-commercial use only. If you distribute the Program, attribute Danial Kordmodanlou as the original author wherever the Program is used or shown.

### FFmpeg

- FFmpeg: [LGPL 2.1+](http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html); some optional parts are [GPL 2+](http://www.gnu.org/licenses/old-licenses/gpl-2.0.html).
- Keijiro’s Unity FFmpeg packages: [MIT](https://github.com/keijiro/FFmpegOut/blob/master/LICENSE.md)

### GauGAN / SPADE

Uses [SPADE](https://github.com/NVlabs/SPADE) (NVIDIA), with customization adapted from [Smart-Sketch](https://github.com/noyoshi/smart-sketch) ([GPL v3](https://github.com/noyoshi/smart-sketch/blob/master/LICENSE)).

```
@inproceedings{park2019SPADE,
  title={Semantic Image Synthesis with Spatially-Adaptive Normalization},
  author={Park, Taesung and Liu, Ming-Yu and Wang, Ting-Chun and Zhu, Jun-Yan},
  booktitle={Proceedings of the IEEE Conference on Computer Vision and Pattern Recognition},
  year={2019}
}
```

### 3D characters

[Unity-chan](https://unity-chan.com/contents/license_en/) and [Mixamo](https://www.mixamo.com) assets as applicable.

## Contact

Danial Kordmodanlou — [kordmodanloo@gmail.com](mailto:kordmodanloo@gmail.com)

Website: [danial-kord.github.io](https://danial-kord.github.io/)

Project: [github.com/Danial-Kord/DigiHuman](https://github.com/Danial-Kord/DigiHuman)



# OpenXR + XR Interaction Toolkit Boilerplate

This Unity project provides a **cross-platform foundation** for developing XR experiences using **OpenXR** and the **XR Interaction Toolkit (XRITK)**.  
It is designed to be lightweight, flexible, and compatible with most modern XR headsets — including **Meta Quest**, **Pico**, **HTC Vive**, and **Varjo** — without relying on vendor-specific SDKs.

The project also supports the **Meta Plugin** for basic passthrough rendering on Quest devices, while remaining fully portable across other OpenXR platforms.

---

## Overview

### Key Features
- **OpenXR Runtime** — Unity’s official, hardware-agnostic XR backend supporting a wide range of devices.  
- **XR Interaction Toolkit (XRITK)** — Handles controller input, grabbing, ray interaction, teleportation, and hand tracking.  
- **Meta Plugin (Optional)** — Enables passthrough rendering on Meta Quest headsets without Meta-exclusive SDK dependencies.  
- **Audio Reactivity System** — Real-time spectrum analysis and beat-driven animation for immersive audiovisual experiences.  
- **Lightweight Libraries** — Includes Kijiro’s Minis, OSCJack, and KlakNDI for real-time media and networked performance setups.

This boilerplate is ideal for **hackathons**, **creative coding**, and **interactive media prototypes** where portability, simplicity, and expressive control are important.

---

## Getting Started

### 1. Prerequisites
- **Unity 6 (2025.x)** or later  
- A **VR headset** supporting OpenXR (Quest, Pico, HTC, Varjo, Windows MR, etc.)  
- **Unity Hub** for project management  
- Optional: microphone or audio input for beat detection tests  

If you are new to Unity:
- Install [Unity Hub](https://unity.com/download)  
- Use it to install Unity 6 with the **Android Build Support** and **OpenXR** modules.  

---

### 2. Opening the Project
1. Launch **Unity Hub**.  
2. Click **Add Project from Disk** and select the `OpenXR_XRITK_Boilerplate` folder.  
3. Open the project.  
   Unity will import all assets and packages; this may take a few minutes the first time.

---

### 3. Switching to Android Platform (for Meta Quest)
If you are building for **Meta Quest (standalone)**:

1. Go to **File → Build Settings…**  
2. Select **Android** and click **Switch Platform**.  
3. Open **Edit → Project Settings → XR Plug-in Management → Android**.  
4. Enable **OpenXR** as the active loader.  

If you are testing through **Quest Link** or **PCVR**, you can stay on the **Windows** platform.

---

### 4. Configuring OpenXR and XR Interaction Toolkit
1. Open **Edit → Project Settings → XR Plug-in Management**.  
2. Enable **OpenXR** for the active platform (Windows or Android).  
3. In **Project Settings → XR Plug-in Management → OpenXR**, enable the following profiles:
   - *Oculus Touch Controller Profile* (for Meta)
   - *HTC Vive Controller Profile* (for Vive)
   - *Khronos Simple Controller Profile* (generic fallback)
   - *Hand Tracking* (optional)
   - *Eye Gaze Interaction* (optional)

For additional details:
- [Unity OpenXR Plugin Manual](https://docs.unity3d.com/Packages/com.unity.xr.openxr@latest/)
- [XR Interaction Toolkit Documentation](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest/)
- [Unity Input System Overview](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/)

---

### 5. Switching to Another Plugin Provider (Other Headset Brands)
To build for a headset brand other than Meta:

1. Open **Edit → Project Settings → XR Plug-in Management**.  
2. Disable the **Meta Plugin** (if enabled).  
3. Under **OpenXR**, select or enable the appropriate feature group:
   - *PicoXR Feature Group* for Pico headsets  
   - *HTC Vive Feature Group* for Vive / SteamVR  
   - *Varjo Feature Group* for Varjo devices  
4. In **Project Settings → Player**, ensure your **Package Name** and **Company Name** match your device requirements.  
5. Rebuild the project for the target platform (Android for Pico/Quest, Windows for PCVR).  

If the headset has its own runtime (like **VIVE Console** or **Varjo Base**), make sure it’s active before launching Play Mode or deploying the build.

---

## Included Libraries

The following lightweight, production-proven libraries are preinstalled:

- **[Kijiro’s Minis](https://github.com/keijiro/Minis)** — Utility toolkit for math, time management, and general creative coding workflows.  
- **[OSCJack](https://github.com/keijiro/OscJack)** — Minimal OSC (Open Sound Control) implementation for networked input and control (e.g., from TouchDesigner, Ableton Live, or VCV Rack).  
- **[KlakNDI](https://github.com/keijiro/KlakNDI)** — Enables real-time NDI video streaming between Unity and external systems.

These are included to support interactive installations, VJing setups, and other live visual use cases.

---

## Audio Reactivity System

The project includes an extensible system for **real-time audio analysis and beat synchronization** through two key scripts:  
`AudioBeatController.cs` and `BeatTransform.cs`.

### AudioBeatController.cs
Analyzes the incoming audio spectrum from either:
- an attached `AudioSource`, or  
- the global `AudioListener` (if no source is defined).

It divides the audio into **bass**, **mid**, and **high** bands, smooths them, detects beats, and predicts future hits to create responsive animations.

Outputs:
- `bass`, `mid`, `high` — Smoothed normalized amplitude of each band.  
- `beatTime` — Continuous time value, that speeds up in sync with the beat.

It can also expose these values to shaders globally:
```csharp
_BeatTime
_AudioBands   // (x=bass, y=mid, z=high)
```

These can drive Shader Graph properties or custom effects for glowing, pulsing, or motion-reactive visuals.

---

### BeatTransform.cs

The **BeatTransform** component takes audio-reactive data from an `AudioBeatController` and applies it to a GameObject’s transform in real time.  
This allows you to create pulsing, orbiting, or rotating objects synchronized with sound or external OSC input.

Attach `BeatTransform` to any object and link it to a `AudioBeatController`.

#### Public Fields

| Field | Type | Description |
|-------|------|--------------|
| **driver** | `AudioBeatController` | The controller providing beat and frequency data. |
| **baseScale** | `Vector3` | The original scale of the object, used as a baseline for scaling. |
| **bassAmount** | `float` | The intensity multiplier for scaling based on bass amplitude. |
| **move** | `bool` | Enables position motion (if `true`, object oscillates). |
| **circular** | `bool` | Enables circular motion pattern using both sine and cosine. |
| **rotate** | `bool` | Enables continuous rotation. |
| **useOsc** | `bool` | Allows the object to react to OSC input instead of audio. |
| **moveAmplitude** | `float` | Controls how far the object moves when motion is enabled. |
| **speed** | `float` | Controls how fast the object oscillates or rotates. |

#### Behavior

- **Scaling**:  
  The object scales relative to its base size according to the bass or OSC input:  
  ```csharp
  float s = 1f + bass * bassAmount;
  transform.localScale = baseScale * s;
  ```

- **Movement**:  
  When `move` is enabled, the object’s position shifts based on a sine wave synchronized with the beat:  
  ```csharp
  pos.z += Mathf.Sin(beatTime * speed) * moveAmplitude;
  if (circular)
      pos.x += Mathf.Cos(beatTime * speed) * moveAmplitude;
  ```

- **Rotation**:  
  When `rotate` is enabled, the object rotates smoothly at a set angular velocity:
  ```csharp
  transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
  ```

#### Example Setup
1. Add an **AudioBeatController** to any GameObject.  
2. Add a **BeatTransform** to another object (e.g., a mesh or prefab).  
3. Drag the controller reference into the `driver` field of the transform.  
4. Enable **Move**, **Circular**, or **Rotate** as desired.  
5. Play an audio clip or feed microphone input to watch the object animate in sync with the beat.  

This setup works in Play Mode, Quest Link, and standalone builds.

---

## Notes for Developers

- The project uses the **Universal Render Pipeline (URP)** for lightweight, modern rendering.  
  Ensure URP is installed and set as the active pipeline (via *Graphics Settings*).  
- The **Meta Plugin** is optional; if present, it enables basic passthrough rendering on Quest devices.  
- The **Samples** folder contains example scenes demonstrating:  
  - OpenXR + XR Interaction Toolkit setup  
  - Grabbing and teleportation  
  - Audio-reactive object movement and scaling  
- **OSC** and **NDI** functionality are included but disabled by default; enable them for live or networked workflows.

---

## Testing the Project

1. Connect your headset (Quest, Pico, or PCVR).  
2. In Unity, open one of the **Samples** scenes.  
3. Enter **Play Mode**.  
4. Move your hands or controllers to test interactions.  
5. Add an **Audio Source** playing music or use a microphone input — objects will begin reacting to the beat.

---

## License

This project is distributed for hackathon under the [MIR license](../LICENSE.txt)

---

## Summary

This boilerplate offers a clean, portable foundation for developing interactive, audio-reactive XR experiences across major headset brands using Unity’s OpenXR and XR Interaction Toolkit.  
It balances clarity and flexibility, allowing both beginners and experienced developers to build, test, and deploy mixed-reality prototypes quickly.

# XR Hackathon Boilerplates

This repository contains two Unity 6 boilerplates for rapid XR prototyping.  
Both share a similar structure but target different SDK stacks and devices.

---

## Folder Structure

### [MetaXR_Boilerplate/](MetaXR_Boilerplate/README.md)
Based on the **Synedelica** project architecture and designed for Meta Quest devices.

Includes:
- Meta XR SDK / Oculus Integration
- Passthrough rendering and shader modification
- Hand tracking, scene understanding, and mixed reality layers
- Meta-specific API access and visual templates for immersive experiences

Use this version if you are building for **Meta Quest 2, 3, or Pro** and want to leverage Meta features such as passthrough modifiation, and color camera access and easy to use building-blocks.

---

### [OpenXR_XRITK_Boilerplate/](OpenXR_XRITK_Boilerplate/README.md)
A vendor-agnostic baseline using Unity’s **XR Interaction Toolkit** and **OpenXR** backend.

Includes:
- OpenXR setup compatible with most headsets (Meta, Pico, HTC, Varjo, etc.)
- Grabbing, teleportation, and UI interaction examples
- Unity Input System for controller mapping
- Cross-platform configuration suitable for PCVR or standalone OpenXR builds

Use this version if you want a **clean, cross-platform XR foundation** without Meta dependencies.
It still supports many native Meta features such passthrough (without direct camera access) through the provided MetaXR plugin. 

Both versions use the Unified Rendering Pipeline (URP) and work with Unity 6 (tested on 6000.2.8f1).
Of course you may mix both versions and take what you need (e.g. Meta's native OVRPassthrough + and XR Interaction Toolkit)

---

## Common Features

- Unity 6 project layout with URP rendering  
- Example scenes for spatial interaction and visuals  
- Optional audio-reactive shader utilities  
- Modular structure for easy extension during the hackathon  

---

## Getting Started

Clone this repository:

```bash
git clone https://github.com/your-org/xr-hackathon-boilerplates.git
```

Then open one of the folders directly in Unity Hub:

1. Launch Unity Hub
2. Click Add project from disk
3. Select either MetaXR_Boilerplate or OpenXR_XRITK_Boilerplate

Each boilerplate has its own README with setup and build instructions.


## License

This project is released under the [MIT License](LICENSE.txt)
You are free to use, modify, and distribute it in your own projects.  
Attribution to the original authors is appreciated.

# Sensor Simulation in Virtual Reality

> An interactive virtual reality learning environment for exploring LiDAR and multibeam sonar sensing, point-cloud acquisition, and surface reconstruction.

![Unity](https://img.shields.io/badge/Unity-6-black?logo=unity)
![XR](https://img.shields.io/badge/XR-OpenXR%20%7C%20XRI-blue)
![Render%20Pipeline](https://img.shields.io/badge/Render%20Pipeline-URP-5c6bc0)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)
![Status](https://img.shields.io/badge/Status-Academic%20Project-success)

## Overview

**Sensor Simulation in Virtual Reality** is a Unity-based VR application designed to support intuitive exploration of range-sensor behaviour. Instead of observing sensor output on a conventional flat display, users can enter a simulated environment, place sensors on moving vehicles, modify scanning parameters through a world-space interface, and observe point clouds being generated in real time.

The project includes two sensing domains:

- **LiDAR simulation** in an urban CityScape environment and office room environment.

The application is intended primarily as an **educational and interactive visualization tool**. It demonstrates scan coverage, blind zones, field of view, point density, range limitations, sensor placement, and basic point-cloud surface reconstruction.

> [!IMPORTANT]
> This project uses geometric raycasting for interactive sensor visualization. It is not intended to be a fully physically calibrated LiDAR or sonar simulator for real-world perception-model benchmarking.

## Demo
(https://drive.google.com/file/d/1X3CHW7tq7UvQ7loeGz-uShBdd0GrY_V0/view?usp=sharing)

<img width="1330" height="651" alt="image" src="https://github.com/user-attachments/assets/f132a09d-2744-48ca-9c6c-18a76edea5f9" />
<img width="1205" height="652" alt="image" src="https://github.com/user-attachments/assets/fa6b811e-ec40-4d91-a2a9-de8c6b9d9bc1" />
<img width="515" height="346" alt="Screenshot 2026-07-09 140240" src="https://github.com/user-attachments/assets/440092d8-d44c-4472-8e97-12c5c4e18e85" />
<img width="1697" height="716" alt="Screenshot 2026-07-09 133006" src="https://github.com/user-attachments/assets/d2bc5a4f-2bc6-4a37-9a60-ff7a91908854" />
<img width="1357" height="725" alt="Screenshot 2026-07-09 141221" src="https://github.com/user-attachments/assets/725e6651-808a-4422-8e57-b11ad627d6a5" />

### VR interaction

- OpenXR-compatible VR interaction using Unity XR Interaction Toolkit.
- Direct and ray-based interaction with sensor prefabs.
- Grab, move, and mount sensors on designated vehicle surfaces.
- Haptic feedback for sensor attachment and detachment.
- World-space UI panels designed for use in VR.
- Controller-based actions for vehicle control, UI visibility, sensor interaction, and scene navigation.

### LiDAR simulation

- Mechanical spinning LiDAR model with continuous yaw rotation.
- Solid-state LiDAR model with a fixed rectangular field of view.
- Configurable scan parameters, including:
  - Horizontal and vertical field of view.
  - Vertical ray count or rays per degree.
  - Spin speed or scan rate.
  - Maximum detection range.
  - Point-cloud display settings.
- Real-time raycasting against scene geometry.
- Pooled point-cloud visualization using quad markers.
- Ring-buffer point reuse to avoid unbounded runtime allocation.

### Surface reconstruction

- **LiDAR:** Marching Cubes reconstruction from recorded point-cloud data.

## Technology Stack

| Area | Technology |
| --- | --- |
| Game engine | Unity 6 (`6000.4.2f1`) |
| Language | C# |
| Rendering | Universal Render Pipeline (URP `17.4.0`) |
| VR backend | OpenXR (`1.16.1`) |
| VR framework | XR Interaction Toolkit (`3.4.1`) |
| Input | Unity Input System (`1.19.0`) |
| Vehicle paths | Dreamteck Splines |
| Text/UI | TextMeshPro and Unity world-space Canvas |
| Reconstruction | Marching Cubes and custom structured row stitching |

### Hardware

- A VR-ready Windows PC.
- An OpenXR-compatible VR headset and tracked controllers.
- Tested conceptually with devices such as Meta Quest through Link/Air Link, Valve Index, and HP Reverb G2.

### Software

- Unity Hub.
- Unity Editor **6000.4.2f1** or a compatible Unity 6 version.
- Git and Git LFS, if large model or scene assets are stored through LFS.
- Required Unity packages listed below.

## Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/<your-organization-or-user>/sensor-simulation-vr.git
   cd sensor-simulation-vr
   ```

2. If the repository uses Git LFS, download the large assets:

   ```bash
   git lfs install
   git lfs pull
   ```

3. Open the project through **Unity Hub**.

4. Select Unity version **6000.4.2f1** when Unity Hub asks for an editor version.

5. Allow Unity to import packages and project assets.

6. Verify that the required packages are installed through:

   ```text
   Window > Package Manager
   ```

7. Configure your headset runtime and confirm that **OpenXR** is enabled:

   ```text
   Edit > Project Settings > XR Plug-in Management
   ```

8. Open one of the main scenes:

   ```text
   Assets/Scenes/CityScape.unity
   Assets/Scenes/Room.unity
   ```

9. Connect the headset, enter Play Mode, and test controller tracking and input bindings.

## Required Packages

The project requires the following Unity packages or equivalent compatible versions:

- Universal Render Pipeline.
- Input System.
- XR Interaction Toolkit.
- OpenXR Plugin.
- Dreamteck Splines.

### LiDAR workflow

1. Open the `CityScape` scene.
2. Start Play Mode with an OpenXR headset connected.
3. Open the world-space sensor UI.
4. Spawn either a **Spinning LiDAR** or **Solid-State LiDAR** prefab.
5. Grab the sensor using direct or ray interaction.
6. Move the sensor onto a valid vehicle mounting surface.
7. Wait for the sensor to attach to its `SensorAnchor`.
8. Change field of view, ray density, range, or scan speed with the UI sliders.
9. Observe the point cloud update in real time.
10. Collect scan data and trigger mesh reconstruction when required.

## Architecture

The application separates sensor state, sensor simulation, visualization, recording, interaction, and reconstruction.

```text
Global Sensor Settings
        |
        v
Sensor Prefab / Visualizer
        |
        +--> Raycasts against scene colliders
        |
        +--> Point-cloud visualizer
        |
        +--> Point-cloud recorder
                   |
                   v
          Mesh Reconstruction
          - Marching Cubes for LiDAR

World-Space UI
        |
        v
Live parameter updates
```

## Contact

For questions about the project, contact:

- **Selvendran Karthikeyan** — selvendranks@gmail.com
- **Zhaodong Li** — st194954@stud.uni-stuttgart.de
- **Metin Arab** — metinarab@outlook.de

# Create with VR — Final Room Assignment

Unity VR room-scale project built from the [Unity Learn Create with VR](https://learn.unity.com/course/create-with-vr) course, extended with assignment-specific features.

**Main scene:** `Assets/Scenes/VRRoom_Assignment.unity`

---

## Course Completion Summary

This project implements the core Create with VR curriculum:

| Course Topic | Implementation in Project |
|---|---|
| VR scene setup & room design | Custom VR room with furniture, shelving, lighting, and environment prefabs |
| XR Origin & camera rig | XR Origin with tracked head and controllers |
| Object interaction | `XRGrabInteractable` objects (Key, Book, Mug, Ball) |
| Socket interactions | Key/Mug/Book sockets with `XRSocketInteractor` |
| Teleportation | Teleport Area covering walkable floor space |
| UI in VR | World-space UI panel in scene |
| Locomotion | Teleportation-based movement with XR Interaction Toolkit |

---

## Assignment Features

### 1. Analog Wall Clock
- **Script:** `Assets/Scripts/Assignment/AnalogWallClock.cs`
- Reads `DateTime.Now` every frame and rotates hour, minute, and second hands
- Hands are rotated programmatically (not Unity Animation window clips)
- **Setup:** Unity menu → **VR Assignment → Add Clock To WallClocklocation**

### 2. Animated Controller Hand Models
- **Script:** `Assets/Scripts/Assignment/ControllerHandAnimator.cs`
- Replaces default controller meshes with rigged `LeftHand.fbx` / `RightHand.fbx` models
- Grip curls all fingers; trigger adds extra curl to the index finger
- Finger joints are found by OpenXR bone names (`L_IndexProximal`, `R_ThumbDistal`, etc.)
- **Setup:** Unity menu → **VR Assignment → Setup Controller Hand Models**

### 3. Additional Polish Features
- **InteractableTooltip** — hover tooltips on grabbable objects
- **AmbientAudioZone** — 3D spatial ambient audio with random one-shots
- Baked lighting and URP performance presets from the VR Template
- Fog, reflection probes, and post-processing for atmosphere

---

## Quick Setup (In Unity)

1. Open **`VRRoom_Assignment`** scene
2. Run **VR Assignment → Setup Complete Assignment** (one-click — does everything below)
3. Press **Play** and verify:
   - Clock hands move with system time
   - Hand fingers curl when pressing grip/trigger (use XR Device Simulator or headset)
   - Grab objects, socket items, and teleport around the room
6. Record your video demonstration

---

## Scene Architecture

```
VRRoom_Assignment
├── Environment          # Room geometry, walls, floor
├── Furniture            # Table, Chair, Shelf
├── InteractableObjects  # Key, Book, Mug, Ball (XRGrabInteractable)
├── Sockets              # KeySocket, MugSocket, BookSocket (XRSocketInteractor)
├── Teleport Area Setup  # TeleportationProvider + Teleport Area collider
├── Lighting             # Directional light, probes, baked lighting
├── UI                   # World-space canvas
├── WallClocklocation    # Analog wall clock (assignment)
└── XR Origin            # Camera, controllers, interactors, locomotion
```

### Key Prefabs Used
- `Complete XR Origin Set Up Variant` — full controller-based XR rig
- `Teleport Anchor` / Teleport Area — locomotion surfaces
- VRTemplate interactable prefabs — grabbable object templates

### Key XR Components
| Component | Role |
|---|---|
| `XROrigin` | Player rig root; moves the user through the world |
| `XRInteractionManager` | Routes hover/select events between interactors and interactables |
| `XRDirectInteractor` | Hand/controller proximity grab |
| `XRRayInteractor` | Ray-based selection and UI interaction |
| `XRGrabInteractable` | Makes objects grabbable |
| `XRSocketInteractor` | Snap objects into place |
| `TeleportationProvider` | Executes teleport requests |
| `TeleportationArea` | Defines walkable teleport surfaces |
| `TeleportationAnchor` | Fixed teleport destination points |

---

## Presentation Questions

### How did you implement the wall clock?
The `AnalogWallClock` script reads the system clock via `DateTime.Now` in `Update()`. It calculates angles for each hand:
- **Second hand:** `(seconds + ms/1000) × 6°`
- **Minute hand:** `(minutes + seconds/60) × 6°`
- **Hour hand:** `(hours % 12 + minutes/60) × 30°`

Each hand transform is rotated around a configurable local axis using `Quaternion.AngleAxis`. No Animation window clips are used — motion is driven entirely by code in real time.

### How did you implement custom controller hands?
Default controller meshes are disabled. Rigged hand FBX models from the XR Hands sample are parented to each controller. The `ControllerHandAnimator` script reads **Grip** and **Trigger** input via `XRInputValueReader<float>` and procedurally rotates finger bone transforms (metacarpal → proximal → intermediate → distal) to simulate grasping and pointing.

### What component matches interactors to the right objects?
**`XRInteractionManager`** is the central coordinator. It uses:
- **Interaction Layer Masks** on both interactors and interactables to filter valid pairs
- **Colliders** on interactables for spatial detection
- Interactor types (`XRDirectInteractor`, `XRRayInteractor`) to determine interaction method
- `IXRSelectFilter` / `IXRHoverFilter` for additional filtering

Only interactors and interactables with overlapping interaction layers can hover/select each other.

### What conditions are required for teleportation?
1. **`TeleportationProvider`** on the XR Origin (processes teleport requests)
2. **`XRRayInteractor`** configured as a teleport interactor with teleport input action enabled
3. Valid **teleportation surface** — a collider with `TeleportationArea` or `TeleportationAnchor`
4. Matching **Interaction Layers** between the teleport interactor and the surface
5. Player **aims and confirms** teleport (trigger release or button press depending on configuration)
6. Destination must pass **validation** (not blocked, within range, valid normal)

### Teleportation Anchor vs Teleportation Area
| | **Teleportation Anchor** | **Teleportation Area** |
|---|---|---|
| **Behavior** | Snaps player to a fixed position & rotation | Allows teleport to any valid point on the surface |
| **Use case** | Doorways, platforms, specific viewpoints | Open floor, large walkable zones |
| **Orientation** | Can enforce facing direction | Uses player-selected facing |

### Performance Techniques Used
- **URP** with VR performance preset (`Performance URP Config`)
- **Baked lightmaps** for static geometry (real-time lights minimized)
- **Light probes** for dynamic object lighting
- **Reflection probes** instead of real-time reflections
- **Occlusion culling** enabled on scene
- **Object pooling** patterns from VRTemplate (projectiles, particles)
- **Static batching** on environment geometry
- **XR Interaction Toolkit** efficient interactor update loops

---

## Video Demonstration Checklist

Record a 3–5 minute walkthrough showing:

- [ ] Room overview and sense of space/progression
- [ ] Teleportation around the environment
- [ ] Grabbing and manipulating objects (Key, Book, Mug, Ball)
- [ ] Socket interactions (placing objects in sockets)
- [ ] **Wall clock** displaying correct real-world time with moving hands
- [ ] **Custom hand models** responding to grip and trigger
- [ ] Any additional polish (UI tooltips, audio, etc.)

---

## Repository Structure

```
Assets/
├── Scenes/VRRoom_Assignment.unity   # Main submission scene
├── Scripts/Assignment/              # Assignment-specific scripts
│   ├── AnalogWallClock.cs
│   ├── ControllerHandAnimator.cs
│   ├── InteractableTooltip.cs
│   ├── AmbientAudioZone.cs
│   └── Editor/AssignmentSetupEditor.cs
├── VRTemplateAssets/                # Course template assets
└── Samples/                         # XR Interaction Toolkit & XR Hands samples
```

---

## Requirements

- Unity 6000.5.0f1 (Unity 6)
- XR Interaction Toolkit 3.4.1
- XR Hands 1.7.3
- OpenXR / Meta Quest / PC VR compatible runtime

---

## Author Notes

**Lessons completed:** All Create with VR course modules (VR basics, interaction, locomotion, UI, room design).


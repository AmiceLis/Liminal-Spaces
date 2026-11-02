# CLAUDE.md

Guidance for Claude (and anyone else) working in this repository.

## What this is

**Liminal Spaces** (product name "Liminal Spaces v2") is a Unity VR experience: a
series of eerie, mostly empty environments (TV mosaic, hotel, cinema, sky ruins,
mazes) that the player moves through in a headset. Much of it is driven by
scene layout, Timeline, video, audio, and lighting rather than code.

The owner is new to Git. When explaining Git steps, use plain language and
spell out each command or button.

## Tech stack

- **Unity 6000.4.0f1** (Unity 6.4). See `ProjectSettings/ProjectVersion.txt`.
  Open the project with exactly this editor version from Unity Hub; opening it
  with a different version triggers an upgrade that rewrites many files.
- **Render pipeline:** URP 17.4 (`com.unity.render-pipelines.universal`).
  Two quality tiers live in `Assets/Settings/`: `PC_RPAsset` / `PC_Renderer`
  and `Mobile_RPAsset` / `Mobile_Renderer`. Color space is Linear.
- **XR:** OpenXR 1.16 via XR Plug-in Management 4.6, with XR Interaction
  Toolkit 3.4 (`com.unity.xr.interaction.toolkit`).
  - OpenXR is the loader for both **Standalone (PC VR)** and **Android**
    (`Assets/XR/XRGeneralSettingsPerBuildTarget.asset`).
  - Enabled interaction profile: Oculus Touch Controller (both targets).
  - The **Meta Quest Support** OpenXR feature is currently *off* for Android.
    It must be turned on (Project Settings > XR Plug-in Management > OpenXR >
    Android tab) before a native Quest build will run on device.
  - XRI Starter Assets sample: `Assets/Samples/XR Interaction Toolkit/3.4.0/`.
  - VR template helper scripts: `Assets/VRTemplateAssets/Scripts/`.
- **Input:** the new Input System only (`activeInputHandler: 1`). The legacy
  `UnityEngine.Input` API (`Input.GetKeyDown` and so on) throws at runtime in
  this project. Use the Input System or XRI input actions instead.
  Project actions: `Assets/InputSystem_Actions.inputactions`.
- **Other packages:** Timeline, AI Navigation, glTFast (for the `.glb`
  models), TextMesh Pro, Visual Scripting, Test Framework.
- **IDE:** Visual Studio (see `.vsconfig`) or Rider.

## Folder layout

`Assets/` is a mix of the project's own work and imported third-party packs.
Many of the project's own assets sit loose at the top of `Assets/`.

Project content:

- `Assets/Scenes/Finais/` — the **final scenes** ("finais" = finals):
  `000 - TV mosaic`, `002 HOTEL rebuild`, `004 CINEMA`, `005 SKY RUINS`.
- `Assets/Scenes/` — `Maze Level 1–3`, `TestScene - March 2026`.
- `Assets/Scenes/Michael/` — one contributor's personal test scenes.
- `Assets/Assets/Final Levels/` — level geometry chunks (hotel, pool, sky,
  sky_new) as FBX plus 4K materials.
- `Assets/Assets/Audio and Video/`, `Assets/Curtains Videos/`, plus loose
  `.mp4`, `.mov`, `.wav` files at the top of `Assets/` — media for the
  cinema and TV scenes.
- `Assets/TV/`, `Assets/mirror/`, `Assets/Materials/`, `Assets/ShaderGraph/`
  and loose `.shadergraph`, `.mat`, `.renderTexture`, `.prefab` files.
- `Assets/Scripts/` and `Assets/DelayedAudio.cs` — the project's own C#.

Third-party packs (treat as vendor code: do not edit or reformat unless asked):

- `Assets/Crest/` (ocean), `Assets/VolumetricFog2/` (Volumetric Fog & Mist 2),
  `Assets/#NVJOB Dynamic Sky/`, `Assets/SkySeries Freebie/`, `Assets/MilkyWay/`,
  `Assets/TextMesh Pro/`, `Assets/Samples/`, `Assets/TutorialInfo/`.

Put new project scripts in `Assets/Scripts/` and new scenes under
`Assets/Scenes/`, rather than at the top of `Assets/`.

## The project's own scripts

All are plain `MonoBehaviour`s in the global namespace, with no assembly
definition (they compile into `Assembly-CSharp`).

- `ExitBox.cs` — a trigger volume. When an object tagged `Player` enters, it
  hides the current level chunk (its parent), activates the next chunk, and
  moves the player rig there. Set `LastScene` on the final box. Used in
  `Maze Level 1–3`, `TestScene - March 2026`, and the
  `SecondVREnvironment` / `ThirdVREnvironment` prefabs.
- `SceneChanger.cs` — meant to load scene index `nextSceneNum` after a timer.
  Note: when `changeAtSpecificTime` is true it calls `Invoke` every frame, and
  `sceneTimer` / `sceneTimerLimit` are not used yet. Used in
  `Scenes/Michael/000_TV_Michael`.
- `FogRaiser.cs` — unfinished. Uses legacy `Input.GetKeyDown("Space")`,
  which throws under the Input System-only setting (see above).
- `DelayedAudio.cs` — plays an `AudioSource` after a delay.

When changing these, keep public field names stable. Renaming a serialized
field silently drops the values set on it in every scene and prefab (use
`[FormerlySerializedAs]` if a rename is needed).

## Build settings

`ProjectSettings/EditorBuildSettings.asset` lists only
`Assets/Scenes/SampleScene.unity`, which no longer exists. Scenes must be added
in File > Build Profiles before `SceneManager.LoadScene(index)` (used by
`SceneChanger`) will work in a build.

## There is no command line build or test here

There are no CI workflows, build scripts, or tests in the repo. Changes are
checked by opening the project in the Unity editor and entering Play mode
(with a headset, or the XR Device Simulator from the XRI samples). Claude
cannot run Unity in this environment, so say clearly what was and was not
verified, and give the owner steps to check a change in the editor.

## Working with Unity files

- **Always keep `.meta` files with their asset.** Every file and folder in
  `Assets/` has a `.meta` holding its GUID. Moving, renaming, or deleting an
  asset means doing the same to its `.meta`; losing one breaks every reference
  to that asset. Prefer doing moves and renames inside the Unity editor.
- Asset serialization is **Force Text**, so `.unity`, `.prefab`, `.mat`,
  `.asset` and `.controller` files are YAML. They can be read and searched, but
  avoid hand-editing them except for small, well-understood changes. Two people
  editing the same scene at once usually produces a merge conflict that is hard
  to resolve, so coordinate who works in which scene.
- Never commit `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`,
  `Build(s)/`, or IDE files (`.csproj`, `.sln`). `.gitignore` already covers these.

## Git and Git LFS

- The default branch is `main`.
- The repository uses **Git LFS**, and the LFS content is large (about 9 GB).
  `.gitattributes` sends `.png .jpg .psd .tga .exr .wav .ogg .mp4 .mov .avi
  .webm .obj .fbx .blend .unitypackage` to LFS.
- **Claude: do not pull LFS content** (`git lfs pull`, `git lfs fetch --all`)
  unless a task truly needs a specific binary. Code, scenes, prefabs, materials
  and `.meta` files are all plain text and need no LFS download. If one file
  is needed, fetch just that one with `git lfs pull --include="<path>"`.
- Some binary types are **not** covered by LFS today: `.glb` (for example
  `Assets/mirror_a.glb` is about 38 MB), `.jpeg`, `.mesh`, `.mp3`, `.tif`.
  GitHub rejects files over 100 MB, so add new large binary types to
  `.gitattributes` (`git lfs track "*.glb"`) before committing them.
- `.plastic/` and `ignore.conf` are left over from Unity Version Control
  (Plastic SCM). Git is now the source of truth; leave them alone unless the
  owner asks to remove them.

## Conventions for changes

- Keep changes small and focused, on a branch, merged into `main` through a
  pull request.
- Match the existing C# style: Allman braces, `public` fields for Inspector
  values, Unity message methods (`Start`, `Update`, `OnTriggerEnter`).
- Cache components in `Start`/`Awake` instead of calling `GetComponent` or
  `Find` in `Update`; frame rate matters a lot in VR.
- Compare tags with `CompareTag("Player")`, not `tag ==`.

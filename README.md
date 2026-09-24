# Practice Math

A kid-friendly math practice game built in **Unity**, targeting **Android** phones and tablets first. The same project can later target iOS or desktop with minimal changes.

## What is in this repo right now

This repository **is** a full Unity project at the root (`Assets/`, `Packages/`, `ProjectSettings/`). Editor version is recorded in `ProjectSettings/ProjectVersion.txt` (Unity **6** / **6000.x** line with **URP 2D** template).

- **`Assets/PracticeMath/Scripts/Core/`** — pure C# math types and a random problem generator (addition, subtraction, multiplication, whole-number division). No dedicated game UI yet; this is the foundation for screens, scoring, and audio.
- **`Assets/Scenes/Home.unity`** — landing hub (grade picker + activity tiles). First scene in **File → Build Settings**.
- **`Assets/Scenes/PracticeMath.unity`** — mixed + − × ÷ practice and 10-question quiz.
- **`Assets/Scenes/TimesTableGrid.unity`** — interactive 1–12 multiplication chart (tap row, column, then the product).
- **`Assets/Scenes/TimesTables.unity`**, **`MultipleChoice.unity`** — times-table quiz (0–12, keypad) and multiple-choice strands (geometry, patterns, money, data).

### Editable UI (prefabs + scenes)

Hub and activity screens use **Canvas prefabs** you can lay out in the Inspector (not built only at runtime).

1. In Unity: **Practice Math → UI → 1. Generate Missing UI Prefabs** (first time), then **3. Sync Scenes With Prefabs** after you change prefabs. Use **1b. Force Regenerate** only to reset layouts from code (overwrites prefab edits).
2. Edit **`Assets/Prefabs/UI/`** (`HomeHubPanel`, `TimesTableGridPanel`, `TimesTablesPanel`, `MultipleChoicePanel`) — colors, fonts, anchors, spacing.
3. Or open **`Home`**, **`TimesTableGrid`**, **`TimesTables`**, or **`MultipleChoice`** scenes and edit the prefab instance on the Canvas.
4. **`PracticeMath`** scene: edit the existing Canvas directly; wire **`HomeHubController`** / tile **`HomeNavTileView`** components on the hub prefab.

Controllers only handle logic; all visuals live on the prefab/scene objects.

## Open the project

1. Install **Unity Hub** and the **same major editor version** as `ProjectVersion.txt` (or let the Hub upgrade the project when prompted).
2. **Add** / **Open** this folder as a Unity project.
3. For Android builds, install the Hub module **Android Build Support** (include **Android SDK & NDK** and **OpenJDK** when offered).

After import, you should see **PracticeMath** under `Assets` with no compile errors.

## Android build checklist

In Unity: **Edit → Project Settings → Player → Android**:

- **Company / Product** names (as you want them on device).
- **Minimum API Level** — pick a reasonable floor (for example API 24+) for the devices you care about.
- **Scripting Backend:** **IL2CPP** is typical for Play Store ARM64.
- **Target Architectures:** enable **ARM64** (required for many store policies); **ARMv7** only if you must support very old devices.

**File → Build Settings → Android → Switch Platform**, then connect a device with **USB debugging** and use **Build And Run**, or produce an **APK/AAB** for sideloading.

## Suggested next steps in the editor

1. Add a **Canvas + TextMeshPro** (or UI Text) for the question and an **Input Field** or on-screen number pad for answers.
2. Add a small **MonoBehaviour** that holds `MathProblemGenerator`, calls `Next(GeneratorSettings.DefaultEarlyElementary())`, displays `Prompt`, and compares the child’s input to `CorrectAnswer`.
3. Tune `GeneratorSettings` in the Inspector (expose it as a serialized field) or load presets by age/grade.

Cross-platform later: add iOS build support in the Hub, fix platform-specific input/safe areas, and run through **Player Settings** per store.

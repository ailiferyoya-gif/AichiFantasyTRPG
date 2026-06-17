# Next Chat Handoff - AichiFantasyTRPG iOS/WebGL Size Optimization

## Project
- Path: `D:\Codex\AichiFantasyTRPG`
- Goal: Convert the Unity game toward an iOS-friendly build while keeping a GitHub Pages WebGL preview usable if possible.

## Current Findings
- `Assets/Resources` is the main size problem, about 587 MB.
- `Assets/Resources/AichiFantasy/Portraits` is about 502 MB across 804 files.
- Prior WebGL artifact `webgl.data` was about 487 MB, which explains long loads and repeated reloads on iPhone Safari.
- The game loads images via `Resources.Load`, so anything under `Resources` is likely included in player data.

## Backups
- Manual pre-change backup:
  `D:\Codex\AichiFantasyTRPG\Backups\20260617_214029_pre_ios_webgl_size_optimization`
- Unity auto backup created on batch startup:
  `D:\Codex\AichiFantasyTRPG\Backups\20260617_214245_auto`

## Changes Made
- Added `Assets/Editor/AichiFantasyBuildAssetOptimizer.cs`.
  - WebGL: backgrounds max 512, portraits max 384, ASTC 6x6, not readable, no mipmaps.
  - iOS: backgrounds max 1024, portraits max 768, ASTC 6x6, not readable, no mipmaps.
- Updated `Assets/Editor/AichiFantasyWebGlBuilder.cs`.
  - Uses the shared optimizer before build.
  - Changes WebGL compression from disabled to Gzip.
  - Enables decompression fallback and data caching.
  - Raises max WebGL memory from 512 MB to 1024 MB.
- Updated `Assets/Editor/AichiFantasyIosBuilder.cs`.
  - Runs the shared iOS optimizer before exporting the Xcode project.
- Unity batch compile succeeded with return code 0.
  - Only existing obsolete API warnings were reported.
- WebGL size-optimized build succeeded:
  `D:\Codex\AichiFantasyTRPG\build\WebGL_SizeOpt`
  - `Build/WebGL_SizeOpt.data.unityweb`: about 14.4 MB
  - `Build/WebGL_SizeOpt.wasm.unityweb`: about 7.51 MB
  - Prior local `Library/Bee/artifacts/WebGL/webgl.data` was about 487 MB before optimization.

## Not Completed
- Did not run iOS Xcode export or IPA packaging.
- Did not delete duplicate portrait files yet.
- Did not move from `Resources` to Addressables/AssetBundles/external staged loading yet.

## Recommended Next Steps
1. Test `build/WebGL_SizeOpt` locally and then via GitHub Pages/iPhone Safari.
2. Run iOS export and check IPA/app size.
3. If WebGL still reloads on device, remove duplicate portrait files or migrate large image sets out of `Resources`.
4. Consider Addressables/AssetBundles/external staged loading only if the optimized single package is still not good enough.

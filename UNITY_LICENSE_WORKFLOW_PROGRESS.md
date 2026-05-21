# Unity License Workflow Progress

## 2026-05-22
- Backup: `Backups/20260522_011530_pre_unity_license_doc_fix`
- Removed unsupported workflow: `.github/workflows/request-unity-activation.yml`
- Updated `IOS_ALTSTORE_BUILD.md` to use the current GameCI activation flow.
- Current flow: activate Unity Personal locally in Unity Hub on Windows, copy `C:\ProgramData\Unity\Unity_lic.ulf` into GitHub secret `UNITY_LICENSE`, and keep `UNITY_EMAIL` / `UNITY_PASSWORD` as Unity account credentials.
- Reason: `game-ci/unity-request-activation-file@v2` now reports unsupported and does not accept `unityVersion`.

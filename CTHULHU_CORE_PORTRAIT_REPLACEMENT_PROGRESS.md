# 中核怪異ポートレート差し替えメモ

## 2026-05-21
- 作業前バックアップ: `Backups/20260521_003839_pre_cthulhu_core_portrait_replacements`
- ChatGPT生成画像を個別に3枚生成。スプライトシート不使用。
- 対象:
  - `locker_womb.png`
  - `window_god.png`
  - `impossible_one.png`
- 生成方針:
  - クトゥルフ神話に出てきそうな、不穏で顔が読めない人外。
  - 単色クロマキー背景で生成後、ローカル処理で透過。
  - 四隅アルファ0を確認。
- `Assets/Resources/AichiFantasy/Portraits` と `Assets/Resources/AichiFantasy/Portraits/Enemies` の両方を同名で差し替え。
- 置き換え後の各ファイルで四隅アルファ0を再確認。
- 検証ログ:
  - `Logs/cthulhu-core-portraits-build.log`

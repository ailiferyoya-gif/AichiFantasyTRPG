# ボスポートレート不快感調整メモ

## 2026-05-21
- 作業前バックアップ: `Backups/20260521_005815_pre_less_grotesque_boss_portraits`
- 対象:
  - `locker_womb.png`
  - `window_god.png`
- 方針:
  - 気持ち悪さ、肉感、ぬめり、内臓感、虫っぽさを避ける。
  - 怖さは上げつつ、鍵・金属・黒硝子・儀式具・異界建築の不穏さへ寄せる。
  - 顔が読めない、人外、クトゥルフ神話に出てきそうな雰囲気を維持。
- ChatGPTで個別生成。スプライトシート不使用。
- クロマキー背景から透過処理し、四隅アルファ0を確認。
- `Assets/Resources/AichiFantasy/Portraits` と `Assets/Resources/AichiFantasy/Portraits/Enemies` の両方を差し替え。
- 置き換え後のRoot/Enemies両方で四隅アルファ0を再確認。
- 検証ログ:
  - `Logs/less-grotesque-boss-portraits-build.log`

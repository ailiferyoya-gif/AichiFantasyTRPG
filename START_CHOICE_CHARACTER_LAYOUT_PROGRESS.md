# 初期選択肢・キャラ表示調整メモ

## 2026-05-20
- 作業前バックアップ: `Backups/20260520_231522_pre_start_choice_character_layout_fix`
- 名駅の底の初期選択肢を、戻り後のハブと同じ5枠に正規化。
  - 本線へ進む
  - 地方へ向かう
  - 拠点で整える
  - 記憶と方針
  - 空港へ向かう
- `AddAiWideExpansion()` など後段の拡張が初期ハブへ選択肢を差し込んでも、最後に `NormalizeMeiekiStartChoices()` で整える形にした。
- キャラ選択/確認中は立ち絵枠を専用レイアウトにし、キャラごとの画像差でメッセージ枠へ重なりにくくした。
- 検証ログ:
  - `Logs/start-choice-character-layout-fix-build.log`
  - `Logs/start-choice-character-layout-fix-ui-smoke.log`

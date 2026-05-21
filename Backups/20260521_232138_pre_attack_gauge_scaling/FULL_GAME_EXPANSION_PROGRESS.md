# Full Game Expansion Progress

## 方針
- 通常ルートの終点は「境界空港長」と通常EDに整理する。
- 「この世のものとは思えないもの」は本線攻略ではなく、専用キャラの特別イベントとして扱う。
- 死亡時の恩恵は記憶片配布や恒久強化ではなく、死因図鑑によるショートカット中心にする。
- STAGEごとに選択結果、ボス行動、ショップ強化の意味を分ける。
- 記憶片は貴重品として扱い、通常周回で大量に増えないようにする。

## 今回の実装
- 作業前バックアップを取得: `Backups/20260521_020905_pre_full_game_expansion`
- 通常ラスボス「境界空港長」のChatGPT生成Portraitを追加し、クロマキー除去で背景透過化。
- STAGE5ボス撃破後の導線を空港最終連続ルートへ接続。
- 境界空港長撃破後に専用の通常ED前シーンを追加。
- 死因図鑑ショートカットを追加。死因数に応じてSTAGE2、STAGE3、空港検疫線へ短絡可能。
- ボスごとの反撃差を追加。境界空港長、各STAGEボス、味噌声、窓外神、搭乗検査官で圧のかかり方を変更。
- 装備特殊効果を追加: 溜め撃ち、初撃遮断、逃げ道、ボス護符。
- STAGEショップに「遮断札」を追加。次戦の初撃を軽減する周回内強化として扱う。
- STAGE4/5の報酬傾向を調整し、海・空港方面の神話/SAN/危険察知の色を強めた。

## 検証
- BuildMainScene 成功: `Logs/full-game-expansion-build.log`
- STAGEルート smoke passed: `Logs/full-game-expansion-stage-smoke.log`
- 通常ラスボスから通常EDまでの route smoke passed: `Logs/full-game-expansion-route-smoke.log`
- UI smoke passed: `Logs/full-game-expansion-ui-smoke.log`
- 境界空港長Portraitは背景透過済み。四隅アルファ0を確認。

## 次に見ること
- 実プレイで境界空港長Portraitの画面内サイズと足元位置を確認。
- ボス固有行動が強すぎないか、特にSTAGE5と境界空港長のSAN圧を確認。
- 死因図鑑ショートカットが便利すぎないか、死因5以上の空港短絡を重点確認。

## 2026-05-21 Portrait Layout Unification
- バックアップ: `Backups/20260521_230645_pre_portrait_layout_unification`
- `Portraits/NPC` と `Portraits/Enemies` も再帰的にTextureImporter設定対象にし、Readable化して正規化処理へ通すよう変更。
- Portrait表示時に `Image.Type.Simple` と `preserveAspect` を毎回再設定し、画像の縦横比崩れを抑制。
- キャラ選択、戦闘、イベントのPortrait表示枠を大きめかつ共通寄りに調整。
- BuildMainScene 成功: `Logs/portrait-layout-unification-build.log`
- UI smoke passed: `Logs/portrait-layout-unification-ui-smoke.log`

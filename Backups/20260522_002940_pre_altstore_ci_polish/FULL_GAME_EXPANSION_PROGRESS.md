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

## 2026-05-21 Attack Gauge Scaling
- バックアップ: `Backups/20260521_232138_pre_attack_gauge_scaling`
- 高攻撃力でも低ゲージ連打で削れすぎないよう、攻撃力の反映率をゲージ量に連動。
  - 低ゲージ: 攻撃力の寄与をかなり低くする。
  - 中ゲージ: 段階的に攻撃力が乗る。
  - 70%以上: 本命として攻撃力が大きく乗る。
  - 95%以上: 最大火力帯。
- 低ゲージ攻撃後ほど短い硬直を追加し、連打より溜める判断を優先しやすくした。
- BuildMainScene 成功: `Logs/attack-gauge-scaling-build.log`
- UI smoke passed: `Logs/attack-gauge-scaling-ui-smoke.log`

## 2026-05-22 iOS / AltStore CI
- バックアップ: `Backups/20260522_002017_pre_github_ios_altstore_workflow`
- GitHub Actions workflow追加: `.github/workflows/build-ios-altstore.yml`
- iOS Xcode書き出し用Editorメソッド追加: `Assets/Editor/AichiFantasyIosBuilder.cs`
- AltStore向け手順メモ追加: `IOS_ALTSTORE_BUILD.md`
- 方針: UnityでiOS Xcodeプロジェクトを書き出し、macOS runnerで署名なし `.app` を作り、`Payload/*.app` 形式の `.ipa` artifactとして出す。
- BuildMainScene 成功: `Logs/ios-altstore-workflow-build.log`

## 2026-05-22 AltStore Release Source
- バックアップ: `Backups/20260522_002522_pre_altstore_release_source`
- GitHub Actions workflowを拡張し、固定Release `altstore-latest` を作り直すようにした。
- Release assetとして `AichiFantasyTRPG-AltStore.ipa`、`AichiFantasyTRPG-app.zip`、`AichiFantasyTRPG-icon.png`、`altstore-source.json` を公開する。
- AltStore Source URLは `https://github.com/<OWNER>/<REPO>/releases/download/altstore-latest/altstore-source.json`。
- `IOS_ALTSTORE_BUILD.md` を更新し、Release公開後のAltStore追加手順を追記。
- ローカルではworkflow YAML/ドキュメントのみ変更。iOS XcodeビルドとRelease公開はGitHub Actions上で実行する。

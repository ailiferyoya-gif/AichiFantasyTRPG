# iOS実装状況

## 実装済み

- Unity iOS向けのXcodeプロジェクト書き出しを `AichiFantasy.Editor.AichiFantasyIosBuilder.BuildIosXcodeProject` に集約。
- iOSビルド前にUnityのビルドターゲットをiOSへ切り替える。
- Bundle ID、表示バージョン、ビルド番号をGitHub Actionsの入力から反映。
- iOS 15.0以上、IL2CPP、.NET Standard 2.1、iPhone向け自動回転設定を明示。
- iPhoneのnotch/home indicatorにUIが重ならないよう、ゲームUIを `SafeArea` ルート配下に配置。
- 背景、暗幕、狂気オーバーレイは全画面表示のまま維持。
- GitHub ActionsはmacOS runnerでUnity iOS exportを実行し、Xcodeプロジェクトの存在を検証してからIPA化する。
- AltStore向けに未署名IPA、AltStore source JSON、アイコンをartifact/releaseへ出力する。

## ビルド手順

1. GitHubのRepository Settings > Secrets and variables > Actionsに `UNITY_LICENSE`、`UNITY_EMAIL`、`UNITY_PASSWORD` を登録する。
2. Actionsで `Build iOS IPA for AltStore` を選ぶ。
3. `Run workflow` を押す。
4. 必要なら `bundle_id` と `bundle_version` を変更する。
5. 成功後、`altstore-latest` releaseから `AichiFantasyTRPG-AltStore.ipa` と `altstore-source.json` を取得する。

## 注意

- このIPAはAltStore等で再署名してテスト導入する用途。App Store提出用の署名済みIPAではない。
- iOSビルドはmacOS runnerとUnity iOS Build Supportが必要。
- Unityライセンスの制限でActionsが失敗する場合は、Unityライセンス種別やGameCIの対応状況を確認する。

## iPhone Web確認

IPA導入前の画面確認用に、WebGLをGitHub Pagesへ公開するworkflowを追加した。

- workflow: `.github/workflows/build-webgl-pages.yml`
- build method: `AichiFantasy.Editor.AichiFantasyWebGlBuilder.BuildWebGlPlayer`
- 手順: `WEBGL_IPHONE_PREVIEW.md`
- iPhone Safariのロード中再読み込み対策として、WebGLビルド時だけ画像を最大512pxへ縮小/圧縮し、Data Cachingを無効化している。

## 今回のバックアップ

変更前バックアップ:

```text
C:\Users\kogit\Documents\Codex\AichiFantasyTRPG\Backups\20260606_212725_pre_ios_implementation
```

## 引継ぎ

`AichiFantasyTRPG` をiOS用に実装。Unity側ではSafe Area対応とiOS build settingsを追加し、GitHub ActionsではiOS exportをmacOS runnerに変更してXcode project検証を追加した。さらにiPhone Safari確認用のWebGL Pages workflowも追加した。ロード中再読み込み対策としてWebGL専用の画像軽量化とキャッシュ無効化を追加済み。次はGitHub Actionsで `Build WebGL iPhone Preview` を実行してiPhone SafariでUI確認し、その後 `Build iOS IPA for AltStore` でIPA生成を確認する。

# iPhone / AltStore Test Build

## 目的
GitHub ActionsでiOS向けのUnityビルドを作り、AltStoreへ渡せる `.ipa` をartifactとして取得する。

## できること
- Unity 6000.4.5f1でiOS Xcodeプロジェクトを書き出す。
- GitHubのmacOS runnerで署名なし `.app` を作る。
- `Payload/*.app` 形式でzipし、AltStore向け `.ipa` としてartifact化する。

## 必要なGitHub Secrets
Repository Settings > Secrets and variables > Actions に以下を登録する。

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

Unity Personal/Plus/ProのCI利用ではUnityライセンス認証が必要。

## 実行手順
1. GitHubへこのプロジェクトをpushする。
2. GitHubのActionsタブを開く。
3. `Build iOS IPA for AltStore` を選ぶ。
4. `Run workflow` を押す。
5. 必要なら `bundle_id` を変更する。
   - 例: `com.kogit.aichifantasytrpg`
6. 成功後、artifact `AichiFantasyTRPG-AltStore-IPA` をダウンロードする。
7. 中の `AichiFantasyTRPG-AltStore.ipa` をAltStoreへ渡す。

## 注意
- このworkflowはApp Store配布用の正式署名IPAではない。
- AltStore側でApple IDを使って再署名してインストールする前提。
- Unity iOS Build SupportがCI側で必要。`game-ci/unity-builder` がUnity書き出しを担当する。
- macOS jobはXcodeで `.app` を作り、`Payload` 形式にして `.ipa` 化する。
- Apple Developer Programの有料アカウントで正式署名したい場合は、証明書とProvisioning ProfileをSecretsに入れる別workflowに分けるのが安全。

## 追加されたファイル
- `.github/workflows/build-ios-altstore.yml`
- `Assets/Editor/AichiFantasyIosBuilder.cs`

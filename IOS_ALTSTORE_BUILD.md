# iPhone / AltStore Test Build

## 目的
GitHub ActionsでiOS向けUnityビルドを作り、AltStoreへ渡せる `.ipa` とAltStore Source JSONをGitHub Releaseに並べる。

## 追加済みの仕組み
- Unity 6000.4.5f1でiOS Xcodeプロジェクトを書き出す。
- GitHubのmacOS runnerで署名なし `.app` を作る。
- `Payload/*.app` 形式でzipし、AltStore向け `.ipa` を作る。
- 固定Release `altstore-latest` に以下を公開する。
  - `AichiFantasyTRPG-AltStore.ipa`
  - `altstore-source.json`
  - `AichiFantasyTRPG-icon.png`

## GitHub Secrets
Repository Settings > Secrets and variables > Actions に以下を登録する。

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

Unity Personalの場合は、GameCIのUnity activation手順で取得した `.ulf` の中身を `UNITY_LICENSE` に入れる。

## 実行手順
1. このプロジェクトをGitHubへpushする。
2. GitHubのActionsタブを開く。
3. `Build iOS IPA for AltStore` を選ぶ。
4. `Run workflow` を押す。
5. `bundle_id` は基本 `com.kogit.aichifantasytrpg` のままでよい。
6. 成功後、Release `altstore-latest` が作り直される。
7. Actionsの実行SummaryにAltStore Source URLとDirect IPA URLが表示される。

## AltStoreへ追加するURL
Release公開後、AltStoreのSourcesへ以下を追加する。

```text
https://github.com/<OWNER>/<REPO>/releases/download/altstore-latest/altstore-source.json
```

`<OWNER>/<REPO>` は実際のGitHubリポジトリ名に置き換える。

ActionsのSummaryにも同じURLが出る。

## 直接IPAを入れる場合
ReleaseまたはActions artifactから以下を取得してAltStoreへ渡す。

```text
AichiFantasyTRPG-AltStore.ipa
```

## 注意
- このworkflowはApp Store配布用の正式署名IPAではない。
- AltStore側でApple IDを使って再署名してインストールする前提。
- GitHub Releaseがprivate repoの場合、iPhone側からSource JSONやIPAへ直接アクセスできない場合がある。確実にAltStore Sourceとして使うならpublic repoか、公開可能な配布先へ置く。
- Apple Developer Programの証明書とProvisioning Profileで正式署名したい場合は、署名済みIPA用の別workflowに分ける。
- Source JSONはCI内で最低限の構造検査を行う。AltStoreで追加できない場合は、Release assetの公開範囲、Bundle ID、IPAの署名状態を確認する。

## 追加ファイル
- `.github/workflows/build-ios-altstore.yml`
- `Assets/Editor/AichiFantasyIosBuilder.cs`

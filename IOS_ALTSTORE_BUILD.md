# iPhone / AltStore Test Build

## 目的
GitHub ActionsでUnityのiOS向けXcodeプロジェクトを作成し、GitHubのmacOS runnerで未署名IPAを組み立て、AltStoreで再署名してiPhoneへ入れられる形にする。

Mac本体は不要。Macが必要な工程はGitHub ActionsのmacOS環境で実行する。

## 追加済みの仕組み
- Unity 6000.4.5f1でiOS Xcodeプロジェクトを書き出す。
- macOS runnerで未署名の `.app` を作る。
- `Payload/*.app` 形式でzip化し、AltStore向け `.ipa` を作る。
- 固定Release `altstore-latest` に以下を公開する。
  - `AichiFantasyTRPG-AltStore.ipa`
  - `altstore-source.json`
  - `AichiFantasyTRPG-icon.png`

## GitHub Secrets
Repository Settings > Secrets and variables > Actions に以下を登録する。

| Name | Secretに入れる内容 |
| --- | --- |
| `UNITY_LICENSE` | Windows上のUnity Hubで発行された `.ulf` ファイルの中身全部 |
| `UNITY_EMAIL` | Unityアカウントのメールアドレス |
| `UNITY_PASSWORD` | Unityアカウントのパスワード |

## UNITY_LICENSEの取り方
GameCIの現在の手順では、古い `.alf` 作成Actionは使用しない。
Windows上のUnity HubでPersonalライセンスを有効化し、生成された `.ulf` を使う。

1. WindowsでUnity Hubを開く。
2. Unityアカウントでログインする。
3. `Preferences` > `Licenses` を開く。
4. `Add` を押す。
5. `Get a free personal license` を選んで有効化する。
6. 次のファイルを開く。

```text
C:\ProgramData\Unity\Unity_lic.ulf
```

`ProgramData` は隠しフォルダなので、見えない場合はエクスプローラーで隠しファイルを表示する。

7. `Unity_lic.ulf` をメモ帳で開く。
8. 中身を全部コピーする。
9. GitHub Secret `UNITY_LICENSE` に貼り付ける。

## iOS IPAビルド手順
1. GitHubのリポジトリを開く。
2. `Settings` > `Secrets and variables` > `Actions` を開く。
3. `UNITY_LICENSE`、`UNITY_EMAIL`、`UNITY_PASSWORD` を登録する。
4. `Actions` タブを開く。
5. `Build iOS IPA for AltStore` を選ぶ。
6. `Run workflow` を押す。
7. `bundle_id` は基本的に `com.kogit.aichifantasytrpg` のままでよい。
8. 成功後、Release `altstore-latest` が作られる。
9. ActionsのSummaryにAltStore Source URLとDirect IPA URLが表示される。

## AltStoreに追加するURL
Release公開後、AltStoreのSourcesへ以下を追加する。

```text
https://github.com/ailiferyoya-gif/AichiFantasyTRPG/releases/download/altstore-latest/altstore-source.json
```

## 直接IPAを使う場合
ReleaseまたはActions artifactから以下を取得する。

```text
AichiFantasyTRPG-AltStore.ipa
```

## 注意
- このworkflowはApp Store配布用の正式署名IPAを作るものではない。
- AltStore側でApple IDを使って再署名してインストールする前提。
- Unity PersonalライセンスでGitHub Actionsが通らない場合は、Unity側のライセンス制限が原因の可能性がある。
- その場合の現実的な代替は、Unity Pro/Plusのシリアルを使う、またはUnity Build AutomationなどUnity公式のクラウドビルドへ寄せること。
- `game-ci/unity-request-activation-file@v2` は現在サポート外なので使用しない。

## 関連ファイル
- `.github/workflows/build-ios-altstore.yml`
- `Assets/Editor/AichiFantasyIosBuilder.cs`

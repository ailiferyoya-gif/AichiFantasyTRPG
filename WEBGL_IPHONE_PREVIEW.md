# iPhone Web確認手順

## 目的

iOSアプリとしてIPAを入れる前に、iPhone Safariで画面サイズ、タップ操作、文字量、Safe Area風の見え方を確認する。

## GitHub Pagesで公開する

1. GitHubのRepository Settings > Secrets and variables > Actionsに `UNITY_LICENSE`、`UNITY_EMAIL`、`UNITY_PASSWORD` を登録する。
2. GitHubのRepository Settings > PagesでSourceを `GitHub Actions` にする。
3. Actionsで `Build WebGL iPhone Preview` を選ぶ。
4. `Run workflow` を押す。
5. 成功後、Actions SummaryのURLをiPhone Safariで開く。

## 確認ポイント

- タイトル、ステータス、選択肢がiPhone画面内に収まるか。
- 下部の選択肢がSafariのホームバー付近に近すぎないか。
- 縦持ちで文章送り、選択肢スクロール、バトルボタンが押しやすいか。
- 横持ちでサイド選択肢レイアウトが崩れないか。
- 背景とキャラクター画像が読み込まれるか。

## 注意

- WebGL確認はWeb版の確認であり、AltStore/IPAのネイティブ動作確認とは別。
- iPhone Safariでは音声再生が初回タップ後に始まる場合がある。
- WebGLは端末性能やSafari設定により読み込みに時間がかかることがある。

## 今回のバックアップ

変更前バックアップ:

```text
C:\Users\kogit\Documents\Codex\AichiFantasyTRPG\Backups\20260606_213745_pre_webgl_iphone_preview
```

## 引継ぎ

iPhone Safari確認用にUnity WebGLビルダーとGitHub Pages公開workflowを追加。次はGitHub Actionsの `Build WebGL iPhone Preview` を実行し、出力されたPages URLをiPhone Safariで開いてUIと操作感を確認する。

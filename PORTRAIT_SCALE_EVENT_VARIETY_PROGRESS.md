# 立ち絵サイズ・イベント人物バリエーション調整メモ

## 2026-05-20
- 作業前バックアップ: `Backups/20260520_234017_pre_portrait_scale_event_variety`
- 立ち絵正規化キャンバスを縦長から正方形へ変更し、キャラごとの見た目サイズ差を抑制。
- バトル中はボスと通常敵で同じ立ち絵表示枠を使うようにし、ボスだけ縦長・大きめに見える状態を抑制。
- ランダムイベントの人物表示を、イベントIDと地域から安定的に振り分けるよう変更。
  - 名駅系、尾張系、三河系、知多/空港系で既存のChatGPT生成NPCポートレートをプール化。
  - 同じ地域で同じ人物ばかり表示される偏りを軽減。
- 検証ログ:
  - `Logs/portrait-scale-event-variety-build.log`
  - `Logs/portrait-scale-event-variety-stage-smoke.log`

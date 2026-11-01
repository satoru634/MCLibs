# 注意すべき癖・既知の制限

## 綴り・名前空間

利用側から参照されているため、以下の綴りは修正せずそのまま使ってください（変更する場合は利用側も合わせて修正が必要です）。

- `BlockModelLoarder`（Loader ではない）、`ExsistsItemID`、`AddTexureImages`、`GetMincraftPath`
- `ResourcePackInfo.cs` の名前空間は `MCToolsCommonLib.Util`（他は `MCToolsCommonLib.Utils`）
- `CommonLib` などの静的メソッドは `static public` の順で宣言されています。新規追加時も合わせてください。

## 前提としているパス構成

- `JarLoader` は jar が `.minecraft/versions/<バージョン>/<バージョン>.jar` にある前提で、3階層上を `.minecraft` とみなします（`GetMincraftPath`）。
- 同じ場所の `<バージョン>.json` から `assetIndex` を読み、アセットインデックスを特定します。
- `GetNameSpace` は jar 内に `META-INF/mods.toml` があれば `modId` を、なければ `minecraft` を名前空間とします。

## バージョン依存の処理

- 1.21.4 以降かどうかは `CommonLib.CompareVersion(Version, "1.21.3") == 1` で判定しています。
- 1.21.4 以降の items 定義は条件分岐を持ちますが、常に既定の分岐（`fallback` / `on_false` / 閾値 0）だけを採用します。

## 既知の制限

- バイオームの色は `colorset.json` の固定値です。1.21.4 以降では反映されない場合があります。
- ブロックエンティティ（チェスト、ベッドなど）は描画されません。
- `MCToolsCommonLib` にはリージョン/NBT 関連など、MCItemViewer では使わない機能も含まれます。他ツールで使われている可能性があるため、未使用に見えても削除しないでください。

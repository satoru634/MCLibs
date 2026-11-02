# ビルド / テスト

```bash
dotnet build MCLibs.sln
dotnet test MCLibs.sln
```

## プロジェクト設定

全プロジェクト（テストプロジェクトを含む）で以下の共通設定です。

- `TargetFramework`: `net8.0-windows10.0.17763.0`
- `PlatformTarget`: `x64`
- `UseWPF`: `true`、`Nullable`: `enable`、`ImplicitUsings`: `enable`

## 依存パッケージ

| プロジェクト | パッケージ |
|---|---|
| MCToolsCommonLib | CsvHelper、MiNET.fnbt（NBT）、Nett（TOML）、OpenCvSharp4（＋runtime.win、WpfExtensions）、System.Text.Json |
| MCModelRenderer | HelixToolkit.SharpDX.Core.Wpf、OpenCvSharp4（＋runtime.win、WpfExtensions）、System.Text.Json |

MCModelRenderer は MCToolsCommonLib を `ProjectReference` で参照しています。

## テスト

- テストプロジェクトは `MCLibsTests`（xUnit）です。両ライブラリを `ProjectReference` で参照しています。
- テストファイルは、対象クラスの名前空間に合わせて `MCLibsTests/<名前空間のフォルダ>/<クラス名>Tests.cs` に配置します（例: `MCLibsTests/MCModelRenderer/Utils/BlockModelLoarderTests.cs`）。テストクラスの名前空間は `MCLibsTests.<対象の名前空間>` です。`ResourcePackInfo` は名前空間が `MCToolsCommonLib.Util` のため、テストも `MCLibsTests/MCToolsCommonLib/Util/` に置きます。
- テストメソッド名は `<メソッド名>_<条件>_<期待結果>`（英語）とし、メソッドごとに `// ── <メソッド名> ───` の区切りコメントを入れます。本文は Arrange / Act / Assert を空行で区切ります。
- ファイルを扱うテストは `IDisposable` を実装し、`TempDirectory` で作った一時フォルダを `Dispose` で削除します。
- `JarLoader` 系のクラスは、テスト内で一時フォルダに `versions/<バージョン>/<バージョン>.jar` 形式のダミー jar（zip）を作って動かします。言語に `en_us` を指定すると、アセットインデックスがなくても動作します。
- 既知の不具合は、正しい挙動を期待するテストを `[Fact(Skip = "既知の不具合: <内容>")]` として残します。不具合を修正したら `Skip` を外してください。
- `ModelRender` の block モデルの描画・出力は `Viewport3DX`（STA スレッドと GPU）が必要なためテスト対象外です。item モデルの出力はビューポートを使わないため、`Output(null!, ...)` でテストしています。
- WPF プロジェクトでは暗黙の using に `System.IO` が含まれないため、必要に応じて明示的に `using` してください。

### テスト用ヘルパー（`MCLibsTests/Helpers/`）

| クラス | 役割 |
|---|---|
| `TempDirectory` | 一時ディレクトリの作成・削除、配下へのファイル作成 |
| `TestZip` | ダミー jar / リソースパック（zip）の作成。テキスト・単色 PNG・ディレクトリエントリを追加できる |
| `TestMinecraft` | `.minecraft` 構成（`versions/` の jar・バージョン JSON、`assets/indexes`、`assets/objects`）の作成 |
| `TestJson` | JSON 文字列を `object`（`JsonElement`）に変換する。モデル系クラスのコンストラクタ引数の作成に使う |

## 補足

- `MCModelRenderer/Resources/colorset.json` は埋め込みリソース（`MCModelRenderer.Resources.colorset.json`）としてビルドされます。

## 利用側からの参照

利用側ツール（例: MCItemViewer）は、このリポジトリを `submodules/MCLibs` にサブモジュールとして配置し、csproj から各プロジェクトを `ProjectReference` で参照します。このリポジトリでコミットした後、利用側ではサブモジュール参照の更新を別途コミットする必要があります。

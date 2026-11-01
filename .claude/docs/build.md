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
- テストファイルは、対象クラスの名前空間に合わせて `MCLibsTests/<名前空間のフォルダ>/<クラス名>Tests.cs` に配置します（例: `MCLibsTests/MCModelRenderer/Utils/BlockModelLoarderTests.cs`）。テストクラスの名前空間は `MCLibsTests.<対象の名前空間>` です。
- `JarLoader` 系のクラスは、テスト内で一時フォルダに `versions/<バージョン>/<バージョン>.jar` 形式のダミー jar（zip）を作って動かします。言語に `en_us` を指定すると、アセットインデックスがなくても動作します。
- WPF プロジェクトでは暗黙の using に `System.IO` が含まれないため、必要に応じて明示的に `using` してください。

## 補足

- `MCModelRenderer/Resources/colorset.json` は埋め込みリソース（`MCModelRenderer.Resources.colorset.json`）としてビルドされます。

## 利用側からの参照

利用側ツール（例: MCItemViewer）は、このリポジトリを `submodules/MCLibs` にサブモジュールとして配置し、csproj から各プロジェクトを `ProjectReference` で参照します。このリポジトリでコミットした後、利用側ではサブモジュール参照の更新を別途コミットする必要があります。

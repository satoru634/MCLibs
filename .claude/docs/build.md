# ビルド

```bash
dotnet build MCLibs.sln
```

## プロジェクト設定

両プロジェクトとも以下の共通設定です。

- `TargetFramework`: `net8.0-windows10.0.17763.0`
- `PlatformTarget`: `x64`
- `UseWPF`: `true`、`Nullable`: `enable`、`ImplicitUsings`: `enable`

## 依存パッケージ

| プロジェクト | パッケージ |
|---|---|
| MCToolsCommonLib | CsvHelper、MiNET.fnbt（NBT）、Nett（TOML）、OpenCvSharp4（＋runtime.win、WpfExtensions）、System.Text.Json |
| MCModelRenderer | HelixToolkit.SharpDX.Core.Wpf、OpenCvSharp4（＋runtime.win、WpfExtensions）、System.Text.Json |

MCModelRenderer は MCToolsCommonLib を `ProjectReference` で参照しています。

## 補足

- テストはまだ設定されていません。
- `MCModelRenderer/Resources/colorset.json` は埋め込みリソース（`MCModelRenderer.Resources.colorset.json`）としてビルドされます。

## 利用側からの参照

利用側ツール（例: MCItemViewer）は、このリポジトリを `submodules/MCLibs` にサブモジュールとして配置し、csproj から各プロジェクトを `ProjectReference` で参照します。このリポジトリでコミットした後、利用側ではサブモジュール参照の更新を別途コミットする必要があります。

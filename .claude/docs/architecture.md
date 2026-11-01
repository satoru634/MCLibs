# アーキテクチャ

## プロジェクト構成

```
MCLibs.sln
├── MCToolsCommonLib   汎用ヘルパー（他のどのプロジェクトにも依存しない）
├── MCModelRenderer    モデル読み込み・3D描画（MCToolsCommonLib に依存）
└── MCLibsTests        xUnit のユニットテスト（両ライブラリに依存）
```

### MCToolsCommonLib

| フォルダ | 主なクラス | 役割 |
|---|---|---|
| `BaseData/` | `Point3D`、`PointXZ`、`PointRange`、`EntityInfoBase`、`InventoryItem` | 座標・範囲判定・エンティティ/インベントリのデータ型 |
| `Common/` | `CommonLib` | 静的ヘルパー。JSON/CSV 読み書き、バージョン比較（`CompareVersion`）、バージョン検出（`GetMinecraftProgramFiles` / `GetMinecraftProgramFilePath`）、ワールド/ディメンション一覧、NBT 文字列化など |
| | `ImageProcessing` | OpenCV の `Mat<Vec4b>` に対する重ね合わせ・色の乗算・マスク生成 |
| | `Setting<T>` | JSON 設定ファイルの読み書き |
| | `ItemBaseViewModel<T>`、`NotifyInfo`、`ProgressInfo` | UI バインド用の小さなデータ型 |
| `Utils/` | `JarLoader` | クライアント jar（zip）から JSON・画像・ファイル一覧を読み込む基底クラス |
| | `MCRegion` | リージョンファイル（.mca）からチャンクの NBT を取得 |
| | `ModCollector`、`ResourcePackInfo` | MOD jar・リソースパックの収集と検索 |
| | `ConvertJsonValue` | JSON 値（`object`）を int/double/string に変換 |

### MCModelRenderer

| フォルダ | 主なクラス | 役割 |
|---|---|---|
| `MCModels/` | `BlockModel`、`ModelElements`、`ModelFace`、`ModelRotation`、`ModelDisplay` | モデル JSON（elements / faces / rotation / display）のデータ型 |
| `Utils/` | `BlockModelLoarder` | `JarLoader` を継承。アイテム一覧の作成、モデル＋テクスチャの読み込み |
| | `ItemModelInfo` | `JarLoader` を継承。1.21.4 以降の `assets/<ns>/items/*.json` からモデルパスを解決 |
| | `ModelRender` | HelixToolkit SharpDX でシーンを構築し `Viewport3DX` に描画、PNG 出力 |
| | `ColorConverter`、`CommonCalc`、`ItemInfo` | 色変換、座標変換・回転行列、アイテム種別と表示名 |
| `Resources/` | `colorset.json` | テクスチャに乗算する固定色（草・葉・スポーンエッグなど） |

## モデル読み込みの流れ（`BlockModelLoarder`）

1. **アイテム一覧の作成**（コンストラクタ → `GetItemIDs`）
   - `assets/<ns>/lang/en_us.json` の `block.<ns>.*` / `item.<ns>.*` キーからアイテムID を抽出する。
   - 表示名は指定言語の lang ファイルから取得する。バニラ（`minecraft`）の場合、jar ではなく `.minecraft/assets/objects/` 内のファイルをアセットインデックスのハッシュから探して読む（`JarLoader.GenerateResourcePath`）。
   - 1.21.4 以降は `assets/<ns>/items/` に存在するものだけに絞り込む。
2. **モデルの読み込み**（`LoadModel(itemName)`）
   - 1.21.3 以前: `assets/<ns>/models/item/<名前>.json` を読む。
   - 1.21.4 以降: `ItemModelInfo.GetModelPath` で items 定義の `type` を辿ってモデルパスを決める（`select` は `fallback`、`condition` は `on_false`、`range_dispatch` は閾値 0 のエントリを採用）。
   - `LoadModelRecursive` が `parent` を再帰的に辿り、`elements` / `textures` / `display` を `BlockModel` に追加する。`elements` が空なら `ModelType` は `item`、あれば `block`。
3. **テクスチャの読み込み**（`LoadTextures`）
   - `#` で始まる参照はスキップし、`assets/<ns>/textures/*.png` を読む。
   - `colorset.json` に色があれば乗算する。
   - `layer0`、`layer1`… は `layer` キーに順に重ね合わせる。

## 描画と出力の流れ（`ModelRender`）

- `Render(viewport, backgroundColor)`: 背景色とライト3つを設定し、`Generate3DModel` で面ごとの `MeshGeometryModel3D` を追加する。
  - item モデルは南面1枚だけ生成する。block モデルは要素ごとに6面生成する。
  - 回転・スケールは `display.gui` があればそれを使い、なければ既定値（block は回転 30/135/0、スケール 0.625）を使う。
  - テクスチャは出力サイズ以下の最大の2の累乗を 16 で割った倍率で、最近傍補間により拡大する。
- `Output(viewport, basePath)`: `<名前空間>_<ID>_x<幅>.png` を出力する。
  - item モデルはビューポートを使わず、生成したテクスチャをそのまま `Cv2.ImWrite` で書き出す。
  - block モデルは `viewport.RenderBitmap()` でキャプチャして PNG エンコードする。

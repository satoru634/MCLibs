using fNbt;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Utils
{
    /// <summary>
    /// Minecraftのリージョンファイルを操作するためのクラス
    /// </summary>
    public class MCRegion : IDisposable
    {
        /// <summary>
        /// MCRegionのデータを格納するメモリストリーム
        /// </summary>
        private MemoryStream _regionData;

        /// <summary>
        /// 破棄済みフラグ
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MCRegion()
        {
            _regionData = new MemoryStream();
        }

        /// <summary>
        /// Disposeメソッド
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == false)
            {
                _disposed = true;
                _regionData.Dispose();

                if (disposing == true)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Disposeメソッド
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~MCRegion()
        {
            Dispose(false);
        }

        /// <summary>
        /// 指定されたパスのリージョンファイルを読み込む
        /// </summary>
        /// <param name="regionPath">リージョンファイルパス</param>
        public void ReadFile(string regionPath)
        {
            using (FileStream fs = new FileStream(regionPath, FileMode.Open))
            {
                fs.CopyTo(_regionData);
                _regionData.Seek(0, SeekOrigin.Begin);
            }

            return;
        }

        /// <summary>
        /// 指定されたチャンクのヘッダーオフセットを取得する
        /// </summary>
        /// <param name="chunkX">チャンクX座標</param>
        /// <param name="chunkZ">チャンクZ座標</param>
        /// <returns></returns>
        public int HeaderOffset(int chunkX, int chunkZ)
        {
            return 4 * (chunkX % 32 + chunkZ % 32 * 32);
        }

        /// <summary>
        /// 指定されたチャンクの位置を取得する
        /// </summary>
        /// <param name="chunkX">チャンクX座標</param>
        /// <param name="chunkZ">チャンクZ座標</param>
        /// <returns>チャンクのオフセット、セクター数</returns>
        public (int offset, int sectors) GetChunkLocation(int chunkX, int chunkZ)
        {
            // チャンクのオフセットを取得
            int bufOffset = HeaderOffset(chunkX, chunkZ);
            int chunkOffset = GetChunkOffset(bufOffset);

            if (chunkOffset == -1)
            {
                return (0, 0);
            }

            // セクター数を取得
            _regionData.Seek(bufOffset + 3, SeekOrigin.Begin);
            int sectors = _regionData.ReadByte();
            return (chunkOffset, sectors);
        }

        /// <summary>
        /// 指定されたチャンクのデータをNBTフォーマットで取得する
        /// </summary>
        /// <param name="chunkX">チャンクX座標</param>
        /// <param name="chunkZ">チャンクZ座標</param>
        /// <returns>チャンクデータ(NBTフォーマット)</returns>
        public NbtTag? GetChunkData(int chunkX, int chunkZ)
        {
            // チャンクの位置を取得
            (int offset, int sectors) location = GetChunkLocation(chunkX, chunkZ);
            if (location == (0, 0))
            {
                return null;
            }

            // チャンクのオフセットを計算
            int offset = location.offset * 4096;

            // チャンクのデータがリージョンファイルの範囲内にあるか確認
            int length = GetDataLength(offset);
            if (!CheckCompression(offset + 4))
            {
                return null;
            }

            _regionData.Seek(0, SeekOrigin.Begin);
            NbtFile nbt = new NbtFile();

            // チャンクのデータを読み込む
            try
            {
                nbt.LoadFromBuffer(_regionData.GetBuffer(), offset + 5, length - 1, NbtCompression.ZLib, null);
            }
            catch
            {
                return null;
            }

            return nbt.RootTag;
        }

        /// <summary>
        /// 指定されたオフセットからチャンクのオフセットを取得する
        /// </summary>
        /// <param name="offset">オフセット</param>
        /// <returns>チャンクのオフセット</returns>
        private int GetChunkOffset(int offset)
        {
            // チャンクのオフセットを取得
            _regionData.Seek(offset, SeekOrigin.Begin);
            List<byte> temp = _regionData.GetBuffer().Skip((int)_regionData.Position).Take(3).ToList();
            temp.Insert(0, 0);
            byte[] chunkOffset = temp.ToArray();

            // チャンクのオフセットが4バイトでない場合はエラー
            if (chunkOffset.Length != 4)
            {
                return -1;
            }

            // リトルエンディアンの場合はバイトオーダーを逆にする
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(chunkOffset);
            }

            // チャンクのオフセットをint型に変換
            return BitConverter.ToInt32(chunkOffset, 0);
        }

        /// <summary>
        /// 指定されたオフセットからデータ長を取得する
        /// </summary>
        /// <param name="offset">オフセット</param>
        /// <returns>データ長</returns>
        private int GetDataLength(int offset)
        {
            // データ長を取得
            _regionData.Seek(offset, SeekOrigin.Begin);
            byte[] temp = _regionData.GetBuffer().Skip((int)_regionData.Position).Take(4).ToArray();

            // リトルエンディアンの場合はバイトオーダーを逆にする
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(temp);
            }

            // データ長をint型に変換
            return BitConverter.ToInt32(temp, 0);
        }

        /// <summary>
        /// 圧縮されたデータか確認する
        /// </summary>
        /// <param name="offset">オフセット</param>
        /// <returns>圧縮されている場合はfalse、圧縮されていない場合はtrue</returns>
        private bool CheckCompression(int offset)
        {
            _regionData.Seek(offset, SeekOrigin.Begin);
            int compression = _regionData.ReadByte();
            if (compression == 1)
            {
                return false;
            }

            return true;
        }
    }
}

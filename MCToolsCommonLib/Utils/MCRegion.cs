using fNbt;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCToolsCommonLib.Util
{
    public class MCRegion
    {
        private MemoryStream RegionData = new MemoryStream();

        public MCRegion()
        {
        }

        public void ReadFile(string regionPath)
        {
            using (FileStream fs = new FileStream(regionPath, FileMode.Open))
            {
                fs.CopyTo(RegionData);
                RegionData.Seek(0, SeekOrigin.Begin);
            }

            return;
        }

        public int HeaderOffset(int chunkX, int chunkZ)
        {
            return 4 * (chunkX % 32 + chunkZ % 32 * 32);
        }

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
            RegionData.Seek(bufOffset + 3, SeekOrigin.Begin);
            int sectors = RegionData.ReadByte();
            return (chunkOffset, sectors);
        }

        public NbtTag? GetChunkData(int chunkX, int chunkZ)
        {
            (int offset, int sectors) location = GetChunkLocation(chunkX, chunkZ);
            if (location == (0, 0))
            {
                return null;
            }

            int offset = location.offset * 4096;
            int length = GetDataLength(offset);
            if (!CheckCompression(offset + 4))
            {
                return null;
            }

            RegionData.Seek(0, SeekOrigin.Begin);
            NbtFile nbt = new NbtFile();

            try
            {
                nbt.LoadFromBuffer(RegionData.GetBuffer(), offset + 5, length - 1, NbtCompression.ZLib, null);
            }
            catch
            {
                return null;
            }

            return nbt.RootTag;
        }

        private int GetChunkOffset(int offset)
        {
            RegionData.Seek(offset, SeekOrigin.Begin);
            List<byte> temp = RegionData.GetBuffer().Skip((int)RegionData.Position).Take(3).ToList();
            temp.Insert(0, 0);
            byte[] chunkOffset = temp.ToArray();

            if (chunkOffset.Length != 4)
            {
                return -1;
            }

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(chunkOffset);
            }

            return BitConverter.ToInt32(chunkOffset, 0);
        }

        private int GetDataLength(int offset)
        {
            RegionData.Seek(offset, SeekOrigin.Begin);
            byte[] temp = RegionData.GetBuffer().Skip((int)RegionData.Position).Take(4).ToArray();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(temp);
            }

            return BitConverter.ToInt32(temp, 0);
        }

        private bool CheckCompression(int offset)
        {
            RegionData.Seek(offset, SeekOrigin.Begin);
            int compression = RegionData.ReadByte();
            if (compression == 1)
            {
                return false;
            }

            return true;
        }
    }
}

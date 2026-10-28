using CsvHelper.Configuration;

namespace MCALib.Data
{
    /// <summary>
    /// MCAVillagerInfoのCSVマッピングを定義するクラス
    /// </summary>
    public class MCAVillagerInfoMap : ClassMap<MCAVillagerInfo>
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MCAVillagerInfoMap(string playerUUID)
        {
            Map(m => m.Name).Index(0).Name("villager_name");
            Map(m => m.Gender).Index(1).Name("gender");
            Map(m => m.UUID).Index(2).Name("UUID");
            Map(m => m.Hearts[playerUUID]).Index(3).Name("hearts");
            Map(m => m.Mood).Index(4).Name("mood");
            Map(m => m.Age).Index(5).Name("age_state");
            Map(m => m.HasBaby).Index(6).Name("has_baby");
            Map(m => m.Profession).Index(7).Name("profession");
            Map(m => m.Traded).Index(8).Name("traded");
            Map(m => m.ResidenceVillageName).Index(9).Name("residence_village_name");
            Map(m => m.CoordinatePos.X).Index(10).Name("position(coordinate_x)");
            Map(m => m.CoordinatePos.Y).Index(11).Name("position(coordinate_y)");
            Map(m => m.CoordinatePos.Z).Index(12).Name("position(coordinate_z)");
            Map(m => m.RegionPos.X).Index(13).Name("position(region_x)");
            Map(m => m.RegionPos.Z).Index(14).Name("position(region_z)");
            Map(m => m.ChunkPos.X).Index(15).Name("position(chunk_x)");
            Map(m => m.ChunkPos.Z).Index(16).Name("position(chunk_z)");
            Map(m => m.WorldChunkPos.X).Index(17).Name("position(world_chunk_x)");
            Map(m => m.WorldChunkPos.Z).Index(18).Name("position(world_chunk_z)");
            Map(m => m.Clothes).Index(19).Name("clothes");
            Map(m => m.Hair).Index(20).Name("hair");
            Map(m => m.GeneEumelanin).Index(21).Name("gene_eumelanin");
            Map(m => m.GeneFace).Index(22).Name("gene_face");
            Map(m => m.GeneHemoglobin).Index(23).Name("gene_hemoglobin");
            Map(m => m.GeneMelanin).Index(24).Name("gene_melanin");
            Map(m => m.GenePheomelanin).Index(25).Name("gene_pheomelanin");
            Map(m => m.GeneSkin).Index(26).Name("gene_skin");
            Map(m => m.HairColorBlue).Index(27).Name("hair_color_blue");
            Map(m => m.HairColorGreen).Index(28).Name("hair_color_green");
            Map(m => m.HairColorRed).Index(29).Name("hair_color_red");
            Map(m => m.Albinism).Index(30).Name("albinism");
            Map(m => m.Hetero).Index(31).Name("hetero");
        }
    }
}

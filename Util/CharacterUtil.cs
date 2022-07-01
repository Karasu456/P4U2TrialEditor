namespace P4U2TrialEditor.Util
{
    internal static class CharacterUtil
    {
        public enum EChara
        {
            ADACHI,
            AIGIS,
            AKIHIKO,
            CHIE,
            ELIZABETH,
            JUNPEI,
            KANJI,
            KEN,
            LABRYS,
            MARGARET,
            MARIE,
            MINAZUKI,
            MITSURU,
            NAOTO,
            NARUKAMI,
            RISE,
            S_LABRYS,
            SHO,
            TEDDIE,
            YOSUKE,
            YUKARI,
            YUKIKO,

            COMMON
        };

        /// <summary>
        /// Convert character enum to resource ID
        /// </summary>
        /// <param name="chara">Character</param>
        /// <returns>ID string</returns>
        public static string GetCharaResID(EChara chara) => chara switch
        {
            EChara.ADACHI => "AD",
            EChara.AIGIS => "AG",
            EChara.AKIHIKO => "AK",
            EChara.CHIE => "CE",
            EChara.ELIZABETH => "EL",
            EChara.JUNPEI => "JU",
            EChara.KANJI => "KA",
            EChara.KEN => "AM",
            EChara.LABRYS => "LA",
            EChara.MARGARET => "NX",
            EChara.MARIE => "MR",
            EChara.MINAZUKI => "NB",
            EChara.MITSURU => "MI",
            EChara.NAOTO => "NA",
            EChara.NARUKAMI => "BC",
            EChara.RISE => "RI",
            EChara.S_LABRYS => "LS",
            EChara.SHO => "NO",
            EChara.TEDDIE => "KU",
            EChara.YOSUKE => "YO",
            EChara.YUKARI => "YK",
            EChara.YUKIKO => "YU",

            _ => "XX"
        };

        /// <summary>
        /// Convert character enum to name
        /// </summary>
        /// <param name="chara">Character</param>
        /// <returns>ID string</returns>
        public static string GetCharaName(EChara chara) => chara switch
        {
            EChara.ADACHI => "Adachi",
            EChara.AIGIS => "Aigis",
            EChara.AKIHIKO => "Akihiko",
            EChara.CHIE => "Chie",
            EChara.ELIZABETH => "Elizabeth",
            EChara.JUNPEI => "Junpei",
            EChara.KANJI => "Kanji",
            EChara.KEN => "Ken",
            EChara.LABRYS => "Labrys",
            EChara.MARGARET => "Margaret",
            EChara.MARIE => "Marie",
            EChara.MINAZUKI => "Minazuki",
            EChara.MITSURU => "Mitsuru",
            EChara.NAOTO => "Naoto",
            EChara.NARUKAMI => "Narukami",
            EChara.RISE => "Rise",
            EChara.S_LABRYS => "Shadow Labrys",
            EChara.SHO => "Sho",
            EChara.TEDDIE => "Teddie",
            EChara.YOSUKE => "Yosuke",
            EChara.YUKARI => "Yukari",
            EChara.YUKIKO => "Yukiko",

            _ => "Unknown"
        };

        /// <summary>
        /// Convert character resource ID to enum value
        /// </summary>
        /// <param name="resID"></param>
        /// <returns></returns>
        public static EChara GetCharaEnum(string resID) => resID switch
        {
            "AD" => EChara.ADACHI,
            "AG" => EChara.AIGIS,
            "AK" => EChara.AKIHIKO,
            "CE" => EChara.CHIE,
            "EL" => EChara.ELIZABETH,
            "JU" => EChara.JUNPEI,
            "KA" => EChara.KANJI,
            "AM" => EChara.KEN,
            "LA" => EChara.LABRYS,
            "NX" => EChara.MARGARET,
            "MR" => EChara.MARIE,
            "NB" => EChara.MINAZUKI,
            "MI" => EChara.MITSURU,
            "NA" => EChara.NAOTO,
            "BC" => EChara.NARUKAMI,
            "RI" => EChara.RISE,
            "LS" => EChara.S_LABRYS,
            "NO" => EChara.SHO,
            "KU" => EChara.TEDDIE,
            "YO" => EChara.YOSUKE,
            "YK" => EChara.YUKARI,
            "YU" => EChara.YUKIKO,

            _ => EChara.COMMON
        };
    }
}
namespace Delta.Domain.Generator.shared
{
    public enum JreDeflateParameters
    {
        LEVEL1_STRATEGY0_NOWRAP = 1, LEVEL2_STRATEGY0_NOWRAP = 2, LEVEL3_STRATEGY0_NOWRAP = 3,
        LEVEL4_STRATEGY0_NOWRAP = 4, LEVEL5_STRATEGY0_NOWRAP = 5, LEVEL6_STRATEGY0_NOWRAP = 6,
        LEVEL7_STRATEGY0_NOWRAP = 7, LEVEL8_STRATEGY0_NOWRAP = 8, LEVEL9_STRATEGY0_NOWRAP = 9,
        LEVEL1_STRATEGY1_NOWRAP = 10, LEVEL2_STRATEGY1_NOWRAP = 11, LEVEL3_STRATEGY1_NOWRAP = 12,
        LEVEL4_STRATEGY1_NOWRAP = 13, LEVEL5_STRATEGY1_NOWRAP = 14, LEVEL6_STRATEGY1_NOWRAP = 15,
        LEVEL7_STRATEGY1_NOWRAP = 16, LEVEL8_STRATEGY1_NOWRAP = 17, LEVEL9_STRATEGY1_NOWRAP = 18,
        LEVEL1_STRATEGY2_NOWRAP = 19, LEVEL2_STRATEGY2_NOWRAP = 20, LEVEL3_STRATEGY2_NOWRAP = 21,
        LEVEL4_STRATEGY2_NOWRAP = 22, LEVEL5_STRATEGY2_NOWRAP = 23, LEVEL6_STRATEGY2_NOWRAP = 24,
        LEVEL7_STRATEGY2_NOWRAP = 25, LEVEL8_STRATEGY2_NOWRAP = 26, LEVEL9_STRATEGY2_NOWRAP = 27,
        LEVEL1_STRATEGY0_WRAP = 28, LEVEL2_STRATEGY0_WRAP = 29, LEVEL3_STRATEGY0_WRAP = 30,
        LEVEL4_STRATEGY0_WRAP = 31, LEVEL5_STRATEGY0_WRAP = 32, LEVEL6_STRATEGY0_WRAP = 33,
        LEVEL7_STRATEGY0_WRAP = 34, LEVEL8_STRATEGY0_WRAP = 35, LEVEL9_STRATEGY0_WRAP = 36,
        LEVEL1_STRATEGY1_WRAP = 37, LEVEL2_STRATEGY1_WRAP = 38, LEVEL3_STRATEGY1_WRAP = 39,
        LEVEL4_STRATEGY1_WRAP = 40, LEVEL5_STRATEGY1_WRAP = 41, LEVEL6_STRATEGY1_WRAP = 42,
        LEVEL7_STRATEGY1_WRAP = 43, LEVEL8_STRATEGY1_WRAP = 44, LEVEL9_STRATEGY1_WRAP = 45,
        LEVEL1_STRATEGY2_WRAP = 46, LEVEL2_STRATEGY2_WRAP = 47, LEVEL3_STRATEGY2_WRAP = 48,
        LEVEL4_STRATEGY2_WRAP = 49, LEVEL5_STRATEGY2_WRAP = 50, LEVEL6_STRATEGY2_WRAP = 51,
        LEVEL7_STRATEGY2_WRAP = 52, LEVEL8_STRATEGY2_WRAP = 53, LEVEL9_STRATEGY2_WRAP = 54
    }

    public class JreDeflateParametersConverter
    {
        public static JreDeflateParameters Of(int level, int strategy, bool nowrap)
        {
            if (level < 1 || level > 9 || strategy < 0 || strategy > 2)
            {
                throw new ArgumentException("Only levels 1-9 and strategies 0-2 are valid.");
            }

            int id = (level * 100) + (strategy * 10) + (nowrap ? 1 : 0);

            return id switch
            {
                100 => JreDeflateParameters.LEVEL1_STRATEGY0_WRAP,
                200 => JreDeflateParameters.LEVEL2_STRATEGY0_WRAP,
                300 => JreDeflateParameters.LEVEL3_STRATEGY0_WRAP,
                400 => JreDeflateParameters.LEVEL4_STRATEGY0_WRAP,
                500 => JreDeflateParameters.LEVEL5_STRATEGY0_WRAP,
                600 => JreDeflateParameters.LEVEL6_STRATEGY0_WRAP,
                700 => JreDeflateParameters.LEVEL7_STRATEGY0_WRAP,
                800 => JreDeflateParameters.LEVEL8_STRATEGY0_WRAP,
                900 => JreDeflateParameters.LEVEL9_STRATEGY0_WRAP,
                110 => JreDeflateParameters.LEVEL1_STRATEGY1_WRAP,
                210 => JreDeflateParameters.LEVEL2_STRATEGY1_WRAP,
                310 => JreDeflateParameters.LEVEL3_STRATEGY1_WRAP,
                410 => JreDeflateParameters.LEVEL4_STRATEGY1_WRAP,
                510 => JreDeflateParameters.LEVEL5_STRATEGY1_WRAP,
                610 => JreDeflateParameters.LEVEL6_STRATEGY1_WRAP,
                710 => JreDeflateParameters.LEVEL7_STRATEGY1_WRAP,
                810 => JreDeflateParameters.LEVEL8_STRATEGY1_WRAP,
                910 => JreDeflateParameters.LEVEL9_STRATEGY1_WRAP,
                120 => JreDeflateParameters.LEVEL1_STRATEGY2_WRAP,
                220 => JreDeflateParameters.LEVEL2_STRATEGY2_WRAP,
                320 => JreDeflateParameters.LEVEL3_STRATEGY2_WRAP,
                420 => JreDeflateParameters.LEVEL4_STRATEGY2_WRAP,
                520 => JreDeflateParameters.LEVEL5_STRATEGY2_WRAP,
                620 => JreDeflateParameters.LEVEL6_STRATEGY2_WRAP,
                720 => JreDeflateParameters.LEVEL7_STRATEGY2_WRAP,
                820 => JreDeflateParameters.LEVEL8_STRATEGY2_WRAP,
                920 => JreDeflateParameters.LEVEL9_STRATEGY2_WRAP,
                101 => JreDeflateParameters.LEVEL1_STRATEGY0_NOWRAP,
                201 => JreDeflateParameters.LEVEL2_STRATEGY0_NOWRAP,
                301 => JreDeflateParameters.LEVEL3_STRATEGY0_NOWRAP,
                401 => JreDeflateParameters.LEVEL4_STRATEGY0_NOWRAP,
                501 => JreDeflateParameters.LEVEL5_STRATEGY0_NOWRAP,
                601 => JreDeflateParameters.LEVEL6_STRATEGY0_NOWRAP,
                701 => JreDeflateParameters.LEVEL7_STRATEGY0_NOWRAP,
                801 => JreDeflateParameters.LEVEL8_STRATEGY0_NOWRAP,
                901 => JreDeflateParameters.LEVEL9_STRATEGY0_NOWRAP,
                111 => JreDeflateParameters.LEVEL1_STRATEGY1_NOWRAP,
                211 => JreDeflateParameters.LEVEL2_STRATEGY1_NOWRAP,
                311 => JreDeflateParameters.LEVEL3_STRATEGY1_NOWRAP,
                411 => JreDeflateParameters.LEVEL4_STRATEGY1_NOWRAP,
                511 => JreDeflateParameters.LEVEL5_STRATEGY1_NOWRAP,
                611 => JreDeflateParameters.LEVEL6_STRATEGY1_NOWRAP,
                711 => JreDeflateParameters.LEVEL7_STRATEGY1_NOWRAP,
                811 => JreDeflateParameters.LEVEL8_STRATEGY1_NOWRAP,
                911 => JreDeflateParameters.LEVEL9_STRATEGY1_NOWRAP,
                121 => JreDeflateParameters.LEVEL1_STRATEGY2_NOWRAP,
                221 => JreDeflateParameters.LEVEL2_STRATEGY2_NOWRAP,
                321 => JreDeflateParameters.LEVEL3_STRATEGY2_NOWRAP,
                421 => JreDeflateParameters.LEVEL4_STRATEGY2_NOWRAP,
                521 => JreDeflateParameters.LEVEL5_STRATEGY2_NOWRAP,
                621 => JreDeflateParameters.LEVEL6_STRATEGY2_NOWRAP,
                721 => JreDeflateParameters.LEVEL7_STRATEGY2_NOWRAP,
                821 => JreDeflateParameters.LEVEL8_STRATEGY2_NOWRAP,
                921 => JreDeflateParameters.LEVEL9_STRATEGY2_NOWRAP,
                _ => throw new ArgumentException("No such parameters")
            };
        }

        public static JreDeflateParameters FromString(string input)
        {
            var parts = input.Split(',');
            if (parts.Length != 3) throw new FormatException("Invalid input format");

            int level = int.Parse(parts[0].Split('=')[1]);
            int strategy = int.Parse(parts[1].Split('=')[1]);
            bool nowrap = bool.Parse(parts[2].Split('=')[1]);

            return Of(level, strategy, nowrap);
        }
    }
}

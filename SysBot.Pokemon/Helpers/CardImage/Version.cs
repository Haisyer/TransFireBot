using System.Collections.Generic;

namespace SysBot.Pokemon
{
    public class Version
    {
        private Dictionary<string, string> nameMapping = new Dictionary<string, string>
    {
        { "Sapphire", "蓝宝石" },
        { "Ruby", "红宝石" },
        { "Emerald", "绿宝石" },
        { "FireRed", "火红" },
        { "LeafGreen", "叶绿" },
        { "HeartGold", "心金" },
        { "SoulSilver", "魂银" },
        { "Diamond", "钻石" },
        { "Pearl", "珍珠" },
        { "Platinum", "白金" },
        { "Colosseum/XD", "竞技场/XD" },
        { "White", "白" },
        { "Black", "黑" },
        { "White 2", "白 2" },
        { "Black 2", "黑 2" },
        { "X", "X" },
        { "Y", "Y" },
        { "Alpha Sapphire", "始源蓝宝石" },
        { "Omega Ruby", "终极红宝石" },
        { "Sun", "太阳" },
        { "Moon", "月亮" },
        { "Ultra Sun", "究极之日" },
        { "Ultra Moon", "究极之月" },
        { "GO", "GO" },
        { "Red", "红" },
        { "Blue [INT]/Green [JP]", "蓝 [国际]/绿 [日]" },
        { "Blue [JP]", "蓝 [日]" },
        { "Yellow", "黄" },
        { "Gold", "金" },
        { "Silver", "银" },
        { "Crystal", "水晶" },
        { "Let's Go, Pikachu!", "Let's Go！皮卡丘" },
        { "Let's Go, Eevee!", "Let's Go！伊布" },
        { "Sword", "剑" },
        { "Shield", "盾" },
        { "Legends: Arceus", "宝可梦传说:阿尔宙斯" },
        { "Brilliant Diamond", "晶灿钻石" },
        { "Shining Pearl", "明亮珍珠" },
        { "Scarlet", "朱" },
        { "Violet", "紫" }
    };

        public string MapToChinese(string englishName)
        {
            if (nameMapping.ContainsKey(englishName))
            {
                return nameMapping[englishName];
            }
            else
            {
                return "未知";
            }
        }
    }
}

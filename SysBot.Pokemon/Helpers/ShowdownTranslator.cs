using PKHeX.Core;
using System.Linq;
using System.Text.RegularExpressions;

namespace SysBot.Pokemon
{
    public class ShowdownTranslator<T> where T : PKM
    {
        public static GameStrings GameStringsZh = GameInfo.GetStrings("zh");
        public static GameStrings GameStringsEn = GameInfo.GetStrings("en");
        
        public static string Chinese2Showdown(string zh)
        {
            string result = "";
            var zhclone = zh;

            // 添加宝可梦
            int candidateSpecieNo = 0;
            int candidateSpecieStringLength = 0;
            for (int i = 1; i < GameStringsZh.Species.Count; i++)
            {
                if (zh.Contains(GameStringsZh.Species[i]) && GameStringsZh.Species[i].Length > candidateSpecieStringLength)
                {
                    candidateSpecieNo = i;
                    candidateSpecieStringLength = GameStringsZh.Species[i].Length;
                }
            }

            if (candidateSpecieNo > 0)
            {
                zh = zh.Replace(GameStringsZh.Species[candidateSpecieNo], "");

                // 处理蛋宝可梦
                if (zh.Contains("的蛋"))
                {
                    result += "Egg ";
                    zh = zh.Replace("的蛋", "");

                    // Showdown 文本差异，29-尼多兰F，32-尼多朗M，876-爱管侍，
                    if (candidateSpecieNo is (ushort)Species.NidoranF) result += "(Nidoran-F)";
                    else if (candidateSpecieNo is (ushort)Species.NidoranM) result += "(Nidoran-M)";
                    else if ((candidateSpecieNo is (ushort)Species.Indeedee) && zh.Contains('母')) result += $"({GameStringsEn.Species[candidateSpecieNo]}-F)";
                    // 识别地区形态
                    else if (zh.Contains("形态"))
                    {
                        foreach (var s in FormDictionary.formDict)
                        {
                            var searchKey = s.Key.EndsWith("形态") ? s.Key : s.Key + "形态";
                            if (!zh.Contains(searchKey)) continue;
                            result += $"({GameStringsEn.Species[candidateSpecieNo]}-{s.Value})";
                            zh = zh.Replace(searchKey, "");
                            break;
                        }
                    }
                    else result += $"({GameStringsEn.Species[candidateSpecieNo]})";
                }
                // 处理非蛋宝可梦
                else
                {
                    // Showdown 文本差异，29-尼多兰F，32-尼多朗M，678-超能妙喵，876-爱管侍，902-幽尾玄鱼, 916-飘香豚
                    if (candidateSpecieNo is (ushort)Species.NidoranF) result = "Nidoran-F";
                    else if (candidateSpecieNo is (ushort)Species.NidoranM) result = "Nidoran-M";
                    else if ((candidateSpecieNo is (ushort)Species.Meowstic or (ushort)Species.Indeedee or (ushort)Species.Basculegion or (ushort)Species.Oinkologne) && zh.Contains("母"))
                        result += $"{GameStringsEn.Species[candidateSpecieNo]}-F";
                    // 识别地区形态
                    else if (zh.Contains("形态"))
                    {
                        foreach (var s in FormDictionary.formDict)
                        {
                            var searchKey = s.Key.EndsWith("形态") ? s.Key : s.Key + "形态";
                            if (!zh.Contains(searchKey)) continue;
                            result = $"{GameStringsEn.Species[candidateSpecieNo]}-{s.Value}";
                            zh = zh.Replace(searchKey, "");
                            break;
                        }
                    }
                    else result = $"{GameStringsEn.Species[candidateSpecieNo]}";
                    zh = zh.Replace(GameStringsZh.Species[candidateSpecieNo], "");
                }
            }
            else
            {
                return result;
            }

            // 添加性别
            if (zh.Contains("公"))
            {
                result += " (M)";
                zh = zh.Replace("公", "");
            }
            else if (zh.Contains("母"))
            {
                result += " (F)";
                zh = zh.Replace("母", "");
            }

            // 添加持有物
            if (zh.Contains("持有"))
            {
                for (int i = 1; i < GameStringsZh.Item.Count; i++)
                {
                    if (GameStringsZh.Item[i].Length == 0) continue;
                    if (!zh.Contains("持有" + GameStringsZh.Item[i])) continue;
                    result += $" @ {GameStringsEn.Item[i]}";
                    zh = zh.Replace("持有" + GameStringsZh.Item[i], "");
                    break;
                }
            }
            else if (zh.Contains("携带"))
            {
                for (int i = 1; i < GameStringsZh.Item.Count; i++)
                {
                    if (GameStringsZh.Item[i].Length == 0) continue;
                    if (!zh.Contains("携带" + GameStringsZh.Item[i])) continue;
                    result += $" @ {GameStringsEn.Item[i]}";
                    zh = zh.Replace("携带" + GameStringsZh.Item[i], "");
                    break;
                }
            }

            // 添加等级
            if (Regex.IsMatch(zh, "\\d{1,3}级"))
            {
                string level = Regex.Match(zh, "(\\d{1,3})级").Groups?[1]?.Value ?? "100";
                result += $"\n.CurrentLevel={level}";
                zh = Regex.Replace(zh, "\\d{1,3}级", "");
            }

            // 添加超极巨化
            if (typeof(T) == typeof(PK8) && zh.Contains("超极巨"))
            {
                result += "\nGigantamax: Yes";
                zh = zh.Replace("超极巨", "");
            }

            //添加初训家信息
            if (zh.Contains("初训家"))
            {
                result += "\n初训家";
                if (Regex.IsMatch(zh, "表ID\\d{1,6}"))
                {
                    string value = Regex.Match(zh, ("表ID(\\d{1,6})")).Groups?[1]?.Value ?? "";
                    result += $"\n.DisplayTID={value}";
                    zh = Regex.Replace(zh, "表ID\\d{1,6}", "");
                }
                if (Regex.IsMatch(zh, "里ID\\d{1,4}"))
                {
                    string value = Regex.Match(zh, ("里ID(\\d{1,4})")).Groups?[1]?.Value ?? "";
                    result += $"\n.DisplaySID={value}";
                    zh = Regex.Replace(zh, "里ID\\d{1,4}", "");
                }
                if (zh.Contains("语言"))
                {
                    if (zh.Contains("JPN")) { result += "\nLanguage: Japanese"; }
                    else if (zh.Contains("ENG")) { result += "\nLanguage: English"; }
                    else if (zh.Contains("FRE")) { result += "\nLanguage: French"; }
                    else if (zh.Contains("ITA")) { result += "\nLanguage: Italian"; }
                    else if (zh.Contains("GER")) { result += "\nLanguage: German"; }
                    else if (zh.Contains("ESP")) { result += "\nLanguage: Spanish"; }
                    else if (zh.Contains("KOR")) { result += "\nLanguage: Korean"; }
                    else if (zh.Contains("CHS")) { result += "\nLanguage: ChineseS"; }
                    else if (zh.Contains("CHT")) { result += "\nLanguage: ChineseT"; }
                }
                if (zh.Contains("名字") && zh.Contains("语言"))
                {
                    if (zh.Contains("ENG") || zh.Contains("FRE") || zh.Contains("ITA") || zh.Contains("GER") || zh.Contains("ESP"))
                    {
                        if (Regex.IsMatch(zh, "名字[A-Za-z0-9]{1,12}"))
                        {
                            string value = Regex.Match(zh, "名字([A-Za-z0-9]{1,12})").Groups?[1].Value ?? "";
                            result += $"\nOT: {value}";
                            zh = Regex.Replace(zh, "名字[A-Za-z0-9]{1,12}", "");
                        }
                    }
                    else if (zh.Contains("JPN") || zh.Contains("KOR") || zh.Contains("CHS") || zh.Contains("CHT"))
                    {
                        if (Regex.IsMatch(zh, "名字[\u0020-\uD7FF\uE000-\uFFFD]{1,6}"))
                        {
                            string value = Regex.Match(zh, "名字([\u0020-\uD7FF\uE000-\uFFFD]{1,6})").Groups?[1].Value ?? "";
                            result += $"\nOT: {value}";
                            zh = Regex.Replace(zh, "名字[\u0020-\uD7FF\uE000-\uFFFD]{1,6}", "");
                        }
                    }
                }
                if (zh.Contains("性别"))
                {
                    if (zh.Contains("男")) { result += "\nOTGender: Male"; }
                    else if (zh.Contains("女")) { result += "\nOTGender: Female"; }
                }
               

            }

            // 添加异色
            if (zh.Contains("异色"))
            {
                result += "\n.PID=$Shiny";
                zh = zh.Replace("异色", "");
            }
            else if (zh.Contains("闪光"))
            {
                result += "\n.PID=$Shiny";
                zh = zh.Replace("闪光", "");
            }
            else if (zh.Contains("星闪"))
            {
                result += "\n.PID=$Shiny";
                zh = zh.Replace("星闪", "");
            }
            else if (zh.Contains("方闪"))
            {
                result += "\n.PID=$Shiny0";
                zh = zh.Replace("方闪", "");
            }

            // 添加头目
            if (typeof(T) == typeof(PA8) && zh.Contains("头目"))
            {
                result += "\nAlpha: Yes";
                zh = zh.Replace("头目", "");
            }

            // 添加球种
            for (int i = 1; i < GameStringsZh.balllist.Length; i++)
            {
                if (GameStringsZh.balllist[i].Length == 0) continue;
                if (!zh.Contains(GameStringsZh.balllist[i])) continue;
                var ballStr = GameStringsEn.balllist[i];
                if (typeof(T) == typeof(PA8) && ballStr is "Poké Ball" or "Great Ball" or "Ultra Ball") ballStr = "LA" + ballStr;
                result += $"\nBall: {ballStr}";
                zh = zh.Replace(GameStringsZh.balllist[i], "");
                break;
            }

            // 添加特性
            for (int i = 1; i < GameStringsZh.Ability.Count; i++)
            {
                if (GameStringsZh.Ability[i].Length == 0) continue;
                if (!zh.Contains(GameStringsZh.Ability[i] + "特性")) continue;
                result += $"\nAbility: {GameStringsEn.Ability[i]}";
                zh = zh.Replace(GameStringsZh.Ability[i] + "特性", "");
                break;
            }

            // 添加性格
            for (int i = 0; i < GameStringsZh.Natures.Count; i++)
            {
                if (GameStringsZh.Natures[i].Length == 0) continue;
                if (!zh.Contains(GameStringsZh.Natures[i])) continue;
                result += $"\n{GameStringsEn.Natures[i]} Nature";
                zh = zh.Replace(GameStringsZh.Natures[i], "");
                break;
            }
            var ivstring = zhclone.Split("个体值");
            if(ivstring.Length > 1 )
            // 添加个体值
            {
                result += "\nIVs: ";
                if (Regex.IsMatch(ivstring[1], "\\d{1,2}生命"))
                {
                    string value = Regex.Match(ivstring[1], "(\\d{1,2})生命").Groups?[1]?.Value ?? "";
                    result += $"{value} HP / ";
                    ivstring[1] = Regex.Replace(ivstring[1], "\\d{1,2}生命", "");
                }
                if (Regex.IsMatch(ivstring[1], "\\d{1,2}攻击"))
                {
                    string value = Regex.Match(ivstring[1], "(\\d{1,2})攻击").Groups?[1]?.Value ?? "";
                    result += $"{value} Atk / ";
                    ivstring[1] = Regex.Replace(ivstring[1], "\\d{1,2}攻击", "");
                }
                if (Regex.IsMatch(ivstring[1], "\\d{1,2}防御"))
                {
                    string value = Regex.Match(ivstring[1], "(\\d{1,2})防御").Groups?[1]?.Value ?? "";
                    result += $"{value} Def / ";
                    ivstring[1] = Regex.Replace(ivstring[1], "\\d{1,2}防御", "");
                }
                if (Regex.IsMatch(ivstring[1], "\\d{1,2}特攻"))
                {
                    string value = Regex.Match(ivstring[1], "(\\d{1,2})特攻").Groups?[1]?.Value ?? "";
                    result += $"{value} SpA / ";
                    ivstring[1] = Regex.Replace(ivstring[1], "\\d{1,2}特攻", "");
                }
                if (Regex.IsMatch(ivstring[1], "\\d{1,2}特防"))
                {
                    string value = Regex.Match(ivstring[1], "(\\d{1,2})特防").Groups?[1]?.Value ?? "";
                    result += $"{value} SpD / ";
                    ivstring[1] = Regex.Replace(ivstring[1], "\\d{1,2}特防", "");
                }
                if (Regex.IsMatch(ivstring[1], "\\d{1,2}速度"))
                {
                    string value = Regex.Match(ivstring[1], "(\\d{1,2})速度").Groups?[1]?.Value ?? "";
                    result += $"{value} Spe";
                    ivstring[1] = Regex.Replace(ivstring[1], "\\d{1,2}速度", "");
                }
                if (result.EndsWith("/ "))
                {
                    result = result.Substring(0, result.Length - 2);
                }
            }
            else
            {
                if (zh.ToUpper().Contains("6V"))//默认
                {
                    result += "\n.IVs=31";
                    zh = zh.Replace("6V", "");
                }
                else if (zh.ToUpper().Contains("5V0攻"))
                {
                    result += "\nIVs: 0 Atk ";
                    zh = zh.Replace("5V0攻", "");
                }
                else if (zh.ToUpper().Contains("5V0速"))
                {
                    result += "\nIVs: 0 Spe ";
                    zh = zh.Replace("5V0速", "");
                }
                else if (zh.ToUpper().Contains("4V0攻0速"))
                {
                    result += "\nIVs: 0 Spe / 0 Atk";
                    zh = zh.Replace("4V0攻0速", "");
                }
                else if (zh.ToUpper().Contains("5V0A"))
                {
                    result += "\nIVs: 0 Atk ";
                    zh = zh.Replace("5V0A", "");
                }
                else if (zh.ToUpper().Contains("5V0S"))
                {
                    result += "\nIVs: 0 Spe ";
                    zh = zh.Replace("5V0S", "");
                }
                else if (zh.ToUpper().Contains("4V0A0S"))
                {
                    result += "\nIVs: 0 Spe / 0 Atk";
                    zh = zh.Replace("4V0A0S", "");
                }
            }

            // 添加努力值
            var evstring = zhclone.Split("努力值");
            if (evstring.Length > 1)
            {
                var ExIVstring=evstring[1].Split("个体值");

                result += "\nEVs: ";
                if (ExIVstring.Length > 1)
                {
                    if (Regex.IsMatch(ExIVstring[0], "\\d{1,3}生命"))
                    {
                        string value = Regex.Match(ExIVstring[0], "(\\d{1,3})生命").Groups?[1]?.Value ?? "";
                        result += $"{value} HP / ";
                        ExIVstring[0] = Regex.Replace(ExIVstring[0], "\\d{1,3}生命", "");
                    }
                    if (Regex.IsMatch(ExIVstring[0], "\\d{1,3}攻击"))
                    {
                        string value = Regex.Match(ExIVstring[0], "(\\d{1,3})攻击").Groups?[1]?.Value ?? "";
                        result += $"{value} Atk / ";
                        ExIVstring[0] = Regex.Replace(ExIVstring[0], "\\d{1,3}攻击", "");
                    }
                    if (Regex.IsMatch(ExIVstring[0], "\\d{1,3}防御"))
                    {
                        string value = Regex.Match(ExIVstring[0], "(\\d{1,3})防御").Groups?[1]?.Value ?? "";
                        result += $"{value} Def / ";
                        ExIVstring[0] = Regex.Replace(ExIVstring[0], "\\d{1,3}防御", "");
                    }
                    if (Regex.IsMatch(ExIVstring[0], "\\d{1,3}特攻"))
                    {
                        string value = Regex.Match(ExIVstring[0], "(\\d{1,3})特攻").Groups?[1]?.Value ?? "";
                        result += $"{value} SpA / ";
                        ExIVstring[0] = Regex.Replace(ExIVstring[0], "\\d{1,3}特攻", "");
                    }
                    if (Regex.IsMatch(ExIVstring[0], "\\d{1,3}特防"))
                    {
                        string value = Regex.Match(ExIVstring[0], "(\\d{1,3})特防").Groups?[1]?.Value ?? "";
                        result += $"{value} SpD / ";
                        ExIVstring[0] = Regex.Replace(ExIVstring[0], "\\d{1,3}特防", "");
                    }
                    if (Regex.IsMatch(ExIVstring[0], "\\d{1,3}速度"))
                    {
                        string value = Regex.Match(ExIVstring[0], "(\\d{1,3})速度").Groups?[1]?.Value ?? "";
                        result += $"{value} Spe";
                        ExIVstring[0] = Regex.Replace(ExIVstring[0], "\\d{1,3}速度", "");
                    }
                    if (result.EndsWith("/ "))
                    {
                        result = result.Substring(0, result.Length - 2);
                    }
                }
                else
                {
                    if (Regex.IsMatch(evstring[1], "\\d{1,3}生命"))
                    {
                        string value = Regex.Match(evstring[1], "(\\d{1,3})生命").Groups?[1]?.Value ?? "";
                        result += $"{value} HP / ";
                        evstring[1] = Regex.Replace(evstring[1], "\\d{1,3}生命", "");
                    }
                    if (Regex.IsMatch(evstring[1], "\\d{1,3}攻击"))
                    {
                        string value = Regex.Match(evstring[1], "(\\d{1,3})攻击").Groups?[1]?.Value ?? "";
                        result += $"{value} Atk / ";
                        evstring[1] = Regex.Replace(evstring[1], "\\d{1,3}攻击", "");
                    }
                    if (Regex.IsMatch(evstring[1], "\\d{1,3}防御"))
                    {
                        string value = Regex.Match(evstring[1], "(\\d{1,3})防御").Groups?[1]?.Value ?? "";
                        result += $"{value} Def / ";
                        evstring[1] = Regex.Replace(evstring[1], "\\d{1,3}防御", "");
                    }
                    if (Regex.IsMatch(evstring[1], "\\d{1,3}特攻"))
                    {
                        string value = Regex.Match(evstring[1], "(\\d{1,3})特攻").Groups?[1]?.Value ?? "";
                        result += $"{value} SpA / ";
                        evstring[1] = Regex.Replace(evstring[1], "\\d{1,3}特攻", "");
                    }
                    if (Regex.IsMatch(evstring[1], "\\d{1,3}特防"))
                    {
                        string value = Regex.Match(evstring[1], "(\\d{1,3})特防").Groups?[1]?.Value ?? "";
                        result += $"{value} SpD / ";
                        evstring[1] = Regex.Replace(evstring[1], "\\d{1,3}特防", "");
                    }
                    if (Regex.IsMatch(evstring[1], "\\d{1,3}速度"))
                    {
                        string value = Regex.Match(evstring[1], "(\\d{1,3})速度").Groups?[1]?.Value ?? "";
                        result += $"{value} Spe";
                        evstring[1] = Regex.Replace(evstring[1], "\\d{1,3}速度", "");
                    }
                    if (result.EndsWith("/ "))
                    {
                        result = result.Substring(0, result.Length - 2);
                    }
                }
            }

            // 添加太晶属性
            if (typeof(T) == typeof(PK9))
            {
                for (int i = 0; i < GameStringsZh.Types.Count; i++)
                {
                    if (GameStringsZh.Types[i].Length == 0) continue;
                    if (!zh.Contains("太晶" + GameStringsZh.Types[i])) continue;
                    result += $"\nTera Type: {GameStringsEn.Types[i]}";
                    zh = zh.Replace("太晶" + GameStringsZh.Types[i], "");
                    break;
                }
            }

            //体型大小
            if (Regex.IsMatch(zh, "\\d{1,3}大小"))
            {
                string value = Regex.Match(zh, "(\\d{1,3})大小").Groups?[1]?.Value ?? "";
                result += $"\n.Scale={value}";
                zh = Regex.Replace(zh, "\\d{1,3}大小", "");
            }
            else if (zh.Contains("最大体型"))
            {
                result += "\n.Scale=255";
                zh = zh.Replace("最大体型", "");
            }
            else if (zh.Contains("最小体型"))
            {
                result += "\n.Scale=0";
                zh = zh.Replace("最小体型", "");
            }
            else if (zh.Contains("体型XXXL"))
            {
                result += "\n.Scale=255";
                zh = zh.Replace("体型XXXL", "");
            }
            else if (zh.Contains("体型XXXS"))
            {
                result += "\n.Scale=0";
                zh = zh.Replace("体型XXXS", "");
            }

            //补充后天获得的全奖章
            if (typeof(T) == typeof(PK9) && zh.Contains("全奖章"))
            {
                result += "\n.Ribbons=$suggestAll\n.RibbonMarkPartner=True\n.RibbonMarkGourmand=True";
                zh = zh.Replace("全奖章", "");
            }
            //为野生宝可梦添加证章
            if (typeof(T) == typeof(PK9))
            {
                if (zh.Contains("最强之证")) { result += "\n.RibbonMarkMightiest=True"; }
                else if (zh.Contains("未知之证")) { result += "\n.RibbonMarkRare=True"; }
                else if (zh.Contains("命运之证")) { result += "\n.RibbonMarkDestiny=True"; }
                else if (zh.Contains("暴雪之证")) { result += "\n.RibbonMarkBlizzard=True"; }
                else if (zh.Contains("阴云之证")) { result += "\n.RibbonMarkCloudy=True"; }
                else if (zh.Contains("正午之证")) { result += "\n.RibbonMarkLunchtime=True"; }
                else if (zh.Contains("浓雾之证")) { result += "\n.RibbonMarkMisty=True"; }
                else if (zh.Contains("降雨之证")) { result += "\n.RibbonMarkRainy=True"; }
                else if (zh.Contains("沙尘之证")) { result += "\n.RibbonMarkSandstorm=True"; }
                else if (zh.Contains("午夜之证")) { result += "\n.RibbonMarkSleepyTime=True"; }
                else if (zh.Contains("降雪之证")) { result += "\n.RibbonMarkSnowy=True"; }
                else if (zh.Contains("落雷之证")) { result += "\n.RibbonMarkStormy=True"; }
                else if (zh.Contains("干燥之证")) { result += "\n.RibbonMarkDry=True"; }
                else if (zh.Contains("黄昏之证")) { result += "\n.RibbonMarkDusk=True"; }
                else if (zh.Contains("拂晓之证")) { result += "\n.RibbonMarkDawn=True"; }
                else if (zh.Contains("上钩之证")) { result += "\n.RibbonMarkFishing=True"; }
                else if (zh.Contains("咖喱之证")) { result += "\n.RibbonMarkCurry=True"; }
                else if (zh.Contains("无虑之证")) { result += "\n.RibbonMarkAbsentMinded=True"; }
                else if (zh.Contains("愤怒之证")) { result += "\n.RibbonMarkAngry=True"; }
                else if (zh.Contains("冷静之证")) { result += "\n.RibbonMarkCalmness=True"; }
                else if (zh.Contains("领袖之证")) { result += "\n.RibbonMarkCharismatic=True"; }
                else if (zh.Contains("狡猾之证")) { result += "\n.RibbonMarkCrafty=True"; }
                else if (zh.Contains("期待之证")) { result += "\n.RibbonMarkExcited=True"; }
                else if (zh.Contains("本能之证")) { result += "\n.RibbonMarkFerocious=True"; }
                else if (zh.Contains("动摇之证")) { result += "\n.RibbonMarkFlustered=True"; }
                else if (zh.Contains("木讷之证")) { result += "\n.RibbonMarkHumble=True"; }
                else if (zh.Contains("理性之证")) { result += "\n.RibbonMarkIntellectual=True"; }
                else if (zh.Contains("热情之证")) { result += "\n.RibbonMarkIntense=True"; }
                else if (zh.Contains("捡拾之证")) { result += "\n.RibbonMarkItemfinder=True"; }
                else if (zh.Contains("紧张之证")) { result += "\n.RibbonMarkJittery=True"; }
                else if (zh.Contains("幸福之证")) { result += "\n.RibbonMarkJoyful=True"; }
                else if (zh.Contains("优雅之证")) { result += "\n.RibbonMarkKindly=True"; }
                else if (zh.Contains("激动之证")) { result += "\n.RibbonMarkPeeved=True"; }
                else if (zh.Contains("自信之证")) { result += "\n.RibbonMarkPrideful=True"; }
                else if (zh.Contains("昂扬之证")) { result += "\n.RibbonMarkPumpedUp=True"; }
                else if (zh.Contains("未知之证")) { result += "\n.RibbonMarkRare=True"; }
                else if (zh.Contains("淘气之证")) { result += "\n.RibbonMarkRowdy=True"; }
                else if (zh.Contains("凶悍之证")) { result += "\n.RibbonMarkScowling=True"; }
                else if (zh.Contains("不振之证")) { result += "\n.RibbonMarkSlump=True"; }
                else if (zh.Contains("微笑之证")) { result += "\n.RibbonMarkSmiley=True"; }
                else if (zh.Contains("悲伤之证")) { result += "\n.RibbonMarkTeary=True"; }
                else if (zh.Contains("不纯之证")) { result += "\n.RibbonMarkThorny=True"; }
                else if (zh.Contains("偶遇之证")) { result += "\n.RibbonMarkUncommon=True"; }
                else if (zh.Contains("自卑之证")) { result += "\n.RibbonMarkUnsure=True"; }
                else if (zh.Contains("爽快之证")) { result += "\n.RibbonMarkUpbeat=True"; }
                else if (zh.Contains("活力之证")) { result += "\n.RibbonMarkVigor=True"; }
                else if (zh.Contains("倦怠之证")) { result += "\n.RibbonMarkZeroEnergy=True"; }
                else if (zh.Contains("疏忽之证")) { result += "\n.RibbonMarkZonedOut=True"; }
                else if (zh.Contains("宝主之证")) { result += "\n.RibbonMarkTitan=True"; }
                if (zh.Contains("大个子之证")) { result += "\n.Scale=255\n.RibbonMarkJumbo=True"; }
                else if (zh.Contains("小不点之证")) { result += "\n.Scale=0\n.RibbonMarkMini=True"; }

            }

            //添加全回忆技能(不支持BDSP)
            if (typeof(T) == typeof(PK9) || typeof(T) == typeof(PK8))
            {
                if (zh.Contains("全技能"))
                {
                    result += "\n.RelearnMoves=$suggestAll";
                    zh = zh.Replace("全技能", "");

                }
                else if (zh.Contains("全招式"))
                {
                    result += "\n.RelearnMoves=$suggestAll";
                    zh = zh.Replace("全招式", "");
                }
            }
            if (typeof(T) == typeof(PA8))
            {
                if (zh.Contains("全技能") || zh.Contains("全招式"))
                {
                    result += "\n.MoveMastery=$suggestAll";
                    zh = zh.Replace("全技能", "");
                }
                else if (zh.Contains("全招式"))
                {
                    result += "\n.MoveMastery=$suggestAll";
                    zh = zh.Replace("全招式", "");
                }
            }

            // 添加技能
            zh += "-";
            for (int moveCount = 0; moveCount < 4; moveCount++)
            {
                for (int i = 0; i < GameStringsZh.Move.Count; i++)
                {
                    if (GameStringsZh.Move[i].Length == 0) continue;
                    if (!zh.Contains("-" + GameStringsZh.Move[i] + "-")) continue;
                    result += $"\n-{GameStringsEn.Move[i]}";
                    zh = zh.Replace("-" + GameStringsZh.Move[i], "");
                    break;
                }
            }
            zh = zh.Replace("-", "");

            return result;
        }

        public static bool IsPS(string str) => GameStringsEn.Species.Skip(1).Any(str.Contains);

    }
}

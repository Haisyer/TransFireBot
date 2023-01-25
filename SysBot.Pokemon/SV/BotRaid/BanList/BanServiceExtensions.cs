using Newtonsoft.Json;
using System.Text;
using SysBot.Base;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace SysBot.Pokemon.SV
{
    public static class BanServiceExtensions
    {
        private static readonly string tableResource = "SysBot.Pokemon.SV.BotRaid.BanList.Resources.substitutionTable.json";
        private static Dictionary<string, string> SubTable = new();

        public static string NormalizeAndClean(this string text, string connectionLabel)
        {
            var normalized = text.Normalize(NormalizationForm.FormKD);
            var nonAlpha = string.Empty;

            for (int i = 0; i < normalized.Length; i++)
            {
                if (char.IsLetterOrDigit(normalized, i))
                    nonAlpha += normalized.Substring(i, 1);
            }

            var lowered = nonAlpha.ToLower();
            if (SubTable is null || SubTable.Count == 0)
                SubTable = tableResource.DeserializeResource<Dictionary<string, string>>(connectionLabel);

            var table = SubTable.ToArray();
            for (int i = 0; i < lowered.Length; i++)
            {
                var letter = lowered[i];
                for (int j = 0; j < table.Length; j++)
                {
                    if (table[j].Value.Contains(letter))
                        lowered = lowered.Replace(letter.ToString(), table[j].Key);
                }
            }
            return lowered;
        }

        public static T DeserializeResource<T>(this string resource, string connectionLabel)
        {
            try
            {
                using Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)!;
                using TextReader reader = new StreamReader(stream);

                JsonSerializer serializer = new();
                var result = (T?)serializer.Deserialize(reader, typeof(T));
                reader.Close();
                return result!;
            }
            catch (Exception e)
            {
                LogUtil.LogError($"Failed to deserialize resource: {resource}.\n{e.Message}", connectionLabel);
                return default!;
            }
        }
    }
}

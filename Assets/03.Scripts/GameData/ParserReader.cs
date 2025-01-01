using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._03.Scripts.GameData
{
    internal class ParserReader
    {

        static public string[] ParseCSVLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            string value = "";

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(value.Trim());
                    value = "";
                }
                else
                {
                    value += c;
                }
            }

            if (!string.IsNullOrEmpty(value))
            {
                result.Add(value.Trim());
            }
            return result.ToArray();
        }
        static public string ApplyLineBreaks(string text)
        {
            return text.Replace(@"\n", "\n");
        }
    }
}

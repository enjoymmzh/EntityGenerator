using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityGeneratorWindows.Common
{
    public class JsonHelper
    {
        public static string Format(string str)
        {
            try
            {
                //格式化json字符串
                JsonSerializer serializer = new JsonSerializer();
                TextReader tr = new StringReader(str);
                JsonTextReader jtr = new JsonTextReader(tr);
                object obj = serializer.Deserialize(jtr);
                if (obj is not null)
                {
                    StringWriter textWriter = new StringWriter();
                    JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 4,
                        IndentChar = ' '
                    };
                    serializer.Serialize(jsonWriter, obj);
                    return textWriter.ToString();
                }
                else
                {
                    return str;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string FormatEx(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return "";
            }

            try
            {
                var example = data.StartsWith("HTTP/") ? data.Substring(data.IndexOf("\r\n\r\n")) : data;
                using var sr = new StringReader(example);
                using var reader = new JsonTextReader(sr);
                var formattedJson = JToken.ReadFrom(reader);
                return formattedJson.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T ToObject<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}

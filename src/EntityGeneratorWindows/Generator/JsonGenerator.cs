using EntityGeneratorWindows.Common;
using EntityGeneratorWindows.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace EntityGeneratorWindows.Generator
{
    public class JsonGenerator
    {
        #region 全局变量
        /// <summary>
        /// 原始json
        /// </summary>
        public string json { get; set; }
        /// <summary>
        /// 命名空间
        /// </summary>
        public string nameSpace { get; set; }
        /// <summary>
        /// 是否全部生成到一个文件中
        /// </summary>
        public bool singleFile { get; set; }
        /// <summary>
        /// 第一个字母是否大写
        /// </summary>
        public static bool firstChar { get; set; }
        /// <summary>
        /// 生成路径
        /// </summary>
        public string path { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<JsonType> Types { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        private HashSet<string> Names = new HashSet<string>();
        /// <summary>
        /// 
        /// </summary>
        public CodeWriter writer = new CodeWriter();
        /// <summary>
        /// 当前项目路径
        /// </summary>
        private string curProjectPath { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        #endregion

        private JsonGenerator() { }
        private static JsonGenerator instance = null;
        public static JsonGenerator Get()
        {
            if (instance is null)
            {
                instance = new JsonGenerator();
            }
            return instance;
        }

        public void GenerateClasses()
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            JObject[] examples;
            var example = json.StartsWith("HTTP/") ? json.Substring(json.IndexOf("\r\n\r\n")) : json;
            using var sr = new StringReader(example);
            using var reader = new JsonTextReader(sr);
            var data = JToken.ReadFrom(reader);
            if (data is JArray)
            {
                examples = ((JArray)data).Cast<JObject>().ToArray();
            }
            else if (data is JObject)
            {
                examples = new[] { (JObject)data };
            }
            else
            {
                throw new Exception("非法JSON");
            }

            Types = new List<JsonType>();
            Names.Add("Root");
            var rootType = new JsonType(this, examples[0]);
            rootType.IsRoot = true;
            rootType.AssignName("Root");
            GenerateClass(examples, rootType);

            if (singleFile)
            {
                WriteClassesToFile(Path.Combine(path, "Root.cs"), Types);
            }
            else
            {

                foreach (var type in Types)
                {
                    var folder = path;
                    WriteClassesToFile(Path.Combine(folder, (!type.IsRoot ? "Root" + "." : string.Empty) + type.AssignedName + ".cs"), new[] { type });
                }
            }
        }

        private void GenerateClass(JObject[] examples, JsonType type)
        {
            var jsonFields = new Dictionary<string, JsonType>();
            var fieldExamples = new Dictionary<string, IList<object>>();

            var first = true;

            foreach (var obj in examples)
            {
                foreach (var prop in obj.Properties())
                {
                    JsonType fieldType;
                    var currentType = new JsonType(this, prop.Value);
                    var propName = prop.Name;
                    if (jsonFields.TryGetValue(propName, out fieldType))
                    {

                        var commonType = fieldType.GetCommonType(currentType);

                        jsonFields[propName] = commonType;
                    }
                    else
                    {
                        var commonType = currentType;
                        if (first) commonType = commonType.MaybeMakeNullable(this);
                        else commonType = commonType.GetCommonType(JsonType.GetNull(this));
                        jsonFields.Add(propName, commonType);
                        fieldExamples[propName] = new List<object>();
                    }
                    var fe = fieldExamples[propName];
                    var val = prop.Value;
                    if (val.Type is JTokenType.Null or JTokenType.Undefined)
                    {
                        if (!fe.Contains(null))
                        {
                            fe.Insert(0, null);
                        }
                    }
                    else
                    {
                        var v = (val.Type is JTokenType.Array or JTokenType.Object) ? val : val.Value<object>();
                        if (!fe.Any(x => v.Equals(x)))
                            fe.Add(v);
                    }
                }
                first = false;
            }

            foreach (var field in jsonFields)
            {
                Names.Add(field.Key.ToLower());
            }

            foreach (var field in jsonFields)
            {
                var fieldType = field.Value;
                if (fieldType.Type == JsonTypeEnum.Object)
                {
                    var subexamples = new List<JObject>(examples.Length);
                    foreach (var obj in examples)
                    {
                        JToken value;
                        if (obj.TryGetValue(field.Key, out value))
                        {
                            if (value.Type == JTokenType.Object)
                            {
                                subexamples.Add((JObject)value);
                            }
                        }
                    }

                    fieldType.AssignName(CreateUniqueClassName(field.Key));
                    GenerateClass(subexamples.ToArray(), fieldType);
                }

                if (fieldType.InternalType is not null && fieldType.InternalType.Type == JsonTypeEnum.Object)
                {
                    var subexamples = new List<JObject>(examples.Length);
                    foreach (var obj in examples)
                    {
                        JToken value;
                        if (obj.TryGetValue(field.Key, out value))
                        {
                            if (value.Type == JTokenType.Array)
                            {
                                foreach (var item in (JArray)value)
                                {
                                    if (!(item is JObject)) throw new NotSupportedException("Arrays of non-objects are not supported yet.");
                                    subexamples.Add((JObject)item);
                                }

                            }
                            else if (value.Type == JTokenType.Object)
                            {
                                foreach (var item in (JObject)value)
                                {
                                    if (!(item.Value is JObject)) throw new NotSupportedException("Arrays of non-objects are not supported yet.");

                                    subexamples.Add((JObject)item.Value);
                                }
                            }
                        }
                    }

                    field.Value.InternalType.AssignName(CreateUniqueClassNameFromPlural(field.Key));
                    GenerateClass(subexamples.ToArray(), field.Value.InternalType);
                }
            }

            type.Fields = jsonFields.Select(x => new JsonFieldInfo(this, x.Key, x.Value, true, fieldExamples[x.Key])).ToArray();

            Types.Add(type);
        }

        private string CreateUniqueClassName(string name)
        {
            name = ToTitleCase(name);

            var finalName = name;
            var i = 2;
            while (Names.Any(x => x.Equals(finalName, StringComparison.OrdinalIgnoreCase)))
            {
                finalName = name + i.ToString();
                i++;
            }

            Names.Add(name);
            return name;
        }

        private string CreateUniqueClassNameFromPlural(string plural)
        {
            plural = ToTitleCase(plural);
            return CreateUniqueClassName(plural);
        }

        internal static string ToTitleCase(string str)
        {
            if (firstChar)
            {
                var sb = new StringBuilder(str.Length);

                for (int i = 0; i < str.Length; i++)
                {
                    var c = str[i];
                    if (i.Equals(0))
                    {
                        sb.Append(char.ToUpper(c));
                    }
                    else
                    {
                        var last = str[i - 1];
                        if (!char.IsLetterOrDigit(last))
                        {
                            sb.Append(char.ToUpper(c));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                }
                return sb.ToString();
            }
            else
            {
                return str;
            }
        }

        private void WriteClassesToFile(string path, IEnumerable<JsonType> types)
        {
            using var sw = new StreamWriter(path, false, Encoding.UTF8);
            sw.WriteLine("using System;");
            sw.WriteLine("using System.Collections.Generic;");
            sw.WriteLine("using System.Linq;");
            sw.WriteLine("using System.Text;");
            sw.WriteLine("using System.Threading.Tasks;");
            sw.WriteLine();
            sw.WriteLine("namespace {0}", this.nameSpace);
            sw.WriteLine("{");

            foreach (var type in types)
            {
                writer.WriteClass(this, sw, type);
            }

            sw.WriteLine("}");
        }

        /// <summary>
        /// 创建csproj项目
        /// </summary>
        /// <param name="classPath"></param>
        /// <param name="projectName"></param>
        public string CreateCsproj()
        {
            #region 内容

            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"<Project Sdk=\"Microsoft.NET.Sdk\">\r\n");
            stringBuilder.Append($"  <PropertyGroup>\r\n");
            if (Global.pversion.Contains("standard"))
            {
                stringBuilder.Append($"	  <TargetFrameworks>{Global.pversion}</TargetFrameworks>\r\n");
            }
            else
            {
                stringBuilder.Append($"	  <TargetFramework>{Global.pversion}</TargetFramework>\r\n");
            }
            stringBuilder.Append($"  </PropertyGroup>\r\n");
            stringBuilder.Append($"  <ItemGroup>\r\n");
            stringBuilder.Append($"  </ItemGroup>\r\n");
            stringBuilder.Append($"  <ItemGroup>\r\n");
            stringBuilder.Append($"  </ItemGroup>\r\n");
            stringBuilder.Append($"</Project>\r\n");
            #endregion

            string classPath = path;
            string projectName = Path.GetFileName(classPath);
            var basePath = path + "\\" + projectName + ".csproj";
            CreateFile(basePath, stringBuilder);

            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 创建sln
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectName"></param>
        public void CreateSln(string modelId)
        {
            string projectName = Path.GetFileName(path);
            var projId = Guid.NewGuid().ToString();
            var templatePath = curProjectPath + "Template\\sln.txt";

            string project = project = string.Format("Project(\"{0}\") = \"{1}\", \"{1}\\{1}.csproj\", \"{2}\"\r\nEndProject\r\n",
                    "{" + projId.ToUpper() + "}", projectName, "{" + modelId.ToUpper() + "}");
            string postSolution = string.Format("{0}.Debug|Any CPU.ActiveCfg = Debug|Any CPU\r\n", "{" + modelId.ToUpper() + "}");
            postSolution += string.Format("{0}.Debug|Any CPU.Build.0 = Debug|Any CPU\r\n", "{" + modelId.ToUpper() + "}");
            postSolution += string.Format("{0}.Release|Any CPU.ActiveCfg = Release|Any CPU\r\n", "{" + modelId.ToUpper() + "}");
            postSolution += string.Format("{0}.Release|Any CPU.Build.0 = Release|Any CPU\r\n", "{" + modelId.ToUpper() + "}");
            string appendText = System.IO.File.ReadAllText(templatePath)
                .Replace("@project", project)
                .Replace("@postSolution", postSolution)
                .Replace("@guid", "{" + Guid.NewGuid().ToString().ToUpper() + "}");

            var parent = Directory.GetParent(path);
            FileHelper.CreateFile(parent + "\\" + projectName + ".sln", appendText, System.Text.Encoding.UTF8);
        }


        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="info"></param>
        internal static void CreateFile(string path, StringBuilder info)
        {
            StreamWriter sr;
            //是否存在文件夹,不存在则创建
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            //如果该文件存在则追加内容，否则创建文件
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            sr = File.CreateText(path);
            sr.Write(info.ToString());
            sr.Flush();
            sr.Close();
        }
    }

    public class CodeWriter
    {
        public string GetTypeName1(JsonType type, JsonGenerator config)
        {
            switch (type.Type)
            {
                case JsonTypeEnum.Anything: return "object";
                case JsonTypeEnum.Array: return "List<" + GetTypeName(type.InternalType, config) + ">";
                case JsonTypeEnum.Dictionary: return "Dictionary<string, " + GetTypeName(type.InternalType, config) + ">";
                case JsonTypeEnum.Boolean: return "bool";
                case JsonTypeEnum.Float: return "double";
                case JsonTypeEnum.Integer: return "int";
                case JsonTypeEnum.Long: return "long";
                case JsonTypeEnum.Date: return "DateTime";
                case JsonTypeEnum.NonConstrained: return "object";
                case JsonTypeEnum.NullableBoolean: return "bool?";
                case JsonTypeEnum.NullableFloat: return "double?";
                case JsonTypeEnum.NullableInteger: return "int?";
                case JsonTypeEnum.NullableLong: return "long?";
                case JsonTypeEnum.NullableDate: return "DateTime?";
                case JsonTypeEnum.NullableSomething: return "object";
                case JsonTypeEnum.Object: return type.AssignedName;
                case JsonTypeEnum.String: return "string";
                default: throw new System.NotSupportedException("Unsupported json type");
            }
        }

        /// <summary>
        /// 新的switch写法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public string GetTypeName(JsonType type, JsonGenerator config) => type.Type switch
        {
            JsonTypeEnum.Anything => "object",
            JsonTypeEnum.Array => "List<" + GetTypeName(type.InternalType, config) + ">",
            JsonTypeEnum.Dictionary => "Dictionary<string, " + GetTypeName(type.InternalType, config) + ">",
            JsonTypeEnum.Boolean => "bool",
            JsonTypeEnum.Float => "double",
            JsonTypeEnum.Integer => "int",
            JsonTypeEnum.Long => "long",
            JsonTypeEnum.Date => "DateTime",
            JsonTypeEnum.NonConstrained => "object",
            JsonTypeEnum.NullableBoolean => "bool?",
            JsonTypeEnum.NullableFloat => "double?",
            JsonTypeEnum.NullableInteger => "int?",
            JsonTypeEnum.NullableLong => "long?",
            JsonTypeEnum.NullableDate => "DateTime?",
            JsonTypeEnum.NullableSomething => "object",
            JsonTypeEnum.Object => type.AssignedName,
            JsonTypeEnum.String => "string",
            _ => throw new System.NotSupportedException("Unsupported json type")
        };

        public void WriteClass(JsonGenerator config, TextWriter sw, JsonType type)
        {
            sw.WriteLine("    /// <summary>");
            sw.WriteLine("    /// ");
            sw.WriteLine("    /// </summary>");
            sw.WriteLine("    public class {0}", type.AssignedName);
            sw.WriteLine("    {");

            foreach (var field in type.Fields)
            {
                sw.WriteLine("        /// <summary>");
                sw.WriteLine("        /// ");
                sw.WriteLine("        /// </summary>");
                sw.WriteLine("        public " + field.Type.GetTypeName() + " " + field.MemberName + " { get; set; }");
            }

            sw.WriteLine("    }");
        }
    }

    public class JsonFieldInfo
    {
        public JsonFieldInfo(JsonGenerator generator, string jsonMemberName, JsonType type, bool usePascalCase, IList<object> Examples)
        {
            this.generator = generator;
            this.JsonMemberName = jsonMemberName;
            this.MemberName = jsonMemberName;
            if (usePascalCase) MemberName = JsonGenerator.ToTitleCase(MemberName);
            this.Type = type;
            this.Examples = Examples;
        }
        private JsonGenerator generator;
        public string MemberName { get; private set; }
        public string JsonMemberName { get; private set; }
        public JsonType Type { get; private set; }
        public IList<object> Examples { get; private set; }


    }

    public class JsonType
    {
        private JsonType(JsonGenerator generator)
        {
            this.generator = generator;
        }

        public JsonType(JsonGenerator generator, JToken token)
            : this(generator)
        {

            Type = GetFirstTypeEnum(token);

            if (Type == JsonTypeEnum.Array)
            {
                var array = (JArray)token;
                InternalType = GetCommonType(generator, array.ToArray());
            }
        }

        internal static JsonType GetNull(JsonGenerator generator)
        {
            return new JsonType(generator, JsonTypeEnum.NullableSomething);
        }

        private JsonGenerator generator;

        internal JsonType(JsonGenerator generator, JsonTypeEnum type)
            : this(generator)
        {
            this.Type = type;
        }


        public static JsonType GetCommonType(JsonGenerator generator, JToken[] tokens)
        {

            if (tokens.Length == 0) return new JsonType(generator, JsonTypeEnum.NonConstrained);

            var common = new JsonType(generator, tokens[0]).MaybeMakeNullable(generator);

            for (int i = 1; i < tokens.Length; i++)
            {
                var current = new JsonType(generator, tokens[i]);
                common = common.GetCommonType(current);
            }

            return common;

        }

        internal JsonType MaybeMakeNullable(JsonGenerator generator)
        {
            return this;
        }


        public JsonTypeEnum Type { get; private set; }
        public JsonType InternalType { get; private set; }
        public string AssignedName { get; private set; }


        public void AssignName(string name)
        {
            AssignedName = name;
        }

        public string GetReaderName()
        {
            if (Type is JsonTypeEnum.Anything or JsonTypeEnum.NullableSomething or JsonTypeEnum.NonConstrained)
            {
                return "ReadObject";
            }
            if (Type is JsonTypeEnum.Object)
            {
                return string.Format("ReadStronglyTypedObject<{0}>", AssignedName);
            }
            else if (Type is JsonTypeEnum.Array)
            {
                return string.Format("ReadArray<{0}>", InternalType.GetTypeName());
            }
            else
            {
                return string.Format("Read{0}", Enum.GetName(typeof(JsonTypeEnum), Type));
            }
        }

        public JsonType GetInnermostType()
        {
            if (Type is not JsonTypeEnum.Array) throw new InvalidOperationException();
            if (InternalType.Type is not JsonTypeEnum.Array) return InternalType;
            return InternalType.GetInnermostType();
        }

        public string GetTypeName()
        {
            return generator.writer.GetTypeName(this, generator);
        }

        public string GetJTokenType()
        {
            switch (Type)
            {
                case JsonTypeEnum.Boolean:
                case JsonTypeEnum.Integer:
                case JsonTypeEnum.Long:
                case JsonTypeEnum.Float:
                case JsonTypeEnum.Date:
                case JsonTypeEnum.NullableBoolean:
                case JsonTypeEnum.NullableInteger:
                case JsonTypeEnum.NullableLong:
                case JsonTypeEnum.NullableFloat:
                case JsonTypeEnum.NullableDate:
                case JsonTypeEnum.String:
                    return "JValue";
                case JsonTypeEnum.Array:
                    return "JArray";
                case JsonTypeEnum.Dictionary:
                    return "JObject";
                case JsonTypeEnum.Object:
                    return "JObject";
                default:
                    return "JToken";

            }
        }

        public JsonType GetCommonType(JsonType type2)
        {
            var commonType = GetCommonTypeEnum(this.Type, type2.Type);

            if (commonType is JsonTypeEnum.Array)
            {
                if (type2.Type is JsonTypeEnum.NullableSomething) return this;
                if (this.Type is JsonTypeEnum.NullableSomething) return type2;
                var commonInternalType = InternalType.GetCommonType(type2.InternalType).MaybeMakeNullable(generator);
                if (commonInternalType != InternalType) return new JsonType(generator, JsonTypeEnum.Array) { InternalType = commonInternalType };
            }

            if (this.Type == commonType) return this;
            return new JsonType(generator, commonType).MaybeMakeNullable(generator);
        }

        private static bool IsNull(JsonTypeEnum type)
        {
            return type is JsonTypeEnum.NullableSomething;
        }

        private JsonTypeEnum GetCommonTypeEnum(JsonTypeEnum type1, JsonTypeEnum type2)
        {
            if (type1 is JsonTypeEnum.NonConstrained) return type2;
            if (type2 is JsonTypeEnum.NonConstrained) return type1;

            switch (type1)
            {
                case JsonTypeEnum.Boolean:
                    if (IsNull(type2)) return JsonTypeEnum.NullableBoolean;
                    if (type2 is JsonTypeEnum.Boolean) return type1;
                    break;
                case JsonTypeEnum.NullableBoolean:
                    if (IsNull(type2)) return type1;
                    if (type2 is JsonTypeEnum.Boolean) return type1;
                    break;
                case JsonTypeEnum.Integer:
                    if (IsNull(type2)) return JsonTypeEnum.NullableInteger;
                    if (type2 is JsonTypeEnum.Float) return JsonTypeEnum.Float;
                    if (type2 is JsonTypeEnum.Long) return JsonTypeEnum.Long;
                    if (type2 is JsonTypeEnum.Integer) return type1;
                    break;
                case JsonTypeEnum.NullableInteger:
                    if (IsNull(type2)) return type1;
                    if (type2 is JsonTypeEnum.Float) return JsonTypeEnum.NullableFloat;
                    if (type2 is JsonTypeEnum.Long) return JsonTypeEnum.NullableLong;
                    if (type2 is JsonTypeEnum.Integer) return type1;
                    break;
                case JsonTypeEnum.Float:
                    if (IsNull(type2)) return JsonTypeEnum.NullableFloat;
                    if (type2 is JsonTypeEnum.Float) return type1;
                    if (type2 is JsonTypeEnum.Integer) return type1;
                    if (type2 is JsonTypeEnum.Long) return type1;
                    break;
                case JsonTypeEnum.NullableFloat:
                    if (IsNull(type2)) return type1;
                    if (type2 is JsonTypeEnum.Float) return type1;
                    if (type2 is JsonTypeEnum.Integer) return type1;
                    if (type2 is JsonTypeEnum.Long) return type1;
                    break;
                case JsonTypeEnum.Long:
                    if (IsNull(type2)) return JsonTypeEnum.NullableLong;
                    if (type2 is JsonTypeEnum.Float) return JsonTypeEnum.Float;
                    if (type2 is JsonTypeEnum.Integer) return type1;
                    break;
                case JsonTypeEnum.NullableLong:
                    if (IsNull(type2)) return type1;
                    if (type2 is JsonTypeEnum.Float) return JsonTypeEnum.NullableFloat;
                    if (type2 is JsonTypeEnum.Integer) return type1;
                    if (type2 is JsonTypeEnum.Long) return type1;
                    break;
                case JsonTypeEnum.Date:
                    if (IsNull(type2)) return JsonTypeEnum.NullableDate;
                    if (type2 is JsonTypeEnum.Date) return JsonTypeEnum.Date;
                    break;
                case JsonTypeEnum.NullableDate:
                    if (IsNull(type2)) return type1;
                    if (type2 is JsonTypeEnum.Date) return type1;
                    break;
                case JsonTypeEnum.NullableSomething:
                    if (IsNull(type2)) return type1;
                    if (type2 is JsonTypeEnum.String) return JsonTypeEnum.String;
                    if (type2 is JsonTypeEnum.Integer) return JsonTypeEnum.NullableInteger;
                    if (type2 is JsonTypeEnum.Float) return JsonTypeEnum.NullableFloat;
                    if (type2 is JsonTypeEnum.Long) return JsonTypeEnum.NullableLong;
                    if (type2 is JsonTypeEnum.Boolean) return JsonTypeEnum.NullableBoolean;
                    if (type2 is JsonTypeEnum.Date) return JsonTypeEnum.NullableDate;
                    if (type2 is JsonTypeEnum.Array) return JsonTypeEnum.Array;
                    if (type2 is JsonTypeEnum.Object) return JsonTypeEnum.Object;
                    break;
                case JsonTypeEnum.Object:
                    if (IsNull(type2)) return type1;
                    if (type2 is JsonTypeEnum.Object) return type1;
                    if (type2 is JsonTypeEnum.Dictionary) throw new ArgumentException();
                    break;
                case JsonTypeEnum.Dictionary:
                    throw new ArgumentException();
                //if (IsNull(type2)) return type1;
                //if (type2 is JsonTypeEnum.Object) return type1;
                //if (type2 is JsonTypeEnum.Dictionary) return type1;
                //  break;
                case JsonTypeEnum.Array:
                    if (IsNull(type2)) return type1;
                    if (type2 is JsonTypeEnum.Array) return type1;
                    break;
                case JsonTypeEnum.String:
                    if (IsNull(type2)) return type1;
                    if (type2 is JsonTypeEnum.String) return type1;
                    break;
            }
            return JsonTypeEnum.Anything;
        }

        private static JsonTypeEnum GetFirstTypeEnum(JToken token)
        {
            var type = token.Type;
            if (type is JTokenType.Integer)
            {
                if ((long)((JValue)token).Value < int.MaxValue) return JsonTypeEnum.Integer;
                else return JsonTypeEnum.Long;

            }
            switch (type)
            {
                case JTokenType.Array: return JsonTypeEnum.Array;
                case JTokenType.Boolean: return JsonTypeEnum.Boolean;
                case JTokenType.Float: return JsonTypeEnum.Float;
                case JTokenType.Null: return JsonTypeEnum.NullableSomething;
                case JTokenType.Undefined: return JsonTypeEnum.NullableSomething;
                case JTokenType.String: return JsonTypeEnum.String;
                case JTokenType.Object: return JsonTypeEnum.Object;
                case JTokenType.Date: return JsonTypeEnum.Date;

                default: return JsonTypeEnum.Anything;

            }
        }

        public IList<JsonFieldInfo> Fields { get; internal set; }

        public bool IsRoot { get; internal set; }
    }

    public enum JsonTypeEnum
    {
        Anything,
        String,
        Boolean,
        Integer,
        Long,
        Float,
        Date,
        NullableInteger,
        NullableLong,
        NullableFloat,
        NullableBoolean,
        NullableDate,
        Object,
        Array,
        Dictionary,
        NullableSomething,
        NonConstrained
    }
}

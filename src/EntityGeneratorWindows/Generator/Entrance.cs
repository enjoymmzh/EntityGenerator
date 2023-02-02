using EntityGeneratorWindows.Common;
using EntityGeneratorWindows.Model;
using System.Threading.Tasks;

namespace EntityGeneratorWindows.Generator
{
    public class Entrance
    {
        /// <summary>
        /// 创建项目
        /// </summary>
        /// <param name="path">生成根目录</param>
        /// <param name="projName">项目名称</param>
        /// <param name="type">生成类型 0：只生成实体 1:生成实体项目 2：生成实体+数据层项目</param>
        /// <param name="class_upper">类名大写</param>
        /// <param name="field_upper">字段名大写</param>
        public static void DBGenerate(string path, string projName, int type, bool class_upper, bool field_upper)
        {
            try
            {
                TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("检测该目录下是否存在同名过往项目"));
                FileHelper.DeleteDirIfExist(path.TrimEnd('\\') + "\\" + projName);
                TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("清理过往项目完成"));

                Generator.DBGenerate.ClasstoUpper = class_upper;
                Generator.DBGenerate.FieldtoUpper = field_upper;

                if (type.Equals(0))
                {
                    TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成实体中。。。"));
                    var dir = path.TrimEnd('\\') + "\\" + projName;
                    Generator.DBGenerate.Get().CreateModel(dir + "Model");
                    TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成完毕，请在 " + dir + " 目录下查看"));
                }
                else
                {
                    var modelDir = path.TrimEnd('\\') + "\\" + projName + "\\" + Global.modelName;

                    TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成实体中。。。"));
                    Generator.DBGenerate.Get().CreateModel(modelDir);

                    TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成实体层项目文件中。。。"));
                    var modelUid = Generator.DBGenerate.Get().CreateModelCsproj(modelDir);
                    Task.Delay(500);

                    var dataUid = "";
                    if (type.Equals(2))
                    {
                        //如果需要建立数据层，再把page等数据层会使用到的实体添加到实体层里
                        TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成分页实体中。。。"));
                        Generator.DBGenerate.Get().CreatePage(modelDir);
                        Task.Delay(500);

                        TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成返回结果实体中。。。"));
                        Generator.DBGenerate.Get().CreateResult(modelDir);
                        Task.Delay(500);

                        TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成查询实体中。。。"));
                        Generator.DBGenerate.Get().CreateQuery(modelDir);
                        Task.Delay(500);

                        //创建数据层
                        var dataDir = path.TrimEnd('\\') + "\\" + projName + "\\" + Global.dataName;

                        TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成数据库连接功能中。。。"));
                        Generator.DBGenerate.Get().CreateDbHelper(dataDir);
                        Task.Delay(500);

                        TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成数据库基类中。。。"));
                        Generator.DBGenerate.Get().CreateBaseDal(dataDir);
                        Task.Delay(500);

                        TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成分页查询范例中。。。"));
                        Generator.DBGenerate.Get().CreateModelDal(dataDir);
                        Task.Delay(500);

                        TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成数据层项目文件中。。。"));
                        dataUid = Generator.DBGenerate.Get().CreateDalCsproj(dataDir);
                        Task.Delay(500);
                    }

                    TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成sln中。。。"));
                    Generator.DBGenerate.Get().CreateSln(modelUid, dataUid, projName, path.TrimEnd('\\') + "\\" + projName);
                    TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成完毕，请在 " + path.TrimEnd('\\') + "\\" + projName + " 目录下查看"));
                }
            }
            catch (System.Exception ex)
            {
                TheEvent.SendDBInfoChangedEvent(null, new InfoEventArgs("生成過程中出現錯誤：" + ex.ToString()));
            }
        }

        /// <summary>
        /// 创建预览
        /// </summary>
        public static string DBPreview(ClassInfo info)
        {
            return Generator.DBGenerate.Get().CreatePreview(info);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="json"></param>
        /// <param name="projname"></param>
        /// <param name="slname"></param>
        /// <param name="singleFile"></param>
        /// <param name="bcsproj"></param>
        /// <param name="firstChar"></param>
        public static void JsonGenerate(string path, string json, string projname, string slname, bool singleFile, bool bcsproj, bool firstChar)
        {
            try
            {
                JsonGenerator.Get().path = bcsproj ? path + "\\" + slname + "\\" + projname : path + "\\" + slname;
                JsonGenerator.Get().json = json;
                JsonGenerator.Get().nameSpace = projname;
                JsonGenerator.Get().singleFile = singleFile;
                JsonGenerator.firstChar = firstChar;

                TheEvent.SendJSONInfoChangedEvent(null, new InfoEventArgs("生成類到文件中..."));
                JsonGenerator.Get().GenerateClasses();
                TheEvent.SendJSONInfoChangedEvent(null, new InfoEventArgs("生成類文件結束"));

                if (bcsproj)
                {
                    TheEvent.SendJSONInfoChangedEvent(null, new InfoEventArgs("生成csproj項目文件中..."));
                    var modelId = JsonGenerator.Get().CreateCsproj();
                    TheEvent.SendJSONInfoChangedEvent(null, new InfoEventArgs("生成csproj項目文件結束"));

                    TheEvent.SendJSONInfoChangedEvent(null, new InfoEventArgs("創建sln項目文件中..."));
                    JsonGenerator.Get().CreateSln(modelId);
                    TheEvent.SendJSONInfoChangedEvent(null, new InfoEventArgs("創建sln項目文件結束"));
                }
            }
            catch (System.Exception ex)
            {
                TheEvent.SendJSONInfoChangedEvent(null, new InfoEventArgs("生成過程中出現錯誤：" + ex.ToString()));
            }
        }

        /// <summary>
        /// 创建预览
        /// </summary>
        public static void JsonPreview()
        {

        }
    }
}

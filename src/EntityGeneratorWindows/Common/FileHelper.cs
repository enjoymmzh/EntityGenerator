using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace EntityGeneratorWindows.Common
{
    internal class FileHelper
    {
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        public static void CreateFile(string filePath, string text, Encoding encoding)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                if (!File.Exists(filePath))
                {
                    string directoryPath = GetDirectoryFromFilePath(filePath);
                    CreateDirectory(directoryPath);

                    //Create File
                    FileInfo file = new FileInfo(filePath);
                    using FileStream stream = file.Create();
                    using StreamWriter writer = new StreamWriter(stream, encoding);
                    writer.Write(text);
                    writer.Flush();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void CreateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// 从文件获取目录
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetDirectoryFromFilePath(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            DirectoryInfo directory = file.Directory;
            return directory.FullName;
        }

        /// <summary>
        /// 目录是否存在
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static bool IsExistDirectory(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public static void DeleteDirIfExist(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        public static void DeleteDir(string file)
        {
            try
            {
                //去除文件夹和子文件的只读属性
                //去除文件夹的只读属性
                System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                //去除文件的只读属性
                System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);

                //判断文件夹是否还存在
                if (Directory.Exists(file))
                {
                    foreach (string f in Directory.GetFileSystemEntries(file))
                    {
                        if (File.Exists(f))
                        {
                            //如果有子文件删除文件
                            File.Delete(f);
                            Console.WriteLine(f);
                        }
                        else
                        {
                            //循环递归删除子文件夹
                            DeleteDir(f);
                        }
                    }

                    //删除空文件夹
                    Directory.Delete(file);
                    Console.WriteLine(file);
                }

            }
            catch (Exception ex) // 异常处理
            {
                Console.WriteLine(ex.Message.ToString());// 异常信息
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath"></param>
        public static void DeleteFile(string filePath)
        {
            if (IsExistFile(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsExistFile(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// 文件夹拷贝
        /// </summary>
        /// <param name="srcFolderPath"></param>
        /// <param name="destFolderPath"></param>
        public static void FolderCopy(string srcFolderPath, string destFolderPath)
        {
            //检查目标目录是否以目标分隔符结束，如果不是则添加之
            if (destFolderPath[destFolderPath.Length - 1] != Path.DirectorySeparatorChar)
                destFolderPath += Path.DirectorySeparatorChar;
            //判断目标目录是否存在，如果不在则创建之
            if (!Directory.Exists(destFolderPath))
                Directory.CreateDirectory(destFolderPath);
            var fileList = Directory.GetFileSystemEntries(srcFolderPath).ToList();
            fileList.ForEach(file => {
                if (Directory.Exists(file))
                    FolderCopy(file, destFolderPath + Path.GetFileName(file));
                else
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)//改变只读文件属性，否则删不掉
                        fi.Attributes = FileAttributes.Normal;
                    try
                    { 
                        File.Copy(file, destFolderPath + Path.GetFileName(file), true); 
                    }
                    catch { }
                }
            });
        }

        public static string ReplaceChar(string str)
        {
            string[] dd ={"◎", "■", "●", "№", "↑", "→", "↓" +
"!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "+", "=", "|", "[", "]", "？", "~", "`", "," +
"！", "<", ">", "‰", "→", "←", "↑", "↓", "¤", "§", "＃", "＆", "＆", "＼", "≡", "≠" +
"≈", "∈", "∪", "∏", "∑", "∧", "∨", "⊥", "‖", "‖", "∠", "⊙", "≌", "≌", "√", "∝", "∞", "∮" +
"∫", "≯", "≮", "＞", "≥", "≤", "≠", "±", "＋", "÷", "×", "/" +
"╄", "╅", "╇", "┻", "┻", "┇", "┭", "┷", "┦", "┣", "┝", "┤", "┷", "┷", "┹", "╉", "╇", "【", "】" +
"┌", "├", "┬", "┼", "┍", "┕", "┗", "┏", "┅", "—" +
"〖", "〗", "←", "〓", "☆", "§", "□", "‰", "◇", "＾", "＠", "△", "▲", "＃", "℃", "※", "≈", "￠"};
            for (int i = 0; i < dd.Length; i++)
            {
                string tt = dd[i];
                str = str.Replace(tt, "");
            }
            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string ReadBin()
        {
            string res = null;
            //if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "data.bin"))
            //{
            //    File.Create(AppDomain.CurrentDomain.BaseDirectory + "data.bin");
            //}
            FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "data.bin", FileMode.OpenOrCreate);
            if (fs.Length > 0)
            {
                BinaryFormatter bf = new BinaryFormatter();
                res = bf.Deserialize(fs) as string;
            }
            fs.Close();
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public static void WriteBin(string obj)
        {
            using FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "data.bin", FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            //要先将User类先设为可以序列化(即在类的前面加[Serializable])
            bf.Serialize(fs, obj);
            fs.Close();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string ReadConfig()
        {
            string res = null;
            //if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "config.bin"))
            //{
            //    File.Create(AppDomain.CurrentDomain.BaseDirectory + "config.bin");
            //}
            FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "config.bin", FileMode.OpenOrCreate);
            if (fs.Length > 0)
            {
                BinaryFormatter bf = new BinaryFormatter();
                res = bf.Deserialize(fs) as string;
            }
            fs.Close();
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public static void WriteConfig(string obj)
        {
            using FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "config.bin", FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            //要先将User类先设为可以序列化(即在类的前面加[Serializable])
            bf.Serialize(fs, obj);
            fs.Close();
        }
    }
}

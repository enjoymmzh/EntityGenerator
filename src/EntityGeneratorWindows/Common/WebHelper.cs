using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EntityGeneratorWindows.Common
{
    public class WebHelper
    {
        public WebHelper() { }

        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string HttpPost(string url, string json = "", string header = "")
        {
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";

            try
            {
                if (!string.IsNullOrWhiteSpace(header))
                {
                    var headers = header.Split(';');
                    foreach (var item in headers)
                    {
                        var value = item.Split(',');
                        request.Headers.Set(value[0], value[1]);
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Header格式不正確");
            }

            byte[] buffer = encoding.GetBytes(json);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string res = reader.ReadToEnd();
            return res;
        }
        
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string HttpGet(string url)
        {
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string res = reader.ReadToEnd();
            return res;
        }
        
    }
}

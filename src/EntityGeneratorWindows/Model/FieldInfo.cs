using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityGeneratorWindows.Model
{
    /// <summary>
    /// 字段信息
    /// </summary>
    public class FieldInfo
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string filedName { get; set; }
        /// <summary>
        /// 字段备注
        /// </summary>
        public string filedComment { get; set; }
        /// <summary>
        /// 字段类型
        /// </summary>
        public string filedType { get; set; }
        /// <summary>
        /// 字段是否可以为空
        /// </summary>
        public bool isNull { get; set; }
        /// <summary>
        /// 是否为key
        /// </summary>
        public bool isKey { get; set; }

    }
}

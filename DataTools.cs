using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome
{
    public class DataTools
    {
        public static Dictionary<string, string> room = new Dictionary<string, string>()
            {
            {"客厅","0" },
            {"卧室","1" },
            {"厨房","2" },
            {"卫生间","3" }
            };
        public static Dictionary<string, string> equipment = new Dictionary<string, string>()
            {
                {"灯","100" },
                { "台灯","101" },
                { "音响","102" },
                { "空调","103" },
                { "电视","104" },
                { "闹钟","105" },
                { "微波炉","106" },
                { "电饭煲","107" },
                { "水壶","108" },
                { "排风扇","109" },
                { "洗衣机","110" },
                { "浴霸","111" }
            }; 
            
    }
}

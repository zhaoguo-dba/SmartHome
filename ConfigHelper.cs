using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome
{
    public class ConfigHelper
    {
        public static string GetConnectionString(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}

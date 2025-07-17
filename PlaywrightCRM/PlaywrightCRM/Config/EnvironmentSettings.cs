using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightCRM.Config
{
    public class EnvironmentConfig
    {
        public required string BaseUrl { get; set; }
        public required string Browser { get; set; }
        public bool Headless { get; set; }
        public int Timeout { get; set; }
        public required string username { get; set; }
        public required string password { get; set; }
        public required string fullname { get; set; }
    }
    public class EnvironmentSettings
    {
        public required Dictionary<string, EnvironmentConfig> Environments { get; set; }
    }
}

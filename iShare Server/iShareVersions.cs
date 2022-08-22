using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace iShare_Server
{
    public static class iShareVersions
    {
        private static string iShare_Server = "1.0";
        private static string iShare_Desktop = "1.0";
        private static string iShare_Android = "1.0";

        public static string iShare_Server_ver
        {
            get { return iShare_Server; }
        }

        public static string iShare_Desktop_ver
        {
            get { return iShare_Desktop; }
        }
        public static string iShare_Android_ver
        {
            get { return iShare_Android; }
        }
    }
}

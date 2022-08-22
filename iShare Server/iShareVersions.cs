using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace iShare_Server
{
    internal class iShareVersions
    {
        private const string iShare_Server = "1.0";
        private const string iShare_Desktop = "1.0";
        private const string iShare_Android = "1.0";

        public string iShare_Server_ver
        {
            get { return iShare_Server; }
        }

        public string iShare_Desktop_ver
        {
            get { return iShare_Desktop; }
        }
        public string iShare_Android_ver
        {
            get { return iShare_Android; }
        }
    }
}

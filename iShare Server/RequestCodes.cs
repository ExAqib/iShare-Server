﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iShare_Server
{
    internal static class RequestCodes
    {
        public const string findByIdPassword = "_find_by_id_password_";
        public const string findByID = "_find_by_id_";
        public const string SendFileToMobile = "send_file_to_mobile";
        public const string appVersion = "APP_VER";
        public const string serverVersion = "SERVER_VER";
        public const string desktopVersion = "DESKTOP_VER";
        public const string startingSharingData = "_starting_sharing_data_";
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models.Communication
{
    public class ListnerConnectionParameters
    {
        public ListnerConnectionParameters(string ip, int port)
        {
            IP = ip;
            Port = port;
        }

        public string IP { get; set; }
        public int Port { get; set; }
    }
}

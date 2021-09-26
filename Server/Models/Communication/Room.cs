using Server.Interfaces.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.Communication
{
    public class Room
    {
        private List<IUserConnection> _userList = new List<IUserConnection>();
        public string Name { get; set; }
        public List<IUserConnection> Users { get => _userList; }
    }
}

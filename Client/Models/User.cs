namespace Client.Models
{
    public class User
    {
        public string ClientID { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public int CallbackPort { get; set; }
        public string NickName { get; set; }

    }
}

namespace Server.Models.Communication
{
    public enum TypeReceiveCommandResponse
    {
        Successful = 1,
        Error = 2
    }
    public class ReceiveCommandResponse
    {
        public TypeReceiveCommandResponse Type { get; set; } = TypeReceiveCommandResponse.Successful;
        public string MessageError { get; set; } = "";
    }
}

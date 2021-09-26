using Server.Models.Communication;

namespace Server.Interfaces
{
    public interface ICommandInterpreter
    {
        IServer Server { get; set; }
        ReceiveCommandResponse Interpret(string command);
    }
}

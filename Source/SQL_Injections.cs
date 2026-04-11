namespace Neuron
{
    public class SQL_Injections
    {
        public static readonly string GetMessages = "SELECT * FROM `MessageBase` WHERE `ChatID` = @CI ORDER BY Date DESC, Time DESC";
        public static readonly string GetGroupContacts = "SELECT pb.ChatID , pb.ChatName , pb.Type , cb.Aes FROM `ContactBase` cb JOIN `ChatBase` pb ON cb.ChatID = pb.ChatID WHERE cb.Member = @Username";
        public static readonly string GetUserContacts = "SELECT cb.ChatID, pb.Name, cb.Aes FROM `ContactBase` cb JOIN `ProfileBase` pb ON cb.SecondMember = pb.Username WHERE cb.Member = @Username";
        public static readonly string LoadProfile = "SELECT * FROM `ProfileBase` WHERE `Username` = @UN";
        public static readonly string LoadMembers = "SELECT pb.Username, pb.Name, pb.Public_Key FROM `Contactbase` cb JOIN `ProfileBase` pb ON cb.Member = pb.Username WHERE cb.ChatID = @CI";
    }
}
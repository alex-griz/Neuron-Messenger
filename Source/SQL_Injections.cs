namespace Neuron
{
    public class SQL_Injections
    {
        public static readonly string GetMessages = "SELECT * FROM `MessageBase` WHERE `ChatID` = @CI ORDER BY Date ASC, Time ASC";
        public static readonly string SendMessage = "INSERT INTO `MessageBase` ( `ChatID`,`Sender`, `Message`, `Time`, `Date`) VALUES (@CI ,@S, @M, @T, @D )";
        public static readonly string GetGroupContacts = "SELECT pb.ChatID , pb.ChatName , pb.Type FROM `ContactBase` cb JOIN `ChatBase` pb ON cb.ChatID = pb.ChatID WHERE cb.Member = @Username";
        public static readonly string GetUserContacts = "SELECT cb.ChatID, pb.Name FROM `ContactBase` cb JOIN `ProfileBase` pb ON cb.SecondMember = pb.Username WHERE cb.Member = @Username";
        public static readonly string LeaveGroup = "DELETE FROM `ContactBase` WHERE `Member` = @ME AND `ChatID` = @CI";
        public static readonly string DeleteGroup = "DELETE FROM `ContactBase` WHERE `ChatID` = @CI";
        public static readonly string DeleteGroupMessages = "DELETE FROM `MessageBase` WHERE `ChatID` = @CI";
        public static readonly string LoadProfile = "SELECT * FROM `ProfileBase` WHERE `Username` = @UN";
        public static readonly string SaveProfile = "UPDATE `ProfileBase` SET `Username` = @UN ,`Name` = @U , `Description` = @D WHERE `Username`= @UI";
        public static readonly string SaveLoginData = "UPDATE `AuthBase` SET `Username` = @UN, `Name`= @U WHERE `Username` = @UI";
        public static readonly string AddAccount = "INSERT INTO `authbase` (`Username`, `Name`, `Password`) VALUES (@Username , @Name, @Password)";
        public static readonly string AddProfileData = "INSERT INTO `ProfileBase` (`Username`, `Name`) VALUES (@UN , @N)";
        public static readonly string LoadMembers = "SELECT pb.Username, pb.Name FROM `Contactbase` cb JOIN `ProfileBase` pb ON cb.Member = pb.Username WHERE cb.ChatID = @CI";
    }
}
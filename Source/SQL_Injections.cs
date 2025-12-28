namespace Neuron
{
    public class SQL_Injections
    {
        public static readonly string GetMessages = "SELECT * FROM `MessageBase` WHERE `ChatID` = @CI ORDER BY Date ASC, Time ASC";
        public static readonly string SendMessage = "INSERT INTO `MessageBase` ( `ChatID`,`Sender`, `Message`, `Time`, `Date`) VALUES (@CI ,@S, @M, @T, @D )";
        public static readonly string GetContacts = "SELECT * FROM `ContactBase` WHERE `Member` = @Username";
        public static readonly string LeaveGroup = "DELETE FROM `ContactBase` WHERE `Member` = @ME AND `ChatID` = @CI";
        public static readonly string DeleteGroup = "DELETE FROM `ContactBase` WHERE `ChatID` = @CI";
        public static readonly string DeleteGroupMessages = "DELETE FROM `MessageBase` WHERE `ChatID` = @CI";
        public static readonly string LoadProfile = "SELECT * FROM `ProfileBase` WHERE `Username` = @UN";
        public static readonly string SaveProfile = "UPDATE `ProfileBase` SET `Username` = @UN ,`Name` = @N , `Description` = @D WHERE `UserID`= @UI";
        public static readonly string SaveLoginData = "UPDATE `AuthBase` SET `Username` = @UN, `Name`= @U WHERE `UserID` = @UI";
        public static readonly string AddAccount = "INSERT INTO `authbase` (`Username`, `Name`, `Password`) VALUES (@Username , @Name, @Password)";
        public static readonly string AddProfileData = "INSERT INTO `ProfileBase` (`Username`, `Name`) VALUES (@UN , @N)";
    }
}
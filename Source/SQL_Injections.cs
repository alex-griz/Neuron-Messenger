namespace Neuron
{
    public class SQL_Injections
    {
        public static readonly string GetMessages = "SELECT * FROM `MessageBase` WHERE `ChatID` = @CI ORDER BY Date ASC, Time ASC";
        public static readonly string SendMessage = "INSERT INTO `MessageBase` ( `ChatID`,`Sender`, `Message`, `Time`, `Date`) VALUES (@CI ,@S, @M, @T, @D )";
        public static readonly string GetContacts = "SELECT * FROM `contactbase` WHERE `Member` = @Username";
        public static readonly string LeaveGroup = "DELETE FROM `ContactBase` WHERE `Member` = @ME AND `ChatID` = @CI";
        public static readonly string DeleteGroup = "DELETE FROM `ContactBase` WHERE `ChatID` = @CI";
        public static readonly string DeleteGroupMessages = "DELETE FROM `MessageBase` WHERE `ChatID` = @CI";
    }
}

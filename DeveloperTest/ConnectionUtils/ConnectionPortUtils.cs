namespace DeveloperTest.ConnectionUtils
{
    public enum Protocols
    {
        IMAP,
        POP3
    }

    public enum EncryptionTypes
    {
        Unencrypted,
        SSLTLS,
        STARTTLS
    }

    public class ConnectionPortUtils
    {
        public static string GetDefaultPortForProtocol(Protocols protocol, EncryptionTypes encryptionType)
        {
            if (protocol == Protocols.IMAP && (encryptionType == EncryptionTypes.SSLTLS || encryptionType == EncryptionTypes.STARTTLS))
                return "993";
            if (protocol == Protocols.IMAP && encryptionType == EncryptionTypes.Unencrypted)
                return "143";

            if (protocol == Protocols.POP3 && (encryptionType == EncryptionTypes.SSLTLS || encryptionType == EncryptionTypes.STARTTLS))
                return "995";
            if (protocol == Protocols.POP3 && encryptionType == EncryptionTypes.Unencrypted)
                return "110";

            return "-1";

        }
    }
}
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
        public static string GetDefaultPortForProtocol(Protocols protocol)
        {
            if (protocol == Protocols.IMAP)
                return "913";
            if (protocol == Protocols.POP3)
                return "110";
            return "-1";

        }
    }
}
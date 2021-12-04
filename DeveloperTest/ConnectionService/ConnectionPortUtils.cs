namespace DeveloperTest.ConnectionService
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
                return Limilabs.Client.IMAP.Imap.DefaultSSLPort.ToString();
            if (protocol == Protocols.IMAP && encryptionType == EncryptionTypes.Unencrypted)
                return Limilabs.Client.IMAP.Imap.DefaultPort.ToString();

            if (protocol == Protocols.POP3 && (encryptionType == EncryptionTypes.SSLTLS || encryptionType == EncryptionTypes.STARTTLS))
                return Limilabs.Client.POP3.Pop3.DefaultSSLPort.ToString();
            if (protocol == Protocols.POP3 && encryptionType == EncryptionTypes.Unencrypted)
                return Limilabs.Client.POP3.Pop3.DefaultPort.ToString();

            return "-1";

        }
    }
}
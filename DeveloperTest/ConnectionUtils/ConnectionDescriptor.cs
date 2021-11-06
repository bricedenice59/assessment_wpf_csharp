namespace DeveloperTest.ConnectionUtils
{
    public class ConnectionDescriptor
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public EncryptionTypes EncryptionType { get; set; }
        public Protocols MailProtocol { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
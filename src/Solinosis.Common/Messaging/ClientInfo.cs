using System;

namespace Solinosis.Common.Messaging
{
    [Serializable]
    public class ClientInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
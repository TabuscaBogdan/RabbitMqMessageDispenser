using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Models
{
    [ProtoContract]
    public class Publication
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public string PublisherId { get; set; }
        [ProtoMember(3)]
        public DateTime Timestamp { get; set; }
        [ProtoMember(4)]
        public string Contents { get; set; }

        [ProtoMember(5)]
        public string SubscriptionMatchId { get; set; }

        public override string ToString()
        {
            return $"{PublisherId} {Timestamp.ToLongTimeString()} {Contents}";
        }
    }
}

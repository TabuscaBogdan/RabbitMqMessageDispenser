using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Models
{
    [ProtoContract]
    public class Subscription
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public string SenderId { get; set; }
        [ProtoMember(3)]
        public string Filter { get; set; }
        [ProtoMember(4)]
        public int ForwardNumber { get; set; }

        public override string ToString()
        {
            return $"{SenderId} {Filter}";
        }
    }
}

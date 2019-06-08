using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Models
{
    public class Publication
    {
        public string Id { get; set; }
        public string PublisherId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Contents { get; set; }

        public string SubscriptionMatchId { get; set; }

        public override string ToString()
        {
            return $"{PublisherId} {Timestamp.ToLongTimeString()} {Contents}";
        }
    }
}

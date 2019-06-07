using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Models
{
    public class Subscription
    {
        public string Id { get; set; }
        public string SenderId { get; set; }
        public string Filter { get; set; }
        public override string ToString()
        {
            return $"{SenderId} {Filter}";
        }
    }
}

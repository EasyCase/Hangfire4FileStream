using System;
using System.Collections.Generic;
using System.Text;

namespace Hangfire4FileStream
{
    public class UserInfo
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public override string ToString()
        {
            return string.Format($"UserId:{UserId}，UserName:{UserName}");
        }
    }
}

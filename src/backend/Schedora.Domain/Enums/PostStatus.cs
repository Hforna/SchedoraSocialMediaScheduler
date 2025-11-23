using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Enums
{
    public enum PostStatus
    {
        Draft = 0,
        Scheduled = 1,
        Publishing = 2,
        Published = 3,
        Failed = 4,
        Cancelled = 5
    }
}

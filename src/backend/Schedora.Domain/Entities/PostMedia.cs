using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class PostMedia : Entity
    {
        public long PostId { get; private set; }
        public long MediaId { get; private set; }
        public int OrderIndex { get; private set; }
        public string? AltText { get; private set; }
        public string? PlatformSpecificSettings
        {
            get; private set;
        }
    }
}

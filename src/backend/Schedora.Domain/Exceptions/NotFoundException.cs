using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string error) : base(error)
        {
        }
    }
}

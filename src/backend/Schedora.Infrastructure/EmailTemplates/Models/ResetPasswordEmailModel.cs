using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Infrastructure.EmailTemplates.Models;

public class ResetPasswordEmailModel
{
    public string UserName { get; set; }
    public string CompanyName { get; set; }
    public string ResetPasswordUrl { get; set; }
    public int ExpirationHours { get; set; }
}


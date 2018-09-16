using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.Enums
{
    public enum NumberTokenType
    {
        PhoneNumberToken
    }

    public enum URLTokenType
    {
        PhoneNumberToken,
        EmailToken,
        ShortToken,
        JwtToken
    }
}

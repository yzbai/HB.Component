using HB.Component.Identity.Entity;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HB.Component.Identity
{
    public enum IdentityError
    {
        InnerError = 0,
        Succeeded = 1,
        NotFound = 2,
        AlreadyExists = 3,
        ArgumentError = 4,
        MobileAlreadyTaken = 5,
        LoginNameAlreadyTaken = 6,
        EmailAlreadyTaken = 7,
        Thrown = 8
    }
}
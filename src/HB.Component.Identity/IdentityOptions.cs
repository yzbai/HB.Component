using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.Identity
{
    public class IdentityOptions : IOptions<IdentityOptions>
    {
        public IdentityOptions Value { get { return this; } }


        //TODO: 考虑是否需要在SecurityStamp改变后，删除SignInToken？
        public IdentityEvents Events { get; set; } = new IdentityEvents();
    }
}

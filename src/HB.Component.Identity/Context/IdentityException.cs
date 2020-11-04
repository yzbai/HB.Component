using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HB.Component.Identity
{
    public class IdentityException : ServerException
    {
        public IdentityException(string? message) : base(message)
        {
        }

        public IdentityException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public IdentityException()
        {
        }

        public IdentityException(ServerErrorCode errorCode, string? message) : base(errorCode, message)
        {
        }

        public IdentityException(ServerErrorCode errorCode, string? message, Exception? innerException) : base(errorCode, message, innerException)
        {
        }

        public IdentityException(ServerErrorCode errorCode) : base(errorCode)
        {
        }
    }
}

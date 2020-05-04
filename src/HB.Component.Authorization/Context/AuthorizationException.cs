using HB.Component.Authorization.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HB.Component.Authorization
{
    public class AuthorizationException : FrameworkException
    {
        private IDictionary? _data;

        public override FrameworkExceptionType ExceptionType { get => FrameworkExceptionType.Authorization; }

        public AuthorizationError Error { get; private set; }

        public string? Operation { get; private set; }

        public AuthorizationException(AuthorizationError error, string message, Exception? innerException= null, [CallerMemberName]string operation="")
            :this(message, innerException)
        {
            Error = error;
            Operation = operation;
        }

        public override IDictionary Data
        {
            get
            {
                if (_data == null)
                {
                    _data = base.Data;
                }

                _data["SignInError"] = Error.ToString();
                _data["Operation"] = Operation;

                return _data;
            }
        }

        public AuthorizationException()
        {
        }

        public AuthorizationException(string? message) : base(message)
        {
        }

        public AuthorizationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

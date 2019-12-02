using HB.Component.Authorization.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HB.Component.Authorization
{
    public class AuthorizationException : Exception
    {
        private IDictionary _data;

        public AuthorizationError Error { get; private set; }

        public string Operation { get; private set; }

        public AuthorizationException(AuthorizationError error, string message, Exception innerException= null, [CallerMemberName]string operation=""):base(message, innerException)
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
    }
}

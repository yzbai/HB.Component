using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HB.Component.Identity
{
    public class IdentityException : FrameworkException
    {
        private IDictionary? _data;

        public override FrameworkExceptionType ExceptionType { get => FrameworkExceptionType.Identity; }

        public IdentityError Error { get; private set; }

        public string? Operation { get; private set; }

        public IdentityException(IdentityError error, string message, Exception? innerException= null,  [CallerMemberName]string operation = "") : this(message, innerException)
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

                _data["IdentityError"] = Error.ToString();
                _data["Operation"] = Operation;

                return _data;
            }
        }

        public IdentityException()
        {
        }

        public IdentityException(string? message) : base(message)
        {
        }

        public IdentityException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HB.Component.Identity
{
    public class IdentityException : Exception
    {
        private IDictionary _data;

        public IdentityError Error { get; private set; }

        public string Operation { get; private set; }

        public IdentityException(IdentityError error, string message, Exception innerException= null,  [CallerMemberName]string operation = "") : base(message, innerException)
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
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace RandomVariable
{
    public sealed class MathParserException : Exception
    {
        public MathParserException()
        {
        }

        public MathParserException(string message) : base(message)
        {
        }

        public MathParserException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

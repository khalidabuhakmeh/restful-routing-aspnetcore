using System;

namespace RestfulRouting.Exceptions
{
    public class NotRegisteredFormatException : Exception
    {
        public NotRegisteredFormatException()
        {
        }

        public NotRegisteredFormatException(string message)
            : base(message)
        {
        }

        public NotRegisteredFormatException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
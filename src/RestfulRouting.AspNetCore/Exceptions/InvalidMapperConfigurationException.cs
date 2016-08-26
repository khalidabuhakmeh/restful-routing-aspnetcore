using System;

namespace RestfulRouting.Exceptions
{
    public class InvalidMapperConfigurationException : Exception
    {
        public InvalidMapperConfigurationException()
        {
        }

        public InvalidMapperConfigurationException(string message) : base(message)
        {
        }

        public InvalidMapperConfigurationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
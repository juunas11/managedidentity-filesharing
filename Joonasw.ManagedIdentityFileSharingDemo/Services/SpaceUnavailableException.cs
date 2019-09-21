using System;
using System.Runtime.Serialization;

namespace Joonasw.ManagedIdentityFileSharingDemo.Services
{
    [Serializable]
    internal class SpaceUnavailableException : Exception
    {
        public SpaceUnavailableException()
        {
        }

        public SpaceUnavailableException(string? message) : base(message)
        {
        }

        public SpaceUnavailableException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SpaceUnavailableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
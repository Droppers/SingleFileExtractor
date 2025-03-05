using System;
using System.Runtime.Serialization;

namespace SingleFileExtractor.Core.Exceptions;

public class UnsupportedExecutableException : Exception
{
    public UnsupportedExecutableException() { }

    public UnsupportedExecutableException(string message) : base(message) { }

    public UnsupportedExecutableException(string message, Exception innerException) :
        base(message, innerException) { }

    protected UnsupportedExecutableException(SerializationInfo info, StreamingContext context) :
        base(info, context) { }
}
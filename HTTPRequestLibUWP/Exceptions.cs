using System;

namespace HTTPRequestLibUWP
{
    public class ExceptionHistory
    {
        private static Exception _lastException;

        public static Exception lastException
        {
            get
            {
                return _lastException;
            }
            internal set
            {
                _lastException = value;
            }
        }
    }

    public class HTTPGetRequestException : Exception
    {
        Exception[] attemptExceptionHistory;

        public HTTPGetRequestException(string message, Exception[] attempts) : base(message)
        {
            attemptExceptionHistory = attempts;
        }

        public HTTPGetRequestException(Exception[] attempts) : base("Get Request Failed")
        {
            attemptExceptionHistory = attempts;
        }

        public override string ToString()
        {
            string retString = base.ToString() + "\n\n[Exception History:";

            foreach (Exception ex in attemptExceptionHistory)
                retString += "\n" + ex.ToString();

            return retString + "]";

        }

    }

    public class HTTPDecodeException : Exception
    {

        public HTTPDecodeException(string message, Exception inner) : base(message, inner)
        {

        }

        public HTTPDecodeException(Exception inner) : base("Decoding HTTP Response Stream failed", inner)
        {

        }

    }
}

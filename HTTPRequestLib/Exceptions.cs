using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTPRequestLib
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
    public class HTTPGetCreateRequestException : Exception
    {
        Exception[] attemptExceptionHistory;

        public HTTPGetCreateRequestException(string message, Exception[] attempts) : base(message)
        {
            attemptExceptionHistory = attempts;
        }

        public HTTPGetCreateRequestException(Exception[] attempts) : base("Failed to create Get Request")
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

    public class HTTPPostCreateRequestException : Exception
    {
        Exception[] attemptExceptionHistory;

        public HTTPPostCreateRequestException(string message, Exception[] attempts) : base(message)
        {
            attemptExceptionHistory = attempts;
        }

        public HTTPPostCreateRequestException(Exception[] attempts) : base("Failed to create Post Request")
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

    public class HTTPResponseException : Exception
    {
        Exception[] attemptExceptionHistory;

        public HTTPResponseException(string message, Exception[] attempts) : base(message)
        {
            attemptExceptionHistory = attempts;
        }

        public HTTPResponseException(Exception[] attempts) : base("Retriving HTTP Response failed")
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


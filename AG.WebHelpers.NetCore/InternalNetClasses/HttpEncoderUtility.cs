using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AG.WebHelpers.InternalNetClasses
{
    public static class HttpEncoderUtility
    {
        public static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 48);
            }
            return (char)(n - 10 + 97);
        }

        public static bool IsUrlSafeChar(char ch)
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
            {
                return true;
            }
            if (ch != '!')
            {
                switch (ch)
                {
                    case '(':
                    case ')':
                    case '*':
                    case '-':
                    case '.':
                        return true;
                    case '+':
                    case ',':
                        break;
                    default:
                        if (ch == '_')
                        {
                            return true;
                        }
                        break;
                }
                return false;
            }
            return true;
        }
    }
}

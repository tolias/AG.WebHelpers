using System;
using System.Text;
using AG.WebHelpers.InternalNetClasses;

namespace AG.WebHelpers
{
    public static class CustomHttpUtility
    {
        /// <summary>Encodes a URL string.</summary>
        /// <returns>An encoded string.</returns>
        /// <param name="str">The text to encode. </param>
        public static string UrlEncode(string str, Func<char, bool> isUrlSafeCharChecker)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncode(str, Encoding.UTF8, isUrlSafeCharChecker);
        }

        /// <summary>Encodes a URL string using the specified encoding object.</summary>
        /// <returns>An encoded string.</returns>
        /// <param name="str">The text to encode. </param>
        /// <param name="e">The <see cref="T:System.Text.Encoding" /> object that specifies the encoding scheme. </param>
        public static string UrlEncode(string str, Encoding e, Func<char, bool> isUrlSafeCharChecker)
        {
            if (str == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e, isUrlSafeCharChecker));
        }

        /// <summary>Converts a string into a URL-encoded array of bytes using the specified encoding object.</summary>
        /// <returns>An encoded array of bytes.</returns>
        /// <param name="str">The string to encode </param>
        /// <param name="e">The <see cref="T:System.Text.Encoding" /> that specifies the encoding scheme. </param>
        public static byte[] UrlEncodeToBytes(string str, Encoding e, Func<char, bool> isUrlSafeCharChecker)
        {
            if (str == null)
            {
                return null;
            }
            byte[] bytes = e.GetBytes(str);
            return UrlEncode(bytes, 0, bytes.Length, isUrlSafeCharChecker);
        }

        public static byte[] UrlEncode(byte[] bytes, int offset, int count, Func<char, bool> isUrlSafeCharChecker)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                char c = (char)bytes[offset + i];
                if (c == ' ')
                {
                    num++;
                }
                else
                {
                    if (!isUrlSafeCharChecker(c))
                    {
                        num2++;
                    }
                }
            }
            if (num != 0 || num2 != 0)
            {
                byte[] array = new byte[count + num2 * 2];
                int num3 = 0;
                for (int j = 0; j < count; j++)
                {
                    byte b = bytes[offset + j];
                    char c2 = (char)b;
                    if (isUrlSafeCharChecker(c2))
                    {
                        array[num3++] = b;
                    }
                    else
                    {
                        if (c2 == ' ')
                        {
                            array[num3++] = 43;
                        }
                        else
                        {
                            array[num3++] = 37;
                            array[num3++] = (byte)HttpEncoderUtility.IntToHex(b >> 4 & 15);
                            array[num3++] = (byte)HttpEncoderUtility.IntToHex((int)(b & 15));
                        }
                    }
                }
                return array;
            }
            if (offset == 0 && bytes.Length == count)
            {
                return bytes;
            }
            byte[] array2 = new byte[count];
            Buffer.BlockCopy(bytes, offset, array2, 0, count);
            return array2;
        }
    }
}

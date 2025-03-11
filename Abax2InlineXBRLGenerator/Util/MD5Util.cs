using System.Text;

namespace Abax2InlineXBRLGenerator.Util;

/// <summary>
/// A util class to generate MD5 hash strings
/// </summary>
public class MD5Util
{
    /// <summary>
    /// Generates a MD5 hash string from another input string
    /// </summary>
    /// <param name="input">the string to use to generate the MD5</param>
    /// <returns>an MD5 string that represents the input string</returns>
    public static string CreateMD5(string input)
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
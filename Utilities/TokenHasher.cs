using System.Security.Cryptography;
using System.Text;

public static class TokenHasher
{
    public static string Hash(string input) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
}
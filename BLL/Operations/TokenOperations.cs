using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace BLL.Operations
{
    public class TokenOperations
    {
        // Generate a fixed length token
        public string GenerateToken(int numberOfBytes = 32)
        {
            return WebEncoders.Base64UrlEncode(GenerateRandomBytes(numberOfBytes));
        }
        // Generate a cryptographically secure array of bytes with a fixed length
        private static byte[] GenerateRandomBytes(int numberOfBytes)
        {
            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                byte[] byteArray = new byte[numberOfBytes];
                provider.GetBytes(byteArray);
                return byteArray;
            }
        }
    }
}
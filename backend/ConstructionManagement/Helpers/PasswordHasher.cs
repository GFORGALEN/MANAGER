using System.Security.Cryptography;
using System.Text;

namespace ConstructionManagement.Helpers
{
    public interface IPasswordHasher
    {
        string Hash(string value);

        bool Verify(string value, string hash);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToBase64String(bytes);
        }

        public bool Verify(string value, string hash)
        {
            return Hash(value) == hash;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.ValueObjects
{
    public class Token : ValueObject
    {
        public string Value { get; private set; }
        public DateTime ExpiresAt { get; private set; }

        private Token(string value, DateTime expiresAt)
        {
            Value = value;
            ExpiresAt = expiresAt;
        }

        public static Token Create(string value, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Token value cannot be empty");

            if (expiresAt <= DateTime.UtcNow)
                throw new ArgumentException("Token expiry must be in the future");

            return new Token(value, expiresAt);
        }

        public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return ExpiresAt;
        }
    }
}

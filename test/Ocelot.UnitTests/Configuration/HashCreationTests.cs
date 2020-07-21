using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;
using Xunit;

namespace Ocelot.UnitTests.Configuration
{
    public class HashCreationTests
    {
        [Fact]
        public void should_create_hash_and_salt()
        {
            var password = "secret";

            var salt = new byte[128 / 8];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var storedSalt = Convert.ToBase64String(salt);

            var storedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}

using HTFLMS.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace HTFLMS.Helper
{
    public class UHelper
    {
        public static string CreateJWT(User user, IConfiguration configuration)
        {
            var secretKey = configuration.GetSection("AppSettings:Key").Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var claims = new Claim[] {
            new Claim(ClaimTypes.Name,user.UserId),
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())
        };

            var signingCredentials = new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string ExtractIdentifierFromUrl(string url)
        {
            string filename = Path.GetFileNameWithoutExtension(url);
            return filename;
        }

        public static bool IsUrl(string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out Uri uri))
            {
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }
            else
            {
                return false;
            }
        }

        public static string ExtractExtensionFromBase64DataUri(string base64DataUri)
        {
            var regex = new Regex(@"data:(?<type>.*?);base64,");
            var match = regex.Match(base64DataUri);
            var type = match.Groups["type"].Value;
            var extension = "";

            switch (type)
            {
                case "image/png":
                    extension = "png";
                    break;
                case "image/jpeg":
                    extension = "jpeg";
                    break;
                case "image/jpg":
                    extension = "jpg";
                    break;
                default:
                    throw new ArgumentException("Unsupported file type");
            }

            return extension;
        }
        public int? ParseDesignationId(string designationValue)
        {
            if (string.IsNullOrWhiteSpace(designationValue))
                return null;

            designationValue = designationValue.Trim();

            int bracketIndex = designationValue.IndexOf('(');
            if (bracketIndex > 0)
                designationValue = designationValue.Substring(0, bracketIndex).Trim();

            if (int.TryParse(designationValue, out int id))
                return id;

            return null;
        }
        public static string GenerateUserId()
        {
            return $"HCC-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(10000, 99999)}";
        }

    }
}

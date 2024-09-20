using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using System;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Thinktecture.IdentityModel.Tokens;

namespace AngularJSAuthentication.Providers
{
    public class CustomJwtFormat : ISecureDataFormat<AuthenticationTicket>
    {

        private readonly string _issuer = string.Empty;

        public CustomJwtFormat(string issuer)
        {
            _issuer = issuer;
        }

        public string SignatureAlgorithm
        {
            get { return "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256"; }
        }

        public string DigestAlgorithm
        {
            get { return "http://www.w3.org/2001/04/xmlenc#sha256"; }
        }

        public string Protect(AuthenticationTicket data)
        {
            //if (data == null)
            //{
            //    throw new ArgumentNullException("data");
            //}

            string audienceId = ConfigurationManager.AppSettings["as:AudienceId"];

            string symmetricKeyAsBase64 = ConfigurationManager.AppSettings["as:AudienceSecret"];

            //var keyByteArray = TextEncodings.Base64Url.Decode(symmetricKeyAsBase64);

            //var signingKey = new HmacSigningCredentials(keyByteArray);

            var issued = data.Properties.IssuedUtc.Value.UtcDateTime;

            var expires = data.Properties.ExpiresUtc.Value.UtcDateTime;

            //var token = new JwtSecurityToken(_issuer, audienceId, data.Identity.Claims, issued.Value.UtcDateTime, expires.Value.UtcDateTime, signingKey);

            //var handler = new JwtSecurityTokenHandler();

            //var jwt = handler.WriteToken(token);

            //return jwt;

            if (data == null) throw new ArgumentNullException("data");

            var issuer = _issuer;
            //var audience = "all";
            var key = TextEncodings.Base64Url.Decode(symmetricKeyAsBase64);//Convert.FromBase64String(symmetricKeyAsBase64);

            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key);


            var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                                       securityKey,
                                        SignatureAlgorithm,
                                        DigestAlgorithm);
            var token = new JwtSecurityToken(issuer, audienceId, data.Identity.Claims,
                                             issued, expires, signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }
}
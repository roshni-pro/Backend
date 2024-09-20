
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API
{
    public class AuthRepository : IDisposable
    {
        private AuthContext _ctx;
        private UserManager<IdentityUser> _userManager;
        public AuthRepository()
        {
            //_ctx = new AuthContext();
            //_userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
        }
        public async Task<IdentityResult> RegisterUser(UserModel userModel)
        {
            using (_ctx = new AuthContext())
            {
                _userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
                IdentityUser user = new IdentityUser
                {
                    UserName = userModel.UserName,
                    Email = userModel.Email,
                    EmailConfirmed = false,
                    PhoneNumber = userModel.DepartmentId
                };
                var result = await _userManager.CreateAsync(user, userModel.Password);
                return result;
            }
        }
        public async Task<IdentityResult> UpdateUser(IdentityUser user)
        {
            IdentityResult user1 = null;
            try
            {
                using (_ctx = new AuthContext())
                {
                    _userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
                    user1 = await _userManager.UpdateAsync(user);
                }
            }
            catch (Exception ex) { }
            return user1;
        }
        public async Task<IdentityUser> ResetPassword(string userName, string password)
        {
            using (_ctx = new AuthContext())
            {
                _userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
                IdentityUser user = await _userManager.FindByEmailAsync(userName);
                var code = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
                var resetResult = await _userManager.ResetPasswordAsync(userName, code, password);
                return user;
            }
        }
        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            using (_ctx = new AuthContext())
            {
                _userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
                IdentityUser user = await _userManager.FindAsync(userName, password);

                return user;
            }
        }
        public Client FindClient(string clientId)
        {
            using (_ctx = new AuthContext())
            {
                var client = _ctx.Clients.Find(clientId);

                return client;
            }
        }
        public async Task<bool> AddRefreshToken(RefreshToken token)
        {
            using (_ctx = new AuthContext())
            {

                var existingToken = _ctx.RefreshTokens.Where(r => r.Subject == token.Subject && r.ClientId == token.ClientId).SingleOrDefault();

                if (existingToken != null)
                {
                    var result = await RemoveRefreshToken(existingToken);
                }

                _ctx.RefreshTokens.Add(token);
                return await _ctx.CommitAsync() > 0;
            }
        }
        public async Task<bool> RemoveRefreshToken(string refreshTokenId)
        {
            using (_ctx = new AuthContext())
            {

                var refreshToken = await _ctx.RefreshTokens.FindAsync(refreshTokenId);

                if (refreshToken != null)
                {
                    _ctx.RefreshTokens.Remove(refreshToken);
                    return await _ctx.CommitAsync() > 0;
                }

                return false;
            }
        }
        public async Task<bool> RemoveRefreshToken(RefreshToken refreshToken)
        {
            using (_ctx = new AuthContext())
            {

                _ctx.RefreshTokens.Remove(refreshToken);
                return await _ctx.CommitAsync() > 0;
            }
        }
        public async Task<RefreshToken> FindRefreshToken(string refreshTokenId)
        {
            using (_ctx = new AuthContext())
            {

                var refreshToken = await _ctx.RefreshTokens.FindAsync(refreshTokenId);

                return refreshToken;
            }
        }
        public List<RefreshToken> GetAllRefreshTokens()
        {
            using (_ctx = new AuthContext())
            {
                return _ctx.RefreshTokens.ToList();
            }
        }
        public async Task<IdentityUser> FindAsync(UserLoginInfo loginInfo)
        {
            using (_ctx = new AuthContext())
            {
                _userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));

                IdentityUser user = await _userManager.FindAsync(loginInfo);

                return user;
            }
        }
        public async Task<IdentityResult> CreateAsync(IdentityUser user)
        {
            using (_ctx = new AuthContext())
            {
                _userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
                var result = await _userManager.CreateAsync(user);

                return result;
            }
        }
        public async Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo login)
        {
            using (_ctx = new AuthContext())
            {
                _userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
                var result = await _userManager.AddLoginAsync(userId, login);

                return result;
            }
        }
        public void Dispose()
        {
            if (_userManager != null)
                _userManager.Dispose();

        }
    }
}
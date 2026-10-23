using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Security.Authentication;
using System.Security.Principal;
using System.Text;

namespace MauiApp2.Services
{
    public class AuthenticationService : IAuthenticationService
    {

        private readonly IConfigurationService _configurationService;

        private readonly IPreferencesService _preferencesService;
        private readonly IRemoteNotificationService _remoteNotificationService;
        private readonly ISecureStorageService _secureStorageService;
        private readonly IStorageService _storageService;
        private readonly ILocalDataUploadService _localDataUploadService;
        private IPublicClientApplication _pca = null;

        private readonly ILogger<AuthenticationService> _logger;

        /// <summary>
        /// The Service for holding common runtime property values 
        /// </summary>
        private readonly ICommonPlatformService _commonPlatformService;

        private readonly bool _useEmbeddedWindow = false;
        private readonly bool _forceRefresh = false;


        public AuthenticationService(ISecureStorageService secureStorageService, IRemoteNotificationService remoteNotificationService,
            IStorageService storageService, IPreferencesService preferencesService, IConfigurationService configurationService,
            ILocalDataUploadService localDataUploadService, ICommonPlatformService commonPlatformService, ILogger<AuthenticationService> logger)
        {
            _secureStorageService = secureStorageService;
            _remoteNotificationService = remoteNotificationService;
            _storageService = storageService;
            _preferencesService = preferencesService;
            _configurationService = configurationService;
            _localDataUploadService = localDataUploadService;
            _commonPlatformService = commonPlatformService;
            _logger = logger;
        }

        private IEnumerable<string> Scopes => new List<string> { $"{_configurationService.ConfigurationValues.ApimScope} offline_access" };

        private IPublicClientApplication PCA
        {
            get
            {
                if (_pca == null)
                {
                    if (_preferencesService.GetPreference("AppInitialized", string.Empty) == "Initialized")
                    {
                        _pca = PublicClientApplicationBuilder.Create(_configurationService.ConfigurationValues.ClientID)
                            .WithB2CAuthority(_configurationService.ConfigurationValues.AuthoritySignUpSignIn)
                            .WithRedirectUri($"msal{_configurationService.ConfigurationValues.ClientID}://auth")
                            .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                            .Build();
                    }
                }

                return _pca;
            }
        }


        public async Task<AuthenticationResult> B2CLoginSilentAsync()
        {
            try
            {
                // Get Account Policy like GMail, Facebook
                IEnumerable<IAccount> accounts = await PCA.GetAccountsAsync();

                AuthenticationResult ar = await PCA.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .WithB2CAuthority(_configurationService.ConfigurationValues.AuthoritySignUpSignIn)
                    .WithForceRefresh(CommonPlatformService.InitialPageLoad)
                    .ExecuteAsync();

                return ar;
            }

            catch (MsalUiRequiredException ex)
            {
                // Token is expired and user must login again
                return await B2CLoginAsync();
            }
            catch (MsalException ex) when (ex.Message.Contains("AADB2C90091") || ex.Message.Contains("AADB2C90273"))
            {
                return null;
            }
            catch (MsalException ex) when (ex.Message.Contains("AADB2C90118"))
            {
                AuthenticationResult ar = await ForgetPasswordAsync();
                return ar;
            }
            catch (MsalException ex) when (ex?.ErrorCode == "authentication_canceled")
            {
                return default;
            }
            catch (MsalException ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
                throw new AuthenticationException(ex.ToString());
            }
            catch (Exception ex) when (ex.Message.Contains("AADB2C90118"))
            {
                // forgot password clicked
                AuthenticationResult ar = await ForgetPasswordAsync();
                return ar;
            }
            catch (Exception ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
                throw new AuthenticationException(ex.Message);
            }
        }


        public async Task<AuthenticationResult> EditProfileAcquireTokenSilentAsync(string emailAddress)
        {
            try
            {
                IEnumerable<IAccount> accounts = await PCA.GetAccountsAsync();

                AuthenticationResult ar = await PCA.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .WithB2CAuthority(_configurationService.ConfigurationValues.AuthorityEditProfile)
                    .WithForceRefresh(_forceRefresh)
                    .ExecuteAsync();

                await SaveAuthenticationResultAsync(ar).ConfigureAwait(false);

                return ar;
            }

            catch (MsalUiRequiredException ex)
            {
                return await EditProfileAsync(emailAddress);
            }
            catch (MsalException ex) when (ex.Message.Contains("AADB2C90091") || ex.Message.Contains("AADB2C90273"))
            {
                return null;
            }
            catch (MsalException ex) when (ex?.ErrorCode == "authentication_canceled")
            {
                return default;
            }
            catch (MsalException ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
                throw new AuthenticationException(ex.ToString());
            }
            catch (Exception ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
                throw new AuthenticationException(ex.Message);
            }
        }

        public async Task<bool> SignInAsync()
        {
            try
            {
                var apimAuthResult = await B2CLoginSilentAsync();

                await _secureStorageService.SaveAccessToken(apimAuthResult.AccessToken).ConfigureAwait(false);
                await _secureStorageService.SaveIdToken(apimAuthResult.IdToken).ConfigureAwait(false);

                await SaveUserDetailsAsync(apimAuthResult);

                AuthenticationStatusHelper.IsDeviceUserLoggedOn = true;

                return true;
            }
            catch (Exception ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
                AuthenticationStatusHelper.IsDeviceUserLoggedOn = false;
                return false;
            }
        }

        public async Task SignOutAsync()
        {
            try
            {
                // Get Account Policy like GMail, Facebook
                IEnumerable<IAccount> accounts = await PCA.GetAccountsAsync();

                while (accounts.Any())
                {
                    await PCA.RemoveAsync(accounts.FirstOrDefault());
                    accounts = await PCA.GetAccountsAsync();
                }

                //Attempt to send any offline cases before they are wiped
                await _localDataUploadService.SendNow();

                // Remove all local cache & storage
                await _secureStorageService.RemoveAccessToken().ConfigureAwait(false);
                await _secureStorageService.RemoveIdToken().ConfigureAwait(false);
                await _secureStorageService.RemoveUserId().ConfigureAwait(false);
                await _storageService.RemoveUserCache().ConfigureAwait(false);

                AuthenticationStatusHelper.IsDeviceUserLoggedOn = false;
            }
            catch (Exception ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
            }
        }


        public async Task<bool> ValidateToken(string authToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authToken))
                {
                    authToken = await _secureStorageService.GetAccessToken();
                }

                if (string.IsNullOrWhiteSpace(authToken))
                {
                    await RemoveAllTokens();

                    AuthenticationStatusHelper.IsDeviceUserLoggedOn = false;

                    return false;
                }

                var expiryDate = CalculateTokenExpiration(authToken);

                if (expiryDate < DateTime.UtcNow)
                {
                    bool loginSuccess = await SignInAsync();
                    if (!loginSuccess)
                    {
                        await RemoveAllTokens();
                    }

                    AuthenticationStatusHelper.IsDeviceUserLoggedOn = loginSuccess;

                    return loginSuccess;
                }

                AuthenticationStatusHelper.IsDeviceUserLoggedOn = true;

                return true;
            }
            catch (Exception ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
            }

            return false;
        }



        public async Task<bool> ResetPasswordAsync()
        {
            try
            {
                var apimAuthResult = await this.ForgetPasswordAsync().ConfigureAwait(false);
                if (apimAuthResult is null)
                {
                    return false;
                }

                await SaveAuthenticationResultAsync(apimAuthResult).ConfigureAwait(false);

                AuthenticationStatusHelper.IsDeviceUserLoggedOn = true;

                return true;
            }
            catch (MsalException ex) when (ex.Message.Contains("AADB2C90091"))
            {
                //AADB2C90091 - User cancellation. User canceled the operation
                LoggingService<AuthenticationService>.LogError(ex);
                return false;
            }
            catch (MsalException ex) when (ex.Message.Contains("AADB2C90273"))
            {
                //AADB2C90273 generic in valid response
                LoggingService<AuthenticationService>.LogError(ex);
                return false;
            }
            catch (MsalException ex) when (ex?.ErrorCode == "authentication_canceled")
            {
                return false;
            }
            catch (Exception ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
                throw new AuthenticationException(ex.Message);
            }
        }


        public async Task<bool> DeleteAccountAsync()
        {
            try
            {
                AuthenticationResult apimAuthResult = await PCA.AcquireTokenInteractive(Scopes)
                    .WithB2CAuthority(_configurationService.ConfigurationValues.AuthorityDeleteAccount)
                    .WithUseEmbeddedWebView(_useEmbeddedWindow)
                    .WithParentActivityOrWindow(_commonPlatformService.UIWindow)
                    .ExecuteAsync();

                AuthenticationStatusHelper.IsDeviceUserLoggedOn = false;

                return true;
            }
            catch (MsalException ex) when (ex.Message.Contains("AADB2C90091") || ex.Message.Contains("AADB2C90273"))
            {
                LoggingService<AuthenticationService>.LogError(ex);
                return false;
            }
            catch (MsalException ex) when (ex?.ErrorCode == "authentication_canceled")
            {
                return false;
            }
            catch (Exception ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
                throw new AuthenticationException(ex.Message);
            }
        }


        private static DateTime CalculateTokenExpiration(string clientToken)
        {
            // Check for a signed-in user.
            if (string.IsNullOrEmpty(clientToken))
            {
                // Raise an exception if there is no token.
                throw new InvalidOperationException(
                "The client isn't signed-in or the token value isn't set.");
            }

            // Get just the JWT part of the token.
            var tokenParts = clientToken.Split('.');
            string jwt;
            if (tokenParts.Length >= 2)
            {
                jwt = tokenParts[1];
            }
            else
            {
                return DateTime.MinValue;
            }

            // Undo the URL encoding.
            jwt = jwt.Replace('-', '+');
            jwt = jwt.Replace('_', '/');
            switch (jwt.Length % 4)
            {
                case 0:
                    break;

                case 2:
                    jwt += "==";
                    break;

                case 3:
                    jwt += "=";
                    break;

                default:
                    throw new InvalidDataException("The base64url string is not valid.");
            }

            // Decode the bytes from base64 and write to a JSON string.
            var bytes = Convert.FromBase64String(jwt);
            var jsonString = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            // Parse as JSON object and get the exp field value, which is the expiration date as a
            // JavaScript primitive date.
            var jsonObj = JObject.Parse(jsonString);
            var exp = Convert.ToDouble(jsonObj["exp"].ToString());

            // Calculate the expiration by adding the exp value (in seconds) to the base date of 1/1/1970.
            var minTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var expire = minTime.AddSeconds(exp);

            return expire;
        }


        private async Task<AuthenticationResult> B2CLoginAsync()
        {
            try
            {
                AuthenticationResult ar = await PCA.AcquireTokenInteractive(Scopes)
                    .WithB2CAuthority(_configurationService.ConfigurationValues.AuthoritySignUpSignIn)
                    .WithUseEmbeddedWebView(_useEmbeddedWindow)
                    .WithParentActivityOrWindow(_commonPlatformService.UIWindow)
                    .ExecuteAsync();

                return ar;
            }
            catch (MsalUiRequiredException)
            {
                // Token is expired and user must login again
                return await B2CLoginAsync();
            }
            catch (MsalException ex) when (ex.Message.Contains("AADB2C90118"))
            {
                AuthenticationResult ar = await ForgetPasswordAsync();
                return ar;
            }
            catch (MsalException ex) when (ex.Message.Contains("AADB2C90091") || ex.Message.Contains("AADB2C90273"))
            {
                return null;
            }
            catch (MsalException ex) when (ex?.ErrorCode == "authentication_canceled")
            {
                throw new AuthenticationException("Authentication Canceled");
            }
            catch (MsalException ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
                throw new AuthenticationException(ex.ToString());
            }
            catch (Exception ex) when (ex.Message.Contains("AADB2C90118"))
            {
                // forgot password clicked
                AuthenticationResult ar = await ForgetPasswordAsync();
                return ar;
            }
            catch (Exception ex)
            {
                throw new AuthenticationException(ex.Message);
            }
        }


        private async Task<AuthenticationResult> EditProfileAsync(string emailAddress)
        {
            try
            {
                IEnumerable<IAccount> accounts = await PCA.GetAccountsAsync().ConfigureAwait(false);

                AuthenticationResult apimAuthResult = await PCA.AcquireTokenInteractive(Scopes)
                    .WithPrompt(Prompt.NoPrompt)
                    .WithAccount(accounts.FirstOrDefault())
                    .WithB2CAuthority(_configurationService.ConfigurationValues.AuthorityEditProfile)
                    .WithUseEmbeddedWebView(_useEmbeddedWindow)
                    .WithParentActivityOrWindow(_commonPlatformService.UIWindow)
                    .WithLoginHint(emailAddress)
                    .ExecuteAsync();

                if (apimAuthResult is null)
                {
                    return default;
                }

                await SaveAuthenticationResultAsync(apimAuthResult).ConfigureAwait(false);

                AuthenticationStatusHelper.IsDeviceUserLoggedOn = true;

                return apimAuthResult;
            }
            catch (MsalException ex) when (ex.Message.Contains("AADB2C90091") || ex.Message.Contains("AADB2C90273"))
            {
                LoggingService<AuthenticationService>.LogError(ex);
                return default;
            }
            catch (MsalException ex) when (ex?.ErrorCode == "authentication_canceled")
            {
                return default;
            }
            catch (Exception ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
                throw new AuthenticationException(ex.Message);
            }
        }

        private string Base64UrlDecode(string encodedString)
        {
            encodedString = encodedString.Replace('-', '+').Replace('_', '/');
            encodedString = encodedString.PadRight(encodedString.Length + ((4 - (encodedString.Length % 4)) % 4), '=');
            var byteArray = Convert.FromBase64String(encodedString);
            var decoded = Encoding.UTF8.GetString(byteArray, 0, byteArray.Count());
            return decoded;
        }

        private async Task<AuthenticationResult> ForgetPasswordAsync()
        {
            try
            {
                AuthenticationResult ar = await PCA.AcquireTokenInteractive(Scopes)
                    .WithB2CAuthority(_configurationService.ConfigurationValues.AuthorityPasswordReset)
                    .WithUseEmbeddedWebView(_useEmbeddedWindow)
                    .WithParentActivityOrWindow(_commonPlatformService.UIWindow)
                    .ExecuteAsync();

                return ar;
            }
            catch (MsalClientException ex) when (ex?.ErrorCode == "authentication_canceled")
            {
                return default;
            }
            catch (MsalException ex)
            {
                // change password canceled from Cancel button
                if (ex.Message.Contains("AADB2C90091"))
                    return null;
                else
                {
                    if (ex?.ErrorCode != "authentication_canceled")
                    {
                        throw new AuthenticationException("Authentication canceled");
                    }
                    else
                    {
                        throw new AuthenticationException(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
                throw new AuthenticationException(ex.Message);
            }
        }

        private IAccount GetAccountByPolicy(IEnumerable<IAccount> accounts, string policy)
        {
            try
            {
                foreach (var account in accounts)
                {
                    string userIdentifier = account.HomeAccountId.ObjectId.Split('.')[0];
                    if (userIdentifier.EndsWith(policy.ToLower(), StringComparison.InvariantCultureIgnoreCase)) return account;
                }
            }
            catch (Exception ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
            }

            return null;
        }

        private async Task RemoveAllTokens()
        {
            // Remove the tokens from cache so that user can re-login.
            await _secureStorageService.RemoveAccessToken().ConfigureAwait(false);
            await _secureStorageService.RemoveIdToken().ConfigureAwait(false);
            await _secureStorageService.RemoveUserId().ConfigureAwait(false);
            await _secureStorageService.RemoveUserDisplayName().ConfigureAwait(false);
        }

        private async Task SaveUserDetailsAsync(AuthenticationResult authResult, bool registerDevice = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authResult?.IdToken))
                {
                    return;
                }

                var idToken = new JwtSecurityToken(authResult.IdToken);
                var objectId = idToken.Payload.FirstOrDefault(x => x.Key == "sub");
                if (!objectId.Equals(new KeyValuePair<string, object>()))
                {
                    await _secureStorageService.SaveUserId(objectId.Value.ToString());
                    if (registerDevice)
                    {

                        // TODO

                        //Device.BeginInvokeOnMainThread(() =>
                        //{
                        //    _remoteNotificationService.Register(objectId.Value.ToString());
                        //});
                    }
                }

                var givenName = idToken.Payload.FirstOrDefault(x => x.Key == "given_name");
                var surName = idToken.Payload.FirstOrDefault(x => x.Key == "family_name");
                if (!givenName.Equals(new KeyValuePair<string, object>()) && !surName.Equals(new KeyValuePair<string, object>()))
                {
                    await _secureStorageService.SaveUserDisplayName((givenName.Value.ToString() + " " + surName.Value.ToString()).Trim());
                }
            }
            catch (Exception ex)
            {
                LoggingService<AuthenticationService>.LogError(ex);
            }
        }


        private async Task SaveAuthenticationResultAsync(AuthenticationResult apimAuthResult)
        {
            await _secureStorageService.SaveAccessToken(apimAuthResult.AccessToken).ConfigureAwait(false);
            await _secureStorageService.SaveIdToken(apimAuthResult.IdToken).ConfigureAwait(false);
            await SaveUserDetailsAsync(apimAuthResult).ConfigureAwait(false);
        }
    }

}

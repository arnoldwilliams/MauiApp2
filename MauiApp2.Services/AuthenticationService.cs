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

        private IEnumerable<string> Scopes => new List<string> { $"{_configurationService?.ConfigurationValues?.ApimScope} offline_access" };

        private IPublicClientApplication PCA
        {
            get
            {
                if (_pca == null)
                {
                    if (_preferencesService?.GetPreference("AppInitialized", string.Empty) == "Initialized")
                    {
                        var config = _configurationService?.ConfigurationValues;
                        if (config is null || string.IsNullOrWhiteSpace(config.ClientID))
                        {
                            return null;
                        }

                        _pca = PublicClientApplicationBuilder.Create(config.ClientID)
                            .WithB2CAuthority(config.AuthoritySignUpSignIn)
                            .WithRedirectUri($"msal{config.ClientID}://auth")
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
                var pca = PCA;
                if (pca is null)
                {
                    return null;
                }

                // Get Account Policy like GMail, Facebook
                IEnumerable<IAccount> accounts = await pca.GetAccountsAsync();

                AuthenticationResult ar = await pca.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
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
            catch (MsalException ex) when (ex?.Message?.Contains("AADB2C90091") == true || ex?.Message?.Contains("AADB2C90273") == true)
            {
                return null;
            }
            catch (MsalException ex) when (ex?.Message?.Contains("AADB2C90118") == true)
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
            catch (Exception ex) when (ex?.Message?.Contains("AADB2C90118") == true)
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
                var pca = PCA;
                if (pca is null)
                {
                    return null;
                }

                IEnumerable<IAccount> accounts = await pca.GetAccountsAsync();

                AuthenticationResult ar = await pca.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
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
            catch (MsalException ex) when (ex?.Message?.Contains("AADB2C90091") == true || ex?.Message?.Contains("AADB2C90273") == true)
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

                if (apimAuthResult is null)
                {
                    AuthenticationStatusHelper.IsDeviceUserLoggedOn = false;
                    return false;
                }

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
                var pca = PCA;
                if (pca is not null)
                {
                    // Get Account Policy like GMail, Facebook
                    IEnumerable<IAccount> accounts = await pca.GetAccountsAsync();

                    while (accounts.Any())
                    {
                        var account = accounts.FirstOrDefault();
                        if (account is null)
                        {
                            break;
                        }

                        await pca.RemoveAsync(account);
                        accounts = await pca.GetAccountsAsync();
                    }
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
            catch (MsalException ex) when (ex?.Message?.Contains("AADB2C90091") == true)
            {
                //AADB2C90091 - User cancellation. User canceled the operation
                LoggingService<AuthenticationService>.LogError(ex);
                return false;
            }
            catch (MsalException ex) when (ex?.Message?.Contains("AADB2C90273") == true)
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
                var pca = PCA;
                if (pca is null)
                {
                    return false;
                }

                AuthenticationResult apimAuthResult = await pca.AcquireTokenInteractive(Scopes)
                    .WithB2CAuthority(_configurationService.ConfigurationValues.AuthorityDeleteAccount)
                    .WithUseEmbeddedWebView(_useEmbeddedWindow)
                    .WithParentActivityOrWindow(_commonPlatformService?.UIWindow)
                    .ExecuteAsync();

                AuthenticationStatusHelper.IsDeviceUserLoggedOn = false;

                return true;
            }
            catch (MsalException ex) when (ex?.Message?.Contains("AADB2C90091") == true || ex?.Message?.Contains("AADB2C90273") == true)
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
            var expToken = jsonObj["exp"];
            if (expToken is null || expToken.Type == JTokenType.Null)
            {
                throw new InvalidDataException("The token does not contain an 'exp' claim.");
            }

            var exp = Convert.ToDouble(expToken.ToString());

            // Calculate the expiration by adding the exp value (in seconds) to the base date of 1/1/1970.
            var minTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var expire = minTime.AddSeconds(exp);

            return expire;
        }


        private async Task<AuthenticationResult> B2CLoginAsync()
        {
            try
            {
                var pca = PCA;
                if (pca is null)
                {
                    return null;
                }

                AuthenticationResult ar = await pca.AcquireTokenInteractive(Scopes)
                    .WithB2CAuthority(_configurationService.ConfigurationValues.AuthoritySignUpSignIn)
                    .WithUseEmbeddedWebView(_useEmbeddedWindow)
                    .WithParentActivityOrWindow(_commonPlatformService?.UIWindow)
                    .ExecuteAsync();

                return ar;
            }
            catch (MsalUiRequiredException ex)
            {
                // Interactive login already required; retrying would recurse indefinitely.
                LoggingService<AuthenticationService>.LogError(ex);
                throw new AuthenticationException(ex.Message);
            }
            catch (MsalException ex) when (ex?.Message?.Contains("AADB2C90118") == true)
            {
                AuthenticationResult ar = await ForgetPasswordAsync();
                return ar;
            }
            catch (MsalException ex) when (ex?.Message?.Contains("AADB2C90091") == true || ex?.Message?.Contains("AADB2C90273") == true)
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
            catch (Exception ex) when (ex?.Message?.Contains("AADB2C90118") == true)
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
                var pca = PCA;
                if (pca is null)
                {
                    return default;
                }

                IEnumerable<IAccount> accounts = await pca.GetAccountsAsync().ConfigureAwait(false);

                AuthenticationResult apimAuthResult = await pca.AcquireTokenInteractive(Scopes)
                    .WithPrompt(Prompt.NoPrompt)
                    .WithAccount(accounts.FirstOrDefault())
                    .WithB2CAuthority(_configurationService.ConfigurationValues.AuthorityEditProfile)
                    .WithUseEmbeddedWebView(_useEmbeddedWindow)
                    .WithParentActivityOrWindow(_commonPlatformService?.UIWindow)
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
            catch (MsalException ex) when (ex?.Message?.Contains("AADB2C90091") == true || ex?.Message?.Contains("AADB2C90273") == true)
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
            if (encodedString is null)
            {
                return null;
            }

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
                var pca = PCA;
                if (pca is null)
                {
                    return default;
                }

                AuthenticationResult ar = await pca.AcquireTokenInteractive(Scopes)
                    .WithB2CAuthority(_configurationService.ConfigurationValues.AuthorityPasswordReset)
                    .WithUseEmbeddedWebView(_useEmbeddedWindow)
                    .WithParentActivityOrWindow(_commonPlatformService?.UIWindow)
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
                if (ex?.Message?.Contains("AADB2C90091") == true)
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
                if (accounts is null || string.IsNullOrEmpty(policy))
                {
                    return null;
                }

                foreach (var account in accounts)
                {
                    var objectId = account?.HomeAccountId?.ObjectId;
                    if (string.IsNullOrEmpty(objectId))
                    {
                        continue;
                    }

                    string userIdentifier = objectId.Split('.')[0];
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
                if (idToken.Payload is null)
                {
                    return;
                }

                var objectId = idToken.Payload.FirstOrDefault(x => x.Key == "sub");
                if (!objectId.Equals(new KeyValuePair<string, object>()) && objectId.Value is not null)
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
                if (!givenName.Equals(new KeyValuePair<string, object>()) && !surName.Equals(new KeyValuePair<string, object>())
                    && givenName.Value is not null && surName.Value is not null)
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
            if (apimAuthResult is null)
            {
                return;
            }

            await _secureStorageService.SaveAccessToken(apimAuthResult.AccessToken).ConfigureAwait(false);
            await _secureStorageService.SaveIdToken(apimAuthResult.IdToken).ConfigureAwait(false);
            await SaveUserDetailsAsync(apimAuthResult).ConfigureAwait(false);
        }
    }

}

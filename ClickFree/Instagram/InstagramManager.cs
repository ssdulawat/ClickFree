using ClickFree.Helpers;
using ClickFree.Properties;
using ClickFree.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ClickFree.Instagram
{
    class InstagramManager
    {
        public const string RedirectURL = "https://www.datalogixxmemory.com/";
        public const string AppID = "884907049081721";
        public const string AppSecret = "0f2e10f5334fea4860aa2b35e2a2e286";
        public const string OAuthURL = "https://api.instagram.com/oauth/authorize?client_id=" + AppID + "&redirect_uri=" + RedirectURL + "&scope=user_profile,user_media&response_type=code";
        // https://api.instagram.com/oauth/authorize?client_id=884907049081721&redirect_uri=https://www.datalogixxmemory.com/&scope=user_profile,user_media&response_type=code

        public const string AccessTokenURL = "https://api.instagram.com/oauth/access_token?client_id=" + AppID + "&client_secret=" + AppSecret + "&grant_type=authorization_code" + "&redirect_uri=" + RedirectURL + "&code=";
        // https://api.instagram.com/oauth/access_token?client_id=884907049081721&redirect_uri=https://www.datalogixxmemory.com/&grant_type=authorization_code&redirect_uri=https://www.datalogixxmemory.com/&code=

        public const string GetPicturesURL = "https://graph.facebook.com/v10.0/me/photos?fields=picture&limit={0}&access_token={1}";
        public const string GetPhotosURL = "https://graph.facebook.com/v10.0/me/photos";
        public const string GetVideosURL = "https://graph.facebook.com/v10.0/me/videos";
        public const string UserInfoURL = "https://graph.facebook.com/v10.0/me?access_token=";
        public const string UserAlbumsURL = "https://graph.facebook.com/v10.0/me/albums?access_token=";
        public const string UserAlbumPhotosURL = "https://graph.facebook.com/v10.0/{0}/photos";


        public static WebException LastRequestException { get; private set; }

        public static void Logout()
        {
            Settings.Default.FacebookCode = null;
            Settings.Default.FacebookAccessToken = null;

            Settings.Default.Save();
        }

        public static bool CheckNetworkConnection()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBoxWindow.ShowMessageBox("No Internet connection", 
                    "Please connect to internet and try again.", MessageBoxWindow.MessageBoxType.Error);

                return false;
            }

            return true;
        }

        public static bool CheckAuthorization(bool showForm = true)
        {
            if (!CheckNetworkConnection())
                return false;

            bool login = true;

            if (!string.IsNullOrWhiteSpace(Settings.Default.FacebookAccessToken))
            {
                UserInfoResult userInfo = GetRequest<UserInfoResult>(UserInfoURL + Settings.Default.FacebookAccessToken);
                if (userInfo != null && !string.IsNullOrEmpty(userInfo.Name))
                {
                    Settings.Default.FacebookUserName = userInfo.Name;
                    Settings.Default.Save();

                    login = false;
                }
            }

            if (showForm)
            {
                var result = InstagramLoginWindow.Show(login);

                if (result == InstagramLoginWindow.LoginState.LoggedOut)
                {
                    return InstagramLoginWindow.Show(true) == InstagramLoginWindow.LoginState.Success;
                }
                else
                {
                    return result == InstagramLoginWindow.LoginState.Success;
                }
            }
            else if (login)
                return InstagramLoginWindow.Show(true) == InstagramLoginWindow.LoginState.Success;
            else return true;
        }

        public static Task<bool> GetAccessCode(string authCode)
        {
            return Task.Run<bool>(() =>
            {
                bool result = false;

                if (!string.IsNullOrWhiteSpace(authCode))
                {
                    try
                    {
                        AccessTokenResult accessToken = GetRequest<AccessTokenResult>(AccessTokenURL + authCode);
                        if (accessToken != null && !string.IsNullOrWhiteSpace(accessToken.AccessToken))
                        {
                            Settings.Default.FacebookCode = authCode;
                            Settings.Default.FacebookAccessToken = accessToken.AccessToken;

                            UserInfoResult userInfo = GetRequest<UserInfoResult>(UserInfoURL + accessToken.AccessToken);
                            if (userInfo != null && !string.IsNullOrEmpty(userInfo.Name))
                            {
                                Settings.Default.FacebookUserName = userInfo.Name;

                                result = true;
                            }

                            Settings.Default.Save();
                        }
                    }
                    catch { /* add log*/}
                }

                return result;
            });
        }

        private static T GetRequest<T>(string url) where T : class
        {
            T result = null;

            try
            {
                LastRequestException = null;

                WebRequest request = WebRequest.Create(url);
                request.Method = "GET";

                using (WebResponse response = request.GetResponse())
                {
                    using (var dataStream = response.GetResponseStream())
                    {
                        result = SerializationManager.Deserialize<T>(dataStream);
                    }
                }
            }
            catch (WebException exception)
            {
                LastRequestException = exception;
            }
            catch { }

            return result;
        }
        

        [DataContract]
        public class AccessTokenResult
        {
            #region Properties

            [DataMember(Name = "access_token")]
            public string AccessToken { get; set; }

            #endregion
        }

        [DataContract]
        public class UserInfoResult
        {
            #region Properties

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "id")]
            public string Id { get; set; }

            #endregion
        }

    }
}

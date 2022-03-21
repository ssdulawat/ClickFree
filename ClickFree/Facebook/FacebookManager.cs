using ClickFree.Helpers;
using ClickFree.Properties;
using ClickFree.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ClickFree.Facebook
{
    public static class FacebookManager
    {
        #region Constants

        public const string RedirectURL = "https://www.datalogixxmemory.com/";
        public const string AppID = "131694084920793";
        public const string AppSecret = "4448ea782a48d0943eb6114313f7c50d";
        public const string OAuthURL = "https://facebook.com/dialog/oauth?client_id=" + AppID + "&redirect_uri=" + RedirectURL + "&scope=user_photos,user_videos&display=popup";
        public const string AccessTokenURL = "https://graph.facebook.com/oauth/access_token?client_id=" + AppID + "&redirect_uri=" + RedirectURL + "&client_secret=" + AppSecret + "&code=";
        public const string GetPicturesURL = "https://graph.facebook.com/v10.0/me/photos?fields=picture&limit={0}&access_token={1}";
        public const string GetPhotosURL = "https://graph.facebook.com/v10.0/me/photos";
        public const string GetVideosURL = "https://graph.facebook.com/v10.0/me/videos";
        public const string UserInfoURL = "https://graph.facebook.com/v10.0/me?access_token=";
        public const string UserAlbumsURL = "https://graph.facebook.com/v10.0/me/albums?access_token=";
        public const string UserAlbumPhotosURL = "https://graph.facebook.com/v10.0/{0}/photos";

        #endregion

        #region Properties

        public static WebException LastRequestException { get; private set; }

        #endregion

        #region Methods

        public static void Logout()
        {
            Settings.Default.FacebookCode = null;
            Settings.Default.FacebookAccessToken = null;

            Settings.Default.Save();
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

        public static Task<MediaResult[]> LoadALLPhotos(CancellationTokenSource token = null)
        {
            return Task.Run<MediaResult[]>(() =>
            {
                List<MediaResult> result = new List<MediaResult>();
                try
                {
                    result.AddRange(LoadImages(GetPhotosURL, token));

                    //load photos from albums
                    UserAlbumsContainerResult userAlbumsContainer = GetRequest<UserAlbumsContainerResult>(UserAlbumsURL + Settings.Default.FacebookAccessToken);
                    if (userAlbumsContainer?.Albums != null && userAlbumsContainer.Albums.Length > 0)
                    {
                        foreach (var album in userAlbumsContainer.Albums)
                        {
                            token?.Token.ThrowIfCancellationRequested();

                            result.AddRange(LoadImages(string.Format(UserAlbumPhotosURL, album.Id), token));
                        }
                    }

                    //remove dublicates
                    List<string> dublicates = new List<string>();
                    foreach (var item in result.ToArray())
                    {
                        if (dublicates.Contains(item.Id))
                        {
                            result.Remove(item);
                            continue;
                        }

                        dublicates.Add(item.Id);
                    }
                }
                catch (OperationCanceledException){ throw; }
                catch { /* add log*/}

                return result.ToArray();
            });
        }

        public static Task<MediaResult[]> LoadALLVideos(CancellationTokenSource token = null)
        {
            return Task.Run<MediaResult[]>(() =>
            {
                List<MediaResult> result = new List<MediaResult>();
                try
                {
                    result.AddRange(LoadVideos(token: token));
                    result.AddRange(LoadVideos("uploaded", token));
                }
                catch (OperationCanceledException) { throw; }
                catch { /* add log*/}

                return result.ToArray();
            });
        }

        public static bool CheckNetworkConnection()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBoxWindow.ShowMessageBox("No Internet connection", "Please connect to internet and try again.", MessageBoxWindow.MessageBoxType.Error);

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
                var result = FacebookLoginWindow.Show(login);

                if (result == FacebookLoginWindow.LoginState.LoggedOut)
                {
                    return FacebookLoginWindow.Show(true) == FacebookLoginWindow.LoginState.Success;
                }
                else
                {
                    return result == FacebookLoginWindow.LoginState.Success;
                }
            }
            else if (login)
                return FacebookLoginWindow.Show(true) == FacebookLoginWindow.LoginState.Success;
            else return true;
        }

        #endregion

        #region Private methods

        private static List<MediaResult> LoadVideos(string type = "tagged"/*uploaded*/, CancellationTokenSource token = null)
        {
            List<MediaResult> result = new List<MediaResult>();
            try
            {
                string after = null;
                string getImages = $"{GetVideosURL}?fields=source,thumbnails&type={type}&access_token={Settings.Default.FacebookAccessToken}";

                do
                {
                    string request = getImages;
                    if (!string.IsNullOrWhiteSpace(after))
                        request += $"&after={after}";

                    UserVideosContainerResult resultContainer = GetRequest<UserVideosContainerResult>(request);
                    if (resultContainer != null && resultContainer.Videos != null && resultContainer.Videos.Length > 0)
                    {
                        foreach (var data in resultContainer.Videos)
                        {
                            token?.Token.ThrowIfCancellationRequested();

                            if (data != null && !string.IsNullOrEmpty(data.Source) && data.Thumbnails?.Data != null && data.Thumbnails.Data.Length > 0)
                            {
                                result.Add(new MediaResult()
                                {
                                    Id = data.Id,
                                    Source = data.Source,
                                    Thumbnail = data.Thumbnails.Data?.FirstOrDefault()?.Uri,
                                    IsVideo = true
                                });
                            }
                        }

                        after = resultContainer.Paging?.Cursors?.After;
                    }
                    else after = null;
                }
                while (!string.IsNullOrWhiteSpace(after));
            }
            catch { /* add log*/}

            return result;
        }

        private static List<MediaResult> LoadImages(string url, CancellationTokenSource token = null)
        {
            List<MediaResult> result = new List<MediaResult>();
            try
            {
                string after = null;
                string getImages = $"{url}?fields=images,album,id,picture&access_token={Settings.Default.FacebookAccessToken}";

                do
                {
                    string request = getImages;
                    if (!string.IsNullOrWhiteSpace(after))
                        request += $"&after={after}";

                    UserImagesContainerResult resultContainer = GetRequest<UserImagesContainerResult>(request);
                    if (resultContainer != null && resultContainer.Images != null && resultContainer.Images.Length > 0)
                    {
                        foreach (var data in resultContainer.Images)
                        {
                            token?.Token.ThrowIfCancellationRequested();

                            if (data != null && data.Images.Length > 0)
                            {
                                result.Add(new MediaResult()
                                {
                                    Id = data.Id,
                                    Source = data.Images.First().Source,
                                    Thumbnail = data.Picture
                                });
                            }
                        }

                        after = resultContainer.Paging?.Cursors?.After;
                    }
                    else after = null;
                }
                while (!string.IsNullOrWhiteSpace(after));
            }
            catch { /* add log*/}

            return result;
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

        #endregion

        #region Nested types

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

        [DataContract]
        public class UserImagesContainerResult
        {
            #region Properties

            [DataMember(Name = "data")]
            public ImageData[] Images { get; set; }

            [DataMember(Name = "paging")]
            public PagingResult Paging { get; set; }
            #endregion

            #region Nested types

            [DataContract]
            public class ImageData
            {
                #region Properties

                [DataMember(Name = "album")]
                public AlbumInfoResult Album { get; set; }
                [DataMember(Name = "images")]
                public UserImageResult[] Images { get; set; }
                [DataMember(Name = "id")]
                public string Id { get; set; }
                [DataMember(Name = "picture")]
                public string Picture { get; set; }

                #endregion
            }

            #endregion
        }

        public interface IFacebookMediaResult
        {
            string Source { get; set; }
            string Id { get; set; }
        }

        public interface IFacebookImageResult: IFacebookMediaResult
        {
        }

        public interface IFacebookVideoResult : IFacebookMediaResult
        {
        }

        public class MediaResult
        {
            public string Thumbnail { get; set; }
            public string Source { get; set; }
            public string Id { get; set; }
            public bool IsVideo { get; set; }
        }

        [DataContract]
        public class UserImageResult: IFacebookImageResult
        {
            #region Properties

            [DataMember(Name = "height")]
            public int Height { get; set; }
            [DataMember(Name = "width")]
            public int Width { get; set; }
            [DataMember(Name = "source")]
            public string Source { get; set; }
            [IgnoreDataMember]
            public string Id { get; set; }

            #endregion
        }

        [DataContract]
        public class UserAlbumsContainerResult
        {
            #region Properties

            [DataMember(Name = "data")]
            public AlbumInfoResult[] Albums { get; set; }

            [DataMember(Name = "paging")]
            public PagingResult Paging { get; set; }

            #endregion
        }

        [DataContract]
        public class AlbumInfoResult
        {
            #region Properties

            [DataMember(Name = "id")]
            public string Id { get; set; }
            [DataMember(Name = "name")]
            public string Name { get; set; }
            [DataMember(Name = "created_time")]
            public string CreatedTime { get; set; }

            #endregion
        }

        [DataContract]
        public class PagingResult
        {
            #region Properties

            [DataMember(Name = "cursors")]
            public CursorsResult Cursors { get; set; }
            [DataMember(Name = "previous")]
            public string Previous { get; set; }
            [DataMember(Name = "next")]
            public string Next { get; set; }

            #endregion

            #region Nested types

            [DataContract]
            public class CursorsResult
            {
                #region Properties

                [DataMember(Name = "before")]
                public string Before { get; set; }
                [DataMember(Name = "after")]
                public string After { get; set; }

                #endregion
            }

            #endregion
        }

        [DataContract]
        public class UserVideosContainerResult
        {
            #region Properties

            [DataMember(Name = "data")]
            public UserVideoResult[] Videos { get; set; }

            [DataMember(Name = "paging")]
            public PagingResult Paging { get; set; }

            #endregion
        }

        [DataContract]
        public class UserVideoResult: IFacebookVideoResult
        {
            #region Properties

            [DataMember(Name = "source")]
            public string Source { get; set; }
            [DataMember(Name = "thumbnails")]
            public VideoThumbnailContainerResult Thumbnails { get; set; }
            [DataMember(Name = "id")]
            public string Id { get; set; }
            #endregion

            #region Nested types

            [DataContract]
            public class VideoThumbnailContainerResult
            {
                #region Properties

                [DataMember(Name = "data")]
                public VideoThumbnailResult[] Data { get; set; }

                #endregion
            }

            [DataContract]
            public class VideoThumbnailResult
            {
                #region Properties

                [DataMember(Name = "id")]
                public string Id { get; set; }
                [DataMember(Name = "scale")]
                public float Scale { get; set; }
                [DataMember(Name = "height")]
                public int Height { get; set; }
                [DataMember(Name = "width")]
                public int Width { get; set; }
                [DataMember(Name = "uri")]
                public string Uri { get; set; }
                [DataMember(Name = "is_preferred")]
                public bool IsPreferred { get; set; }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}

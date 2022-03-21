using ClickFree.Facebook;
using ClickFree.Properties;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace ClickFree.ViewModel
{
    public class OnboardingVM : NavigationVM
    {
        #region Ctor
        public OnboardingVM(INavigation navigation)
            : base(navigation)
        {
        }
        #endregion

        #region Fields

        #region Commands

        private ICommand mSubmitEmailCommand = null;
        private ICommand mSendEmailCommand = null;
        private ICommand mNextPageCommand = null;
        private ICommand mPrevPageCommand = null;
        private ICommand mSkipCommand = null;
        private ICommand mStartCommand = null;

        #endregion

        private string mEmail;
        private bool mEmailSubmitted = false;
        private int mCurrentPage = 0;

        #endregion

        #region Properties

        #region Commands
        public ICommand SkipCommand
        {
            get
            {
                if (mSkipCommand == null)
                {
                    mSkipCommand = new RelayCommand(() =>
                    {
                        base.NavigateTo(NavigateEnum.Main);
                    });
                }

                return mSkipCommand;
            }
        }

        public ICommand StartCommand
        {
            get
            {
                if (mStartCommand == null)
                {
                    mStartCommand = new RelayCommand(() =>
                    {
                        Settings.Default.OnBoardingPassed = true;
                        Settings.Default.Save();

                        base.NavigateTo(NavigateEnum.Main);
                    });
                }

                return mStartCommand;
            }
        }

        public ICommand NextPageCommand
        {
            get
            {
                if (mNextPageCommand == null)
                {
                    mNextPageCommand = new RelayCommand(() =>
                    {
                        CurrentPage++;
                    },
                    () =>
                    {
                        return CurrentPage < 2;// && (CurrentPage != 1 || EmailSubmitted);
                    });
                }

                return mNextPageCommand;
            }
        }

        public ICommand PrevPageCommand
        {
            get
            {
                if (mPrevPageCommand == null)
                {
                    mPrevPageCommand = new RelayCommand(() =>
                    {
                        CurrentPage--;
                    },
                    () =>
                    {
                        return CurrentPage > 0;
                    });
                }

                return mPrevPageCommand;
            }
        }

        public ICommand SubmitEmailCommand
        {
            get
            {
                if (mSubmitEmailCommand == null)
                {
                    mSubmitEmailCommand = new RelayCommand(() =>
                    {
                        if (FacebookManager.CheckNetworkConnection())
                        {
                            try
                            {
                                HttpWebRequest request = WebRequest.CreateHttp("https://api.sendgrid.com/v3/marketing/contacts");
                                request.ContentType = "application/json";
                                request.Method = "PUT";
                                request.Headers.Add("authorization", "Bearer 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");
                                using (var rStream = request.GetRequestStream())
                                {
                                    var rString = "{\"list_ids\":[\"3f131b5e-5d77-4dff-bc80-bb2ee76119d7\"],\"contacts\":[{\"email\":\"" + Email + "\"}]}";
                                    var rStringBytes = Encoding.UTF8.GetBytes(rString);

                                    rStream.Write(rStringBytes, 0, rStringBytes.Length);
                                }
                                using (var response = request.GetResponse())
                                {
                                    using (var responseStream = response.GetResponseStream())
                                    {
                                        StreamReader sr = new StreamReader(responseStream);

                                        var text = sr.ReadToEnd();
                                    }
                                }

                                EmailSubmitted = true;
                            }
                            catch
                            {
                            } 
                        }
                    },
                    () =>
                    {
                        return IsValidEmail(Email);
                    });
                }

                return mSubmitEmailCommand;
            }
        }

        public ICommand SendEmailCommand
        {
            get
            {
                if (mSendEmailCommand == null)
                {
                    mSendEmailCommand = new RelayCommand(() =>
                    {
                        Process.Start("mailto:wecare@clickfreebackup.com?subject=Click Free app question (Windows)");
                    });
                }

                return mSendEmailCommand;
            }
        }

        #endregion

        public bool EmailSubmitted { get => mEmailSubmitted; set => Set(ref mEmailSubmitted, value); }
        public int CurrentPage { get => mCurrentPage; set => Set(ref mCurrentPage, value); }
        public string Email
        {
            get => mEmail;
            set
            {
                if (Set(ref mEmail, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        #endregion

        #region Methods

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        #endregion
    }
}

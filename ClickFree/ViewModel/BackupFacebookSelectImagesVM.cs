using ClickFree.Facebook;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ClickFree.ViewModel
{
    public class BackupFacebookSelectImagesVM : NavigationVM
    {
        #region Nested types

        public class MediaContainerItem: ViewModelBase
        {
            #region Fields

            private ICommand mSelectCommand;

            private bool mbIsSelected = false;
            private bool mbIsDownloading = false;
            private bool mbIsFailed = false;
            private BitmapImage mImage = null;

            #endregion

            #region Properties

            public ICommand SelectCommand
            {
                get
                {
                    if (mSelectCommand == null)
                    {
                        mSelectCommand = new RelayCommand(() =>
                        {
                            IsSelected = !IsSelected;
                        });
                    }

                    return mSelectCommand;
                }
            }

            public FacebookManager.MediaResult Media { get; private set; }

            public BitmapImage ImageSource
            {
                get
                {
                    if (mImage == null)
                    {
                        IsFailed = false;
                        IsDownloading = true;
                    }
                    else
                    {
                        IsFailed = false;
                        IsDownloading = false;
                    }

                    return mImage;
                }

                set
                {
                    Set(ref mImage, value);
                }
            }

            public bool IsSelected { get => mbIsSelected; set => Set(ref mbIsSelected, value); }
            public bool IsDownloading { get => mbIsDownloading; set => Set(ref mbIsDownloading, value); }
            public bool IsFailed { get => mbIsFailed; set => Set(ref mbIsFailed, value); }

            #endregion

            #region Ctor

            public MediaContainerItem(FacebookManager.MediaResult media)
            {
                this.Media = media;
            }

            #endregion
        }

        #endregion

        #region Fields

        #region Commands

        private ICommand mSelectAllCommand = null;
        private ICommand mDeselectAllCommand = null;
        private ICommand mTransferCommand = null;

        #endregion

        private bool mbLoadingInProgress = false;
        private CancellationTokenSource mCancellationTokenSource = new CancellationTokenSource();

        #endregion

        #region Properties

        #region Commands

        public ICommand SelectAllCommand
        {
            get
            {
                if (mSelectAllCommand == null)
                {
                    mSelectAllCommand = new RelayCommand(() =>
                    {
                        foreach (var item in Items)
                            item.IsSelected = true;
                    }, 
                    ()=>
                    {
                        return Items.Where(i => i.IsSelected == false).FirstOrDefault() != null;
                    });
                }

                return mSelectAllCommand;
            }
        }

        public ICommand DeselectAllCommand
        {
            get
            {
                if (mDeselectAllCommand == null)
                {
                    mDeselectAllCommand = new RelayCommand(() =>
                    {
                        foreach (var item in Items)
                            item.IsSelected = false;
                    },
                    () =>
                    {
                        return Items.Where(i => i.IsSelected).FirstOrDefault() != null;
                    });
                }

                return mDeselectAllCommand;
            }
        }

        public ICommand TransferCommand
        {
            get
            {
                if (mTransferCommand == null)
                {
                    mTransferCommand = new RelayCommand(() =>
                    {
                        NavigateTo(NavigateEnum.BackupFacebookDest, Items.Where(i => i.IsSelected).Select(i => i.Media).ToArray());
                    },
                    () =>
                    {
                        return Items.Count > 0 && Items.Where(i => i.IsSelected).FirstOrDefault() != null;
                    });
                }

                return mTransferCommand;
            }
        }

        #endregion

        public bool LoadingInProgress
        {
            get
            {
                return mbLoadingInProgress;
            }
            set
            {
                Set(ref mbLoadingInProgress, value);
            }
        }

        public ObservableCollection<MediaContainerItem> Items { get; private set; } = new ObservableCollection<MediaContainerItem>();

        #endregion

        #region Ctor

        public BackupFacebookSelectImagesVM(INavigation navigation) : base(navigation)
        {
        }

        #endregion

        #region Overrides

        protected internal override async void Activated(object parameter)
        {
            try
            {
                Items.Clear();

                LoadingInProgress = true;

                foreach (var photo in await FacebookManager.LoadALLPhotos(mCancellationTokenSource))
                {
                    Items.Add(new MediaContainerItem(photo)
                    {
                        IsDownloading = true
                    });
                }

                foreach (var video in await FacebookManager.LoadALLVideos(mCancellationTokenSource))
                {
                    Items.Add(new MediaContainerItem(video)
                    {
                        IsDownloading = true
                    });
                }

                //download thumbnails
                await Task.Run(() =>
                {
                    try
                    {
                        foreach (var item in Items)
                        {
                            mCancellationTokenSource.Token.ThrowIfCancellationRequested();

                            using (WebClient wc = new WebClient())
                            {
                                try
                                {
                                    wc.DownloadProgressChanged += (s, e) =>
                                    {
                                        mCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                    };

                                    BitmapImage bi = new BitmapImage();

                                    bi.BeginInit();
                                    bi.CacheOption = BitmapCacheOption.OnLoad;
                                    bi.StreamSource = new MemoryStream(wc.DownloadData(new Uri(item.Media.Thumbnail, UriKind.RelativeOrAbsolute)));
                                    bi.EndInit();
                                    bi.Freeze();

                                    Dispatcher.CurrentDispatcher.Invoke(() => { item.ImageSource = bi; });
                                }
                                catch
                                {
                                    item.IsDownloading = false;
                                    item.IsFailed = true;
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        /**/
                    }
                    catch
                    {
                        /**/
                    }
                });
            }
            finally
            {
                LoadingInProgress = false;
            }
        }

        protected internal override void Deactivated()
        {
            mCancellationTokenSource?.Cancel();

            mCancellationTokenSource = new CancellationTokenSource();

            LoadingInProgress = false;
        }

        #endregion
    }
}

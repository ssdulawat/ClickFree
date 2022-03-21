using ClickFree.Helpers;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Media;
using ClickFree.Windows;

namespace ClickFree.ViewModel
{
    public class BackupToUSBSelectVM : NavigationVM
    {
        #region Nested types

        public abstract class IOInfoBase
        {
            #region Properties

            public string Name { get; set; }
            public string Path { get; set; }

            #endregion
        }

        public class DiskInfo : IOInfoBase
        {
        }

        public class DirectoryInfo : IOInfoBase
        {
        }

        public class ImageFileInfo : IOInfoBase
        {
        }

        public class VideoFileInfo : IOInfoBase
        {
        }

        public class BackInfo : IOInfoBase
        {
            #region Ctor

            public BackInfo()
            {
                Name = "...";
            }

            #endregion
        }

        public class RetryInfo : IOInfoBase
        {
            #region Ctor

            public RetryInfo()
            {
                Name = "Retry";
            }

            #endregion
        }

        public class HeaderInfo
        {
            #region Properties

            public string Name { get; set; }
            public string Path { get; set; }
            public bool IsStart { get; set; }

            #endregion
        }

        #endregion

        #region Fields

        private ICommand mGoToFolderCommand = null;
        private ICommand mSelectAllCommand = null;
        private ICommand mClearAllCommand = null;
        private ICommand mBackupCommand = null;
        private ICommand mDropCommand = null;
        private ICommand mDoDragCommand = null;
        private ICommand mDeleteFromSelectedCommand = null;
        private ICommand mHeaderClickCommand = null;
        private ICommand mAddToSelectedCommand = null;

        #endregion

        #region Properties

        #region Commands

        public ICommand AddToSelectedCommand
        {
            get
            {
                if (mAddToSelectedCommand == null)
                {
                    mAddToSelectedCommand = new RelayCommand<IOInfoBase>(o =>
                    {
                        AddToSelected(o);
                    });
                }

                return mAddToSelectedCommand;
            }
        }

        public ICommand GoToFolderCommand
        {
            get
            {
                if (mGoToFolderCommand == null)
                {
                    mGoToFolderCommand = new RelayCommand<IOInfoBase>(folder =>
                    {
                        if (folder is DirectoryInfo || folder is DiskInfo)
                        {
                            Navigate(folder.Path);
                        }
                        else if (folder is BackInfo)
                        {
                            Navigate(Path.GetDirectoryName(folder.Path));
                        }
                        else
                        {
                            AddToSelected(folder);
                        }
                    });
                }

                return mGoToFolderCommand;
            }
        }

        public ICommand SelectAllCommand
        {
            get
            {
                if (mSelectAllCommand == null)
                {
                    mSelectAllCommand = new RelayCommand(() =>
                    {
                        foreach (var item in CurrentDirList)
                            AddToSelected(item);
                    });
                }

                return mSelectAllCommand;
            }
        }

        public ICommand ClearAllCommand
        {
            get
            {
                if (mClearAllCommand == null)
                {
                    mClearAllCommand = new RelayCommand(() =>
                    {
                        SelectedDirList.Clear();
                    });
                }

                return mClearAllCommand;
            }
        }

        public ICommand BackupCommand
        {
            get
            {
                if (mBackupCommand == null)
                {
                    mBackupCommand = new RelayCommand(() =>
                    {
                        if (DriveManager.CheckAccess())
                        {
                            string toFolder = Path.Combine(DriveManager.SelectedUSBDrive.Name, Constants.WindowsBackupFolderName);

                            var ownerWindow = Application.Current.Windows[Application.Current.Windows.Count - 1];
                            BackupToClickFreeWindow window = new BackupToClickFreeWindow(SelectedDirList.Select(s => s.Path).ToList(), toFolder)
                            {
                                Owner = ownerWindow
                            };
                            window.ShowDialog();

                            if (window.SuccessfullyBackuped)
                            {
                                SelectedDirList.Clear();

                                NavigateTo(NavigateEnum.Main);
                            }
                        }
                    }, 
                    () =>
                    {
                         return SelectedDirList.Count > 0;
                    });
                }

                return mBackupCommand;
            }
        }

        public ICommand DropCommand
        {
            get
            {
                if (mDropCommand == null)
                {
                    mDropCommand = new RelayCommand<DragEventArgs>(o =>
                    {
                        var model = o.Data.GetData(Assembly.GetExecutingAssembly().GetType(o.Data.GetFormats()[0]));

                        if (model is IOInfoBase)
                        {
                            AddToSelected(model as IOInfoBase);
                        }
                    }, 
                    o =>
                    {
                         return true;
                    });
                }

                return mDropCommand;
            }
        }

        public ICommand DoDragCommand
        {
            get
            {
                if (mDoDragCommand == null)
                {
                    mDoDragCommand = new RelayCommand<MouseEventArgs>(o =>
                    {
                        var args = o as MouseEventArgs;

                        if (args.LeftButton == MouseButtonState.Pressed
                            && !(args.OriginalSource is System.Windows.Controls.Primitives.Thumb))
                        {
                            var lb = args.Source as ListBox;

                            if (lb != null && lb.SelectedItem != null && !(lb.SelectedItem is BackInfo))
                            {
                                DragDrop.DoDragDrop(lb, lb.SelectedItem, DragDropEffects.All);
                            }
                        }
                    },
                    o =>
                    {
                        return true;
                    });
                }

                return mDoDragCommand;
            }
        }

        public ICommand DeleteFromSelectedCommand
        {
            get
            {
                if (mDeleteFromSelectedCommand == null)
                {
                    mDeleteFromSelectedCommand = new RelayCommand<IOInfoBase>(o =>
                    {
                        SelectedDirList.Remove(o);
                    });
                }

                return mDeleteFromSelectedCommand;
            }
        }

        public ICommand HeaderClickCommand
        {
            get
            {
                if (mHeaderClickCommand == null)
                {
                    mHeaderClickCommand = new RelayCommand<MouseButtonEventArgs>(args =>
                    {
                        if ((args.OriginalSource as FrameworkElement)?.Tag is HeaderInfo hi)
                        {
                            if (hi.IsStart)
                                Navigate(null);
                            else
                            {
                                Navigate(hi.Path);
                            }
                        }
                    });
                }

                return mHeaderClickCommand;
            }
        }

        #endregion

        public ObservableCollection<HeaderInfo> Headers { get; set; } = new ObservableCollection<HeaderInfo>();
        public ObservableCollection<IOInfoBase> CurrentDirList { get; private set; } = new ObservableCollection<IOInfoBase>();
        public ObservableCollection<IOInfoBase> SelectedDirList { get; private set; } = new ObservableCollection<IOInfoBase>();
        public IOInfoBase CurrentDirSelectedItem { get; set; }
        public IOInfoBase SelectedDirSelectedItem { get; set; }

        #endregion

        #region Ctor

        public BackupToUSBSelectVM(INavigation navigation)
            : base(navigation)
        {
            Navigate(null);
        }

        #endregion

        #region Methods

        private void AddToSelected(IOInfoBase info)
        {
            if (info != null && !(info is BackInfo))
            {
                bool skip = false;
                foreach (var item in SelectedDirList)
                {
                    if (info.Path.StartsWith(item.Path, StringComparison.InvariantCultureIgnoreCase))
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip)
                {
                    foreach (var item in SelectedDirList.ToArray())
                    {
                        if (item.Path.StartsWith(info.Path, StringComparison.InvariantCultureIgnoreCase))
                        {
                            SelectedDirList.Remove(item);
                        }
                    }

                    SelectedDirList.Add(info);
                }
            }

            CommandManager.InvalidateRequerySuggested();
        }

        public void Navigate(string path)
        {
            CurrentDirList.Clear();
            Headers.Clear();

            Headers.Add(new HeaderInfo()
            {
                Name = "This PC",
                IsStart = true
            });
            
            if (string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        if (drive.DriveType == DriveType.Fixed)
                        {
                            CurrentDirList.Add(new DiskInfo()
                            {
                                Name = drive.Name,
                                Path = drive.RootDirectory.FullName
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    CurrentDirList.Clear();
                    CurrentDirList.Add(new RetryInfo()
                    {
                        Path = path
                    });
                }
            }
            else
            {
                CurrentDirList.Add(new BackInfo()
                {
                    Path = path
                });

                var result = FileManager.Search(path, new FileManager.SearchParameters(FileManager.SearchFilterEnum.ALL)
                {
                    IncludeSubFolders = false
                }, new System.Threading.CancellationToken());

                if (result != null)
                {
                    foreach (var dir in result.Directories)
                    {
                        CurrentDirList.Add(new DirectoryInfo()
                        {
                            Name = Path.GetFileName(dir),
                            Path = dir
                        });
                    }

                    var videos = FileManager.SearchParameters.GetFilters(FileManager.SearchFilterEnum.Videos, ".");
                    foreach (var file in result.Files)
                    {
                        if (videos.Contains(Path.GetExtension(file), StringComparer.InvariantCultureIgnoreCase))
                        {
                            CurrentDirList.Add(new VideoFileInfo()
                            {
                                Name = Path.GetFileNameWithoutExtension(file),
                                Path = file
                            });
                        }
                        else
                        {
                            CurrentDirList.Add(new ImageFileInfo()
                            {
                                Name = Path.GetFileNameWithoutExtension(file),
                                Path = file
                            });
                        }
                    }
                }

                string headerPath = null;

                foreach (var item in path.Split(new[] { Path.DirectorySeparatorChar, Path.VolumeSeparatorChar }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (headerPath == null)
                        headerPath = item + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar;
                    else
                        headerPath = Path.Combine(headerPath, item);

                    Headers.Add(new HeaderInfo()
                    {
                        Name = item,
                        Path = headerPath,
                        IsStart = false
                    });
                }
            }
        }

        #endregion
    }

    public class CurrentDirectoiesHeaderTemplate:DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            ScrollViewer sv = GetParent<ScrollViewer>(container);

            if (element != null && item is BackupToUSBSelectVM.HeaderInfo hi)
            {
                try
                {
                    if (hi.IsStart)
                    {
                        return element.FindResource("first") as DataTemplate;
                    }
                    else
                    {
                        return element.FindResource("middle") as DataTemplate;
                    }
                }
                finally
                {
                    sv?.ScrollToRightEnd();
                }

                
            }

            return null;
        }

        private static T GetParent<T>(DependencyObject currentObj) where T:DependencyObject
        {
            T result = null;

            if (currentObj != null)
            {
                var parent = System.Windows.Media.VisualTreeHelper.GetParent(currentObj);

                if (parent is T res)
                {
                    result = res;
                }
                else
                {
                    result = GetParent<T>(parent);
                }
            }

            return result;
        }

        private static ScrollViewer FindScrollViewer(DependencyObject root)
        {
            var queue = new Queue<DependencyObject>(new[] { root });

            do
            {
                var item = queue.Dequeue();

                if (item is ScrollViewer)
                    return (ScrollViewer)item;

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(item); i++)
                    queue.Enqueue(VisualTreeHelper.GetChild(item, i));
            } while (queue.Count > 0);

            return null;
        }
    }
}

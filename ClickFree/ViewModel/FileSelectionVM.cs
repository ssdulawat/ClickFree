using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickFree.ViewModel.Controls
{
    public class FileSelectionVM : VMBase
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

        #endregion

        #region Properties

        public ObservableCollection<IOInfoBase> CurrentDirList { get; private set; } = new ObservableCollection<IOInfoBase>();

        #endregion

        #region Ctor
        public FileSelectionVM()
        {
            Navigate(null);
        }

        #endregion

        #region Methods

        public void Navigate(string path)
        {
            CurrentDirList.Clear();
            CurrentDirList.Add(new BackInfo());

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

            }
        }

        #endregion
    }

}

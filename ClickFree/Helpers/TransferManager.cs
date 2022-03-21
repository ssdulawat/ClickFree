using ClickFree.Exceptions;
using ClickFree.Facebook;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using ZetaLongPaths;
using static ClickFree.Facebook.FacebookManager;
using static ClickFree.Helpers.FileManager;

namespace ClickFree.Helpers
{
    public class TransferManager
    {
        #region Nested types

        public class TransferStartInfo
        {
            public long TotalSize { get; internal set; }
            public long CurrentSize { get; internal set; }
            public int TotalFiles { get; internal set; }
            public int CurrentPosition { get; internal set; }
        }

        public class TransferProgressInfo : TransferStartInfo
        {
            #region Properties
            public string FileSource { get; internal set; }
            public string FileDestination { get; internal set; }
            #endregion
        }

        public class TransferFinishedInfo : TransferStartInfo
        {
            public FailedReason FailedReson { get; set; }
            public List<ErrorInfo> Errors { get; internal set; } = new List<ErrorInfo>();
        }

        public class ErrorInfo
        {
            #region Properties

            public string FilePath { get; private set; }
            public Exception Error { get; private set; }

            #endregion

            #region Ctor
            public ErrorInfo(string file, Exception e)
            {
                Error = e;
                FilePath = file;
            }
            #endregion
        }

        public enum FailedReason
        {
            None,
            AccessDenied,
            UsbNotFound,
            FolderNotFound,
            Cancelled,
            SearchFailed,
            NoInternet,
            Other
        }

        #endregion

        #region Properties

        public bool IsInProgress { get { return mTransferTask != null && mTransferTask.Status == TaskStatus.Running && mCancellationTokenSource!= null && !mCancellationTokenSource.IsCancellationRequested; } }

        #endregion

        #region Fields

        private CancellationTokenSource mCancellationTokenSource;
        private Task mTransferTask = null;
        private bool mbTransferToUSB = false;

        #endregion

        #region Events

        /// <summary>
        /// /*true - convert, false - leave as it is, null - cancel operation*/
        /// </summary>
        public event Func<SearchResult, bool?> AppleFormatDetected; 
        public event Action SearchStart;
        public event Action<SearchResult> SearchFinished;
        public event Action<TransferStartInfo> Start;
        public event Action<TransferProgressInfo> Progress;
        public event Action<TransferFinishedInfo> Finished;

        #endregion

        #region Ctor

        public TransferManager()
        {
        }

        #endregion

        #region Methods

        public async Task StartTransfer(string to, List<string> filesToTransfer, bool convertAppleFiles)
        {
            await CancelAsync();

            mCancellationTokenSource = new CancellationTokenSource();

            mTransferTask = Task.Run(() =>
            {
                TransferFilesToPC(to, filesToTransfer, convertAppleFiles);
            });
        }

        public async Task ScanAndTransfer(string from, string to, FileManager.SearchParameters searchParameters = null)
        {
            await CancelAsync();

            mCancellationTokenSource = new CancellationTokenSource();

            if (searchParameters == null)
                searchParameters = new FileManager.SearchParameters(FileManager.SearchFilterEnum.NOFILTER);

            mTransferTask = Task.Run(() =>
            {
                if (!ValidatePath(from, out mbTransferToUSB))
                {
                    var info = new TransferFinishedInfo()
                    {
                        FailedReson = FailedReason.Other,
                    };

                    info.Errors.Add(new ErrorInfo($"Invalid path :'{to}'", null));

                    OnFinished(info);

                    return;
                }

                bool convertAppleFiles = false;
                SearchResult searchResult = null;

                try
                {
                    OnSearchFinished(searchResult = FileManager.Search(from, searchParameters, mCancellationTokenSource.Token));

                    //check if operation cancelled
                    if (mCancellationTokenSource.IsCancellationRequested)
                    {
                        OnFinished(new TransferFinishedInfo()
                        {
                            FailedReson = FailedReason.Cancelled
                        });

                        return;
                    }

                    if (searchResult.AppleFormatDetected)
                    {
                        var result = OnAppleFormatDetected(searchResult);
                        if (result.HasValue)
                        {
                            convertAppleFiles = result.Value;
                        }
                        else
                        {
                            OnFinished(new TransferFinishedInfo()
                            {
                                FailedReson = FailedReason.Cancelled
                            });

                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var info = new TransferFinishedInfo()
                    {
                        FailedReson = FailedReason.Other,
                    };

                    info.Errors.Add(new ErrorInfo(from, ex));

                    OnFinished(info);

                    return;
                }

                TransferFilesToPC(to, searchResult.Files, convertAppleFiles);
            });
        }

        public async Task ScanAndBackup(List<string> from, string to, FileManager.SearchParameters searchParameters = null)
        {
            await CancelAsync();

            mCancellationTokenSource = new CancellationTokenSource();

            if (searchParameters == null)
                searchParameters = new FileManager.SearchParameters(FileManager.SearchFilterEnum.ALL);

            mTransferTask = Task.Run(() =>
            {
                if (!ValidatePath(to, out mbTransferToUSB))
                {
                    var info = new TransferFinishedInfo()
                    {
                        FailedReson = FailedReason.Other,
                    };

                    info.Errors.Add(new ErrorInfo($"Invalid path :'{to}'", null));

                    OnFinished(info);

                    return;
                }

                SearchResult searchResult = new SearchResult();

                try
                {
                    foreach (var f in from)
                    {
                        if (File.Exists(f))
                        {
                            searchResult.Files.Add(f);
                        }
                        else if (Directory.Exists(f))
                        {
                            var result = FileManager.Search(f, searchParameters, mCancellationTokenSource.Token);

                            if (searchResult == null)
                                searchResult = result;
                            else
                            {
                                searchResult.Directories.AddRange(result.Directories);
                                searchResult.Files.AddRange(result.Files);
                                searchResult.Errors.AddRange(result.Errors);
                                if (result.AppleFormatDetected)
                                {
                                    searchResult.AppleFormatDetected = true;
                                }
                            }
                        }
                    }

                    OnSearchFinished(searchResult);

                    //check if operation cancelled
                    if (mCancellationTokenSource.IsCancellationRequested)
                    {
                        OnFinished(new TransferFinishedInfo()
                        {
                            FailedReson = FailedReason.Cancelled
                        });

                        return;
                    }
                }
                catch (Exception ex)
                {
                    var info = new TransferFinishedInfo()
                    {
                        FailedReson = FailedReason.Other,
                    };

                    info.Errors.Add(new ErrorInfo(""/*from*/, ex));

                    OnFinished(info);

                    return;
                }

                BackupFilesToClickFree(to, searchResult.Files);
            });
        }

        public async Task BackupFromFacebookToSelectedPath(List<MediaResult> from, string to)
        {
            await CancelAsync();

            mCancellationTokenSource = new CancellationTokenSource();

            mTransferTask = Task.Run(() =>
            {
                if (!ValidatePath(to, out mbTransferToUSB))
                {
                    var info = new TransferFinishedInfo()
                    {
                        FailedReson = FailedReason.Other,
                    };

                    info.Errors.Add(new ErrorInfo($"Invalid path :'{to}'", null));

                    OnFinished(info);

                    return;
                }

                if (from == null)
                {
                    try
                    {
                        var photos = FacebookManager.LoadALLPhotos(mCancellationTokenSource).Result;
                        mCancellationTokenSource.Token.ThrowIfCancellationRequested();

                        var videos = FacebookManager.LoadALLVideos(mCancellationTokenSource).Result;
                        mCancellationTokenSource.Token.ThrowIfCancellationRequested();

                        from = new List<MediaResult>();
                        from.AddRange(photos);
                        from.AddRange(videos);
                    }
                    catch (OperationCanceledException)
                    {
                        var info = new TransferFinishedInfo()
                        {
                            FailedReson = FailedReason.Cancelled,
                        };

                        OnFinished(info);

                        return;
                    }
                    catch (Exception ex)
                    {
                        var info = new TransferFinishedInfo()
                        {
                            FailedReson = FailedReason.Other,
                        };

                        info.Errors.Add(new ErrorInfo(""/*from*/, ex));

                        OnFinished(info);

                        return;
                    }
                }

                TransferFromFacebookToClickFree(to, from);
            });
        }

        public async Task CancelAsync()
        {
            if (mTransferTask != null)
            {
                mCancellationTokenSource.Cancel();
                await mTransferTask;

                mTransferTask = null;
                mCancellationTokenSource = null;
            }

            mbTransferToUSB = false;
        }

        #endregion

        #region Private

        private bool ValidatePath(string path, out bool isUsbDrive)
        {
            isUsbDrive = false;

            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }
            else if(!Directory.Exists(Path.GetPathRoot(path)))
            {
                return false;
            }
            else if (DriveManager.SelectedUSBDrive != null 
                                && path.StartsWith(DriveManager.SelectedUSBDrive.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                isUsbDrive = true;
            }

            return true;
        }

        public bool CopyFile(string fromPath, string toPath, TransferProgressInfo progressInfo, int eachReadLength = 1024 * 1024, Action updated = null)
        {
            using (FileStream fromFile = new FileStream(fromPath, FileMode.Open, FileAccess.Read))
            {
                FileMode mode = FileMode.OpenOrCreate;
                if (File.Exists(toPath))
                    mode = FileMode.Truncate;

                using (FileStream toFile = new FileStream(toPath, mode, FileAccess.Write))
                {
                    int toCopyLength = 0;
                    if (eachReadLength < fromFile.Length)
                    {
                        byte[] buffer = new byte[eachReadLength];
                        long copied = 0;
                        while (copied <= fromFile.Length - eachReadLength)
                        {
                            if (mCancellationTokenSource.IsCancellationRequested)
                                return false;

                            toCopyLength = fromFile.Read(buffer, 0, eachReadLength);
                            fromFile.Flush();
                            toFile.Write(buffer, 0, eachReadLength);
                            toFile.Flush();

                            // Current position of flow
                            toFile.Position = fromFile.Position;
                            copied += toCopyLength;

                            //update size
                            progressInfo.CurrentSize += toCopyLength;
                            updated?.Invoke();
                        }
                        int left = (int)(fromFile.Length - copied);
                        toCopyLength = fromFile.Read(buffer, 0, left);
                        fromFile.Flush();
                        toFile.Write(buffer, 0, left);
                        toFile.Flush();

                        //update size
                        progressInfo.CurrentSize += toCopyLength;
                        updated?.Invoke();
                    }
                    else
                    {
                        byte[] buffer = new byte[fromFile.Length];
                        toCopyLength = fromFile.Read(buffer, 0, buffer.Length);
                        fromFile.Flush();
                        toFile.Write(buffer, 0, buffer.Length);
                        toFile.Flush();

                        //update size
                        progressInfo.CurrentSize += toCopyLength;
                        updated?.Invoke();
                    }
                }
            }

            return true;
        }

        private void TransferFilesToPC(string to, List<string> filesToTransfer, bool convertAppleFiles)
        {
            if (filesToTransfer != null && filesToTransfer.Count > 0)
            {
                TransferProgressInfo progressInfo = new TransferProgressInfo()
                {
                    TotalFiles = filesToTransfer.Count,
                    CurrentPosition = 1
                };

                TransferFinishedInfo finishedInfo = new TransferFinishedInfo()
                {
                    TotalFiles = filesToTransfer.Count
                };

                try
                {
                    List<string> otherFiles = null, appleFiles = null;

                    if (convertAppleFiles)
                    {
                        otherFiles = new List<string>(filesToTransfer.Count);
                        appleFiles = new List<string>(filesToTransfer.Count);

                        foreach (var file in filesToTransfer)
                        {
                            foreach (var ext in Constants.AppleFileExtensions)
                                if (file.EndsWith(ext, StringComparison.CurrentCultureIgnoreCase))
                                    appleFiles.Add(file);
                                else otherFiles.Add(file);

                            if (mCancellationTokenSource.Token.IsCancellationRequested)
                            {
                                finishedInfo.FailedReson = FailedReason.Cancelled;
                                OnFinished(finishedInfo);
                                return;
                            }
                        }
                    }
                    else
                    {
                        otherFiles = filesToTransfer;
                    }

                    try
                    {
                        //calc total files size
                        progressInfo.TotalSize = filesToTransfer.Select(s => new ZlpFileInfo(s).Length).Sum();
                        finishedInfo.TotalSize = progressInfo.TotalSize;
                    }
                    catch (DirectoryNotFoundException notFoundException)
                    {
                        CheckUSBDevice(notFoundException);
                    }
                    catch (UnauthorizedAccessException accessDenied)
                    {
                        CheckUSBDevice(accessDenied);
                    }
                    catch (IOException ioException)
                    {
                        CheckUSBDevice(ioException);
                    }

                    //raise start event
                    OnStart(new TransferStartInfo()
                    {
                        TotalFiles = progressInfo.TotalFiles,
                        TotalSize = progressInfo.TotalSize,
                        CurrentPosition = 1
                    });

                    if (!Directory.Exists(to))
                        Directory.CreateDirectory(to);

                    #region Common files
                    //transfer data
                    for (int i = 0, length = otherFiles.Count; i < length; i++, progressInfo.CurrentPosition++)
                    {
                        mCancellationTokenSource.Token.ThrowIfCancellationRequested();

                        string file = otherFiles[i];

                        try
                        {
                            string dest = Path.Combine(to, file.Substring(3, file.Length - 3));
                            string destFolder = Path.GetDirectoryName(dest);

                            if (!Directory.Exists(destFolder))
                                Directory.CreateDirectory(destFolder);

                            progressInfo.FileSource = file;
                            progressInfo.FileDestination = dest;
                            //progressInfo.CurrentSize += new FileInfo(file).Length;

                            if (File.Exists(dest)) //skip if the same size of the files
                            {
                                var fileSize = new ZlpFileInfo(file).Length;
                                if (fileSize != new ZlpFileInfo(dest).Length)
                                    CopyFile(file, dest, progressInfo, updated: () =>
                                    {
                                        OnProgress(progressInfo);
                                    });
                                else
                                {
                                    progressInfo.CurrentSize += fileSize;
                                }
                            }
                            else
                            {
                                CopyFile(file, dest, progressInfo, updated: ()=>
                                {
                                    OnProgress(progressInfo);
                                });
                            }
                        }
                        catch (OperationCanceledException operationCanceled)
                        {
                            throw operationCanceled;
                        }
                        catch (DirectoryNotFoundException notFoundException)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file, notFoundException));
                            /*LOG*/

                            CheckUSBDevice(notFoundException);
                        }
                        catch (UnauthorizedAccessException accessDenied)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file, accessDenied));
                            /*LOG*/

                            CheckUSBDevice(accessDenied);
                        }
                        catch (IOException ioException)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file, ioException));
                            /*LOG*/

                            CheckUSBDevice(ioException, false);
                        }
                        catch (Exception ex)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file, ex));
                            /*LOG*/

                            CheckUSBDevice(ex);
                        }
                        finally
                        {
                            OnProgress(progressInfo);
                        }
                    }

                    #endregion

                    #region Apple files
                    //apple format - convert to jpeg
                    if (appleFiles != null)
                    {
                        for (int i = 0, length = appleFiles.Count; i < length; i++, progressInfo.CurrentPosition++)
                        {
                            mCancellationTokenSource.Token.ThrowIfCancellationRequested();

                            string file = appleFiles[i];

                            try
                            {
                                string dest = Path.Combine(to, file.Substring(3, file.Length - 3));
                                string destFolder = Path.GetDirectoryName(dest);

                                if (!Directory.Exists(destFolder))
                                    Directory.CreateDirectory(destFolder);

                                progressInfo.FileSource = file;
                                progressInfo.FileDestination = dest;
                                progressInfo.CurrentSize += new FileInfo(file).Length;

                                using (var image = new MagickImage(file))
                                {
                                    // Sets the output format to jpeg
                                    image.Format = MagickFormat.Jpeg;
                                    
                                    // Create byte array that contains a jpeg file
                                    File.WriteAllBytes(GetUniqueJpegName(Path.Combine(System.IO.Path.GetDirectoryName(dest), Path.GetFileNameWithoutExtension(dest))), image.ToByteArray());
                                }
                            }
                            catch (OperationCanceledException operationCanceled)
                            {
                                throw operationCanceled;
                            }
                            catch (DirectoryNotFoundException notFoundException)
                            {
                                finishedInfo.Errors.Add(new ErrorInfo(file, notFoundException));
                                /*LOG*/

                                CheckUSBDevice(notFoundException);
                            }
                            catch (UnauthorizedAccessException accessDenied)
                            {
                                finishedInfo.Errors.Add(new ErrorInfo(file, accessDenied));
                                /*LOG*/

                                CheckUSBDevice(accessDenied);
                            }
                            catch (IOException ioException)
                            {
                                finishedInfo.Errors.Add(new ErrorInfo(file, ioException));
                                /*LOG*/

                                CheckUSBDevice(ioException, false);
                            }
                            catch (Exception ex)
                            {
                                finishedInfo.Errors.Add(new ErrorInfo(file, ex));
                                /*LOG*/

                                CheckUSBDevice(ex);
                            }
                            finally
                            {
                                OnProgress(progressInfo);
                            }
                        }
                    }

                    #endregion
                }
                catch (UsbDriveNotFoundException)
                {
                    finishedInfo.FailedReson = FailedReason.UsbNotFound;
                }
                catch (DirectoryNotFoundException)
                {
                    finishedInfo.FailedReson = FailedReason.FolderNotFound;
                }
                catch (UnauthorizedAccessException)
                {
                    finishedInfo.FailedReson = FailedReason.AccessDenied;
                }
                catch (OperationCanceledException)
                {
                    finishedInfo.FailedReson = FailedReason.Cancelled;
                }
                catch
                {
                    finishedInfo.FailedReson = FailedReason.Other;
                }
                finally
                {
                    finishedInfo.CurrentSize = progressInfo.CurrentSize;
                    finishedInfo.CurrentPosition = --progressInfo.CurrentPosition;

                    OnFinished(finishedInfo);
                }
            }
            else
            {
                OnFinished(new TransferFinishedInfo()
                {
                });
            }

            //helper
            string GetUniqueJpegName(string name, bool untilFindUnique = false)
            {
                int i = 0;
                string result = name + ".jpeg";

                while (File.Exists(result) && (untilFindUnique || i == 0))
                {
                    result = name + i + ".jpeg";

                    i++;
                }

                return result;
            }
        }

        private void BackupFilesToClickFree(string to, List<string> filesTobackup)
        {
            if (filesTobackup != null && filesTobackup.Count > 0)
            {
                TransferProgressInfo progressInfo = new TransferProgressInfo()
                {
                    TotalFiles = filesTobackup.Count,
                    CurrentPosition = 1
                };

                TransferFinishedInfo finishedInfo = new TransferFinishedInfo()
                {
                    TotalFiles = filesTobackup.Count
                };

                try
                {
                    List<string> copyToList = new List<string>();

                    string clickFree = Path.DirectorySeparatorChar + Constants.ClickFreeFolderName + Path.DirectorySeparatorChar;
                    string rootFolder = Path.GetDirectoryName(to);
                    foreach (string backupFile in filesTobackup)
                    {
                        string endPath;
                        if (backupFile.Contains(clickFree))
                        {
                            int index = backupFile.LastIndexOf(Constants.ClickFreeFolderName) + Constants.ClickFreeFolderName.Length;

                            endPath = backupFile.Substring(index, backupFile.Length - index);

                            if (endPath.StartsWith($"{Path.DirectorySeparatorChar}"))
                                endPath = endPath.Substring(1, endPath.Length - 1);

                            copyToList.Add(Path.Combine(rootFolder, endPath));
                        }
                        else
                        {
                            endPath = backupFile.Split(new[] { Path.VolumeSeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Last();

                            if (endPath.StartsWith($"{Path.DirectorySeparatorChar}"))
                                endPath = endPath.Substring(1, endPath.Length - 1);

                            copyToList.Add(Path.Combine(to, endPath));
                        }
                    }

                    //raise start event
                    OnStart(new TransferStartInfo()
                    {
                        TotalFiles = progressInfo.TotalFiles,
                        TotalSize = progressInfo.TotalFiles,
                        CurrentPosition = 1
                    });

                    //calculate total size
                    foreach (var file in filesTobackup)
                    {
                        try
                        {
                            //calc total files size
                            progressInfo.TotalSize += new ZlpFileInfo(file).Length;
                            finishedInfo.TotalSize = progressInfo.TotalSize;
                        }
                        catch (DirectoryNotFoundException notFoundException)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file, notFoundException));

                            CheckUSBDevice(notFoundException);
                        }
                        catch (UnauthorizedAccessException accessDenied)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file, accessDenied));

                            CheckUSBDevice(accessDenied);
                        }
                        catch (IOException ioException)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file, ioException));
                            /*LOG*/

                            CheckUSBDevice(ioException, false);
                        } 
                    }

                    //raise start event
                    OnStart(new TransferStartInfo()
                    {
                        TotalFiles = progressInfo.TotalFiles,
                        TotalSize = progressInfo.TotalSize,
                        CurrentPosition = 1
                    });

                    if (!Directory.Exists(to))
                        Directory.CreateDirectory(to);

                    #region Common files
                    //transfer data
                    for (int i = 0, length = filesTobackup.Count; i < length; i++, progressInfo.CurrentPosition++)
                    {
                        mCancellationTokenSource.Token.ThrowIfCancellationRequested();

                        string copyTo = copyToList[i];
                        string file = filesTobackup[i];

                        try
                        {
                            string destFolder = Path.GetDirectoryName(copyTo);

                            if (!Directory.Exists(destFolder))
                                Directory.CreateDirectory(destFolder);

                            progressInfo.FileSource = file;
                            progressInfo.FileDestination = copyTo;
                            //progressInfo.CurrentSize += new FileInfo(file).Length;

                            if (File.Exists(copyTo)) //skip if the same size of the files
                            {
                                var fileSize = new ZlpFileInfo(file).Length;
                                if (fileSize != new ZlpFileInfo(copyTo).Length)
                                    CopyFile(file, copyTo, progressInfo, updated: () =>
                                    {
                                        OnProgress(progressInfo);
                                    });
                                else
                                {
                                    progressInfo.CurrentSize += fileSize;
                                }
                            }
                            else
                            {
                                CopyFile(file, copyTo, progressInfo, updated: () =>
                                {
                                    OnProgress(progressInfo);
                                });
                            }
                        }
                        catch (OperationCanceledException operationCanceled)
                        {
                            throw operationCanceled;
                        }
                        catch (DirectoryNotFoundException notFoundException)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file, notFoundException));
                            /*LOG*/

                            CheckUSBDevice(notFoundException);
                        }
                        catch (UnauthorizedAccessException accessDenied)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file, accessDenied));
                            /*LOG*/

                            CheckUSBDevice(accessDenied);
                        }
                        catch (IOException ioException)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file, ioException));
                            /*LOG*/

                            CheckUSBDevice(ioException, false);
                        }
                        catch (Exception ex)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file, ex));
                            /*LOG*/

                            CheckUSBDevice(ex);
                        }
                        finally
                        {
                            OnProgress(progressInfo);
                        }
                    }

                    #endregion
                }
                catch (UsbDriveNotFoundException)
                {
                    finishedInfo.FailedReson = FailedReason.UsbNotFound;
                }
                catch (DirectoryNotFoundException)
                {
                    finishedInfo.FailedReson = FailedReason.FolderNotFound;
                }
                catch (UnauthorizedAccessException)
                {
                    finishedInfo.FailedReson = FailedReason.AccessDenied;
                }
                catch (OperationCanceledException)
                {
                    finishedInfo.FailedReson = FailedReason.Cancelled;
                }
                catch
                {
                    finishedInfo.FailedReson = FailedReason.Other;
                }
                finally
                {
                    finishedInfo.CurrentSize = progressInfo.CurrentSize;
                    finishedInfo.CurrentPosition = --progressInfo.CurrentPosition;

                    OnFinished(finishedInfo);
                }
            }
            else
            {
                OnFinished(new TransferFinishedInfo()
                {
                });
            }
        }

        private void TransferFromFacebookToClickFree(string to, List<MediaResult> filesToBackup)
        {
            if (filesToBackup != null && filesToBackup.Count > 0)
            {
                TransferProgressInfo progressInfo = new TransferProgressInfo()
                {
                    TotalFiles = filesToBackup.Count,
                    CurrentPosition = 1
                };

                TransferFinishedInfo finishedInfo = new TransferFinishedInfo()
                {
                    TotalFiles = filesToBackup.Count
                };

                try
                {
                    List<string> copyToList = new List<string>();

                    string clickFree = Path.DirectorySeparatorChar + Constants.ClickFreeFolderName + Path.DirectorySeparatorChar;
                    string rootFolder = Path.GetDirectoryName(to);
                    int index = 1;
                    foreach (MediaResult backupFile in filesToBackup)
                    {
                        try
                        {
                            Uri file = new Uri(backupFile.Source);

                            if (string.IsNullOrWhiteSpace(file.LocalPath))
                            {
                                if (backupFile.IsVideo)
                                    copyToList.Add(Path.Combine(to, $"{index}_{backupFile.Id}.mp4"));
                                else
                                    copyToList.Add(Path.Combine(to, $"{index}_{backupFile.Id}.jpg"));
                            }
                            else
                            {
                                var fileName = Path.GetFileName(file.LocalPath);
                                if (string.IsNullOrWhiteSpace(fileName))
                                {
                                    if (backupFile.IsVideo)
                                        copyToList.Add(Path.Combine(to, $"{index}_{backupFile.Id}.mp4"));
                                    else
                                        copyToList.Add(Path.Combine(to, $"{index}_{backupFile.Id}.jpg"));
                                }
                                else
                                    copyToList.Add(Path.Combine(to, $"{index}_{fileName}"));
                            }
                        }
                        catch
                        {
                            if (backupFile.IsVideo)
                                copyToList.Add(Path.Combine(to, $"{index}_{backupFile.Id}.mp4"));
                            else
                                copyToList.Add(Path.Combine(to, $"{index}_{backupFile.Id}.jpg"));
                        }

                        index++;
                    }

                    //raise start event
                    OnStart(new TransferStartInfo()
                    {
                        TotalFiles = progressInfo.TotalFiles,
                        TotalSize = progressInfo.TotalFiles,
                        CurrentPosition = 1
                    });

                    //raise start event
                    OnStart(new TransferStartInfo()
                    {
                        TotalFiles = progressInfo.TotalFiles,
                        TotalSize = progressInfo.TotalSize,
                        CurrentPosition = 1
                    });

                    if (!Directory.Exists(to))
                        Directory.CreateDirectory(to);

                    #region Common files
                    //transfer data
                    for (int i = 0, length = filesToBackup.Count; i < length; i++, progressInfo.CurrentPosition++)
                    {
                        mCancellationTokenSource.Token.ThrowIfCancellationRequested();

                        var copyTo  = copyToList[i];
                        var file    = filesToBackup[i];

                        try
                        {
                            string destFolder = Path.GetDirectoryName(copyTo);

                            if (!Directory.Exists(destFolder))
                                Directory.CreateDirectory(destFolder);

                            progressInfo.FileSource = file.Source;
                            progressInfo.FileDestination = copyTo;

                            if (!File.Exists(copyTo))
                            {
                                using (WebClient client = new WebClient()
                                {
                                    
                                })
                                {
                                    client.DownloadProgressChanged += (s, e) =>
                                    {
                                        mCancellationTokenSource.Token.ThrowIfCancellationRequested();
                                    };

                                    client.DownloadFile(file.Source, copyTo);
                                }
                            }
                        }
                        catch (OperationCanceledException operationCanceled)
                        {
                            throw operationCanceled;
                        }
                        catch (DirectoryNotFoundException notFoundException)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file.Source, notFoundException));
                            /*LOG*/

                            CheckUSBDevice(notFoundException);
                        }
                        catch (UnauthorizedAccessException accessDenied)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file.Source, accessDenied));
                            /*LOG*/

                            CheckUSBDevice(accessDenied);
                        }
                        catch (IOException ioException)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file.Source, ioException));
                            /*LOG*/

                            CheckUSBDevice(ioException, false);
                        }
                        catch (WebException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            finishedInfo.Errors.Add(new ErrorInfo(file.Source, ex));
                            /*LOG*/

                            CheckUSBDevice(ex);
                        }
                        finally
                        {
                            OnProgress(progressInfo);
                        }
                    }

                    #endregion
                }
                catch (UsbDriveNotFoundException)
                {
                    finishedInfo.FailedReson = FailedReason.UsbNotFound;
                }
                catch (DirectoryNotFoundException)
                {
                    finishedInfo.FailedReson = FailedReason.FolderNotFound;
                }
                catch (UnauthorizedAccessException)
                {
                    finishedInfo.FailedReson = FailedReason.AccessDenied;
                }
                catch (OperationCanceledException)
                {
                    finishedInfo.FailedReson = FailedReason.Cancelled;
                }
                catch (WebException)
                {
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        finishedInfo.FailedReson = FailedReason.Other;
                    }
                    else
                    {
                        finishedInfo.FailedReson = FailedReason.NoInternet;
                    }
                }
                catch
                {
                    finishedInfo.FailedReson = FailedReason.Other;
                }
                finally
                {
                    finishedInfo.CurrentSize = progressInfo.CurrentSize;
                    finishedInfo.CurrentPosition = --progressInfo.CurrentPosition;

                    OnFinished(finishedInfo);
                }
            }
            else
            {
                OnFinished(new TransferFinishedInfo()
                {
                });
            }
        }

        #region Helper

        private void CheckUSBDevice(Exception currentException, bool throwFurther = true)
        {
            if (mbTransferToUSB && DriveManager.SelectedUSBDrive == null)
                //throw new UsbDriveNotFoundException("Transfer images to PC", currentException);
                throw new UsbDriveNotFoundException("USB not found", currentException);
            else if (throwFurther)
                throw currentException;
        }

        #endregion

        #region Events
        private void OnStart(TransferStartInfo info)
        {
            Start?.Invoke(info);
        }

        private void OnProgress(TransferProgressInfo info)
        {
            Progress?.Invoke(info);
        }

        private void OnFinished(TransferFinishedInfo info)
        {
            Finished?.Invoke(info);
        }

        private void OnSearchStart()
        {
            SearchStart?.Invoke();
        }

        private void OnSearchFinished(SearchResult result)
        {
            SearchFinished?.Invoke(result);
        }

        private bool? OnAppleFormatDetected(SearchResult searchResult)
        {
            if (AppleFormatDetected == null)
                return false;
            else return AppleFormatDetected.Invoke(searchResult);
        } 
        #endregion

        #endregion
    }
}

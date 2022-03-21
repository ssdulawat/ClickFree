using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClickFree.Helpers
{
    public class FileManager
    {
        #region Nested types

        public class SearchError
        {
            #region Properties

            public string Path { get; set; }
            public Exception Error { get; set; }

            #endregion

            #region ctor

            public SearchError(string path, Exception e)
            {
                Path = path;
                Error = e;
            }

            #endregion
        }

        [Flags]
        public enum SearchFilterEnum
        {
            //all files
            NOFILTER = -1,

            //photos
            Jpg = 1,
            Png = 2,
            Bmp = 4,
            Gif = 8,
            Heic = 16,

            //video
            Hevc = 2048,
            Mov = 4096,
            ThreeGp = 8192, //3gp
            mp4 = 16384,
            mkv = 32768,

            //------
            Photos = Jpg | Png | Bmp | Gif | Heic,
            Videos = Hevc | Mov | ThreeGp | mp4 | mkv,
            ALL = Photos | Videos
        }
        
        public class SearchParameters
        {
            #region Fields

            private List<string> SkipFolders = new List<string>();
            private List<string> SearchFilters = new List<string>();
            private SearchFilterEnum mSearchFilterEnum = SearchFilterEnum.NOFILTER;

            #endregion

            #region Properties

            public bool SkipHiddenFolders { get; set; } = true;
            public bool IncludeSubFolders { get; set; } = true;

            #endregion

            #region Ctor

            public SearchParameters()
            {
            }

            public SearchParameters(SearchFilterEnum searchFilter)
            {
                SetSearchFilter(searchFilter);
            }

            public SearchParameters(string[] skipFolders, SearchFilterEnum searchFilter)
            {
                if (skipFolders != null)
                    SkipFolders.AddRange(skipFolders);

                SetSearchFilter(searchFilter);
            }

            #endregion

            #region Methods

            public bool SkipFolder(string folder)
            {
                bool result = false;

                foreach (string skipF in SkipFolders)
                {
                    if (string.Compare(skipF, folder, true) == 0)
                    {
                        result = true;
                        break;
                    }
                }

                return result;
            }

            public bool GetFiles(string dirName, List<string> files, List<SearchError> errors)
            {
                int fileCount = files.Count;

                if (SearchFilters.Count == 0)
                    SetSearchFilter(SearchFilterEnum.ALL);

                foreach (string filter in SearchFilters)
                {
                    try
                    {
                        files.AddRange(Directory.GetFiles(dirName, filter));
                    }
                    catch (Exception e)
                    {
                        errors?.Add(new SearchError(dirName, e));
                    }
                }

                return files.Count != fileCount;
            }

            private void SetSearchFilter(SearchFilterEnum searchFilter)
            {
                mSearchFilterEnum = searchFilter;

                SearchFilters.Clear();
                SearchFilters.AddRange(GetFilters(searchFilter));
                
                //if (mSearchFilterEnum == SearchFilterEnum.NOFILTER)
                //{
                //    SearchFilters.AddRange(Constants.IgnoreFileExtensions);
                //    SearchFilters.Add("*.*");
                //}
                //else if (mSearchFilterEnum == SearchFilterEnum.ALL)
                //{
                //    SearchFilters.Add("*.jpg");
                //    SearchFilters.Add("*.jpeg");
                //    SearchFilters.Add("*.png");
                //    SearchFilters.Add("*.bmp");
                //    SearchFilters.Add("*.gif");
                //    SearchFilters.Add("*.heic");
                //    SearchFilters.Add("*.hevc");
                //    SearchFilters.Add("*.mov");
                //    SearchFilters.Add("*.3gp");
                //    SearchFilters.Add("*.mp4");
                //}
                //else
                //{
                //    foreach (SearchFilterEnum sf in Enum.GetValues(typeof(SearchFilterEnum)))
                //    {
                //        if ((mSearchFilterEnum & sf) == sf)
                //        {
                //            switch (sf)
                //            {
                //                case SearchFilterEnum.Jpg:
                //                    SearchFilters.Add("*.jpg");
                //                    SearchFilters.Add("*.jpeg");
                //                    break;
                //                case SearchFilterEnum.Png:
                //                    SearchFilters.Add("*.png");
                //                    break;
                //                case SearchFilterEnum.Bmp:
                //                    SearchFilters.Add("*.bmp");
                //                    break;
                //                case SearchFilterEnum.Gif:
                //                    SearchFilters.Add("*.gif");
                //                    break;
                //                case SearchFilterEnum.Heic:
                //                    SearchFilters.Add("*.heic");
                //                    break;
                //                case SearchFilterEnum.Hevc:
                //                    SearchFilters.Add("*.hevc");
                //                    break;
                //                case SearchFilterEnum.Mov:
                //                    SearchFilters.Add("*.mov");
                //                    break;
                //                case SearchFilterEnum.ThreeGp:
                //                    SearchFilters.Add("*.3gp");
                //                    break;
                //                case SearchFilterEnum.mp4:
                //                    SearchFilters.Add("*.mp4");
                //                    break;
                //            }
                //        }
                //    }
                //}
            }

            public void AddSkipFolder(string skipFolder)
            {
                SkipFolders.Add(skipFolder);
            }

            public void RemoveSkipFolder(string skipFolder)
            {
                SkipFolders.Remove(skipFolder);
            }

            #endregion

            #region Static

            public static List<string> GetFilters(SearchFilterEnum filterEnum, string mask = "*.")
            {
                List<string> searchFilters = new List<string>();

                if (filterEnum == SearchFilterEnum.NOFILTER)
                {
                    searchFilters.AddRange(Constants.IgnoreFileExtensions);
                    searchFilters.Add($"{mask}*");
                }
                else
                {
                    foreach (SearchFilterEnum sf in Enum.GetValues(typeof(SearchFilterEnum)))
                    {
                        if ((filterEnum & sf) == sf)
                        {
                            switch (sf)
                            {
                                case SearchFilterEnum.Jpg:
                                    searchFilters.Add($"{mask}jpg");
                                    searchFilters.Add($"{mask}jpeg");
                                    break;
                                case SearchFilterEnum.Png:
                                    searchFilters.Add($"{mask}png");
                                    break;
                                case SearchFilterEnum.Bmp:
                                    searchFilters.Add($"{mask}bmp");
                                    break;
                                case SearchFilterEnum.Gif:
                                    searchFilters.Add($"{mask}gif");
                                    break;
                                case SearchFilterEnum.Heic:
                                    searchFilters.Add($"{mask}heic");
                                    break;
                                case SearchFilterEnum.Hevc:
                                    searchFilters.Add($"{mask}hevc");
                                    break;
                                case SearchFilterEnum.Mov:
                                    searchFilters.Add($"{mask}mov");
                                    break;
                                case SearchFilterEnum.ThreeGp:
                                    searchFilters.Add($"{mask}3gp");
                                    break;
                                case SearchFilterEnum.mp4:
                                    searchFilters.Add($"{mask}mp4");
                                    break;
                                case SearchFilterEnum.mkv:
                                    searchFilters.Add($"{mask}mkv");
                                    break;
                            }
                        }
                    }
                }

                return searchFilters;
            }

            #endregion
        }

        public class SearchResult
        {
            #region Properties

            public List<string> Directories { get; set; } = new List<string>();
            public List<string> Files { get; set; } = new List<string>();
            public List<SearchError> Errors { get; set; } = new List<SearchError>();
            public bool AppleFormatDetected { get; set; } = false;

            #endregion
        }

        #endregion

        #region Methods

        public static SearchResult Search(UsbDisk searchDisk, SearchParameters searchParam, CancellationToken cancellationToken)
        {
            return Search(searchDisk.Name, searchParam, cancellationToken);
        }

        public static SearchResult Search(string startDir, SearchParameters searchParam, CancellationToken cancellationToken)
        {
            if (startDir == null)
                throw new ArgumentNullException();

            if (!Directory.Exists(startDir))
                throw new DirectoryNotFoundException(startDir);

            if (searchParam == null) searchParam = new SearchParameters();

            //result
            SearchResult result = new SearchResult();

            try
            {
                Stack<string> searchDirs = new Stack<string>(new[] { startDir });
                while (searchDirs.Count > 0)
                {
                    var searchDir = searchDirs.Pop();

                    if (searchParam.GetFiles(searchDir, result.Files, result.Errors))
                        result.Directories.Add(searchDir);

                    if (searchParam.IncludeSubFolders)
                    {
                        //lookup for subdir
                        foreach (var dir in Directory.GetDirectories(searchDir))
                        {
                            if (!searchParam.SkipFolder(dir))
                            {
                                try
                                {
                                    var dirInfo = new DirectoryInfo(dir);
                                    if (searchParam.SkipHiddenFolders)
                                    {
                                        if ((dirInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                                            continue;
                                    }

                                    searchDirs.Push(dir);
                                }
                                catch (Exception ex)
                                {
                                    result.Errors.Add(new SearchError(dir, ex));
                                    continue;
                                }
                            }
                        }
                    }
                    else
                    {
                        result.Directories.Clear();

                        //lookup for subdir
                        foreach (var dir in Directory.GetDirectories(searchDir))
                        {
                            if (!searchParam.SkipFolder(dir))
                            {
                                try
                                {
                                    var dirInfo = new DirectoryInfo(dir);
                                    if (searchParam.SkipHiddenFolders)
                                    {
                                        if ((dirInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                                            continue;
                                    }

                                    result.Directories.Add(dir);
                                }
                                catch (Exception ex)
                                {
                                    result.Errors.Add(new SearchError(dir, ex));
                                    continue;
                                }
                            }
                        }

                        break;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException ex)
            {
                result.Errors.Add(new SearchError("Operation Canceled", ex));
            }
            catch (Exception ex)
            {
                result.Errors.Add(new SearchError(startDir, ex));
            }

            //detect unspported files
            Parallel.ForEach(result.Files, (f, loopstate) =>
            {
                foreach (var ext in Constants.AppleFileExtensions)
                {
                    if (f.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
                    {
                        lock (result)
                        {
                            result.AppleFormatDetected = true;
                            loopstate.Stop();
                        }
                    } 
                }
            });

            return result;
        }

        //public static SearchResult Search(string searchDir, SearchParameters searchParam, CancellationToken? cancellationToken = null)
        //{
        //    if (searchDir == null)
        //        throw new ArgumentNullException();

        //    if (!Directory.Exists(searchDir))
        //        throw new DirectoryNotFoundException(searchDir);

        //    if (searchParam == null) searchParam = new SearchParameters();

        //    //result
        //    SearchResult result = new SearchResult();

        //    //start scan
        //    ScanRecursive(searchDir, true);

        //    //return result
        //    return result;

        //    //helper methods
        //    void ScanRecursive(string dirName, bool root)
        //    {
        //        if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
        //            return;

        //        try
        //        {
        //            FileAttributes dirAttributes = new DirectoryInfo(dirName).Attributes;
        //            if (root || ((dirAttributes & FileAttributes.System) != FileAttributes.System &&
        //                         (dirAttributes & FileAttributes.Temporary) != FileAttributes.Temporary &&
        //                         !searchParam.SkipFolder(dirName)
        //                        ))
        //            {

        //                if (searchParam.GetFiles(dirName, result.Files, result.Errors))
        //                    result.Directories.Add(dirName);

        //                foreach (var dir in Directory.GetDirectories(dirName))
        //                    ScanRecursive(dir, false);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            result.Errors.Add(new SearchError()
        //            {
        //                Path = dirName,
        //                Error = e
        //            });
        //        }
        //    }
        //}

        #endregion
    }
}

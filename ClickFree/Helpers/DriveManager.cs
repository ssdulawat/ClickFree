using ClickFree.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ClickFree.Helpers
{
    public enum DriveState
    {
        Added,
        Removed
    }

    public static class DriveManager
    {
        #region Nested types

        private class DriveStateEventArgs : EventArgs
        {
            #region Properties

            public DriveState State { get; private set; }
            public string Drive { get; private set; }

            #endregion

            #region Constructor

            public DriveStateEventArgs(DriveState state, string drive)
            {
                this.State = state;
                this.Drive = drive;
            }

            #endregion
        }

        private class DriverWindow : NativeWindow, IDisposable
        {
            #region Nested types

            [StructLayout(LayoutKind.Sequential)]
            public struct DEV_BROADCAST_VOLUME
            {
                public int dbcv_size;           // size of the struct
                public int dbcv_devicetype;     // DBT_DEVTYP_VOLUME
                public int dbcv_reserved;       // reserved; do not use
                public int dbcv_unitmask;       // Bit 0=A, bit 1=B, and so on (bitmask)
                public short dbcv_flags;        // DBTF_MEDIA=0x01, DBTF_NET=0x02 (bitmask)
            }

            #endregion

            #region Constants
            private const int WM_DEVICECHANGE = 0x0219;             // device state change
            private const int DBT_DEVICEARRIVAL = 0x8000;           // detected a new device
            private const int DBT_DEVICEQUERYREMOVE = 0x8001;       // preparing to remove
            private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;    // removed 
            private const int DBT_DEVTYP_VOLUME = 0x00000002;       // logical volume 
            #endregion

            #region ctor
            public DriverWindow()
            {
                // create a generic window with no class name
                base.CreateHandle(new CreateParams());
            }

            #endregion

            #region Events

            public event EventHandler<DriveStateEventArgs> DriveStateChanged;

            #endregion

            #region Implementation

            public void Dispose()
            {
                base.DestroyHandle();
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Translate the dbcv_unitmask bitmask to a drive letter by finding the first
            /// enabled low-order bit; its offset equals the letter where offset 0 is 'A'.
            /// </summary>
            private string ToUnitName(int mask)
            {
                int offset = 0;
                while ((offset < 26) && ((mask & 0x00000001) == 0))
                {
                    mask = mask >> 1;
                    offset++;
                }

                if (offset < 26)
                {
                    return $"{Convert.ToChar(Convert.ToInt32('A') + offset)}:\\";
                }

                return "?:";
            }

            protected virtual void OnDriveStateChanged(DriveStateEventArgs args)
            {
                if (args.Drive != null && DriveStateChanged != null)
                {
                    DriveStateChanged.EndInvoke(DriveStateChanged.BeginInvoke(this, args, null, null));
                }

            }

            #endregion

            #region Overrides

            protected override void WndProc(ref Message message)
            {
                base.WndProc(ref message);

                if ((message.Msg == WM_DEVICECHANGE) && (message.LParam != IntPtr.Zero))
                {
                    DEV_BROADCAST_VOLUME volume = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(
                        message.LParam, typeof(DEV_BROADCAST_VOLUME));

                    if (volume.dbcv_devicetype == DBT_DEVTYP_VOLUME)
                    {
                        switch (message.WParam.ToInt32())
                        {
                            case DBT_DEVICEARRIVAL:
                                OnDriveStateChanged(new DriveStateEventArgs(DriveState.Added, ToUnitName(volume.dbcv_unitmask)));
                                break;

                            case DBT_DEVICEQUERYREMOVE:
                                // can intercept
                                break;

                            case DBT_DEVICEREMOVECOMPLETE:
                                OnDriveStateChanged(new DriveStateEventArgs(DriveState.Removed, ToUnitName(volume.dbcv_unitmask)));
                                break;
                        }
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Properties

        public static bool HasUsbDrives { get { return InsertedDrives != null && InsertedDrives.Count > 0; } }
        public static List<UsbDisk> InsertedDrives { get; private set; }
        public static UsbDisk SelectedUSBDrive
        {
            get
            {
                RefreshDrives();

                if (InsertedDrives == null || InsertedDrives.Count == 0)
                    mSelectedDrive = null;
                else if (mSelectedDrive != null) //refresh selected drive instance
                    mSelectedDrive = InsertedDrives?.Where(id => string.Compare(id.PNPDeviceID, mSelectedDrive.PNPDeviceID, true) == 0).FirstOrDefault();

                return mSelectedDrive;
            }
            set
            {
                if (value == null || InsertedDrives == null || InsertedDrives.Count == 0)
                    mSelectedDrive = value;
                else if (InsertedDrives.Contains(value))
                    mSelectedDrive = value;
                else mSelectedDrive = InsertedDrives?.Where(id => string.Compare(id.PNPDeviceID, mSelectedDrive.PNPDeviceID, true) == 0).FirstOrDefault();
            }
        }

        #endregion

        #region Fields

        private static UsbDisk mSelectedDrive = null;
        private static DriverWindow DriveWindowObject;
        private static Action<DriveState, UsbDisk> DriveStateChangedHandler;

        #endregion

        #region ctor

        static DriveManager()
        {
            DriveStateChanged += DriveManager_DriveStateChanged;
            RefreshDrives();
        }

        #endregion

        #region Events

        public static event Action<DriveState, UsbDisk> DriveStateChanged
        {
            add
            {
                if (DriveWindowObject == null)
                {
                    DriveWindowObject = new DriverWindow();
                    DriveWindowObject.DriveStateChanged += DriveWindowStateChanged;
                }

                DriveStateChangedHandler = (Action<DriveState, UsbDisk>)Delegate.Combine(DriveStateChangedHandler, value);
            }
            remove
            {
                DriveStateChangedHandler = (Action<DriveState, UsbDisk>)Delegate.Remove(DriveStateChangedHandler, value);

                if (DriveStateChangedHandler == null && DriveWindowObject != null)
                {
                    DriveWindowObject.DriveStateChanged -= DriveWindowStateChanged;

                    DriveWindowObject.Dispose();
                    DriveWindowObject = null;
                }
            }
        }

        #endregion

        #region Event handlers

        private static void DriveManager_DriveStateChanged(DriveState state, UsbDisk disk)
        {
            RefreshDrives();
        }

        #endregion

        #region Static
        public static bool CheckAccess(bool showWarningMessage = true)
        {
            //RefreshDrives(); called in SelectedUSBDrive

#warning temporary(show the dialog with ability choose the needed flash drive
            if (SelectedUSBDrive == null)
                mSelectedDrive = InsertedDrives?.FirstOrDefault();

            bool result = SelectedUSBDrive != null;
            if (!result && showWarningMessage)
                MessageBoxWindow.ShowMessageBox("Could not establish connection with ClickFree.", "Please connect/ re - connect ClickFree to your computer USB port.", MessageBoxWindow.MessageBoxType.Error);

            return result;
        }

        public static UsbDisk GetDiskByName(string name)
        {
            UsbDisk disk = null;

            try
            {
                if (name != null && name.EndsWith("\\"))
                    name = name.Remove(name.Length - 1, 1);

                using (var partitionSearch = new ManagementObjectSearcher($"associators of {{Win32_LogicalDisk.DeviceID='{name}'}} where AssocClass = Win32_LogicalDiskToPartition"))
                {
                    using (ManagementObject partition = partitionSearch.First())
                    {

                        if (partition != null)
                        {
                            disk = new UsbDisk(name);

                            using (var driveSearch = new ManagementObjectSearcher($"associators of {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}}  where resultClass = Win32_DiskDrive"))
                            {
                                using (ManagementObject drive = driveSearch.First())
                                {
                                    if (drive != null)
                                    {
                                        disk.Model = drive["Model"].ToString();
                                        disk.DeviceID = drive["DeviceID"]?.ToString();
                                        disk.PNPDeviceID = drive["PNPDeviceID"]?.ToString();
                                    }

                                    using (var volumeSearch = new ManagementObjectSearcher($"select FreeSpace, Size, VolumeName, FileSystem, VolumeSerialNumber from Win32_LogicalDisk where Name='{name}'"))
                                    {
                                        using (ManagementObject volume = volumeSearch.First())
                                        {
                                            if (volume != null)
                                            {
                                                disk.Volume = volume["VolumeName"].ToString();
                                                disk.FreeSpace = (ulong)volume["FreeSpace"];
                                                disk.Size = (ulong)volume["Size"];
                                                disk.FileSystem = volume["FileSystem"]?.ToString();
                                                disk.VolumeSerialNumber = volume["VolumeSerialNumber"]?.ToString();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                /*Add logging*/
            }

            return disk;
        }

        public static List<UsbDisk> GetAvailableDisks()
        {
            List<UsbDisk> disks = new List<UsbDisk>();

            try
            {
                using (var driveSearch = new ManagementObjectSearcher("select * from Win32_DiskDrive where InterfaceType='USB'"))
                {
                    // browse all USB WMI physical disks
                    foreach (ManagementObject drive in driveSearch.Get())
                    {
                        using (var partitionSearch = new ManagementObjectSearcher($"associators of {{Win32_DiskDrive.DeviceID='{drive["DeviceID"]}'}} where AssocClass = Win32_DiskDriveToDiskPartition"))
                        {
                            using (ManagementObject partition = partitionSearch.First())
                            {
                                if (partition != null)
                                {
                                    using (var logicalSearch = new ManagementObjectSearcher($"associators of {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} where AssocClass = Win32_LogicalDiskToPartition"))
                                    {
                                        // associate partitions with logical disks (drive letter volumes)
                                        using (ManagementObject logical = logicalSearch.First())
                                        {
                                            if (logical != null)
                                            {
                                                using (var volumeSearch = new ManagementObjectSearcher($"select FreeSpace, Size, VolumeName, FileSystem, VolumeSerialNumber from Win32_LogicalDisk where Name='{logical["Name"]}'"))
                                                {
                                                    // finally find the logical disk entry to determine the volume name
                                                    using (ManagementObject volume = volumeSearch.First())
                                                    {
                                                        if (volume != null)
                                                        {
                                                            UsbDisk disk = new UsbDisk(logical["Name"].ToString())
                                                            {
                                                                Model = drive["Model"]?.ToString(),
                                                                DeviceID = drive["DeviceID"]?.ToString(),
                                                                PNPDeviceID = drive["PNPDeviceID"]?.ToString(),
                                                                Volume = volume["VolumeName"]?.ToString(),
                                                                FreeSpace = (ulong)volume["FreeSpace"],
                                                                Size = (ulong)volume["Size"],
                                                                FileSystem = volume["FileSystem"]?.ToString(),
                                                                VolumeSerialNumber = volume["VolumeSerialNumber"]?.ToString()
                                                            };

                                                            disks.Add(disk);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                /*Add logging*/
            }

            return disks;
        }

        private static void DriveWindowStateChanged(object sender, DriveStateEventArgs args)
        {
            if (DriveStateChangedHandler != null)
            {
                UsbDisk disk;

                if (args.State == DriveState.Added)
                    disk = GetDiskByName(args.Drive);
                else disk = new UsbDisk(args.Drive);

                DriveStateChangedHandler(args.State, disk);
            }
        }

        private static void RefreshDrives()
        {
            InsertedDrives = DriveManager.GetAvailableDisks();
        }

        #endregion
    }

    public static class WMIExtension
    {
        public static ManagementObject First(this ManagementObjectSearcher searcher)
        {
            ManagementObject result = null;
            foreach (ManagementObject item in searcher.Get())
            {
                result = item;
                break;
            }
            return result;
        }
    }

    public class UsbDisk
    {
        #region Constants
        public const int KB = 1024;
        public const int MB = KB * 1000;
        public const int GB = MB * 1000;
        #endregion

        #region Ctor
        public UsbDisk(string name)
        {
            if (name.Length == 1)
            {
                this.Name = $"{name}:\\";
            }
            else if (name.Length == 2)
            {
                if (name[1] == ':')
                    this.Name = $"{name}\\";
                else this.Name = $"{name[0]}:\\";
            }
            else
            {
                this.Name = name;
            }

            this.Model = String.Empty;
            this.Volume = String.Empty;
            this.FreeSpace = 0;
            this.Size = 0;
        }
        #endregion

        #region Properties
        public ulong FreeSpace
        {
            get;
            internal set;
        }

        public string Model
        {
            get;
            internal set;
        }

        public string Name
        {
            get;
            private set;
        }

        public ulong Size
        {
            get;
            internal set;
        }

        public string Volume
        {
            get;
            internal set;
        }

        public string DeviceID
        {
            get;
            internal set;
        }

        public string PNPDeviceID
        {
            get;
            internal set;
        }

        public string FileSystem
        {
            get;
            internal set;
        }

        public string VolumeSerialNumber
        {
            get;
            internal set;
        }
        #endregion

        #region overrides
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Name);
            builder.Append(" ");
            builder.Append(Volume);
            builder.Append(" (");
            builder.Append(Model);
            builder.Append(") ");
            builder.Append(FormatByteCount(FreeSpace));
            builder.Append(" free of ");
            builder.Append(FormatByteCount(Size));

            return builder.ToString();
        }
        #endregion

        #region Implementation
        private string FormatByteCount(ulong bytes)
        {
            string format = null;

            if (bytes < KB)
            {
                format = String.Format("{0} Bytes", bytes);
            }
            else if (bytes < MB)
            {
                bytes = bytes / KB;
                format = String.Format("{0} KB", bytes.ToString("N"));
            }
            else if (bytes < GB)
            {
                double dree = bytes / MB;
                format = String.Format("{0} MB", dree.ToString("N1"));
            }
            else
            {
                double gree = bytes / GB;
                format = String.Format("{0} GB", gree.ToString("N1"));
            }

            return format;
        }
        #endregion
    }
}

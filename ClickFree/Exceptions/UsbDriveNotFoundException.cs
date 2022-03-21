using System;

namespace ClickFree.Exceptions
{
    public class UsbDriveNotFoundException: Exception
    {
        #region Ctor
        public UsbDriveNotFoundException()
        {
        }
        public UsbDriveNotFoundException(string message, Exception e): base(message, e)
        {
        }
        #endregion
    }
}

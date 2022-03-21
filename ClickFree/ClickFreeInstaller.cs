using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace ClickFree
{
    [RunInstaller(true)]
    public partial class ClickFreeInstaller : System.Configuration.Install.Installer
    {
        #region Overrides
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);

            //MessageBox.Show("1");
            //    Process.Start(Context.Parameters["assemblypath"]);
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);
            //MessageBox.Show("2");

            //if (Context.IsParameterTrue("CHECKBOXSTARTCLICKFREE"))
            {
                //Process.Start(Context.Parameters["assemblypath"]);
                //Process.Start($"{Context.Parameters["assemblypath"]}ClickFree.exe");
            }
        }
        #endregion
    }
}

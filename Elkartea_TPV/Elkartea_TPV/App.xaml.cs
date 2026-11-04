using System.Windows;

namespace Elkartea_TPV
{
    public partial class App : Application
    {
        public App()
        {
            SQLitePCL.Batteries_V2.Init();
        }
    }
}
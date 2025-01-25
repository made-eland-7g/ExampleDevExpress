using System.Configuration;
using System.Data;
using System.Windows;

namespace DevExampleProject
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            MainWindow window = new MainWindow();

            window.Show();


        }
    }

}

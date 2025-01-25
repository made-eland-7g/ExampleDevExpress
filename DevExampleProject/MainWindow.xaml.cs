using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DevExampleProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadDllButton_Click(object sender, RoutedEventArgs e)
        {
            string binaries = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "ChildProject", "bin", "Debug", "net9.0-windows10.0.19041");

            AssemblyLoadContext loadContext = new AssemblyLoadContext("ChildProjectContext", isCollectible: true);

            List<UserControl> userControls = new List<UserControl>();

            foreach (string dllFile in System.IO.Directory.GetFiles(binaries, "*.dll"))
            {
                try
                {
                    Assembly assembly = loadContext.LoadFromAssemblyPath(dllFile);

                    if (System.IO.Path.GetFileName(dllFile).Contains("ChildProject.dll"))
                    {
                        Type[] types = assembly.GetTypes();

                        foreach (Type type in types)
                        {
                            if (type.IsSubclassOf(typeof(UserControl)))
                            {
                                userControls.Add(new DeferredUserControlV2(type, dllFile, loadContext)); // Pass the context
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading DLL {System.IO.Path.GetFileName(dllFile)}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (userControls.Count > 0)
            {
                foreach (DeferredUserControlV2 deferredUserControl in userControls)
                {
                    UserControl userControlInstance = deferredUserControl.GetInstance();

                    if (userControlInstance != null) // Check if instance creation succeeded
                    {
                        DllContainer.Children.Clear();
                        DllContainer.Children.Add(userControlInstance);
                    }
                    else
                    {
                        MessageBox.Show("Failed to create UserControl instance.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("No UserControls found in the bin\\net9.0 folder.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (System.IO.Directory.GetFiles(binaries, "*.dll").Length == 0)
            {
                MessageBox.Show("No DLL files found in the bin\\net9.0 folder.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            //Unload the context
            loadContext.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public class DeferredUserControlV2 : UserControl
        {
            private Type _userControlType;
            private string _dllFile;
            private UserControl _instance;
            private AssemblyLoadContext _loadContext; // Store the context

            public DeferredUserControlV2(Type userControlType, string dllFile, AssemblyLoadContext loadContext)
            {
                _userControlType = userControlType;
                _dllFile = dllFile;
                _loadContext = loadContext;
            }

            public UserControl GetInstance()
            {
                if (_instance == null)
                {
                    try
                    {
                        // Load the assembly using the stored context
                        Assembly assembly = _loadContext.LoadFromAssemblyPath(_dllFile);
                        _instance = (UserControl)Activator.CreateInstance(assembly.GetType(_userControlType.FullName));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating instance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }
                return _instance;
            }
        }

    }
}
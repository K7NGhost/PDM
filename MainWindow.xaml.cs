using Microsoft.Extensions.DependencyInjection;
using PDM.Src.Models;
using PDM.Src.ViewModels;
using PDM.ui;
using PDM.ui.Pages;
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

namespace PDM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var dbManagerVM = new DatabaseManagerViewModel();
            this.DataContext = dbManagerVM;
            var dashboardPage = App.ServiceProvider.GetRequiredService<DashboardPage>();
            MainFrame.Navigate(dashboardPage);
        }

        private void NavToNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoForward)
            {
                MainFrame.GoForward();
            }
        }

        private void NavToPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void NavToDashboardPage_Click(object sender, RoutedEventArgs e)
        {
            var dashboardPage = App.ServiceProvider.GetRequiredService<DashboardPage>();
            MainFrame.Navigate(dashboardPage);
        }

        private void NavToPhoneListPage_Click(object sender, RoutedEventArgs e)
        {
            var phoneListPage = App.ServiceProvider.GetRequiredService<PhoneListPage>();
            MainFrame.Navigate(phoneListPage);
        }

        private void NavToAddPhonePage_Click(object sender, RoutedEventArgs e)
        {
            var addPhonePage = App.ServiceProvider.GetRequiredService<AddPhonePage>();
            MainFrame.Navigate(addPhonePage);
        }
    }
}
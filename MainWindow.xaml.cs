using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PDM.Src.Models;
using PDM.Src.ViewModels;
using PDM.ui;
using PDM.ui.Pages;
using PDM.ui.Windows;
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
            DataContext = dbManagerVM;
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

        private void NavToIdPhonePage_Click(object sender, RoutedEventArgs e)
        {
            var idPhonePage = App.ServiceProvider.GetRequiredService<IdentifyPhonePage>();
            MainFrame.Navigate(idPhonePage);
        }

        private void NavToAdminPage_Click(Object sender, RoutedEventArgs e)
        {
            var login = new AdminLoginWindow();
            if (login.ShowDialog() == true && login.IsAuthenticated)
            {
                var adminPage = new AdminPage();
                MainFrame.Navigate(adminPage);
            }
        }

        private void NavToSettingsPage_Click(object sender, RoutedEventArgs e)
        {
            var settingsPage = App.ServiceProvider.GetRequiredService<SettingsPage>();
            MainFrame.Navigate(settingsPage);
        }

        private void NavToAboutPage_Click(object sender, RoutedEventArgs e)
        {
            var aboutPage = new AboutPage();
            MainFrame.Navigate(aboutPage);
        }

        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavList.SelectedItem is ListBoxItem item)
            {
                switch(item.Tag)
                {
                    case "PhoneListPage":
                        NavToPhoneListPage_Click(sender, e); break;
                    case "DashboardPage":
                        NavToDashboardPage_Click(sender, e); break;
                    case "AddPhonePage":
                        NavToAddPhonePage_Click(sender, e); break;
                    case "IdPhonePage":
                        NavToIdPhonePage_Click(sender, e); break;
                    case "SettingsPage":
                        NavToSettingsPage_Click(sender, e); break;
                    case "AdminPage":
                        NavToAdminPage_Click(sender, e); break;
                    case "AboutPage":
                        NavToAboutPage_Click(sender, e); break;
                    default:
                        break;
                }
            }
        }
    }
}
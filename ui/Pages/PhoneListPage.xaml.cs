using Microsoft.Extensions.DependencyInjection;
using PDM.Src.ViewModels;
using PDM.ui.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PDM.ui
{
    /// <summary>
    /// Interaction logic for PhoneList.xaml
    /// </summary>
    public partial class PhoneListPage : Page
    {
        public PhoneListPage()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<PhoneListViewModel>();
        }

        private void SelectColumns_Click(object sender, RoutedEventArgs e)
        {
            var selector = new SelectVisColumnsWindow();
            selector.Owner = Window.GetWindow(this);
            selector.CheckList.ItemsSource = PhoneGrid.Columns;
            selector.ShowDialog();
        }
    }
}

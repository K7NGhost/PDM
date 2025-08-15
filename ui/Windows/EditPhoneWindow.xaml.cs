using Microsoft.Extensions.DependencyInjection;
using PDM.Src.ViewModels;
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
using System.Windows.Shapes;

namespace PDM.ui.Windows
{
    /// <summary>
    /// Interaction logic for EditPhoneWindow.xaml
    /// </summary>
    public partial class EditPhoneWindow : Window
    {
        public EditPhoneWindow()
        {
            InitializeComponent();
        }



        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void UploadImageF_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PhoneDataViewModel vm)
            {
                vm.UploadImage(data => vm.SelectedPhone.ImageDataF = data);
            }
        }

        private void UploadImageB_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PhoneDataViewModel vm)
            {
                vm.UploadImage(data => vm.SelectedPhone.ImageDataB = data);
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PhoneDataViewModel vm && sender is Button btn && btn.Tag is string tag)
            {
                vm.RemoveImage(tag);
            }
        }
    }
}

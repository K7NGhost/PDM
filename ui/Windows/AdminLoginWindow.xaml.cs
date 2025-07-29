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
    /// Interaction logic for AdminLoginWindow.xaml
    /// </summary>
    public partial class AdminLoginWindow : Window
    {
        private const string AdminPassword = "password123";
        public bool IsAuthenticated { get; private set; }
        public AdminLoginWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordInput.Password == AdminPassword)
            {
                IsAuthenticated = true;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Invalid password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordInput.Clear();
                PasswordInput.Focus();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsAuthenticated = false;
            DialogResult = false;
            Close();
        }
    }
}

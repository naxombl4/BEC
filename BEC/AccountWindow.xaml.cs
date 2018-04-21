using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BEC
{
    /// <summary>
    /// Логика взаимодействия для AccountWindow.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        public AccountWindow()
        {
            InitializeComponent();
        }

        private void SendAccount_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = this.Owner as MainWindow;
            if (LoginTextBox.Text == "")
            {
                LoginLabel.Foreground = Brushes.Red;
                return;
            }
            if (PasswordTextBox.Password == "")
            {
                LoginLabel.Foreground = Brushes.Black;
                PasswordLabel.Foreground = Brushes.Red;
                return;
            }
            LoginLabel.Foreground = PasswordLabel.Foreground = Brushes.Black;
            main.username = LoginTextBox.Text;
            main.password = PasswordTextBox.Password;
            Close();
        }

        private void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendAccount_Click(sender, e);
        }
    }
}

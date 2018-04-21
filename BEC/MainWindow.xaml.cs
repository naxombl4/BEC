using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
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

namespace BEC
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public string username, password;

        private void SetPathIP_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text files (*.txt)|*.txt";
            dialog.FilterIndex = 1;
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == true)
            {
                PathToIP.Text = dialog.FileName;
            }

        }

        private void SetPathCMD_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text files (*.txt)|*.txt";
            dialog.FilterIndex = 1;
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == true)
            {
                PathToCMD.Text = dialog.FileName;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (PathToIP.Text == "")
            {
                PathToIPLabel.Foreground = Brushes.Red;
                StatusText.Text = "Не выбран файл со списком IP адресов!";
                return;
            }

            if (PathToCMD.Text == "")
            {
                PathToIPLabel.Foreground = Brushes.Black;
                PathToCMDLabel.Foreground = Brushes.Red;
                StatusText.Text = "Не выбран файл со списком команд!";
                return;
            }

            PathToCMDLabel.Foreground = PathToIPLabel.Foreground = Brushes.Black;
            StatusText.Text = "Стартуем...";

            StatusText.Text = "Пришло время ввода учетных данных";

            AccountWindow AW = new AccountWindow();
            AW.Owner = this;
            AW.ShowDialog();

            if (username != null)
            {
                try
                {
                    StreamReader sr = new StreamReader(PathToIP.Text);
                    string line = "";
                    ArrayList listIP = new ArrayList();
                    ArrayList listCMD = new ArrayList();
                    IPAddress ip;

                    StatusText.Text = "Читаем список адресов...";

                    do
                    {
                        line = sr.ReadLine();
                        if (line != null)
                        {
                            line = line.Trim(' ');
                            if (IPAddress.TryParse(line, out ip))
                            {
                                listIP.Add(line);
                            }
                        }
                    }
                    while (line != null);

                    sr.Close();
                    line = "";
                    StatusText.Text = "Читаем список команд...";
                    StreamReader ars = new StreamReader(PathToCMD.Text);

                    do
                    {
                        line = ars.ReadLine();
                        if (line != null)
                            listCMD.Add(line);
                    }
                    while (line != null);

                    ars.Close();

                    DataTable OutputTable = new DataTable("Output");
                    DataColumn listIPAddress = OutputTable.Columns.Add("IP адрес", typeof(string));
                    OutputTable.PrimaryKey = new DataColumn[] { OutputTable.Columns[0] };
                    DataColumn state = OutputTable.Columns.Add("Состояние", typeof(string));
                    DataColumn readiness = OutputTable.Columns.Add("Готовность", typeof(int));
                    DataColumn log = OutputTable.Columns.Add("Ответ оборудования", typeof(string));

                    for (int i = 0; i < listIP.Count; i++)
                    {
                        OutputTable.Rows.Add(listIP[i], "Не отработан", 0, "");
                    }

                    GridOutput.DataContext = OutputTable.DefaultView;

                    Parallel.ForEach(listIP.OfType<string>().ToArray(), ipaddress =>
                    {
                        //StatusText.Text = "Создаем подключение к " + ipaddress + " ...";
                        int index = OutputTable.Rows.IndexOf(OutputTable.Rows.Find(ipaddress));
                        TelnetEngine TE = new TelnetEngine();
                        TE.login = username;
                        TE.password = password;
                        string statechk = TE.CreateConnection(ipaddress);

                        switch (statechk)
                        {
                            case "Login incorrect":
                                OutputTable.Rows[index][1] = "Ошибка: Некорректные учетные данные!";
                                OutputTable.Rows[index][2] = 100;
                                OutputTable.Rows[index][3] = statechk;
                                //StatusText.Text = "Ошибка учетной записи для " + ipaddress;
                                break;
                            case "Timeout":
                                OutputTable.Rows[index][1] = "Ошибка: Таймаут подключения!";
                                OutputTable.Rows[index][2] = 100;
                                OutputTable.Rows[index][3] = statechk;
                                //StatusText.Text = "Таймаут подключения для " + ipaddress;
                                break;
                            case "Success":
                                OutputTable.Rows[index][2] = 10;
                                int percentage;
                                for (int i = 0; i < listCMD.Count; i++)
                                {
                                    percentage = 90 / listCMD.Count;
                                    string command = listCMD[i].ToString();
                                    StringBuilder outputtext = new StringBuilder();
                                    if (command.IndexOf("<ipaddress>") != -1)
                                        command = command.Replace("<ipaddress>", ipaddress);
                                    //StatusText.Text = "Выполняем команду " + command + " на оборудовании " + ipaddress + " ...";
                                    TE.SendCMD(command);
                                    //StatusText.Text = "Получаем ответ от " + ipaddress + " ...";
                                    string result = TE.ReadStream();
                                    outputtext.Append(result);
                                    if (result.IndexOf('^') != -1)
                                    {
                                        OutputTable.Rows[index][1] = "Ошибка: Некорректный формат команд для оборудования";
                                        OutputTable.Rows[index][2] = 100;
                                        OutputTable.Rows[index][3] = outputtext.ToString();
                                        //StatusText.Text = "Некорректный формат команды для " + ipaddress;
                                        TE.CloseConnection();
                                        return;
                                    }
                                    OutputTable.Rows[index][2] = Convert.ToInt32(OutputTable.Rows[index][2]) + percentage;
                                    //StatusText.Text = "Выводим ответ от " + ipaddress + " ...";
                                    OutputTable.Rows[index][3] = outputtext.ToString();

                                }
                                TE.CloseConnection();
                                OutputTable.Rows[index][2] = 100;
                                //StatusText.Text = "Выполнение команд для " + ipaddress + " завершено";
                                break;
                        }
                    }
                    );

                    StatusText.Text = "Завершено успешно!";
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
    }
}

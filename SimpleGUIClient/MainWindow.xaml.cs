using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace SimpleGUIClient
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient server;
        private StreamWriter writer;
        private StreamReader reader;

        public MainWindow()
        {
            InitializeComponent();

            server = new TcpClient("localhost", 1234);

            NetworkStream stream = server.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            writer.AutoFlush = true;

            new Thread(TextUpdater).Start();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                writer.WriteLine(send.Text);
            }
            catch (Exception)
            {
            }
        }

        private void TextUpdater()
        {
            try
            {
                while (true)
                {
                    UpdateText(reader.ReadLine());
                }
            }
            catch (IOException)
            {
                UpdateText("Server error");
            }
        }

        private void UpdateText(string message)
        {
            try
            {
                received.Dispatcher.Invoke(() =>
                {
                    received.Text += "\n" + message;
                    received.ScrollToEnd();
                });
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            server.Close();
        }
    }
}

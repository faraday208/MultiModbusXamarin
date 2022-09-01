using System;
using System.Net.Sockets;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ModbusXamarinDeneme
{
    public partial class App : Application
    {
        public static string serverIP = "192.168.1.10";
        public static bool[] ConnectionOks;
        public static TcpClient[] Clients;
        private int port = 5000;

        public App()
        {
            Clients = new TcpClient[1];
            ConnectionOks = new Boolean[1];
            InitializeComponent();
            CreateTCPIP();

            MainPage = new MainPage();
        }

        public void CreateTCPIP()
        {
            Clients[0] = new TcpClient(serverIP, 502);
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
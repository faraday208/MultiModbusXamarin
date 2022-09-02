using System;
using System.Net.Sockets;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ModbusXamarinDeneme
{
    public partial class App : Application
    {
        public static string[] IPS
        {
            get
            {
                return new string[]
                {
                    "192.168.1.150","192.168.1.32","192.168.1.35"
                };
            }
        }
        public static TcpClient[] Clients { get; set; }

        public App()
        {
            Clients = new TcpClient[3];
            InitializeComponent();
            CreateTCPIP();

            MainPage = new MainPage();
        }

        public void CreateTCPIP()
        {
            Clients[0] = new TcpClient(IPS[0], 502);

            Clients[1] = new TcpClient(IPS[1], 502);

            Clients[2] = new TcpClient(IPS[2], 502);
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
using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using Xamarin.Forms;

namespace ModbusXamarinDeneme
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private Int16 transactionID = 1;
        private Int16 registerCount = 30;
        private string result;

        public string Result
        {
            get { return result; }
            set
            {
                result = value;
                OnPropertyChanged("Result");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public enum ModbusFunctions : Int16
        {
            ReadCoil = 0x01,
            ReadDiscreteInputs = 0x02,
            ReadHoldingRegisters = 0x03,
            ReadInputRegisters = 0x04,
            WriteSingleCoil = 0x05,
            WriteSingleRegister = 0x06,
            WriteMultipleCoils = 0x0F,
            WriteMultipleRegisters = 0x10,
            ReadWriteMultipleRegisters = 0x17
        }

        public MainPage()
        {
            InitializeComponent();
            var a = new Binding
            {
                Path = "Result",
                Source = this,
            };
            lblreturn.SetBinding(Label.TextProperty, a);
            Device.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                ReadData();
                return true;
            });
        }

        private void ReadData()
        {
            Byte[] data = new Byte[12];
            //    { 0x00,0x01, // işlem tanımlayıcı
            //                0x00, 0x00, //protokol tanımlayıcı
            //                0x00, 0x06, //PDU uzunluğu
            //                0x01, // Address
            //                3,
            //                0x00,0x00, // 0.cıdan itibaren
            //                0x00,0x01 //(register *2 byte)
            //};
            Byte[] tBytes = ConvertInt16ToByteArray(transactionID);
            Byte[] RBytes = ConvertInt16ToByteArray(registerCount);

            data[0] = tBytes[0];
            data[1] = tBytes[1];
            data[2] = 0x00;
            data[3] = 0x00;
            data[4] = 0x00;
            data[5] = 0x06;
            data[6] = 0x01;
            data[7] = 0x03;
            data[8] = 0x00;
            data[9] = 0x00;
            data[10] = RBytes[0];
            data[11] = RBytes[1];

            Byte[] rData = new byte[9 * registerCount * 2];
            if (App.Clients[0].Connected)
            {
                NetworkStream networkStream = App.Clients[0].GetStream();
                networkStream.Write(data, 0, data.Length);
                while (true)
                {
                    int bytes = networkStream.Read(rData, 0, rData.Length);
                    if (bytes > 0)
                    {
                        StringBuilder sb = new StringBuilder(rData.Length * 2);
                        foreach (Byte b in rData)
                        {
                            sb.AppendFormat("0x{0:x2}-", b);
                        }
                        Console.WriteLine($"Gelen Data: {sb}");
                        lblreturn.Text = $"Gelen Data: {sb}";
                        break;
                    }
                }
                networkStream.Close();
                transactionID++;
            }
            else
            {
                App.Clients[0] = new TcpClient(App.serverIP, 502);
                ReadData();
            }
        }

        public Byte[] ConvertInt16ToByteArray(Int16 v)
        {
            byte[] intBytes = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            return intBytes;
        }
    }
}
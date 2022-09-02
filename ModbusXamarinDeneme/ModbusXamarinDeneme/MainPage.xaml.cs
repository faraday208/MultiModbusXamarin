using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using Xamarin.Forms;

namespace ModbusXamarinDeneme
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private Int16 transactionID = 1;
        private Int16 registerCount = 30;
        public string result;

        public string Result
        {
            get { return result; }
            set
            {
                result = value;
                OnPropertyChanged("Result");
            }
        }

        public string writeResult;

        public string WriteResult
        {
            get { return writeResult; }
            set
            {
                writeResult = value;
                OnPropertyChanged("WriteResult");
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
            lblWriteReturn.SetBinding(Label.TextProperty, new Binding { Path = "WriteResult", Source = this });
            Device.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                ReadData();
                return true;
            });
        }

        private void WriteSingleRegister(Int16 address, Int16 value)
        {
            Byte[] data = new Byte[12];
            Byte[] rData = new byte[12];

            Byte[] tBytes = ConvertInt16ToByteArray(transactionID);
            Byte[] ABytes = ConvertInt16ToByteArray(address);
            Byte[] VBytes = ConvertInt16ToByteArray(value);
            data[0] = tBytes[0];
            data[1] = tBytes[1];
            data[2] = 0x00;
            data[3] = 0x00;
            data[4] = 0x00;
            data[5] = 0x06;
            data[6] = 0x01;
            data[7] = 0x06;
            data[8] = ABytes[0];
            data[9] = ABytes[1];
            data[10] = VBytes[0];
            data[11] = VBytes[1];

            if (App.Clients[0].Connected)
            {
                NetworkStream networkStream = App.Clients[0].GetStream();
                networkStream.Write(data, 0, data.Length);
                while (true)
                {
                    int bytes = networkStream.Read(rData, 0, rData.Length);
                    if (bytes > 0)
                    {
                        int[] res = new int[2];
                        if (!data[0].Equals(rData[0]) || !data[1].Equals(rData[1]) || !data[10].Equals(rData[10]) || !data[11].Equals(rData[11]))
                        {
                            break;
                        }

                        res[0] = Convert.ToInt16(rData[8]) * 256 + Convert.ToInt16(rData[9]);
                        res[1] = Convert.ToInt16(rData[10]) * 256 + Convert.ToInt16(rData[11]);
                        string sb = "İşlem Tamam:" + res[0].ToString() + "nolu kayıt " + res[1].ToString() + " olarak değiştirildi";

                        WriteResult = sb;
                        break;
                    }
                }
                //networkStream.Close();
                transactionID++;
            }
            else
            {
                App.Clients[0] = new TcpClient(App.serverIP, 502);
                ReadData();
            }
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
                        int[] res = new int[registerCount];
                        if (!data[0].Equals(rData[0]) || !data[1].Equals(rData[1]))
                        {
                            break;
                        }
                        for (int i = 0; i < registerCount; i++)
                        {
                            res[i] = Convert.ToInt16(rData[9 + (i * 2)]) * 256 + Convert.ToInt16(rData[10 + (i * 2)]);
                        }
                        string sb = "Gelen Data:{";
                        foreach (int b in res)
                        {
                            sb += b.ToString() + "-";
                        }
                        sb += "}";

                        Result = sb;
                        break;
                    }
                }
                //networkStream.Close();
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

        private void Button_Clicked(object sender, EventArgs e)
        {
            WriteSingleRegister(Convert.ToInt16(entryAddress.Text), Convert.ToInt16(entryValue.Text));
        }
    }
}
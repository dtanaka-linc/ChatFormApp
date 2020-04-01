using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;

namespace UDPFormsApp
{

    public partial class Form1 : Form
    {
        private UdpClient udpClient;
        private Encoding encoding;
        private DateTime dateTime;

        public Form1()
        {
            InitializeComponent();

            //クライアントの設定
            udpClient = null;
            //エンコーディングの設定
            encoding = Encoding.UTF8;
            //受信可能な状態でないと送信ボタンが押せないよう設定
            Button2.Enabled = false;

        }

        //Button1のClickイベントハンドラ:データ受信の待機
        private void Button1_Click(object sender, EventArgs e)
        {

            try
            {
                //UdpClientを作成
                udpClient = new UdpClient(int.Parse(TextBox1.Text));
                //非同期的なデータ受信を開始する
                udpClient.BeginReceive(ReceiveCallback, udpClient);
            } 
            catch (SocketException ex)
            {
                Console.WriteLine("受信待機時エラー({0}/{1})",
                    ex.Message, ex.ErrorCode);
                return;
            }

            string displayMsg = string.Format("[you] > データ受信の待機中　ポート：" + TextBox1.Text);
            RichTextBox1.BeginInvoke(
                new Action<string>(ShowTextboxString), displayMsg);

            //同じポートで２回以上接続開始しないようにボタンを押せなくする
            Button1.Enabled = false;

            //送信ボタンが押せるようにする
            Button2.Enabled = true;
        }

        //データ受信時の処理
        private void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient udp =(System.Net.Sockets.UdpClient)ar.AsyncState;

            //非同期受信を終了する
            System.Net.IPEndPoint remoteEP = null;

            byte[] rcvBytes;

            try
            {
                rcvBytes = udp.EndReceive(ar, ref remoteEP);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("データ受信エラー({0}/{1})",
                    ex.Message, ex.ErrorCode);
                return;
            }
            catch (ObjectDisposedException ex)
            {
                //すでに閉じている時は終了
                Console.WriteLine("Socketは閉じられています。");
                return;
            }

            //データを文字列に変換・テキストボックスに表示
            string rcvMsg = System.Text.Encoding.UTF8.GetString(rcvBytes);

            string displayMsg = string.Format("[{0} ({1})] > {2}",
                remoteEP.Address, remoteEP.Port, rcvMsg);

            RichTextBox1.BeginInvoke(
                new Action<string>(ShowTextboxString), displayMsg);

            //再びデータ受信を開始する
            udp.BeginReceive(ReceiveCallback, udp);
        }

        //Button2のClickイベントハンドラ：データを送信
        private void Button2_Click(object sender, EventArgs e)
        {
            //送信するデータを作成する
            byte[] sendBytes = encoding.GetBytes(TextBox4.Text);

            //非同期的にデータを送信する
            udpClient.BeginSend(sendBytes, sendBytes.Length,
                TextBox2.Text, int.Parse(TextBox3.Text),
                SendCallback, udpClient);
            //自分が送信したメッセージを表示する
            string displayMsg = string.Format("[you] >" + TextBox4.Text);

            RichTextBox1.BeginInvoke(
                new Action<string>(ShowTextboxString), displayMsg);
        }

        //データを送信した時
        private void SendCallback(IAsyncResult ar)
        {
            System.Net.Sockets.UdpClient udp =
                (System.Net.Sockets.UdpClient)ar.AsyncState;

            //非同期送信を終了する
            try
            {
                udp.EndSend(ar);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("送信エラー({0}/{1})",
                    ex.Message, ex.ErrorCode);
            }
            catch (ObjectDisposedException ex)
            {
                //すでに閉じている時は終了
                Console.WriteLine("Socketは閉じられています。");
            }
           
        }

        //フォームのFormClosedイベントハンドラ：UdpClientを閉じる
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (udpClient != null)
            {
                udpClient.Close();
            }
        }

        //テキストボックスにメッセージを表示する
        private void ShowTextboxString(string str)
        {
            dateTime = DateTime.Now;
            RichTextBox1.Text = dateTime + str + "\r\n" + RichTextBox1.Text;
        }
    }

}

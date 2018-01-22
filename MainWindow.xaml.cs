using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace ChiHya {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        bool isPlay = false;
        const int MessageBufferSize = 65536;
        ClientWebSocket _ws = null;

        private async void WS_Connect() {
            if (_ws == null) {
                _ws = new ClientWebSocket();
            }

            if (_ws.State != WebSocketState.Open) {
                await _ws.ConnectAsync(new Uri("ws://shijou.moe:5577"), CancellationToken.None);

                while (_ws.State == WebSocketState.Open) {
                    var buff = new ArraySegment<byte>(new byte[MessageBufferSize]);
                    var ret = await _ws.ReceiveAsync(buff, CancellationToken.None);
                    var json = (new UTF8Encoding()).GetString(buff.Take(ret.Count).ToArray());
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    Title.Content = data["title"];
                    Artist.Content = data["artist"];
                }
            }
        }

        public MainWindow() {
            InitializeComponent();
            // イベント割当
            Handle.MouseLeftButtonDown += (o, e) => DragMove();
            CloseButton.Click += (o, e) => this.Close();
            MiniButton.Click += (o, e) => this.WindowState = WindowState.Minimized;
            this.MouseEnter += (o, e) => this.Opacity = 1;
            this.MouseLeave += (o, e) => this.Opacity = 0.6;
            // プレイヤー
            Player.Source = new Uri("http://shijou.moe:8000/imas-radio-lq.mp3");
            WS_Connect();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e) {
            if (isPlay) {
                Player.Stop();
                Player.Close();
                PlayButton.Content = "開始";
                isPlay = false;
            } else {
                Player.Play();
                PlayButton.Content = "停止";
                isPlay = true;
            }
        }
    }
}

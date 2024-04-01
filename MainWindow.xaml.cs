using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace DividedScreen
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, WebView2> webViewList = new Dictionary<string, WebView2>();

        JObject message = new JObject(
                new JProperty("target", ""),
                new JProperty("action", "select"),
                new JProperty("coffee", "")
            );
        
        public MainWindow()
        {
            InitializeComponent();

            webView1st.Source = new Uri("http://127.0.0.1:5500/index.html");

            InitializeAsync();
        }

        async void InitializeAsync()
        {
            await webView1st.EnsureCoreWebView2Async(null);
            await webView2nd.EnsureCoreWebView2Async(null);

            webView1st.CoreWebView2.WebMessageReceived += webView1st_WebMessageReceived;
            webView2nd.CoreWebView2.WebMessageReceived += webView2nd_WebMessageReceived;

            webViewList.Add("webView1st", webView1st);
            webViewList.Add("webView2nd", webView2nd);
        }

        private void webView1st_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            //Console.WriteLine("webView1st_WebMessageReceived : " + e.WebMessageAsJson);

            runMessage(JObject.Parse(e.WebMessageAsJson));
        }

        private void webView2nd_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            //Console.WriteLine("webView2nd_WebMessageReceived : " + e.WebMessageAsJson);

            runMessage(JObject.Parse(e.WebMessageAsJson));
        }

        private async void runMessage( JObject message )
        {
            var target = (string)message["target"];
            var action = (string)message["action"];

            if (action.Equals("navigation"))
            {
                //webView2nd.Source = new Uri((string)message["uri"]);
                webViewList[target].Source = new Uri((string)message["uri"]);
            }
            else if (action.Equals("select"))
            {
                var result = await webViewList[target].CoreWebView2.ExecuteScriptAsync($@"selectCoffee('{(string)message["coffee"]}', '{target}');");
            }
        }

        private void find_Click(object sender, RoutedEventArgs e)
        {
            message["target"] = "webView1st";
            message["coffee"] = item.Text;
            runMessage(message);

            message["target"] = "webView2nd";
            runMessage(message);
        }

        private async Task<bool> PrintPDF()
        {
            //var dialog = new SaveFileDialog
            //{
            //    DefaultExt = "pdf",
            //    AddExtension = true,
            //    FileName = "left"
            //};

            //if (dialog.ShowDialog() == false) return;

            var printSettings = webView1st.CoreWebView2.Environment.CreatePrintSettings();
            printSettings.ShouldPrintBackgrounds = true;
            printSettings.ScaleFactor = 1;
            printSettings.MarginTop = printSettings.MarginBottom = printSettings.MarginLeft = printSettings.MarginRight = 1;
            return await webView1st.CoreWebView2.PrintToPdfAsync("C:\\Users\\maninzoo\\Documents\\left.pdf", printSettings);
        }

        private async void pdf_Click(object sender, RoutedEventArgs e)
        {
            Task<bool> t = PrintPDF();

            if( await t )
            {
                Console.WriteLine("Printed");
            }
        }
    }
}

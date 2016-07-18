using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HTTPRequestLibUWP;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Post getReq = new Post();



            getReq.Request("http://posttestserver.com/post.php", "some stuff", new HTTPAsyncCallback(RequestCallback));

            Task.Run(() => doRequest());
;            
        }

        public async Task doRequest()
        {
            Post getReq = new Post();
            HTTPResponse resp = await getReq.Request("http://posttestserver.com/post.php", "some stuff");
            string s = System.Text.Encoding.UTF8.GetString(resp.bytes);
        }

        public void RequestCallback(HTTPResponse resp)
        {
            string s = System.Text.Encoding.UTF8.GetString(resp.bytes);
        }
    }
}
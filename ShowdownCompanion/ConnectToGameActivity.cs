using System;
using System.Collections.Generic;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using Android.Support.V7.Widget;

namespace ShowdownCompanion
{
    [Activity(Label = "Connect")]
    public class ConnectToGameActivity : Activity
    {
        private const int listenPort = 11000;
        private TextView statusText;
        private RecyclerView recyclerView;
        private ConnectListAdapter adapter;
        private static List<ComputerInfo> serverList = new List<ComputerInfo>();
        private readonly object lockList = new object();
        private bool messageReceived;
        private string ipString;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ConnectLayout);
            // Create your application here
            Button searchButton = FindViewById<Button>(Resource.Id.SearchButton);
            searchButton.Click += BeginSearch;

            statusText = FindViewById<TextView>(Resource.Id.statusText);

            recyclerView = FindViewById<RecyclerView>(Resource.Id.serverRecyclerView);

            // improve performance if you know that changes in content
            // do not change the size of the RecyclerView
            recyclerView.HasFixedSize = false;

            // use a linear layout manager
            LinearLayoutManager layoutManager = new LinearLayoutManager(this);
            recyclerView.SetLayoutManager(layoutManager);

            // specify an adapter
            adapter = new ConnectListAdapter()
            {
                items = serverList
            };

            adapter.ItemClick += OnItemClick;

            recyclerView.SetAdapter(adapter);

            Button manualButton = FindViewById<Button>(Resource.Id.manualConnectButton);
            manualButton.Click += ManualButton_Click;

        }

        private void ManualButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(ManualConnectionActivity));
            StartActivity(intent);
        }

        void OnItemClick(object sender, string ipFromClick)
        {
            ipString = ipFromClick;
            ConnectToServer();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        public void ConnectToServer()
        {
            IPAddress.TryParse(ipString, out IPAddress ip);

            if (ip == null)
            {
                Toast.MakeText(Application.Context, "Inncorect IP Address Format. Try Again...", ToastLength.Long).Show();
                return;
            }

            Intent intent = new Intent(this, typeof(InGameActivity));

            intent.PutExtra("ipAddress", ipString);
            intent.PutExtra("port", "3333");
            StartActivity(intent);
        }

        #region Server Code
        private void BeginSearch(Object sender, EventArgs e)
        {
            serverList.Clear();
            adapter.items = serverList;
            adapter.NotifyDataSetChanged();
            ThreadPool.QueueUserWorkItem(o => ReceiveMessages());
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient u = ((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = ((UdpState)(ar.AsyncState)).e;
            try
            {
                Byte[] receiveBytes = u.EndReceive(ar, ref e);
                string receiveString = Encoding.ASCII.GetString(receiveBytes);
                Console.WriteLine("Received: {0}", receiveString);
                
                lock (lockList)
                {
                    string[] receiveItems = receiveString.Split(';');
                    if (receiveItems.Length > 1)
                    {
                        if (serverList.FindIndex((elm) => elm.Name.Equals(receiveItems[1])) < 0)
                        {
                            serverList.Add(new ComputerInfo()
                            {
                                IpString = receiveItems[0],
                                Name = receiveItems[1]
                            });
                        }
                    }
                }

                }
            catch { }
            finally {
                messageReceived = true;
            }
            
        }

        public void ReceiveMessages()
        {
            messageReceived = false;
            // Receive a message and write it to the console.
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, listenPort);
            UdpClient client = new UdpClient(endPoint);

            UdpState state = new UdpState
            {
                e = endPoint,
                u = client
            };

            Console.WriteLine("listening for messages");
            client.BeginReceive(new AsyncCallback(ReceiveCallback), state);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            RunOnUiThread(() =>
            {
                Console.WriteLine("Scanning...");
                statusText.Text = "Status: Scanning...";
                Toast.MakeText(this, "Status: Scanning...", ToastLength.Long).Show();
            });
            while(stopwatch.ElapsedMilliseconds < 7000)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Trying...");
                if (messageReceived)
                {
                    messageReceived = false;
                    client.BeginReceive(new AsyncCallback(ReceiveCallback), state);
                }
            }
            client.Close();
            stopwatch.Stop();
            Console.WriteLine("Status: Done Searching");
            RunOnUiThread(() =>
            {
                if (serverList.Count == 0)
                {
                    statusText.Text = "No computers found. Try Again or try manual entry.";
                    Toast.MakeText(this, "No computers found. Try Again or try manual entry", ToastLength.Short).Show();
                }
                else
                {
                    statusText.Text = "Status: Done Searching";
                    Toast.MakeText(this, "Done searching! Computers found!", ToastLength.Short).Show();
                    adapter.items = serverList;
                    adapter.NotifyDataSetChanged();
                }
            });
        }

        class UdpState
        {
            public IPEndPoint e;
            public UdpClient u;
        }
#endregion

    }
}
using System;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace ShowdownCompanion
{
    [Activity(Label = "InGameActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class InGameActivity : Activity, View.IOnTouchListener
    {
        // The port number for the remote device.
        private static int port;
        private static IPAddress ipAddress;
        private static Socket sender;
        private static Vibrator vibrator;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.InGame);

            View decorView = Window.DecorView;
            var uiOptions = (int)decorView.SystemUiVisibility;
            var newUiOptions = uiOptions;

            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            //newUiOptions |= (int)SystemUiFlags.Fullscreen;
            newUiOptions |= (int)SystemUiFlags.HideNavigation;
            newUiOptions |= (int)SystemUiFlags.Immersive;

            decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;

            if (!IPAddress.TryParse(Intent.Extras.GetString("ipAddress"), out ipAddress))
            {
                Console.WriteLine("ERROR in IP"); //TODO Better error handling
            }
            if(!int.TryParse(Intent.Extras.GetString("port"), out port))
            {
                Console.WriteLine("ERROR in port"); //TODO Better error handling
            }

            vibrator = (Vibrator)GetSystemService(VibratorService);

            View v = FindViewById(Resource.Id.InGameScreen);
            v.SetOnTouchListener(this);

        }

        protected override void OnStart()
        {
            base.OnStart();
            if (sender == null || !sender.Connected)
            {
                var task = new Task(() => StartClient());
                task.Start();
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (sender != null)
            {
                if (sender.Connected)
                {
                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
            }
        }

        public void StartClient()
        {
            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP  socket.
                sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);

                    using (var h = new Handler(Looper.MainLooper))
                    {
                        h.Post(() => {
                            Toast.MakeText(Application.Context, "Connected to Game", ToastLength.Long).Show();
                            TextView text = FindViewById<TextView>(Resource.Id.connectString);
                            text.Text = "Connected";
                            text.ContentDescription = "Connected";
                        });
                    }
                    
                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());
                    vibrator.Vibrate(new Int64[]{ 0, 200, 200, 200, 200, 200, 200}, -1);

                    // An incoming connection needs to be processed.
                    while (true)
                    {
                        if (sender.Connected)
                        {
                            bytes = new byte[1024];
                            int bytesRec = sender.Receive(bytes);
                            if(bytesRec != 0)
                            {
                                string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                                Console.WriteLine("Received {0}", data);
                                ProcessMessage(data);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ProcessMessage(string message)
        {
            if (message.Equals("wall;"))
            {
                vibrator.Vibrate(100);
            }
            else if (message.Equals("ball;"))
            {
                vibrator.Vibrate(300);
            }
        }

        private void SendMessage(string message)
        {
            if (sender.Connected)
            {
                // Encode the data string into a byte array.
                byte[] msg = Encoding.ASCII.GetBytes(message);

                // Send the data through the socket.
                int bytesSent = sender.Send(msg);
            }
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            if(e.Action == MotionEventActions.Down)
            {
                Console.WriteLine("DOWN");
                SendMessage("up;");
            }
            if(e.Action == MotionEventActions.Up)
            {
                Console.WriteLine("UP");
                SendMessage("down;");
            }
            return true;
        }
    }
}
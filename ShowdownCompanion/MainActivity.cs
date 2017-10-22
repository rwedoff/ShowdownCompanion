using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Views;
using Android.Content;
using System.Collections.Generic;
using Android.Preferences;
using Android.Net.Wifi;
using System.Net;

namespace ShowdownCompanion
{
    [Activity(Label = "ShowdownCompanion", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private ISharedPreferences preferences;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            preferences = PreferenceManager.GetDefaultSharedPreferences(this);
            EditText ipEdit = FindViewById<EditText>(Resource.Id.ipEditText);
            ipEdit.Text = preferences.GetString("ipAddress", "0.0.0.0");

            WifiManager wifiManager = (WifiManager)GetSystemService(WifiService);

            WifiInfo wifiInfo = wifiManager.ConnectionInfo;
            
            TextView textView = FindViewById<TextView>(Resource.Id.internetInfo);

            String wfState = "Wifi State: " + wifiInfo.SSID;
            textView.Text = wfState;

            Button connectButton = FindViewById<Button>(Resource.Id.connectButton);
            connectButton.Click += ConnectToServer;
        }

        public void ConnectToServer(Object sender, EventArgs e)
        {
            EditText ipEdit = FindViewById<EditText>(Resource.Id.ipEditText);

            if (ipEdit == null)
                return;
            
            String ipString = ipEdit.Text.ToString();

            IPAddress ip = null;
            IPAddress.TryParse(ipString, out ip);

            if(ip == null)
            {
                Toast.MakeText(Application.Context, "Inncorect IP Address Format. Try Again...", ToastLength.Long).Show();
                return;
            }

            ISharedPreferencesEditor preferencesEditor = preferences.Edit();
            preferencesEditor.PutString("ipAddress", ipString);
            preferencesEditor.Apply();

            Intent intent = new Intent(this, typeof(InGameActivity));

            intent.PutExtra("ipAddress", ipString);
            intent.PutExtra("port", "3333");
            StartActivity(intent);
        }
    }
}


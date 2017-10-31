using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.Net;
using Android.Net.Wifi;
using Android.Preferences;

namespace ShowdownCompanion
{
    [Activity(Label = "Manual Connection")]
    public class ManualConnectionActivity : Activity
    {
        private ISharedPreferences preferences;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.ManualConnection);
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

            IPAddress.TryParse(ipString, out IPAddress ip);

            if (ip == null)
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
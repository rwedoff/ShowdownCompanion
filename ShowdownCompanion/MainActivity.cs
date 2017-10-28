﻿using Android.App;
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
    [Activity(Label = "ShowdownCompanion", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            
            WifiManager wifiManager = (WifiManager)GetSystemService(WifiService);

            WifiInfo wifiInfo = wifiManager.ConnectionInfo;

            TextView textView = FindViewById<TextView>(Resource.Id.internetInfo);

            String wfState = "Wifi State: " + wifiInfo.SSID;
            textView.Text = wfState;

            Button connectButton = FindViewById<Button>(Resource.Id.connectButton);
            connectButton.Click += SearchForServers;
        }

        public void SearchForServers(Object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(ConnectToGameActivity));
            StartActivity(intent);
        }
    }
}


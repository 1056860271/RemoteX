﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;

[assembly: Xamarin.Forms.Dependency(typeof(RemoteX.Droid.BluetoothManager))]
namespace RemoteX.Droid
{
    /// <summary>
    /// 对IBluetoothManager的Android端的实现
    /// 使用使和Android.Bluetooth.BluetoothManager区分开来，两个不一样
    /// 
    /// </summary>
    class BluetoothManager : RemoteX.Bluetooth.IBluetoothManager
    {
        BluetoothAdapter _BluetoothAdapter;
        bool _IsDiscoverying;
        Receiver _DiscoveryStartedReceiver;
        Receiver _DevicesFoundReceiver;
        Receiver _DiscoveryFinishedReceiver;

        public BluetoothManager()
        {
            _IsDiscoverying = false;
            _BluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            _DiscoveryStartedReceiver = new Receiver(this);
            _DevicesFoundReceiver = new Receiver(this);
            _DiscoveryFinishedReceiver = new Receiver(this);
        }
        public bool IsDiscoverying
        {
            get
            {
                return _IsDiscoverying;
            }
            private set
            {
                this._IsDiscoverying = value;
            }
        }
        public bool Supported
        {
            get
            {
                if (_BluetoothAdapter != null)
                {
                    return true;
                }
                return false;
            }
        }

        public event RemoteX.Bluetooth.BluetoothScanResultHandler onDevicesFound;
        public event RemoteX.Bluetooth.BluetoothStartEndScanHandler onDiscoveryFinished;
        public event RemoteX.Bluetooth.BluetoothStartEndScanHandler onDiscoveryStarted;

        public void searchForBlutoothDevices()
        {
            IntentFilter startFilter = new IntentFilter(BluetoothAdapter.ActionDiscoveryStarted);
            IntentFilter foundFilter = new IntentFilter(BluetoothDevice.ActionFound);
            IntentFilter finshFilter = new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished);

            Application.Context.RegisterReceiver(_DiscoveryStartedReceiver, startFilter);
            Application.Context.RegisterReceiver(_DevicesFoundReceiver, foundFilter);
            Application.Context.RegisterReceiver(_DiscoveryFinishedReceiver, finshFilter);

            _BluetoothAdapter.StartDiscovery();
        }

        public IConnection createRfcommClientConnection(RemoteX.Bluetooth.IBluetoothDevice deviceWrapper, Guid guid)
        {
            UUID uuid = UUID.FromString(guid.ToString());
            if(!(deviceWrapper is BluetoothDeviceWrapper))
            {
                return null;
            }
            BluetoothDevice device = (deviceWrapper as BluetoothDeviceWrapper).BluetoothDevice;
            BluetoothClientConnection clientConnection = new BluetoothClientConnection(device, uuid);
            System.Diagnostics.Debug.WriteLine("Connection Created");
            return clientConnection;
        }

        private class Receiver : BroadcastReceiver
        {
            BluetoothManager _BluetoothManager;
            public Receiver(BluetoothManager bluetoothManager)
            {
                this._BluetoothManager = bluetoothManager;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;
                if (BluetoothAdapter.ActionDiscoveryStarted == action)
                {
                    this._BluetoothManager.IsDiscoverying = true;
                    _BluetoothManager.onDiscoveryStarted?.Invoke(_BluetoothManager);
                    Application.Context.UnregisterReceiver(this);
                }
                if (BluetoothDevice.ActionFound == action)
                {
                    BluetoothDevice device = intent.GetParcelableExtra(BluetoothDevice.ExtraDevice) as BluetoothDevice;
                    BluetoothDeviceWrapper deviceWrapper = new BluetoothDeviceWrapper(device);

                    _BluetoothManager.onDevicesFound?.Invoke(_BluetoothManager, new RemoteX.Bluetooth.IBluetoothDevice[] { deviceWrapper });
                }
                if (BluetoothAdapter.ActionDiscoveryFinished == action)
                {
                    _BluetoothManager.IsDiscoverying = false;
                    _BluetoothManager.onDiscoveryFinished?.Invoke(_BluetoothManager);
                    Application.Context.UnregisterReceiver(this);
                    Application.Context.UnregisterReceiver(_BluetoothManager._DevicesFoundReceiver);
                }

            }
        }
    }
}
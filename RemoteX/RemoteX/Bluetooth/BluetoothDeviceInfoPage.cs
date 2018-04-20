﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Xamarin.Forms;

namespace RemoteX.Bluetooth
{
    public class BluetoothDeviceInfoPage : ContentPage
    {
        private IBluetoothDevice _BluetoothDevice;
        ObservableCollection<Guid> guidList;

        private Guid? _SelectedGuid;

        public BluetoothDeviceInfoPage(IBluetoothDevice bluetoothDevice)
        {
            this._SelectedGuid = null;
            this._BluetoothDevice = bluetoothDevice;
            this.Title = _BluetoothDevice.Name;
            guidList = new ObservableCollection<Guid>();
            Guid[] lastestUuids = _BluetoothDevice.LastestFetchedUuids;
            if (lastestUuids != null)
            {
                foreach (var guid in lastestUuids)
                {
                    guidList.Add(guid);
                }
            }

            Label nameLabel = new Label();
            nameLabel.BindingContext = _BluetoothDevice;
            nameLabel.SetBinding(Label.TextProperty, "Name");

            Label addressLabel = new Label();
            addressLabel.BindingContext = _BluetoothDevice;
            addressLabel.SetBinding(Label.TextProperty, "Address");

            ListView uuidListView = new ListView();
            uuidListView.ItemsSource = guidList;
            uuidListView.IsPullToRefreshEnabled = true;
            uuidListView.Refreshing += (object sender, EventArgs e) =>
            {
                if (_BluetoothDevice.IsFetchingUuids)
                {
                    uuidListView.EndRefresh();
                }
                _BluetoothDevice.FetchUuidsWithSdp();
            };
            uuidListView.ItemSelected += onGuidSelected;
            if (guidList.Count == 0)
            {
                uuidListView.BeginRefresh();
            }

            _BluetoothDevice.OnUuidsFetched += (IBluetoothDevice device, Guid[] guids) =>
            {
                System.Diagnostics.Debug.WriteLine("UUID GET");
                if (uuidListView.IsRefreshing)
                {
                    uuidListView.EndRefresh();
                }
                guidList.Clear();
                if (guids != null)
                {
                    foreach (var guid in guids)
                    {
                        guidList.Add(guid);
                    }
                }
            };
            Button connectButton = new Button
            {
                Text = "Connect",
            };
            connectButton.Clicked += onConnectClicked;

            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children = {
                    nameLabel,
                    addressLabel,
                    uuidListView,
                    new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        VerticalOptions = LayoutOptions.End,
                        Children ={connectButton }
                    }
                }
            };
        }

        private void onGuidSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }
            this._SelectedGuid = e.SelectedItem as Guid?;
        }

        private async void onConnectClicked(object sender, EventArgs e)
        {
            if (_SelectedGuid == null)
            {
                Debug.WriteLine("NO UUID SELECTED");
                return;
            }
            ConnectionEstablishState connectState = ConnectionEstablishState.failed;
            while (connectState == ConnectionEstablishState.failed)
            {
                IConnection connection = DependencyService.Get<IBluetoothManager>().createRfcommClientConnection(_BluetoothDevice, (Guid)_SelectedGuid);
                connectState = await connection.ConnectAsync();
                Debug.WriteLine("EMMMM" + connectState);
            }

        }
    }
}
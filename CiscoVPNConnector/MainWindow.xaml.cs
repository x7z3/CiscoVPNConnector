using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CiscoVPNConnecter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string DisconnectSign = "⬛";
        private const string DisconnectTooltipText = "Disconnect";

        private const string ConnectSign = "▶";
        private const string ConnectTooltipText = "Connect";

        private const string ConnectingSign = "⌛";
        private const string ConnectingTooltipText = "Connecting";

        private readonly string _fileName = "CiscoVpnConnector.json";
        private readonly string _userProfileDir = Environment.GetEnvironmentVariable("USERPROFILE");
        private readonly string _settingsFile;
        private AppSettingsModel _appSettings = new();

        public MainWindow()
        {
            _settingsFile = $"{_userProfileDir}\\{_fileName}";

            InitializeComponent();
            ReadSettings();
            SetWindowSettings(_appSettings);

            if (VpnConnector.IsConnected())
                SetButtonsState(true, VpnConnect_1, VpnConnect_2, VpnConnect_3);
            else
                SetButtonsState(false, VpnConnect_1, VpnConnect_2, VpnConnect_3);

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveSettings();
            base.OnClosing(e);
        }

        private void SetButtonsState(bool areButtonsConnected, params Button[] buttons)
        {
            foreach (var button in buttons)
            {
                if (areButtonsConnected) ButtonConnectedState(button);
                else ButtonDisconnectedState(button);
            }
        }

        public void ReadSettings()
        {
            if (File.Exists(_settingsFile))
            {
                string json = File.ReadAllText(_settingsFile);
                _appSettings = JsonSerializer.Deserialize<AppSettingsModel>(json);
            }
        }

        public void SaveSettings()
        {
            _appSettings.VpnHost_1 = VpnHost_1.Text;
            _appSettings.VpnHost_2 = VpnHost_2.Text;
            _appSettings.VpnHost_3 = VpnHost_3.Text;

            _appSettings.UserName = Login.Text;
            _appSettings.UserPassword = Password.Password;

            string json = JsonSerializer.Serialize<AppSettingsModel>(_appSettings);
            File.WriteAllText(_settingsFile, json);
        }

        private void SetWindowSettings(AppSettingsModel appSettings)
        {
            if (appSettings.VpnHost_1 != null)
                VpnHost_1.Text = appSettings.VpnHost_1;
            if (appSettings.VpnHost_2 != null)
                VpnHost_2.Text = appSettings.VpnHost_2;
            if (appSettings.VpnHost_3 != null)
                VpnHost_3.Text = appSettings.VpnHost_3;

            if (appSettings.UserPassword != null)
                Password.Password = appSettings.UserPassword;
            if (appSettings.UserName != null)
                Login.Text = appSettings.UserName;
        }

        private void ButtonConnectedState(Button button)
        {
            button.Dispatcher.Invoke(() =>
            {
                button.Background = Brushes.Green;
                button.Content = DisconnectSign;
                button.ToolTip = DisconnectTooltipText;
                button.IsEnabled = true;
            });
        }

        private void ButtonDisconnectedState(Button button)
        {
            button.Dispatcher.Invoke(() =>
            {
                button.Background = Brushes.Orange;
                button.Content = ConnectSign;
                button.ToolTip = ConnectTooltipText;
                button.IsEnabled = true;
            });

        }

        private void ButtonWaitState(Button button)
        {
            button.Dispatcher.Invoke(() =>
            {
                button.Content = ConnectingSign;
                button.ToolTip = ConnectingTooltipText;
                button.IsEnabled = false;
            });
        }

        private void EnableButtons(bool enableButtons, params Button[] buttons)
        {
            foreach (var button in buttons) button.Dispatcher.Invoke(() => button.IsEnabled = enableButtons);
        }

        private void ButtonsState(bool setConnectedState, params Button[] buttons)
        {
            foreach (var button in buttons)
            {
                if (setConnectedState) ButtonConnectedState(button);
                else ButtonDisconnectedState(button);
            }
        }

        private void ConnectAndChangeButtonState(string host, Button button, params Button[] otherButtons)
        {
            ButtonWaitState(button);
            EnableButtons(false, VpnConnect_1, VpnConnect_2, VpnConnect_3);
            if (!VpnConnector.IsConnected())
            {
                if (Connect(host))
                {
                    ButtonConnectedState(button);
                    EnableButtons(false, otherButtons);
                }
            }
            else
            {
                if (VpnConnector.Disconnect())
                {
                    ButtonDisconnectedState(button);
                    EnableButtons(true, VpnConnect_1, VpnConnect_2, VpnConnect_3);
                    SetButtonsState(false, VpnConnect_1, VpnConnect_2, VpnConnect_3);
                }
            }
        }

        private bool Connect(string host)
        {
            return VpnConnector.Connect(host, _appSettings.UserName, _appSettings.UserPassword);
        }

        private async void VpnConnect_1_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => ConnectAndChangeButtonState(_appSettings.VpnHost_1, (Button)sender, VpnConnect_2, VpnConnect_3));
        }

        private async void VpnConnect_2_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => ConnectAndChangeButtonState(_appSettings.VpnHost_2, (Button)sender, VpnConnect_1, VpnConnect_3));
        }

        private async void VpnConnect_3_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => ConnectAndChangeButtonState(_appSettings.VpnHost_3, (Button)sender, VpnConnect_1, VpnConnect_2));
        }
    }
}

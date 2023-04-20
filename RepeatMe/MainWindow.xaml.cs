using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using RepeatMe.Models;
using Button = RepeatMe.Models.Button;

namespace RepeatMe
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Setting sampleSettings = new Setting
        {
            StandardDelayTime = 50,
            SensibilityRange = 50,
            MaxRotationNumber = 0,
            DefaultKeyPress = "z",
            PressKeys = new Dictionary<string, int[]>()
                {
                    { "1", new int[] {4000,5000,6000} },
                    { "2", new int[] {2000,7000 } }
                },
            Interval = 10
        };


        int processId;
        SmartDispatcherTimer timer;

        private Setting settings = new Setting();

        bool enableDefaultKey = true;
        int rotation = 0;

        short deafultKeyPress;

        Dictionary<short, int[]> pressKeys = new Dictionary<short, int[]>();


        double interval;
        double millisecondInterval => interval * 1000 - 200; // 200 as buffer

        IntPtr hndl() { return new WindowInteropHelper(this).Handle; }

        public MainWindow()
        {
            InitializeComponent();
            processlist.DisplayMemberPath = nameof(MyProcess.ProcessDisplay);
            // default settings
            settings = sampleSettings;

            timer = new SmartDispatcherTimer();
            timer.TickTask = Timer_Tick;

            ResetConfig();
        }

        private async Task Timer_Tick()
        {
            if (settings.MaxRotationNumber != 0 && rotation >= settings.MaxRotationNumber)
            {
                Stop();
                return;
            }
            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < millisecondInterval && timer.IsEnabled)
            {
                var currentWindow = User.GetForegroundWindow();
                if (currentWindow != processId)
                {
                    await Task.Delay(1000);
                    continue;
                }
                foreach (var key in pressKeys)
                {
                    for (int i = 0; i < key.Value.Length; i++)
                    {
                        if (sw.ElapsedMilliseconds - settings.SensibilityRange < key.Value[i] && sw.ElapsedMilliseconds + settings.SensibilityRange > key.Value[i])
                        {
                            Button.PressKey(key.Key);
                            await Task.Delay(50);
                        }
                    }
                }
                if (enableDefaultKey)
                {
                    Button.PressKey(deafultKeyPress);
                }
                await Task.Delay(settings.StandardDelayTime);
            }
            rotation++;
        }

        private void processlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            processId = (((ListBox)sender).SelectedItem as MyProcess).ProcessId;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) => Stop();
        private void Stop()
        {
            timer.Stop();
            btnFind.IsEnabled = true;
            processlist.IsEnabled = true;
            txbInterval.IsEnabled = true;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            btnLoad.IsEnabled = true;
            btnSave.IsEnabled = true;
            rectStatus.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }

        private Task Start()
        {
            settings.Interval = interval;

            rotation = 0;

            timer.Interval = TimeSpan.FromSeconds(interval);
            timer.Start();

            var t = Timer_Tick();

            btnFind.IsEnabled = false;
            processlist.IsEnabled = false;
            txbInterval.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            btnLoad.IsEnabled = false;
            btnSave.IsEnabled = false;
            rectStatus.Fill = new SolidColorBrush(Color.FromRgb(124, 252, 0));
            return t;
        }


        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (processId == 0)
                return;

            var result = double.TryParse(txbInterval.Text, out interval);
            if (!result) return;

            await Start();
            
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            enableDefaultKey = ((CheckBox)sender).IsChecked ?? false;
        }

        private void Button_Click(object sender, RoutedEventArgs e) => FindProcess();

        private void Window_Loaded(object sender, RoutedEventArgs e) => FindProcess();


        private void FindProcess()
        {
            processlist.Items.Clear();
            for (int window = User.GetWindow(User.GetDesktopWindow(), 5); window != 0; window = User.GetWindow(window, 2))
            {
                if (window == hndl().ToInt32())
                {
                    window = User.GetWindow(window, 2);
                }
                if (User.IsWindowVisible(window) != 0)
                {
                    StringBuilder _stringBuilder = new StringBuilder(50);
                    User.GetWindowText(window, _stringBuilder, _stringBuilder.Capacity);
                    string text = _stringBuilder.ToString();
                    if (text.Length > 0)
                    {
                        processlist.Items.Add(new MyProcess(text, window));
                    }
                }
            }
            
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = ".json";
            openFileDlg.Filter = "Json documents (.json)|*.json";

            if (openFileDlg.ShowDialog() == true)
            {
                settings = JsonConvert.DeserializeObject<Setting>(System.IO.File.ReadAllText(openFileDlg.FileName));
                ResetConfig();
            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Json document (*.json)|*.json";
            saveFileDialog.FileName = "config";

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(settings));
        }

        private void ResetConfig()
        {
            txbInterval.Text = settings.Interval.ToString();

            enableDefaultKey = !string.IsNullOrEmpty(settings.DefaultKeyPress);

            deafultKeyPress = enableDefaultKey ? (short)Enum.Parse(typeof(Button.BT7), $"KEY_{settings.DefaultKeyPress.ToUpper()}") : default;

            pressKeys.Clear();

            foreach (var item in settings.PressKeys)
            {
                pressKeys.Add((short)Enum.Parse(typeof(Button.BT7), $"KEY_{item.Key.ToUpper()}"), item.Value);
            }
        }
    }
}

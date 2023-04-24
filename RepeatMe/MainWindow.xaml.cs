using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using RepeatMe.Models;
using Button = RepeatMe.Models.Button;
using System.Windows.Threading;
using System.Linq;
using System.Diagnostics;
using static RepeatMe.Models.Button;
using System.Windows.Input;
using System.Threading;
using System.ComponentModel;

namespace RepeatMe
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Setting sampleSettings = new Setting
        {
            DefaultDelayTime = 50,
            SensibilityRange = 50,
            MaxRotationNumber = 0,
            DefaultKeyPress = "z",
            DefaultKeyModifiers = new List<string> { "SHIFT" },
            PressKeys = new List<KeyToPressDictionary>()
                {
                    new KeyToPressDictionary{
                        Key = "1",
                     Values =  new int[] {4000,5000,6000} },
                    new KeyToPressDictionary{
                        Key = "2",
                        Modifiers = new List<string> { "SHIFT" },
                    Values = new int[] {2000,7000 } }
                },
            Interval = 10
        };


        int processId;
        SmartDispatcherTimer timer = new SmartDispatcherTimer();
        DispatcherTimer dequeueTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        Queue<LetterTime> queue = new Queue<LetterTime>();

        private Setting settings = new Setting();

        bool enableDefaultKey = true;
        int rotation = 0;

        LetterTime deafultKeyPress;


        LetterTime[] pressKeys;

        double interval;

        IntPtr hndl() { return new WindowInteropHelper(this).Handle; }

        public MainWindow()
        {
            InitializeComponent();

            processlist.DisplayMemberPath = nameof(MyProcess.ProcessDisplay);
            // default settings
            settings = sampleSettings;
            timer.TickTask = Timer_Tick;
            dequeueTimer.Tick += DequeueTimer_Tick;
            ResetConfig();
        }

        private void DequeueTimer_Tick(object sender, EventArgs e)
        {
            if (!timer.IsRunning || queue.Count == 0 || currentWindow != processId)
                return;
            var x = queue.Dequeue();
            Button.PressKey(x.Letter, x.Modifiers);
        }
        CancellationTokenSource checkWindowToken = new CancellationTokenSource();
        private async Task Timer_Tick()
        {
            if (settings.MaxRotationNumber != 0 && rotation >= settings.MaxRotationNumber)
            {
                Stop();
                return;
            }

            if (timer.Interval.TotalSeconds == 0)
            {
                //timer.Stop();
                timer.Interval = TimeSpan.FromSeconds(interval);
                //timer.Start();
            }

            var sourceCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(interval-0.2));
            //if(rotation !=0)
            //    source.Cancel();
            //source.Token.WaitHandle
            //source = new CancellationTokenSource();

            var tasks = new List<Task>
            {
                // should be moved outside this tick, maybe should run in Start() and end in Stop()
                //Task.Run(() => UpdateForegroundWindow().WaitOrCancel(sourceCancellationToken.Token)),
                Task.Run(() => TaskKeys().WaitOrCancel(sourceCancellationToken.Token)),
                Task.Run(() => TaskDefault().WaitOrCancel(sourceCancellationToken.Token))
            };
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
            rotation++;
        }
        private async Task TaskKeys()
        {
            //TODO should consider waste time before window got focus
            foreach (var key in pressKeys)
            {
                if (!timer.IsEnabled)
                    return;
                await Task.Delay(key.Time);
                if (currentWindow == processId)
                {
                    queue.Enqueue(key);
                }
            }
        }
        int currentWindow;
        private async void UpdateForegroundWindow()
        {
            //while (timer.IsEnabled)
            while (!checkWindowToken.IsCancellationRequested)
            {
                currentWindow = User.GetForegroundWindow();
                await Task.Delay(100);
            }
        }

        private async Task TaskDefault()
        {
            while (timer.IsEnabled)
            {
                if (!enableDefaultKey || currentWindow != processId)
                {
                    await Task.Delay(100);
                    continue;
                }
                queue.Enqueue(deafultKeyPress);
                await Task.Delay(settings.DefaultDelayTime);
            }
        }



        private void processlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selItem = ((ListBox)sender).SelectedItem;
            if(selItem == null)
            {
                processId =0;
                return;
            }
            processId = (((ListBox)sender).SelectedItem as MyProcess).ProcessId;
        }

        private void Rename_Click(object sender, RoutedEventArgs e){
            if(string.IsNullOrEmpty(TxbRename.Text) || processId == 0)
                return;
            if (RenameClass.Names.ContainsKey(processId))
            {
                RenameClass.Names[processId] = TxbRename.Text;
            }
            else
            {
                RenameClass.Names.Add(processId,TxbRename.Text);
            }
            TxbRename.Text = String.Empty;
            FindProcess();
        }
        private void Show_Click(object sender, RoutedEventArgs e)
        {
            User.SetForegroundWindow(processId);
        }
        private void End_Process_Click(object sender, RoutedEventArgs e)
        {
            var selectedProcess= (int)(processlist.SelectedItem as MyProcess).WindowsProcessId;
            var process = Process.GetProcessById(selectedProcess);
            if (process == null)
                return;
            process.Kill();
            FindProcess();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e) => Stop();

        private void Stop()
        {
            checkWindowToken.Cancel();
            timer.Stop();
            dequeueTimer.Stop();
            queue.Clear();

            btnFind.IsEnabled = true;
            processlist.IsEnabled = true;
            txbInterval.IsEnabled = true;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            btnLoad.IsEnabled = true;
            btnSave.IsEnabled = true;
            rectStatus.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }

        private void Start()
        {
            checkWindowToken = new CancellationTokenSource();
            settings.Interval = interval;

            rotation = 0;

            timer.Interval = TimeSpan.FromSeconds(0);
            
            timer.Start();
            UpdateForegroundWindow();
            
            dequeueTimer.Start();

            btnFind.IsEnabled = false;
            processlist.IsEnabled = false;
            txbInterval.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            btnLoad.IsEnabled = false;
            btnSave.IsEnabled = false;
            rectStatus.Fill = new SolidColorBrush(Color.FromRgb(124, 252, 0));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (processId == 0)
                return;
            var result = double.TryParse(txbInterval.Text, out interval);
            if (!result) return;

            Start();
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

            var x = HandleCustom.GetOpenWindowsHndlId();
            for (int window = User.GetWindow(User.GetDesktopWindow(), 5); window != 0; window = User.GetWindow(window, 2))
            {
                if (window == hndl().ToInt32())
                {
                    // skip current window
                    window = User.GetWindow(window, 2);
                }
                if (User.IsWindowVisible(window) != 0)
                {
                    StringBuilder _stringBuilder = new StringBuilder(50);
                    User.GetWindowText(window, _stringBuilder, _stringBuilder.Capacity);
                    string text = _stringBuilder.ToString();
                    if (text.Length > 0)
                    {
                        if (x.ContainsKey(window))
                        {
                            processlist.Items.Add(new MyProcess(text, window, x[window]));
                        }
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
                File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        private void ResetConfig()
        {
            txbInterval.Text = settings.Interval.ToString();

            enableDefaultKey = !string.IsNullOrEmpty(settings.DefaultKeyPress);

            deafultKeyPress = new LetterTime(
                (Button.BT7)Enum.Parse(typeof(Button.BT7), $"KEY_{settings.DefaultKeyPress.ToUpper()}"),
                settings.DefaultKeyModifiers.Select(x => (Button.BT6)Enum.Parse(typeof(Button.BT6), x.ToUpper())).ToArray());
            var tmpList = new List<LetterTime>();
            foreach (var key in settings.PressKeys)
            {
                var letter = (Button.BT7)Enum.Parse(typeof(Button.BT7), $"KEY_{key.Key.ToUpper()}");
                var modifiers = key.Modifiers.Select(x => (Button.BT6)Enum.Parse(typeof(Button.BT6), x.ToUpper())).ToArray();
                tmpList.AddRange(key.Values.Select(time => new LetterTime
                (
                    letter,
                    time,
                    modifiers
                )));
            }
            pressKeys = tmpList.OrderBy(x => x.Time).ToArray();
            var old = 0;
            for (int x = 0; x < pressKeys.Length; x++)
            {
                if (pressKeys[x].Time == old)
                {
                    pressKeys[x].Time = 0;
                    continue;
                }
                var t = pressKeys[x].Time;
                pressKeys[x].Time -= old;
                old = t;
            }
        }
    }
}

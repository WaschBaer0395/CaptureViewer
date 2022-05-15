using System.Collections.Generic;
using System.Windows;
using MahApps.Metro.Controls;
using System;
using System.Windows.Threading;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Windows.Media.Animation;
using System.Threading;
using System.Drawing;
using Video;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace CaptureViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private double last_Height = 0;
        private double last_Width = 0;
        private double last_x_pos = 0;
        private double last_y_pos = 0;
        private System.Windows.Point _lastMove;
        private DispatcherTimer _timer = new DispatcherTimer();
        private bool _timer_started = false;
        //private AudioInputToolbox atr = new AudioInputToolbox();
        private VideoInputHandler video = new VideoInputHandler();

        public MainWindow()
        {
            InitializeComponent();
            var CaptureDevicesNames = new List<string> { };
            _timer.Tick += new EventHandler(Timer_Tick);
            _timer.Interval = new TimeSpan(0, 0, 3);

            // Getting the Default Audiodevice
            //var enumerator = new MMDeviceEnumerator();
            //var default_audoio_device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);


            // Creating the Device lists for Capturedevices, and AudioIn Devices
            Device_List.ItemsSource = Refresh_V_Device_List();
            A_Device_List.ItemsSource = Refresh_A_Device_List();
        }

        private void Settings_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Settings_Button.Foreground = System.Windows.Media.Brushes.White;
        }

        private void Settings_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Settings_Button.Foreground = System.Windows.Media.Brushes.DarkGray;
        }

        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            Settings_Flyout.IsOpen = true;
        }


        private void Fullscreen_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Fullscreen_Button.Foreground = System.Windows.Media.Brushes.White;
        }

        private void Fullscreen_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Fullscreen_Button.Foreground = System.Windows.Media.Brushes.DarkGray;
        }

        private void Fullscreen_Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowStyle != WindowStyle.SingleBorderWindow)
            {
                this.UseNoneWindowStyle = false;
                this.IgnoreTaskbarOnMaximize = false;
                this.IsWindowDraggable = true;
                this.ShowTitleBar = true;
                this.WindowState = WindowState.Normal;
                this.ResizeMode = ResizeMode.CanResize;
                this.WindowStyle = WindowStyle.SingleBorderWindow;

                //restoring original position and dimensions
                this.Height = last_Height;
                this.Width = last_Width;
                this.Left = last_x_pos;
                this.Top = last_y_pos;
                this.Topmost = false;

                //changing the button back to the maximize button
                var curr_cont = (MahApps.Metro.IconPacks.PackIconFeatherIcons)Fullscreen_Button.Content;
                curr_cont.Kind = MahApps.Metro.IconPacks.PackIconFeatherIconsKind.Maximize2;
                Fullscreen_Button.Content = curr_cont;
                Fullscreen_Button.BorderThickness = new Thickness(0);
            }
            else
            {
                // MetroWindow Specific
                this.UseNoneWindowStyle = true;
                this.IgnoreTaskbarOnMaximize = true;

                //saving the original dimensions, and position, to restore after minimizing again
                last_Height = this.Height;
                last_Width = this.Width;
                last_x_pos = this.Left;
                last_y_pos = this.Top;

                //actual maximizing of the window into exclusive fullscreen
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
                this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                this.Left = 0;
                this.Top = 0;

                //changing the button to display a minimize button
                var curr_cont = (MahApps.Metro.IconPacks.PackIconFeatherIcons)Fullscreen_Button.Content;
                curr_cont.Kind = MahApps.Metro.IconPacks.PackIconFeatherIconsKind.Minimize2;
                Fullscreen_Button.Content = curr_cont;
                Fullscreen_Button.BorderThickness = new Thickness(0);
            }
        }

        private void MetroWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var p = e.GetPosition((IInputElement)sender);
            if (_lastMove != p)
            {
                if (_timer_started == false && Ui_Controls.Visibility == Visibility.Collapsed)
                {
                    // begin animation when timer has not been started yet
                    this.ShowControls();
                    _timer_started = true;
                }
                // restart the timer whenever the user moves the cursor
                _timer.Start();
                // save the new mouse position
                _lastMove = p;
            }
        }

        private void HideControls()
        {
            _timer_started = false;
            Storyboard sb = (Storyboard)this.FindResource("FadeOut");
            sb.Begin(Ui_Controls);
        }

        private void ShowControls()
        {
            _timer_started = true;
            Storyboard sb = (Storyboard)this.FindResource("FadeIn");
            sb.Begin(Ui_Controls);
        }

        private void Timer_Tick(object sender, object e)
        {
            if (Ui_Controls.Visibility == Visibility.Visible)
            {
                _timer.Stop();
                this.HideControls();
            }
        }

        void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Image img = (Bitmap)eventArgs.Frame.Clone();

                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    frameHolder.Source = video.CreateBitmap(img);
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Device_List_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (video.GetDevice() != null)
            {
                video.Stop();
            }

            video.RefreshDevice((int)Device_List.SelectedIndex);
            video.GetDevice().NewFrame += new NewFrameEventHandler(Cam_NewFrame);
            Display_More_Settings();
            this.No_Device.Visibility = Visibility.Hidden;

            int index = 0;
            foreach (string value in this.A_Device_List.ItemsSource)
            {
                if (value.Contains(this.Device_List.SelectedItem.ToString())) 
                {
                    //System.Console.WriteLine(value);
                    this.A_Device_List.SelectedIndex = index;
                }
                index++;
            }
        }

        private void Display_More_Settings()
        {
            List<String> _res = new List<String>();
            foreach (VideoCapabilities v in video.VideoCapabilities)
            {
                _res.Add(v.FrameSize.Height + "p " + v.FrameRate + "fps");
            }

            if (_res.Count <= 0)
            {
                this.Video_Settings.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.Video_Settings.Visibility = Visibility.Visible;
                this.Device_Resolutions.ItemsSource = _res;
                this.Device_Resolutions.SelectedIndex = 0;
            }
            this.Audio_Settings.Visibility = Visibility.Visible;
        }

        private void Device_Resolutions_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Device_Resolutions.SelectedIndex >= 0)
            {
                video.SelCapabilities = video.VideoCapabilities[Device_Resolutions.SelectedIndex];
            }
            try
            {
                video.Stop();
                if (video.SelCapabilities != null)
                {
                    video.FrameRate = video.SelCapabilities.FrameRate;
                    video.FrameSize = video.SelCapabilities.FrameSize;
                }

                video.Start();
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine(ae.Message);
            }
        }

        private void Refresh_Devices(object sender, RoutedEventArgs e)
        {
            Device_List.ItemsSource = Refresh_V_Device_List();
            A_Device_List.ItemsSource = Refresh_A_Device_List();
        }

        private List<string> Refresh_V_Device_List()
        {
            // Getting a list of all found Capturedevices
            FilterInfoCollection _FIC;
            _FIC = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            var vlist = new List<string> { };
            foreach (FilterInfo filterinfo in _FIC)
            {
                vlist.Add(filterinfo.Name);
            }
            return vlist;
        }

        private List<string> Refresh_A_Device_List()
        {
            var alist = new List<string> { };
            //create enumerator
            var enumerator = new MMDeviceEnumerator();
            //cycle through all audio devices
            for (int i = 0; i < WaveIn.DeviceCount; i++) 
            {
                Console.WriteLine(enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)[i]);
                alist.Add(enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)[i].FriendlyName);
            }

            //clean up
            enumerator.Dispose();

            return alist;
        }

        private void Save_Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            Settings_Flyout.IsOpen = false;
        }

        private void Save_Settings_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Save_Settings_Button_Icon.Foreground = System.Windows.Media.Brushes.DarkGray;
            Save_Settings_Button_Icon.Background = System.Windows.Media.Brushes.Transparent;
            Save_Settings_Button.Background = System.Windows.Media.Brushes.Transparent;
            Save_Settings_Button.Foreground = System.Windows.Media.Brushes.Transparent;

        }

        private void Save_Settings_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Save_Settings_Button_Icon.Foreground = System.Windows.Media.Brushes.White;
            Save_Settings_Button_Icon.Background = System.Windows.Media.Brushes.Transparent;
            Save_Settings_Button.Background = System.Windows.Media.Brushes.Transparent;
            Save_Settings_Button.Foreground = System.Windows.Media.Brushes.Transparent;
        }

        private void A_Device_List_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // A_Device_List.SelectedIndex
            // selected audio index play stuff here 
        }
    }
}

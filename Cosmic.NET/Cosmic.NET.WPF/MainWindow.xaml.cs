using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using ReactiveUI;

namespace Cosmic.NET.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IViewFor<MainWindowVM>
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.HNoughtText, v => v.HNought.Text));
                d(this.Bind(ViewModel, vm => vm.OmegaMatterText, v => v.OmegaMatter.Text));
                d(this.Bind(ViewModel, vm => vm.OmegaLambdaText, v => v.OmegaLambda.Text));
                d(this.Bind(ViewModel, vm => vm.RedshiftText, v => v.Redshift.Text));
                d(this.OneWayBind(ViewModel, vm => vm.CosmoText, v => v.SingleSourceOutput.Text));
                d(this.Bind(ViewModel, vm => vm.BatchFile.FullName, v => v.InputFilename.Text));
                d(this.Bind(ViewModel, vm => vm.BatchFile.FullName, v => v.InputFilename.ToolTip));
                d(this.Bind(ViewModel, vm => vm.OutputFile.FullName, v => v.OutputFilename.Text));
                d(this.Bind(ViewModel, vm => vm.OutputFile.FullName, v => v.OutputFilename.ToolTip));
                d(this.BindCommand(ViewModel, vm => vm.GetInputFile, v => v.BrowseForInputFile));
                d(this.BindCommand(ViewModel, vm => vm.GetOutputFile, v => v.BrowseForOutputFile));
                d(this.BindCommand(ViewModel, vm => vm.ComputeBatch, v => v.ComputeBatch));
                d(this.OneWayBind(ViewModel, vm => vm.SaveNotificationText, v => v.SavedNotification.Text));
                d(this.WhenAnyValue(x => x.ViewModel.SaveNotificationText)
                      .Subscribe(
                          text =>
                          {
                              if (string.IsNullOrEmpty(text)) return;
                              var sb = new Storyboard();
                              Storyboard.SetTargetProperty(_fadeOutAnimation, new PropertyPath(OpacityProperty));
                              sb.Children.Add(_fadeOutAnimation);
                              sb.Begin(SavedNotification);
                          }));
                d(ViewModel
                    .ParseError
                    .RegisterHandler(
                          interaction =>
                          {
                              MessageBox.Show(
                                            this,
                                            interaction.Input,
                                            "Invalid Input",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Warning);
                              interaction.SetOutput(Unit.Default);
                          }));
                d(ViewModel
                    .GetFileToOpen
                    .RegisterHandler(
                          interaction =>
                          {
                              var dlg = new OpenFileDialog { Filter = "Text file|*.txt" };
                              var gotAFile = dlg.ShowDialog();
                              interaction.SetOutput(gotAFile == true ? new FileInfo(dlg.FileName) : null);
                          }));
                d(ViewModel
                      .GetFileToSave
                      .RegisterHandler(
                          interaction =>
                          {
                              var dlg = new SaveFileDialog
                              {
                                  Filter = @"Text document (.txt)|*.txt|CSV document (.csv)|*.csv"
                              };
                              var gotAFile = dlg.ShowDialog();
                              interaction.SetOutput(new Tuple<FileInfo, MainWindowVM.FileType>(
                                                            gotAFile == true ? new FileInfo(dlg.FileName) : null,
                                                            dlg.FilterIndex == 1 ? MainWindowVM.FileType.Txt : MainWindowVM.FileType.Csv)); // FilterIndex is 1-based
                          }));
            });

            Deactivated += OnDeactivated;
            Activated += OnActivated;
            SourceInitialized += CoreHostView_SourceInitialized;
        }

        private static DoubleAnimationUsingKeyFrames _fadeOutAnimation = new DoubleAnimationUsingKeyFrames
        {
            KeyFrames = new DoubleKeyFrameCollection
            {
                new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))),
                new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))),
                new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))),
                new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3)))
            }
        };
        
        #region IViewFor

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(MainWindowVM),
            typeof(MainWindow),
            new PropertyMetadata(default(MainWindowVM)));

        public MainWindowVM ViewModel
        {
            get { return (MainWindowVM)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (MainWindowVM)value; }
        }

        #endregion

        private readonly List<IDisposable> _rolloverEffects = new List<IDisposable>();
        private void OnActivated(object sender, EventArgs eventArgs)
        {
            WindowBorder.BorderBrush = SystemColors.HighlightBrush;
            TitleText.Opacity = 1.0;
            foreach (var effect in _rolloverEffects)
                effect.Dispose();
            _rolloverEffects.Clear();
            MinimizeWindowImage.Opacity = 1.0;
            CloseWindowImage.Opacity = 1.0;
        }

        private void OnDeactivated(object sender, EventArgs eventArgs)
        {
            WindowBorder.BorderBrush = SystemColors.ActiveBorderBrush;
            TitleText.Opacity = 0.6;
            _rolloverEffects.Add(
                this.WhenAnyValue(x => x.MinimizeWindow.IsMouseOver)
                    .Subscribe(b => MinimizeWindowImage.Opacity = b ? 1.0 : DisabledOpacity));
            _rolloverEffects.Add(
                this.WhenAnyValue(x => x.CloseWindow.IsMouseOver)
                    .Subscribe(b => CloseWindowImage.Opacity = b ? 1.0 : DisabledOpacity));
        }

        private const double DisabledOpacity = 0.2;

        #region Custom Window Management

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (WindowState == WindowState.Maximized)
            {
                var startPos = Mouse.GetPosition(this);
                var mouseOnScreen = PointToScreen(startPos);
                var frac = startPos.X / Width;
                WindowState = WindowState.Normal;
                Top = 0;
                Left = mouseOnScreen.X - Math.Min(1 - WindowButtons.ActualWidth / Width, frac) * Width;
            }
            DragMove();
        }

        private void CloseWindow_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeWindow_OnClick(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        ///////////////////////////////////////////////////

        private static IntPtr WindowProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:/* WM_GETMINMAXINFO */
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = (MinMaxInfo)Marshal.PtrToStructure(lParam, typeof(MinMaxInfo));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            const int monitorDefaultToNearest = 0x00000002;
            var monitor = MonitorFromWindow(hwnd, monitorDefaultToNearest);

            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MonitorInfo();
                GetMonitorInfo(monitor, monitorInfo);
                var rcWorkArea = monitorInfo.rcWork;
                var rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        private void CoreHostView_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            var hwndSource = HwndSource.FromHwnd(handle);
            hwndSource?.AddHook(WindowProc);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;
            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MinMaxInfo
        {
            public Point ptReserved;
            public Point ptMaxSize;
            public Point ptMaxPosition;
            public Point ptMinTrackSize;
            public Point ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MonitorInfo
        {
            /// <summary>
            /// </summary>            
            public int cbSize = Marshal.SizeOf(typeof(MonitorInfo));

            /// <summary>
            /// </summary>            
            public Rect rcMonitor = new Rect();

            /// <summary>
            /// </summary>            
            public Rect rcWork = new Rect();

            /// <summary>
            /// </summary>            
            public int dwFlags = 0;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Rect
        {
            public readonly int left;
            public readonly int top;
            public readonly int right;
            public readonly int bottom;

            private static readonly Rect Empty;

            public int Width => Math.Abs(right - left);

            public int Height => bottom - top;

            public Rect(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


            public Rect(Rect rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }

            public bool IsEmpty => left >= right || top >= bottom;

            /// <summary> Return a user friendly representation of this struct </summary>
            public override string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) return false;
                return this == (Rect)obj;
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }

            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(Rect rect1, Rect rect2)
            {
                return rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom;
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(Rect rect1, Rect rect2)
            {
                return !(rect1 == rect2);
            }
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MonitorInfo lpmi);

        /// <summary>
        /// 
        /// </summary>
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        #endregion
    }
}

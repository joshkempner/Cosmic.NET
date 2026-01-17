using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Cosmic.NET.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[IViewFor<MainWindowVM>]
public partial class MainWindow {
    public MainWindow() {
        InitializeComponent();

        this.WhenActivated(d => {
            this.Bind(ViewModel, vm => vm.HNoughtText, v => v.HNought.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.OmegaMatterText, v => v.OmegaMatter.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.OmegaLambdaText, v => v.OmegaLambda.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.RedshiftText, v => v.Redshift.Text).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CosmoText, v => v.SingleSourceOutput.Text).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.CopyOutputCommand, v => v.CopyOutputToClipboard).DisposeWith(d);
#pragma warning disable CS8602 // Dereference of a possibly null reference. - RxUI allows null values in property expressions
            this.Bind(ViewModel, vm => vm.BatchFile.FullName, v => v.InputFilename.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.BatchFile.FullName, v => v.InputFilename.ToolTip).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.BatchFile.FullName, v => v.InputDescription.Visibility,
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                s => string.IsNullOrWhiteSpace(s) ? Visibility.Visible : Visibility.Hidden).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.GetInputFileCommand, v => v.BrowseForInputFile).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.ComputeAndSaveCommand, v => v.ComputeAndSave).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.SaveNotificationText, v => v.SavedNotification.Text).DisposeWith(d);
            this.WhenAnyValue(x => x.ViewModel.SaveNotificationText)
                .Where(text => !string.IsNullOrEmpty(text))
                .Subscribe(
                    _ => {
                        var sb = new Storyboard();
                        Storyboard.SetTargetProperty(FadeOutAnimation, new PropertyPath(OpacityProperty));
                        sb.Children.Add(FadeOutAnimation);
                        sb.Begin(SavedNotification);
                    }); // this.WhenAnyValue is disposed automatically
            ViewModel
                .ParseError
                .RegisterHandler(
                    interaction => {
                        MessageBox.Show(
                            this,
                            interaction.Input,
                            "Invalid Input",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        interaction.SetOutput(Unit.Default);
                    }).DisposeWith(d);
            ViewModel
                .GetFileToOpen
                .RegisterHandler(
                    interaction => {
                        var dlg = new OpenFileDialog { Filter = "Text file|*.txt" };
                        var gotAFile = dlg.ShowDialog();
                        interaction.SetOutput(gotAFile == true ? new FileInfo(dlg.FileName) : null);
                    }).DisposeWith(d);
            ViewModel
                .GetFileToSave
                .RegisterHandler(
                    interaction => {
                        var dlg = new SaveFileDialog {
                            Filter = "Text document (.txt)|*.txt|CSV document (.csv)|*.csv"
                        };
                        var gotAFile = dlg.ShowDialog();
                        interaction.SetOutput(
                            (gotAFile == true ? new FileInfo(dlg.FileName) : null,
                                dlg.FilterIndex == 1
                                    ? MainWindowVM.FileType.Txt
                                    : MainWindowVM.FileType.Csv)); // FilterIndex is 1-based
                    }).DisposeWith(d);
        });

        Deactivated += OnDeactivated;
        Activated += OnActivated;
        SourceInitialized += CoreHostView_SourceInitialized;

        Version.Text = $"v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}";
    }

    private static readonly DoubleAnimationUsingKeyFrames FadeOutAnimation = new() {
        KeyFrames = [
            new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))),
            new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))),
            new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))),
            new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3)))
        ]
    };

    private readonly List<IDisposable> _rolloverEffects = [];
    private void OnActivated(object? sender, EventArgs eventArgs) {
        WindowBorder.BorderBrush = SystemColors.HighlightBrush;
        TitleText.Opacity = 1.0;
        foreach (var effect in _rolloverEffects)
            effect.Dispose();
        _rolloverEffects.Clear();
        MinimizeWindowImage.Opacity = 1.0;
        CloseWindowImage.Opacity = 1.0;
    }

    private void OnDeactivated(object? sender, EventArgs eventArgs) {
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

    private void TitleBar_MouseMove(object sender, MouseEventArgs e) {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        if (WindowState == WindowState.Maximized) {
            var startPos = Mouse.GetPosition(this);
            var mouseOnScreen = PointToScreen(startPos);
            var frac = startPos.X / Width;
            WindowState = WindowState.Normal;
            Top = 0;
            Left = mouseOnScreen.X - Math.Min(1 - WindowButtons.ActualWidth / Width, frac) * Width;
        }
        DragMove();
    }

    private void CloseWindow_OnClick(object sender, RoutedEventArgs e) {
        Close();
    }

    private void MinimizeWindow_OnClick(object sender, RoutedEventArgs e) {
        SystemCommands.MinimizeWindow(this);
    }

    ///////////////////////////////////////////////////

    private static IntPtr WindowProc(
        IntPtr hwnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled) {
        switch (msg) {
            case 0x0024:/* WM_GETMINMAXINFO */
                WmGetMinMaxInfo(hwnd, lParam);
                handled = true;
                break;
        }

        return (IntPtr)0;
    }

    private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
        var mmi = Marshal.PtrToStructure<MinMaxInfo>(lParam);

        // Adjust the maximized size and position to fit the work area of the correct monitor
        const int monitorDefaultToNearest = 0x00000002;
        var monitor = MonitorFromWindow(hwnd, monitorDefaultToNearest);

        if (monitor != IntPtr.Zero) {
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

    private void CoreHostView_SourceInitialized(object? sender, EventArgs e) {
        var handle = new WindowInteropHelper(this).Handle;
        var hwndSource = HwndSource.FromHwnd(handle);
        hwndSource?.AddHook(WindowProc);
    }

    /// <summary>
    /// Construct a point of coordinates (x,y).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Point(int x, int y) {
        /// <summary>
        /// x coordinate of point.
        /// </summary>
        public int x = x;
        /// <summary>
        /// y coordinate of point.
        /// </summary>
        public int y = y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MinMaxInfo {
        public Point ptReserved;
        public Point ptMaxSize;
        public Point ptMaxPosition;
        public Point ptMinTrackSize;
        public Point ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MonitorInfo {
        /// <summary>
        /// </summary>            
        public int cbSize = Marshal.SizeOf<MonitorInfo>();

        /// <summary>
        /// </summary>            
        public Rect rcMonitor = new();

        /// <summary>
        /// </summary>            
        public Rect rcWork = new();

        /// <summary>
        /// </summary>            
        public int dwFlags = 0;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public readonly struct Rect(int left, int top, int right, int bottom) {
        public readonly int left = left;
        public readonly int top = top;
        public readonly int right = right;
        public readonly int bottom = bottom;

        private static readonly Rect Empty;

        public int Width => Math.Abs(right - left);

        public int Height => bottom - top;


        public Rect(Rect rcSrc) : this(rcSrc.left, rcSrc.top, rcSrc.right, rcSrc.bottom) { }

        public bool IsEmpty => left >= right || top >= bottom;

        /// <summary> Return a user friendly representation of this struct </summary>
        public override string ToString() {
            if (this == Empty) { return "RECT {Empty}"; }
            return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
        }

        /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
        public override bool Equals(object obj) {
            if (obj is not Rect rect) return false;
            return this == rect;
        }

        /// <summary>Return the HashCode for this struct (not guaranteed to be unique)</summary>
        public override int GetHashCode() {
            return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
        }

        /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
        public static bool operator ==(Rect rect1, Rect rect2) {
            return rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom;
        }

        /// <summary> Determine if 2 RECT are different(deep compare)</summary>
        public static bool operator !=(Rect rect1, Rect rect2) {
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
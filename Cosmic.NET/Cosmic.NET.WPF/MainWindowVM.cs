using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Cosmo;
using ReactiveUI;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace Cosmic.NET.WPF
{
    public class MainWindowVM : ReactiveObject
    {
        private readonly TimeSpan ThrottleDelay = TimeSpan.FromMilliseconds(500);

        private const string HNoughtPos = "H-nought must be positive.";
        private const string HNoughtNum = "H-nought must be a number.";
        private const string OmegaMatterPos = "Omega-matter must be greater than or equal to zero.";
        private const string OmegaMatterNum = "Omega-matter must be a number.";
        private const string OmegaLambdaNum = "Omega-Lambda must be a number.";
        private const string ZNotNeg = "The redshift must be at least 0.";
        private const string ZNum = "The redshift must be a number.";

        private readonly List<string> _activeErrors = new();

        public enum FileType
        {
            Txt,
            Csv
        }

        public Interaction<string, Unit> ParseError { get; } = new();
        public Interaction<string, FileInfo> GetFileToOpen { get; } = new();
        public Interaction<string, Tuple<FileInfo, FileType>> GetFileToSave { get; } = new();

        /// <summary>
        /// Copy the single-redshift output to the clipboard.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CopyOutputToClipboard { get; }
        /// <summary>
        /// Prompt the user for the input file for batch processing.
        /// </summary>
        public ReactiveCommand<Unit, Unit> GetInputFile { get; }
        /// <summary>
        /// Prompt the user for an output file, then compute the set of redshifts in the provided batch file and write them to the output file.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ComputeAndSave { get; }

        public MainWindowVM()
        {
            CopyOutputToClipboard = ReactiveCommand.CreateFromTask(
                                                CopyOutput,
                                                this.WhenAnyValue(
                                                        x => x.CosmoText,
                                                        t => !string.IsNullOrEmpty(t)));
            GetInputFile = ReactiveCommand.CreateFromTask(GetTheInputFile);
            ComputeAndSave = ReactiveCommand.CreateFromTask(
                                                RunBatch,
                                                this.WhenAnyValue(
                                                        x => x.BatchFile,
                                                        input => input?.Exists ?? false));

            HNought = 71;
            OmegaMatter = 0.27;
            OmegaLambda = 0.73;
            Redshift = 0.1;
            HNoughtText = HNought.ToString(CultureInfo.InvariantCulture);
            OmegaMatterText = OmegaMatter.ToString(CultureInfo.InvariantCulture);
            OmegaLambdaText = OmegaLambda.ToString(CultureInfo.InvariantCulture);
            RedshiftText = Redshift.ToString(CultureInfo.InvariantCulture);

            this.WhenAnyValue(x => x.HNoughtText)
                .Throttle(ThrottleDelay)
                .DistinctUntilChanged()
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Subscribe(h =>
                {
                    if (double.TryParse(h, out var temp))
                    {
                        if (temp <= 0)
                            ShowError(HNoughtPos);
                        else
                        {
                            HNought = temp;
                            ClearError(HNoughtPos);
                            ClearError(HNoughtNum);
                        }
                    }
                    else
                        ShowError(HNoughtNum);
                });


            this.WhenAnyValue(x => x.OmegaMatterText)
                .Throttle(ThrottleDelay)
                .DistinctUntilChanged()
                .Where(om => !string.IsNullOrWhiteSpace(om))
                .Subscribe(om =>
                {
                    if (double.TryParse(om, out var temp))
                    {
                        if (temp < 0)
                            ShowError(OmegaMatterPos);
                        else
                        {
                            OmegaMatter = temp;
                            ClearError(OmegaMatterPos);
                            ClearError(OmegaMatterNum);
                        }
                    }
                    else
                        ShowError(OmegaMatterNum);
                });

            this.WhenAnyValue(x => x.OmegaLambdaText)
                .Throttle(ThrottleDelay)
                .DistinctUntilChanged()
                .Where(ol => !string.IsNullOrWhiteSpace(ol))
                .Subscribe(ol =>
                {
                    if (double.TryParse(ol, out var temp))
                    {
                        OmegaLambda = temp;
                        ClearError(OmegaLambdaNum);
                    }
                    else
                        ShowError(OmegaLambdaNum);
                });

            this.WhenAnyValue(x => x.RedshiftText)
                .Throttle(ThrottleDelay)
                .DistinctUntilChanged()
                .Where(z => !string.IsNullOrWhiteSpace(z))
                .Subscribe(z =>
                {
                    if (double.TryParse(z, out var temp))
                    {
                        if (temp < 0)
                            ShowError(ZNotNeg);
                        else
                        {
                            Redshift = temp;
                            ClearError(ZNotNeg);
                            ClearError(ZNum);
                        }
                    }
                    else
                        ShowError(ZNum);
                });

            this.WhenAnyValue(
                    x => x.HNought,
                    x => x.OmegaMatter,
                    x => x.OmegaLambda,
                    x => x.Redshift,
                    (h, om, ol, z) =>
                    {
                        try
                        {
                            var cosmo = new Cosmology(h, om, ol) { Redshift = z };
                            return cosmo.ToString();
                        }
                        catch (Exception)
                        {
                            return string.Empty;
                        }
                    })
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.CosmoText, out _cosmoText);
        }

        private async Task CopyOutput()
        {
            await Task.Run(() => Application.Current.Dispatcher.Invoke(() => Clipboard.SetText(CosmoText)));
        }

        private async Task GetTheInputFile()
        {
            var file = await GetFileToOpen.Handle(string.Empty);
            if (file != null)
                BatchFile = file;
        }

        private async Task RunBatch()
        {
            await Task.Run(async () =>
            {
                var (outputFile, fileType) = await GetFileToSave.Handle("cosmic.txt");
                if (outputFile == null) return;
                OutputFile = outputFile;
                _fileType = fileType;

                RunOnUiThread(() => SaveNotificationText = string.Empty);
                // read entire input file before doing any calculations
                var inputLines = new List<double>();
                try
                {
                    using var sr = new StreamReader(BatchFile.FullName);
                    string line;
                    while ((line = await sr.ReadLineAsync()) != null)
                        inputLines.Add(double.Parse(line));
                }
                catch (Exception)
                {
                    RunOnUiThread(async () => await ParseError.Handle("An error occurred reading the input file. Check the file to make sure it has one redshift per line and nothing else."));
                    return;
                }

                // process the input redshifts one at a time, writing each to the output file as we go
                try
                {
                    var cosmology = new Cosmology(HNought, OmegaMatter, OmegaLambda);
                    var separator = _fileType == FileType.Txt ? "\t" : ",";
                    await using var sw = new StreamWriter(OutputFile.FullName);
                    await sw.WriteLineAsync(cosmology.GetShortFormHeader("# ", separator));
                    foreach (var redshift in inputLines)
                    {
                        cosmology.Redshift = redshift;
                        await sw.WriteLineAsync(cosmology.GetShortFormOutput(separator));
                    }
                }
                catch (Exception)
                {
                    RunOnUiThread(async () => await ParseError.Handle("An error occurred writing the output file."));
                    return;
                }
                RunOnUiThread(() => SaveNotificationText = $"Results saved to {OutputFile.Name}");
            });
        }

        /// <summary>
        /// Gets or sets the current text shown in the UI for the Hubble constant.
        /// </summary>
        public string HNoughtText
        {
            get => _hNoughtText;
            set => this.RaiseAndSetIfChanged(ref _hNoughtText, value);
        }
        private string _hNoughtText;

        /// <summary>
        /// Gets or sets the current value of the Hubble constant.
        /// </summary>
        private double HNought
        {
            get => _hNought;
            set => this.RaiseAndSetIfChanged(ref _hNought, value);
        }
        private double _hNought;

        /// <summary>
        /// Gets or sets the current text shown in the UI for the density attributed to matter.
        /// </summary>
        public string OmegaMatterText
        {
            get => _omegaMatterText;
            set => this.RaiseAndSetIfChanged(ref _omegaMatterText, value);
        }
        private string _omegaMatterText;

        /// <summary>
        /// Gets or sets the current value of the density attributed to matter.
        /// </summary>
        private double OmegaMatter
        {
            get => _omegaMatter;
            set => this.RaiseAndSetIfChanged(ref _omegaMatter, value);
        }
        private double _omegaMatter;

        /// <summary>
        /// Gets or sets the current text shown in the UI for the density attributed to the cosmological constant.
        /// </summary>
        public string OmegaLambdaText
        {
            get => _omegaLambdaText;
            set => this.RaiseAndSetIfChanged(ref _omegaLambdaText, value);
        }
        private string _omegaLambdaText;

        /// <summary>
        /// Gets or sets the current value of the density attributed to the cosmological constant.
        /// </summary>
        private double OmegaLambda
        {
            get => _omegaLambda;
            set => this.RaiseAndSetIfChanged(ref _omegaLambda, value);
        }
        private double _omegaLambda;

        /// <summary>
        /// Gets or sets the current text shown in the UI for the single redshift.
        /// </summary>
        public string RedshiftText
        {
            get => _redshiftText;
            set => this.RaiseAndSetIfChanged(ref _redshiftText, value);
        }
        private string _redshiftText;

        /// <summary>
        /// Gets or sets the current single redshift.
        /// </summary>
        private double Redshift
        {
            get => _redshift;
            set => this.RaiseAndSetIfChanged(ref _redshift, value);
        }
        private double _redshift;

        /// <summary>
        /// Gets the latest calculated output for the current cosmology and single redshift.
        /// </summary>
        public string CosmoText => _cosmoText.Value;
        private readonly ObservableAsPropertyHelper<string> _cosmoText = ObservableAsPropertyHelper<string>.Default();

        /// <summary>
        /// Gets or sets the full path to the input file for batch mode.
        /// </summary>
        public FileInfo BatchFile
        {
            get => _batchFile;
            private set => this.RaiseAndSetIfChanged(ref _batchFile, value);
        }
        private FileInfo _batchFile;

        /// <summary>
        /// Gets or sets the full path to the output file for batch mode.
        /// </summary>
        private FileInfo OutputFile
        {
            get => _outputFile;
            set => this.RaiseAndSetIfChanged(ref _outputFile, value);
        }
        private FileInfo _outputFile;

        private FileType _fileType;

        public string SaveNotificationText
        {
            get => _saveNotificationText;
            private set => this.RaiseAndSetIfChanged(ref _saveNotificationText, value);
        }
        private string _saveNotificationText;

        private void ClearError(string errorMsg)
        {
            _activeErrors.Remove(errorMsg);
        }

        private void ShowError(string errorMsg)
        {
            if (_activeErrors.Contains(errorMsg)) return;
            _activeErrors.Add(errorMsg);
            RunOnUiThread(async () => await ParseError.Handle(errorMsg));
        }

        private static void RunOnUiThread(Action action)
        {
            RunOnUiThread(_ => action());
        }

        private static void RunOnUiThread(Action<object> action, object param = null)
        {
            if (Application.Current?.Dispatcher.CheckAccess() ?? false)
            {
                action(param); // we're on the ui thread, just go for it
                return;
            }
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(action, param);
            else
                throw new InvalidOperationException("Unable to run on UI thread!");
        }

    }
}

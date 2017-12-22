using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Cosmo;
using ReactiveUI;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace Cosmic.NET.WPF
{
    public class MainWindowVM : ReactiveObject
    {
        private const double ThrottleDelay = 0.5;

        private const string HNoughtPos = "H-nought must be positive.";
        private const string HNoughtNum = "H-nought must be a number.";
        private const string OmegaMatterPos = "Omega-matter must be greater than or equal to zero.";
        private const string OmegaMatterNum = "Omega-matter must be a number.";
        private const string OmegaLambdaNum = "Omega-Lambda must be a number.";
        private const string ZNotNeg = "The redshift must be at least 0.";
        private const string ZNum = "The redshift must be a number.";

        private readonly List<string> _activeErrors = new List<string>();

        public enum FileType
        {
            Txt,
            Csv
        }

        public Interaction<string, Unit> ParseError { get; }
        public Interaction<string, FileInfo> GetFileToOpen { get; }
        public Interaction<string, Tuple<FileInfo, FileType>> GetFileToSave { get; }

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
            ParseError = new Interaction<string, Unit>();
            GetFileToOpen = new Interaction<string, FileInfo>();
            GetFileToSave = new Interaction<string, Tuple<FileInfo, FileType>>();

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

            this.WhenAnyValue(x => x.HNought)
                .Subscribe(h => HNoughtText = h.ToString(CultureInfo.InvariantCulture));

            this.WhenAnyValue(x => x.OmegaMatter)
                .Subscribe(om => OmegaMatterText = om.ToString(CultureInfo.InvariantCulture));

            this.WhenAnyValue(x => x.OmegaLambda)
                .Subscribe(ol => OmegaLambdaText = ol.ToString(CultureInfo.InvariantCulture));

            this.WhenAnyValue(x => x.Redshift)
                .Subscribe(z => RedshiftText = z.ToString(CultureInfo.InvariantCulture));

            this.WhenAnyValue(x => x.HNoughtText)
                .Throttle(TimeSpan.FromSeconds(ThrottleDelay))
                .Subscribe(h =>
                {
                    if (string.IsNullOrWhiteSpace(h)) return;
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
                .Throttle(TimeSpan.FromSeconds(ThrottleDelay))
                .Subscribe(om =>
                {
                    if (string.IsNullOrWhiteSpace(om)) return;
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
                .Throttle(TimeSpan.FromSeconds(ThrottleDelay))
                .Subscribe(ol =>
                {
                    if (string.IsNullOrWhiteSpace(ol)) return;
                    if (double.TryParse(ol, out var temp))
                    {
                        OmegaLambda = temp;
                        ClearError(OmegaLambdaNum);
                    }
                    else
                        ShowError(OmegaLambdaNum);
                });

            this.WhenAnyValue(x => x.RedshiftText)
                .Throttle(TimeSpan.FromSeconds(ThrottleDelay))
                .Subscribe(z =>
                {
                    if (string.IsNullOrWhiteSpace(z)) return;
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
                var fileAndFilter = await GetFileToSave.Handle("cosmic.txt");
                if (fileAndFilter.Item1 == null) return;
                OutputFile = fileAndFilter.Item1;
                _fileType = fileAndFilter.Item2;

                RunOnUiThread(() => SaveNotificationText = string.Empty);
                // read entire input file before doing any calculations
                var inputLines = new List<double>();
                try
                {
                    using (var sr = new StreamReader(BatchFile.FullName))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                            inputLines.Add(double.Parse(line));
                    }
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
                    using (var sw = new StreamWriter(OutputFile.FullName))
                    {
                        sw.WriteLine(cosmology.ShortFormHeader("# ", separator));
                        foreach (var redshift in inputLines)
                        {
                            cosmology.Redshift = redshift;
                            sw.WriteLine(cosmology.ShortFormOutput(separator));
                        }
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
            get { return _hNoughtText; }
            set { this.RaiseAndSetIfChanged(ref _hNoughtText, value); }
        }
        private string _hNoughtText;

        /// <summary>
        /// Gets or sets the current value of the Hubble constant.
        /// </summary>
        public double HNought
        {
            get { return _hNought; }
            set { this.RaiseAndSetIfChanged(ref _hNought, value); }
        }
        private double _hNought;

        /// <summary>
        /// Gets or sets the current text shown in the UI for the density attributed to matter.
        /// </summary>
        public string OmegaMatterText
        {
            get { return _omegaMatterText; }
            set { this.RaiseAndSetIfChanged(ref _omegaMatterText, value); }
        }
        private string _omegaMatterText;

        /// <summary>
        /// Gets or sets the current value of the density attributed to matter.
        /// </summary>
        public double OmegaMatter
        {
            get { return _omegaMatter; }
            set { this.RaiseAndSetIfChanged(ref _omegaMatter, value); }
        }
        private double _omegaMatter;

        /// <summary>
        /// Gets or sets the current text shown in the UI for the density attributed to the cosmological constant.
        /// </summary>
        public string OmegaLambdaText
        {
            get { return _omegaLambdaText; }
            set { this.RaiseAndSetIfChanged(ref _omegaLambdaText, value); }
        }
        private string _omegaLambdaText;

        /// <summary>
        /// Gets or sets the current value of the density attributed to the cosmological constant.
        /// </summary>
        public double OmegaLambda
        {
            get { return _omegaLambda; }
            set { this.RaiseAndSetIfChanged(ref _omegaLambda, value); }
        }
        private double _omegaLambda;

        /// <summary>
        /// Gets or sets the current text shown in the UI for the single redshift.
        /// </summary>
        public string RedshiftText
        {
            get { return _redshiftText; }
            set { this.RaiseAndSetIfChanged(ref _redshiftText, value); }
        }
        private string _redshiftText;

        /// <summary>
        /// Gets or sets the current single redshift.
        /// </summary>
        public double Redshift
        {
            get { return _redshift; }
            set { this.RaiseAndSetIfChanged(ref _redshift, value); }
        }
        private double _redshift;

        /// <summary>
        /// Gets the latest calculated output for the current cosmology and single redshift.
        /// </summary>
        public string CosmoText => _cosmoText.Value;
        private readonly ObservableAsPropertyHelper<string> _cosmoText;

        /// <summary>
        /// Gets or sets the full path to the input file for batch mode.
        /// </summary>
        public FileInfo BatchFile
        {
            get { return _batchFile; }
            set { this.RaiseAndSetIfChanged(ref _batchFile, value); }
        }
        private FileInfo _batchFile;

        /// <summary>
        /// Gets or sets the full path to the output file for batch mode.
        /// </summary>
        public FileInfo OutputFile
        {
            get { return _outputFile; }
            set { this.RaiseAndSetIfChanged(ref _outputFile, value); }
        }
        private FileInfo _outputFile;

        private FileType _fileType;

        public string SaveNotificationText
        {
            get { return _saveNotificationText; }
            set { this.RaiseAndSetIfChanged(ref _saveNotificationText, value); }
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

        private static void RunOnUiThread(Action<object> action, object parm = null)
        {
            if (System.Windows.Application.Current?.Dispatcher.CheckAccess() ?? false)
            {
                action(parm); // we're on the ui thread, just go for it
                return;
            }
            if (System.Windows.Application.Current != null)
                System.Windows.Application.Current.Dispatcher.Invoke(action, parm);
            else
                throw new InvalidOperationException("Unable to run on UI thread!");
        }

    }
}

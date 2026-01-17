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
using ReactiveUI.SourceGenerators;

namespace Cosmic.NET.WPF;

public partial class MainWindowVM : ReactiveObject {
    private readonly TimeSpan _throttleDelay = TimeSpan.FromMilliseconds(500);

    private const string HNoughtPos = "H-nought must be positive.";
    private const string HNoughtNum = "H-nought must be a number.";
    private const string OmegaMatterPos = "Omega-matter must be greater than or equal to zero.";
    private const string OmegaMatterNum = "Omega-matter must be a number.";
    private const string OmegaLambdaNum = "Omega-Lambda must be a number.";
    private const string ZNotNeg = "The redshift must be at least 0.";
    private const string ZNum = "The redshift must be a number.";

    private readonly List<string> _activeErrors = [];

    private readonly IObservable<bool> _canCopyOutput;
    private readonly IObservable<bool> _canComputeAndSave;

    public enum FileType {
        Txt,
        Csv
    }

    public Interaction<string, Unit> ParseError { get; } = new();
    public Interaction<string, FileInfo?> GetFileToOpen { get; } = new();
    public Interaction<string, (FileInfo?, FileType)> GetFileToSave { get; } = new();

    public MainWindowVM() {
        _canCopyOutput = this.WhenAnyValue(
            x => x.CosmoText,
            t => !string.IsNullOrEmpty(t));
        _canComputeAndSave =
            this.WhenAnyValue(
                x => x.BatchFile,
                input => input?.Exists ?? false);

        HNought = 71;
        OmegaMatter = 0.27;
        OmegaLambda = 0.73;
        Redshift = 0.1;
        HNoughtText = HNought.ToString(CultureInfo.InvariantCulture);
        OmegaMatterText = OmegaMatter.ToString(CultureInfo.InvariantCulture);
        OmegaLambdaText = OmegaLambda.ToString(CultureInfo.InvariantCulture);
        RedshiftText = Redshift.ToString(CultureInfo.InvariantCulture);

        this.WhenAnyValue(x => x.HNoughtText)
            .Throttle(_throttleDelay)
            .DistinctUntilChanged()
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .Subscribe(h => {
                if (double.TryParse(h, out var temp)) {
                    if (temp <= 0)
                        ShowError(HNoughtPos);
                    else {
                        HNought = temp;
                        ClearError(HNoughtPos);
                        ClearError(HNoughtNum);
                    }
                } else
                    ShowError(HNoughtNum);
            });


        this.WhenAnyValue(x => x.OmegaMatterText)
            .Throttle(_throttleDelay)
            .DistinctUntilChanged()
            .Where(om => !string.IsNullOrWhiteSpace(om))
            .Subscribe(om => {
                if (double.TryParse(om, out var temp)) {
                    if (temp < 0)
                        ShowError(OmegaMatterPos);
                    else {
                        OmegaMatter = temp;
                        ClearError(OmegaMatterPos);
                        ClearError(OmegaMatterNum);
                    }
                } else
                    ShowError(OmegaMatterNum);
            });

        this.WhenAnyValue(x => x.OmegaLambdaText)
            .Throttle(_throttleDelay)
            .DistinctUntilChanged()
            .Where(ol => !string.IsNullOrWhiteSpace(ol))
            .Subscribe(ol => {
                if (double.TryParse(ol, out var temp)) {
                    OmegaLambda = temp;
                    ClearError(OmegaLambdaNum);
                } else
                    ShowError(OmegaLambdaNum);
            });

        this.WhenAnyValue(x => x.RedshiftText)
            .Throttle(_throttleDelay)
            .DistinctUntilChanged()
            .Where(z => !string.IsNullOrWhiteSpace(z))
            .Subscribe(z => {
                if (double.TryParse(z, out var temp)) {
                    if (temp < 0)
                        ShowError(ZNotNeg);
                    else {
                        Redshift = temp;
                        ClearError(ZNotNeg);
                        ClearError(ZNum);
                    }
                } else
                    ShowError(ZNum);
            });

        _cosmoTextHelper =
            this.WhenAnyValue(
                    x => x.HNought,
                    x => x.OmegaMatter,
                    x => x.OmegaLambda,
                    x => x.Redshift,
                    (h, om, ol, z) => {
                        try {
                            var cosmo = new Cosmology(h, om, ol) { Redshift = z };
                            return cosmo.ToString();
                        } catch (Exception) {
                            return null;
                        }
                    })
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(CosmoText));
    }

    /// <summary>
    /// Copy the single-redshift output to the clipboard.
    /// </summary>
    [ReactiveCommand(CanExecute = nameof(_canCopyOutput))]
    private async Task CopyOutput() {
        await Observable.Start(() => Clipboard.SetText(CosmoText ?? string.Empty), RxApp.MainThreadScheduler);
    }

    /// <summary>
    /// Prompt the user for the input file for batch processing.
    /// </summary>
    [ReactiveCommand]
    private async Task GetInputFile() {
        var file = await GetFileToOpen.Handle(string.Empty);
        if (file != null)
            BatchFile = file;
    }

    /// <summary>
    /// Prompt the user for an output file, then compute the set of redshifts in the provided batch file and write them to the output file.
    /// </summary>
    [ReactiveCommand(CanExecute = nameof(_canComputeAndSave))]
    private async Task ComputeAndSave() {
        await Observable.StartAsync(async () => {
            if (BatchFile == null) return;
            var (outputFile, fileType) = await GetFileToSave.Handle("cosmic.txt");
            if (outputFile == null) return;
            OutputFile = outputFile;
            _fileType = fileType;

            await Observable.Start(() => SaveNotificationText = string.Empty, RxApp.MainThreadScheduler);
            // read entire input file before doing any calculations
            var inputLines = new List<double>();
            try {
                using var sr = new StreamReader(BatchFile.FullName);
                while (await sr.ReadLineAsync() is { } line)
                    inputLines.Add(double.Parse(line));
            } catch (Exception) {
                await Observable.StartAsync(
                    async () =>
                        await ParseError.Handle(
                            "An error occurred reading the input file. Check the file to make sure it has one redshift per line and nothing else."),
                    RxApp.MainThreadScheduler);
                return;
            }

            // process the input redshifts one at a time, writing each to the output file as we go
            try {
                var cosmology = new Cosmology(HNought, OmegaMatter, OmegaLambda);
                var separator = _fileType switch {
                    FileType.Txt => "\t",
                    FileType.Csv => ",",
                    _ => throw new Exception("Unexpected file type.")
                };
                var leader = _fileType switch {
                    FileType.Txt => "# ",
                    FileType.Csv => string.Empty,
                    _ => throw new Exception("Unexpected file type.")
                };
                await using var sw = new StreamWriter(OutputFile.FullName);
                await sw.WriteLineAsync(cosmology.GetShortFormHeader(leader, separator));
                foreach (var redshift in inputLines) {
                    cosmology.Redshift = redshift;
                    await sw.WriteLineAsync(cosmology.GetShortFormOutput(separator));
                }
            } catch (Exception) {
                await Observable.StartAsync(async () => await ParseError.Handle("An error occurred writing the output file."),
                    RxApp.MainThreadScheduler);
                return;
            }

            await Observable.Start(() => SaveNotificationText = $"Results saved to {OutputFile.Name}",
                RxApp.MainThreadScheduler);
        }, RxApp.TaskpoolScheduler);
    }

    /// <summary>
    /// Gets or sets the current text shown in the UI for the Hubble constant.
    /// </summary>
    [Reactive] public partial string HNoughtText { get; set; }

    /// <summary>
    /// Gets or sets the current value of the Hubble constant.
    /// </summary>
    [Reactive] private partial double HNought { get; set; }

    /// <summary>
    /// Gets or sets the current text shown in the UI for the density attributed to matter.
    /// </summary>
    [Reactive] public partial string OmegaMatterText { get; set; }

    /// <summary>
    /// Gets or sets the current value of the density attributed to matter.
    /// </summary>
    [Reactive] private partial double OmegaMatter { get; set; }

    /// <summary>
    /// Gets or sets the current text shown in the UI for the density attributed to the cosmological constant.
    /// </summary>
    [Reactive] public partial string OmegaLambdaText { get; set; }

    /// <summary>
    /// Gets or sets the current value of the density attributed to the cosmological constant.
    /// </summary>
    [Reactive] private partial double OmegaLambda { get; set; }

    /// <summary>
    /// Gets or sets the current text shown in the UI for the single redshift.
    /// </summary>
    [Reactive] public partial string RedshiftText { get; set; }

    /// <summary>
    /// Gets or sets the current single redshift.
    /// </summary>
    [Reactive] private partial double Redshift { get; set; }

    /// <summary>
    /// Gets the latest calculated output for the current cosmology and single redshift.
    /// </summary>
    [ObservableAsProperty] public partial string? CosmoText { get; }

    /// <summary>
    /// Gets or sets the full path to the input file for batch mode.
    /// </summary>
    [Reactive] public partial FileInfo? BatchFile { get; private set; }

    /// <summary>
    /// Gets or sets the full path to the output file for batch mode.
    /// </summary>
    [Reactive] private partial FileInfo? OutputFile { get; set; }

    private FileType _fileType;

    [Reactive] public partial string? SaveNotificationText { get; private set; }

    private void ClearError(string errorMsg) {
        _activeErrors.Remove(errorMsg);
    }

    private void ShowError(string errorMsg) {
        if (_activeErrors.Contains(errorMsg)) return;
        _activeErrors.Add(errorMsg);
        Observable.StartAsync(async () => await ParseError.Handle(errorMsg), RxApp.MainThreadScheduler);
    }
}
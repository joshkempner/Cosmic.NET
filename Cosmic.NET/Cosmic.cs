using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Cosmo;

namespace Cosmic.NET;

public partial class Cosmic : Form {

    #region Fields

    private ErrorProvider _errorProvider1;
    private Cosmology _cosmology;

    private int _saveToFilterIndex;

    #endregion

    #region Costructors etc.

    public Cosmic() {
        InitializeComponent();
    }

    #endregion

    #region Form Event Handlers

    private void Cosmic_Load(object sender, EventArgs e) {
        _errorProvider1 = new ErrorProvider();
        _cosmology = new Cosmology();

        // initialize the cosmology
        HubbleConstant.Text = _cosmology.H0.ToString(CultureInfo.CurrentCulture);
        OmegaMatter.Text = _cosmology.OmegaMatter.ToString(CultureInfo.CurrentCulture);
        OmegaLambda.Text = _cosmology.OmegaLambda.ToString(CultureInfo.CurrentCulture);

        // set a default redshift
        Redshift.Text = @"0.1";
        _cosmology.Redshift = Convert.ToDouble(Redshift.Text);
    }

    private void Cosmic_Layout(object sender, LayoutEventArgs e) {
        // adjust the layout of the cosmological parameters fields
        OmegaMatter.Left = (OmegaLambda.Left - HubbleConstant.Left) / 2 + HubbleConstant.Left;
        MatterLabel.Left = OmegaMatter.Left - 14;
        OmegaMatterLabel.Left = MatterLabel.Left - 12;
    }

    private void Cosmic_FormClosing(object sender, FormClosingEventArgs e) {
        e.Cancel = false;
    }

    #endregion

    #region Validation Handlers

    private void HubbleConstant_Validated(object sender, EventArgs e) {
        // clear errors
        _errorProvider1.SetError(HubbleConstant, "");

        // nothing else to do since the value has already been set by the Validating event handler
    }

    private void HubbleConstant_Validating(object sender, CancelEventArgs e) {
        try {
            // try setting it since the set method includes validation, but only if the value has changed
            if (_cosmology.H0.ToString(CultureInfo.CurrentCulture) != HubbleConstant.Text)
                _cosmology.H0 = Convert.ToDouble(HubbleConstant.Text);
        } catch (Exception ex) {
            // cancel the event, select the text, and set an error
            e.Cancel = true;
            HubbleConstant.Select(0, HubbleConstant.Text.Length);
            _errorProvider1.SetError(HubbleConstant, ex.Message);
        }
    }

    private void OmegaMatter_Validated(object sender, EventArgs e) {
        // clear errors
        _errorProvider1.SetError(OmegaMatter, "");

        // nothing else to do since the value has already been set by the Validating event handler
    }

    private void OmegaMatter_Validating(object sender, CancelEventArgs e) {
        try {
            // try setting it since the set method includes validation, but only if the value has changed
            if (_cosmology.OmegaMatter.ToString(CultureInfo.CurrentCulture) != OmegaMatter.Text)
                _cosmology.OmegaMatter = Convert.ToDouble(OmegaMatter.Text);
        } catch (Exception ex) {
            // cancel the event, select the text, and set an error
            e.Cancel = true;
            OmegaMatter.Select(0, OmegaMatter.Text.Length);
            _errorProvider1.SetError(OmegaMatter, ex.Message);
        }
    }

    private void OmegaLambda_Validated(object sender, EventArgs e) {
        // clear errors
        _errorProvider1.SetError(OmegaLambda, "");

        // nothing else to do since the value has already been set by the Validating event handler
    }

    private void OmegaLambda_Validating(object sender, CancelEventArgs e) {
        try {
            // try setting it, but only if the value has changed
            if (_cosmology.OmegaLambda.ToString(CultureInfo.CurrentCulture) != OmegaLambda.Text)
                _cosmology.OmegaLambda = Convert.ToDouble(OmegaLambda.Text);
        } catch (FormatException) {
            // cancel the event, select the text, and set an error
            e.Cancel = true;
            OmegaLambda.Select(0, OmegaLambda.Text.Length);
            _errorProvider1.SetError(OmegaLambda, "The dark energy density must be given as a number.");
        } catch (Exception ex) {
            // OmegaLambda set method won't throw an exception, so this is unexpected
            MessageBox.Show(ex.Message, @"Unexpected Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SingleSource_CheckedChanged(object sender, EventArgs e) {
        // enable redshift field
        RedshiftLabel.Enabled = SingleSource.Checked;
        Redshift.Enabled = SingleSource.Checked;

        // disable the Calculate button unless a redshift has been specified
        Calculate.Enabled = Redshift.Text.Length != 0;
    }

    private void MultiSource_CheckedChanged(object sender, EventArgs e) {
        // enable fields for open/save files
        OpenFromLabel.Enabled = MultiSource.Checked;
        OpenFrom.Enabled = MultiSource.Checked;
        OpenFromBrowseButton.Enabled = MultiSource.Checked;
        SaveToLabel.Enabled = MultiSource.Checked;
        SaveTo.Enabled = MultiSource.Checked;
        SaveToBrowseButton.Enabled = MultiSource.Checked;

        // disable the Calculate button unless both open and save files are specified
        Calculate.Enabled = (OpenFrom.Text.Length != 0 && SaveTo.Text.Length != 0);
    }

    private void Redshift_Validated(object sender, EventArgs e) {
        // clear errors
        _errorProvider1.SetError(Redshift, "");

        // enable Calculate button
        Calculate.Enabled = true;
    }

    private void Redshift_Validating(object sender, CancelEventArgs e) {
        try {
            // try setting it since the set method includes validation, but only if the value has changed
            if (_cosmology.Redshift.ToString(CultureInfo.CurrentCulture) != Redshift.Text)
                _cosmology.Redshift = Convert.ToDouble(Redshift.Text);
        } catch (Exception ex) {
            // cancel the event, select the text, and set an error
            e.Cancel = true;
            Redshift.Select(0, Redshift.Text.Length);
            _errorProvider1.SetError(Redshift, ex.Message);
        }
    }

    private void OpenFrom_Validated(object sender, EventArgs e) {
        // disable the Calculate button unless both open and save files are specified
        Calculate.Enabled = (OpenFrom.Text.Length != 0 && SaveTo.Text.Length != 0);
    }

    private void SaveTo_Validated(object sender, EventArgs e) {
        // disable the Calculate button unless both open and save files are specified
        Calculate.Enabled = (OpenFrom.Text.Length != 0 && SaveTo.Text.Length != 0);
    }

    #endregion

    #region Click and KeyPress Handlers

    private void HubbleConstant_KeyPress(object sender, KeyPressEventArgs e) {
        if (e.KeyChar != (char)Keys.Return) return;
        e.Handled = true;

        // validate the current value
        var ce = new CancelEventArgs();
        HubbleConstant_Validating(sender, ce);

        // proceed only if the Hubble constant validated
        if (!ce.Cancel) {
            // change focus to the Calculate button if active or to the group box if not
            if (Calculate.Enabled)
                Calculate.Focus();
            else
                CosmologicalParameters.Focus();
        }
    }

    private void OmegaMatter_KeyPress(object sender, KeyPressEventArgs e) {
        if (e.KeyChar != (char)Keys.Return) return;
        e.Handled = true;

        // validate the current value
        var ce = new CancelEventArgs();
        OmegaMatter_Validating(sender, ce);

        // proceed only if Omega-matter validated
        if (!ce.Cancel) {
            // change focus to the Calculate button if active or to the group box if not
            if (Calculate.Enabled)
                Calculate.Focus();
            else
                CosmologicalParameters.Focus();
        }
    }

    private void OmegaLambda_KeyPress(object sender, KeyPressEventArgs e) {
        if (e.KeyChar != (char)Keys.Return) return;
        e.Handled = true;

        // validate the current value
        var ce = new CancelEventArgs();
        OmegaLambda_Validating(sender, ce);

        // proceed only if Omega-Lambda validated
        if (!ce.Cancel) {
            // change focus to the Calculate button if active or to the group box if not
            if (Calculate.Enabled)
                Calculate.Focus();
            else
                CosmologicalParameters.Focus();
        }
    }

    private void Redshift_KeyPress(object sender, KeyPressEventArgs e) {
        if (e.KeyChar != (char)Keys.Return) return;
        // don't pass the event to the parent
        e.Handled = true;

        // validate the current value that's been entered for the redshift
        var ce = new CancelEventArgs();
        Redshift_Validating(sender, ce);

        // proceed only if the redshift validated
        if (!ce.Cancel) {
            // Raise a click event on the Calculate button
            InvokeOnClick(Calculate, EventArgs.Empty);
        }
    }

    private void OpenFromBrowseButton_Click(object sender, EventArgs e) {
        // create and set up open file dialog
        var dlg = new OpenFileDialog {
            DefaultExt = ".txt",
            Filter = @"Text documents (.txt)|*.txt|All files|*.*"
        };

        // show open file dialog and process results
        if (dlg.ShowDialog() != DialogResult.OK) return;
        OpenFrom.Text = dlg.FileName; // multiselect is false by default

        // enable the Calculate button if appropriate
        if (SaveTo.Text.Length != 0)
            Calculate.Enabled = true;
    }

    private void SaveToBrowseButton_Click(object sender, EventArgs e) {
        // create and set up open file dialog
        var dlg = new SaveFileDialog {
            DefaultExt = ".txt",
            Filter = @"Text document (.txt)|*.txt|CSV document (.csv)|*.csv"
        };

        // show open file dialog and process results
        if (dlg.ShowDialog() != DialogResult.OK) return;
        // set the text field's value
        SaveTo.Text = dlg.FileName;

        // save the chosen filter for when we write the file
        _saveToFilterIndex = dlg.FilterIndex;

        // enable the Calculate button if appropriate
        if (OpenFrom.Text.Length != 0)
            Calculate.Enabled = true;
    }

    private void Calculate_Click(object sender, EventArgs e) {
        if (SingleSource.Checked) {
            _cosmology.Redshift = Convert.ToDouble(Redshift.Text);

            // get the distance measures into a string and append them to the output text box
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.Append(_cosmology);
            sb.AppendLine();
            Results.Text += sb.ToString();
        } else {
            // validate input and output file paths
            var infile = OpenFrom.Text;
            var outfile = SaveTo.Text;

            // read entire input file before doing any calculations
            List<double> inputLines = [];
            try {
                using var sr = new StreamReader(infile);
                while (sr.ReadLine() is { } line) {
                    inputLines.Add(double.Parse(line));
                }
                inputLines.TrimExcess();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, @"Error reading file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // process the input redshifts one at a time, writing each to the output file as we go
            try {
                var separator = _saveToFilterIndex == 1 ? "\t" : ",";
                using var sw = new StreamWriter(outfile);
                sw.WriteLine(_cosmology.GetShortFormHeader("# ", separator));
                foreach (var redshift in inputLines) {
                    _cosmology.Redshift = redshift;
                    sw.WriteLine(_cosmology.GetShortFormOutput(separator));
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, @"Error writing file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    #endregion

    #region Other Event Handlers

    private void Results_TextChanged(object sender, EventArgs e) {
        Results.SelectionStart = Results.Text.Length;
        Results.SelectionLength = 0;
        Results.ScrollToCaret();
    }

    #endregion
}
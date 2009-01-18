namespace Cosmic.NET
{
    partial class Cosmic
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CosmologicalParameters = new System.Windows.Forms.GroupBox();
            this.OmegaLambda = new System.Windows.Forms.TextBox();
            this.LambdaLabel = new System.Windows.Forms.Label();
            this.OmegaLambdaLabel = new System.Windows.Forms.Label();
            this.OmegaMatter = new System.Windows.Forms.TextBox();
            this.MatterLabel = new System.Windows.Forms.Label();
            this.OmegaMatterLabel = new System.Windows.Forms.Label();
            this.HubbleConstant = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SourceParameters = new System.Windows.Forms.GroupBox();
            this.Calculate = new System.Windows.Forms.Button();
            this.SaveToBrowseButton = new System.Windows.Forms.Button();
            this.SaveTo = new System.Windows.Forms.TextBox();
            this.SaveToLabel = new System.Windows.Forms.Label();
            this.OpenFromBrowseButton = new System.Windows.Forms.Button();
            this.OpenFrom = new System.Windows.Forms.TextBox();
            this.OpenFromLabel = new System.Windows.Forms.Label();
            this.Redshift = new System.Windows.Forms.TextBox();
            this.RedshiftLabel = new System.Windows.Forms.Label();
            this.MultiSource = new System.Windows.Forms.RadioButton();
            this.SingleSource = new System.Windows.Forms.RadioButton();
            this.ResultsGroupBox = new System.Windows.Forms.GroupBox();
            this.Results = new System.Windows.Forms.TextBox();
            this.CosmologicalParameters.SuspendLayout();
            this.SourceParameters.SuspendLayout();
            this.ResultsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // CosmologicalParameters
            // 
            this.CosmologicalParameters.Controls.Add(this.OmegaLambda);
            this.CosmologicalParameters.Controls.Add(this.LambdaLabel);
            this.CosmologicalParameters.Controls.Add(this.OmegaLambdaLabel);
            this.CosmologicalParameters.Controls.Add(this.OmegaMatter);
            this.CosmologicalParameters.Controls.Add(this.MatterLabel);
            this.CosmologicalParameters.Controls.Add(this.OmegaMatterLabel);
            this.CosmologicalParameters.Controls.Add(this.HubbleConstant);
            this.CosmologicalParameters.Controls.Add(this.label2);
            this.CosmologicalParameters.Controls.Add(this.label1);
            this.CosmologicalParameters.Location = new System.Drawing.Point(12, 12);
            this.CosmologicalParameters.Name = "CosmologicalParameters";
            this.CosmologicalParameters.Size = new System.Drawing.Size(245, 51);
            this.CosmologicalParameters.TabIndex = 0;
            this.CosmologicalParameters.TabStop = false;
            this.CosmologicalParameters.Text = "Cosmological Parameters";
            // 
            // OmegaLambda
            // 
            this.OmegaLambda.Location = new System.Drawing.Point(188, 18);
            this.OmegaLambda.Name = "OmegaLambda";
            this.OmegaLambda.Size = new System.Drawing.Size(34, 20);
            this.OmegaLambda.TabIndex = 8;
            this.OmegaLambda.Validated += new System.EventHandler(this.OmegaLambda_Validated);
            this.OmegaLambda.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OmegaLambda_KeyPress);
            this.OmegaLambda.Validating += new System.ComponentModel.CancelEventHandler(this.OmegaLambda_Validating);
            // 
            // LambdaLabel
            // 
            this.LambdaLabel.AutoSize = true;
            this.LambdaLabel.Font = new System.Drawing.Font("Symbol", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.LambdaLabel.Location = new System.Drawing.Point(174, 26);
            this.LambdaLabel.Name = "LambdaLabel";
            this.LambdaLabel.Size = new System.Drawing.Size(11, 11);
            this.LambdaLabel.TabIndex = 7;
            this.LambdaLabel.Text = "L";
            // 
            // OmegaLambdaLabel
            // 
            this.OmegaLambdaLabel.AutoSize = true;
            this.OmegaLambdaLabel.Font = new System.Drawing.Font("Symbol", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.OmegaLambdaLabel.Location = new System.Drawing.Point(163, 21);
            this.OmegaLambdaLabel.Name = "OmegaLambdaLabel";
            this.OmegaLambdaLabel.Size = new System.Drawing.Size(15, 13);
            this.OmegaLambdaLabel.TabIndex = 6;
            this.OmegaLambdaLabel.Text = "W";
            // 
            // OmegaMatter
            // 
            this.OmegaMatter.Location = new System.Drawing.Point(110, 17);
            this.OmegaMatter.Name = "OmegaMatter";
            this.OmegaMatter.Size = new System.Drawing.Size(34, 20);
            this.OmegaMatter.TabIndex = 5;
            this.OmegaMatter.Validated += new System.EventHandler(this.OmegaMatter_Validated);
            this.OmegaMatter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OmegaMatter_KeyPress);
            this.OmegaMatter.Validating += new System.ComponentModel.CancelEventHandler(this.OmegaMatter_Validating);
            // 
            // MatterLabel
            // 
            this.MatterLabel.AutoSize = true;
            this.MatterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MatterLabel.Location = new System.Drawing.Point(96, 25);
            this.MatterLabel.Name = "MatterLabel";
            this.MatterLabel.Size = new System.Drawing.Size(15, 13);
            this.MatterLabel.TabIndex = 4;
            this.MatterLabel.Text = "m";
            // 
            // OmegaMatterLabel
            // 
            this.OmegaMatterLabel.AutoSize = true;
            this.OmegaMatterLabel.Font = new System.Drawing.Font("Symbol", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.OmegaMatterLabel.Location = new System.Drawing.Point(85, 20);
            this.OmegaMatterLabel.Name = "OmegaMatterLabel";
            this.OmegaMatterLabel.Size = new System.Drawing.Size(15, 13);
            this.OmegaMatterLabel.TabIndex = 3;
            this.OmegaMatterLabel.Text = "W";
            // 
            // HubbleConstant
            // 
            this.HubbleConstant.Location = new System.Drawing.Point(32, 17);
            this.HubbleConstant.Name = "HubbleConstant";
            this.HubbleConstant.Size = new System.Drawing.Size(34, 20);
            this.HubbleConstant.TabIndex = 2;
            this.HubbleConstant.Validated += new System.EventHandler(this.HubbleConstant_Validated);
            this.HubbleConstant.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HubbleConstant_KeyPress);
            this.HubbleConstant.Validating += new System.ComponentModel.CancelEventHandler(this.HubbleConstant_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(17, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "H";
            // 
            // SourceParameters
            // 
            this.SourceParameters.Controls.Add(this.Calculate);
            this.SourceParameters.Controls.Add(this.SaveToBrowseButton);
            this.SourceParameters.Controls.Add(this.SaveTo);
            this.SourceParameters.Controls.Add(this.SaveToLabel);
            this.SourceParameters.Controls.Add(this.OpenFromBrowseButton);
            this.SourceParameters.Controls.Add(this.OpenFrom);
            this.SourceParameters.Controls.Add(this.OpenFromLabel);
            this.SourceParameters.Controls.Add(this.Redshift);
            this.SourceParameters.Controls.Add(this.RedshiftLabel);
            this.SourceParameters.Controls.Add(this.MultiSource);
            this.SourceParameters.Controls.Add(this.SingleSource);
            this.SourceParameters.Location = new System.Drawing.Point(12, 69);
            this.SourceParameters.Name = "SourceParameters";
            this.SourceParameters.Size = new System.Drawing.Size(245, 181);
            this.SourceParameters.TabIndex = 2;
            this.SourceParameters.TabStop = false;
            this.SourceParameters.Text = "Source Parameters";
            // 
            // Calculate
            // 
            this.Calculate.Location = new System.Drawing.Point(6, 151);
            this.Calculate.Name = "Calculate";
            this.Calculate.Size = new System.Drawing.Size(75, 23);
            this.Calculate.TabIndex = 10;
            this.Calculate.Text = "Calculate";
            this.Calculate.UseVisualStyleBackColor = true;
            this.Calculate.Click += new System.EventHandler(this.Calculate_Click);
            // 
            // SaveToBrowseButton
            // 
            this.SaveToBrowseButton.AutoSize = true;
            this.SaveToBrowseButton.Enabled = false;
            this.SaveToBrowseButton.Location = new System.Drawing.Point(213, 123);
            this.SaveToBrowseButton.Name = "SaveToBrowseButton";
            this.SaveToBrowseButton.Size = new System.Drawing.Size(26, 23);
            this.SaveToBrowseButton.TabIndex = 9;
            this.SaveToBrowseButton.Text = "...";
            this.SaveToBrowseButton.UseVisualStyleBackColor = true;
            this.SaveToBrowseButton.Click += new System.EventHandler(this.SaveToBrowseButton_Click);
            // 
            // SaveTo
            // 
            this.SaveTo.Enabled = false;
            this.SaveTo.Location = new System.Drawing.Point(35, 125);
            this.SaveTo.Name = "SaveTo";
            this.SaveTo.ReadOnly = true;
            this.SaveTo.Size = new System.Drawing.Size(172, 20);
            this.SaveTo.TabIndex = 8;
            this.SaveTo.Validated += new System.EventHandler(this.SaveTo_Validated);
            // 
            // SaveToLabel
            // 
            this.SaveToLabel.AutoSize = true;
            this.SaveToLabel.Enabled = false;
            this.SaveToLabel.Location = new System.Drawing.Point(32, 108);
            this.SaveToLabel.Name = "SaveToLabel";
            this.SaveToLabel.Size = new System.Drawing.Size(45, 13);
            this.SaveToLabel.TabIndex = 7;
            this.SaveToLabel.Text = "save to:";
            // 
            // OpenFromBrowseButton
            // 
            this.OpenFromBrowseButton.AutoSize = true;
            this.OpenFromBrowseButton.Enabled = false;
            this.OpenFromBrowseButton.Location = new System.Drawing.Point(213, 83);
            this.OpenFromBrowseButton.Name = "OpenFromBrowseButton";
            this.OpenFromBrowseButton.Size = new System.Drawing.Size(26, 23);
            this.OpenFromBrowseButton.TabIndex = 6;
            this.OpenFromBrowseButton.Text = "...";
            this.OpenFromBrowseButton.UseVisualStyleBackColor = true;
            this.OpenFromBrowseButton.Click += new System.EventHandler(this.OpenFromBrowseButton_Click);
            // 
            // OpenFrom
            // 
            this.OpenFrom.Enabled = false;
            this.OpenFrom.Location = new System.Drawing.Point(35, 85);
            this.OpenFrom.Name = "OpenFrom";
            this.OpenFrom.ReadOnly = true;
            this.OpenFrom.Size = new System.Drawing.Size(172, 20);
            this.OpenFrom.TabIndex = 5;
            this.OpenFrom.Validated += new System.EventHandler(this.OpenFrom_Validated);
            // 
            // OpenFromLabel
            // 
            this.OpenFromLabel.AutoSize = true;
            this.OpenFromLabel.Enabled = false;
            this.OpenFromLabel.Location = new System.Drawing.Point(32, 68);
            this.OpenFromLabel.Name = "OpenFromLabel";
            this.OpenFromLabel.Size = new System.Drawing.Size(57, 13);
            this.OpenFromLabel.TabIndex = 4;
            this.OpenFromLabel.Text = "open from:";
            // 
            // Redshift
            // 
            this.Redshift.Location = new System.Drawing.Point(160, 19);
            this.Redshift.Name = "Redshift";
            this.Redshift.Size = new System.Drawing.Size(60, 20);
            this.Redshift.TabIndex = 3;
            this.Redshift.Validated += new System.EventHandler(this.Redshift_Validated);
            this.Redshift.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Redshift_KeyPress);
            this.Redshift.Validating += new System.ComponentModel.CancelEventHandler(this.Redshift_Validating);
            // 
            // RedshiftLabel
            // 
            this.RedshiftLabel.AutoSize = true;
            this.RedshiftLabel.Location = new System.Drawing.Point(113, 22);
            this.RedshiftLabel.Name = "RedshiftLabel";
            this.RedshiftLabel.Size = new System.Drawing.Size(41, 13);
            this.RedshiftLabel.TabIndex = 2;
            this.RedshiftLabel.Text = "redshift";
            // 
            // MultiSource
            // 
            this.MultiSource.AutoSize = true;
            this.MultiSource.Location = new System.Drawing.Point(7, 44);
            this.MultiSource.Name = "MultiSource";
            this.MultiSource.Size = new System.Drawing.Size(104, 17);
            this.MultiSource.TabIndex = 1;
            this.MultiSource.Text = "Multiple sources:";
            this.MultiSource.UseVisualStyleBackColor = true;
            this.MultiSource.CheckedChanged += new System.EventHandler(this.MultiSource_CheckedChanged);
            // 
            // SingleSource
            // 
            this.SingleSource.AutoSize = true;
            this.SingleSource.Checked = true;
            this.SingleSource.Location = new System.Drawing.Point(7, 20);
            this.SingleSource.Name = "SingleSource";
            this.SingleSource.Size = new System.Drawing.Size(92, 17);
            this.SingleSource.TabIndex = 0;
            this.SingleSource.TabStop = true;
            this.SingleSource.Text = "Single source:";
            this.SingleSource.UseVisualStyleBackColor = true;
            this.SingleSource.CheckedChanged += new System.EventHandler(this.SingleSource_CheckedChanged);
            // 
            // ResultsGroupBox
            // 
            this.ResultsGroupBox.Controls.Add(this.Results);
            this.ResultsGroupBox.Location = new System.Drawing.Point(12, 257);
            this.ResultsGroupBox.Name = "ResultsGroupBox";
            this.ResultsGroupBox.Size = new System.Drawing.Size(245, 169);
            this.ResultsGroupBox.TabIndex = 3;
            this.ResultsGroupBox.TabStop = false;
            this.ResultsGroupBox.Text = "Results";
            // 
            // Results
            // 
            this.Results.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Results.Location = new System.Drawing.Point(7, 20);
            this.Results.Multiline = true;
            this.Results.Name = "Results";
            this.Results.ReadOnly = true;
            this.Results.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Results.Size = new System.Drawing.Size(232, 143);
            this.Results.TabIndex = 0;
            this.Results.TextChanged += new System.EventHandler(this.Results_TextChanged);
            // 
            // Cosmic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(269, 438);
            this.Controls.Add(this.ResultsGroupBox);
            this.Controls.Add(this.SourceParameters);
            this.Controls.Add(this.CosmologicalParameters);
            this.MinimumSize = new System.Drawing.Size(277, 472);
            this.Name = "Cosmic";
            this.Text = "Cosmic";
            this.Load += new System.EventHandler(this.Cosmic_Load);
            this.Layout += new System.Windows.Forms.LayoutEventHandler(this.Cosmic_Layout);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Cosmic_FormClosing);
            this.CosmologicalParameters.ResumeLayout(false);
            this.CosmologicalParameters.PerformLayout();
            this.SourceParameters.ResumeLayout(false);
            this.SourceParameters.PerformLayout();
            this.ResultsGroupBox.ResumeLayout(false);
            this.ResultsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox CosmologicalParameters;
        private System.Windows.Forms.TextBox OmegaMatter;
        private System.Windows.Forms.Label MatterLabel;
        private System.Windows.Forms.Label OmegaMatterLabel;
        private System.Windows.Forms.TextBox HubbleConstant;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox OmegaLambda;
        private System.Windows.Forms.Label LambdaLabel;
        private System.Windows.Forms.Label OmegaLambdaLabel;
        private System.Windows.Forms.GroupBox SourceParameters;
        private System.Windows.Forms.RadioButton SingleSource;
        private System.Windows.Forms.TextBox Redshift;
        private System.Windows.Forms.Label RedshiftLabel;
        private System.Windows.Forms.RadioButton MultiSource;
        private System.Windows.Forms.Button SaveToBrowseButton;
        private System.Windows.Forms.TextBox SaveTo;
        private System.Windows.Forms.Label SaveToLabel;
        private System.Windows.Forms.Button OpenFromBrowseButton;
        private System.Windows.Forms.TextBox OpenFrom;
        private System.Windows.Forms.Label OpenFromLabel;
        private System.Windows.Forms.Button Calculate;
        private System.Windows.Forms.GroupBox ResultsGroupBox;
        private System.Windows.Forms.TextBox Results;
    }
}


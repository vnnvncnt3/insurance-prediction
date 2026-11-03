namespace HealthcareInsuranceFinal
{
    partial class Form1
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
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.openDlg = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.nudHidden = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nudLr = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.nudEpochs = new System.Windows.Forms.NumericUpDown();
            this.chkAuto = new System.Windows.Forms.CheckBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.curvePanel = new System.Windows.Forms.Panel();
            this.grpPred = new System.Windows.Forms.GroupBox();
            this.lblResult = new System.Windows.Forms.Label();
            this.btnPredict = new System.Windows.Forms.Button();
            this.chkSmoker = new System.Windows.Forms.CheckBox();
            this.nudChildren = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.nudBmi = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.cboSex = new System.Windows.Forms.ComboBox();
            this.nudAge = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudHidden)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEpochs)).BeginInit();
            this.grpPred.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudChildren)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBmi)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAge)).BeginInit();
            this.SuspendLayout();
            // 
            // txtPath
            // 
            this.txtPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPath.Location = new System.Drawing.Point(12, 12);
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.Size = new System.Drawing.Size(622, 27);
            this.txtPath.TabIndex = 0;
            this.txtPath.Text = "insurance.csv";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(661, 12);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(147, 27);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            // 
            // openDlg
            // 
            this.openDlg.FileName = "openFileDialog1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Hidden neurons:";
            // 
            // nudHidden
            // 
            this.nudHidden.Location = new System.Drawing.Point(140, 51);
            this.nudHidden.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.nudHidden.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudHidden.Name = "nudHidden";
            this.nudHidden.Size = new System.Drawing.Size(120, 22);
            this.nudHidden.TabIndex = 3;
            this.nudHidden.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(298, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Learning rate:";
            // 
            // nudLr
            // 
            this.nudLr.DecimalPlaces = 3;
            this.nudLr.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudLr.Location = new System.Drawing.Point(404, 53);
            this.nudLr.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudLr.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudLr.Name = "nudLr";
            this.nudLr.Size = new System.Drawing.Size(120, 22);
            this.nudLr.TabIndex = 5;
            this.nudLr.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(591, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Epochs:";
            // 
            // nudEpochs
            // 
            this.nudEpochs.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudEpochs.Location = new System.Drawing.Point(670, 55);
            this.nudEpochs.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.nudEpochs.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudEpochs.Name = "nudEpochs";
            this.nudEpochs.Size = new System.Drawing.Size(136, 22);
            this.nudEpochs.TabIndex = 7;
            this.nudEpochs.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // chkAuto
            // 
            this.chkAuto.AutoSize = true;
            this.chkAuto.Checked = true;
            this.chkAuto.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAuto.Location = new System.Drawing.Point(15, 89);
            this.chkAuto.Name = "chkAuto";
            this.chkAuto.Size = new System.Drawing.Size(85, 20);
            this.chkAuto.TabIndex = 8;
            this.chkAuto.Text = "Auto-tune";
            this.chkAuto.UseVisualStyleBackColor = true;
            // 
            // btnRun
            // 
            this.btnRun.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRun.Location = new System.Drawing.Point(538, 89);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(132, 37);
            this.btnRun.TabIndex = 9;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtLog.Location = new System.Drawing.Point(12, 176);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(387, 367);
            this.txtLog.TabIndex = 11;
            this.txtLog.WordWrap = false;
            // 
            // curvePanel
            // 
            this.curvePanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.curvePanel.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.curvePanel.Location = new System.Drawing.Point(405, 176);
            this.curvePanel.Name = "curvePanel";
            this.curvePanel.Size = new System.Drawing.Size(403, 178);
            this.curvePanel.TabIndex = 12;
            // 
            // grpPred
            // 
            this.grpPred.Controls.Add(this.lblResult);
            this.grpPred.Controls.Add(this.btnPredict);
            this.grpPred.Controls.Add(this.chkSmoker);
            this.grpPred.Controls.Add(this.nudChildren);
            this.grpPred.Controls.Add(this.label6);
            this.grpPred.Controls.Add(this.nudBmi);
            this.grpPred.Controls.Add(this.label5);
            this.grpPred.Controls.Add(this.cboSex);
            this.grpPred.Controls.Add(this.nudAge);
            this.grpPred.Controls.Add(this.label4);
            this.grpPred.Location = new System.Drawing.Point(405, 360);
            this.grpPred.Name = "grpPred";
            this.grpPred.Size = new System.Drawing.Size(403, 183);
            this.grpPred.TabIndex = 13;
            this.grpPred.TabStop = false;
            this.grpPred.Text = "Predict insurance charges (after training)";
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(148, 140);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(231, 16);
            this.lblResult.TabIndex = 21;
            this.lblResult.Text = "Predicted charges: (train a model first)";
            // 
            // btnPredict
            // 
            this.btnPredict.Location = new System.Drawing.Point(24, 132);
            this.btnPredict.Name = "btnPredict";
            this.btnPredict.Size = new System.Drawing.Size(118, 33);
            this.btnPredict.TabIndex = 20;
            this.btnPredict.Text = "Predict";
            this.btnPredict.UseVisualStyleBackColor = true;
            // 
            // chkSmoker
            // 
            this.chkSmoker.AutoSize = true;
            this.chkSmoker.Location = new System.Drawing.Point(189, 61);
            this.chkSmoker.Name = "chkSmoker";
            this.chkSmoker.Size = new System.Drawing.Size(76, 20);
            this.chkSmoker.TabIndex = 14;
            this.chkSmoker.Text = "Smoker";
            this.chkSmoker.UseVisualStyleBackColor = true;
            // 
            // nudChildren
            // 
            this.nudChildren.Location = new System.Drawing.Point(265, 29);
            this.nudChildren.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudChildren.Name = "nudChildren";
            this.nudChildren.Size = new System.Drawing.Size(57, 22);
            this.nudChildren.TabIndex = 19;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(186, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 16);
            this.label6.TabIndex = 18;
            this.label6.Text = "Children:";
            // 
            // nudBmi
            // 
            this.nudBmi.DecimalPlaces = 1;
            this.nudBmi.Location = new System.Drawing.Point(71, 93);
            this.nudBmi.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nudBmi.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudBmi.Name = "nudBmi";
            this.nudBmi.Size = new System.Drawing.Size(57, 22);
            this.nudBmi.TabIndex = 17;
            this.nudBmi.Value = new decimal(new int[] {
            28,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(21, 95);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 16);
            this.label5.TabIndex = 16;
            this.label5.Text = "BMI:";
            // 
            // cboSex
            // 
            this.cboSex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSex.FormattingEnabled = true;
            this.cboSex.Items.AddRange(new object[] {
            "female",
            "male"});
            this.cboSex.Location = new System.Drawing.Point(24, 57);
            this.cboSex.Name = "cboSex";
            this.cboSex.Size = new System.Drawing.Size(104, 24);
            this.cboSex.TabIndex = 15;
            // 
            // nudAge
            // 
            this.nudAge.Location = new System.Drawing.Point(71, 29);
            this.nudAge.Minimum = new decimal(new int[] {
            18,
            0,
            0,
            0});
            this.nudAge.Name = "nudAge";
            this.nudAge.Size = new System.Drawing.Size(57, 22);
            this.nudAge.TabIndex = 14;
            this.nudAge.Value = new decimal(new int[] {
            35,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 16);
            this.label4.TabIndex = 14;
            this.label4.Text = "Age:";
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(688, 89);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 37);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(895, 611);
            this.Controls.Add(this.grpPred);
            this.Controls.Add(this.curvePanel);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.chkAuto);
            this.Controls.Add(this.nudEpochs);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nudLr);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.nudHidden);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtPath);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.nudHidden)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEpochs)).EndInit();
            this.grpPred.ResumeLayout(false);
            this.grpPred.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudChildren)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBmi)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAge)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.OpenFileDialog openDlg;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudHidden;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudLr;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudEpochs;
        private System.Windows.Forms.CheckBox chkAuto;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Panel curvePanel;
        private System.Windows.Forms.GroupBox grpPred;
        private System.Windows.Forms.ComboBox cboSex;
        private System.Windows.Forms.NumericUpDown nudAge;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.Button btnPredict;
        private System.Windows.Forms.CheckBox chkSmoker;
        private System.Windows.Forms.NumericUpDown nudChildren;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nudBmi;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnCancel;
    }
}


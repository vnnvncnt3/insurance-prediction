using Backprop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace HealthcareInsuranceFinal
{
    public partial class Form1 : Form
    {
        private BackgroundWorker worker;
        private Pipeline pipeline;
        private PipelineResult trained;
        private readonly List<int> curveEpochs = new List<int>();
        private readonly List<double> curveTrain = new List<double>();
        private readonly List<double> curveTest = new List<double>();

        public Form1()
        {
            InitializeComponent();
            SetupBehavior();

            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_Completed;
        }

        private void SetupBehavior()
        {
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, curvePanel, new object[] { true });

            this.FormClosing += delegate { if (pipeline != null) pipeline.CancelRequested = true; };

            string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            txtPath.Text = Path.Combine(projectRoot, "insurance.csv");
            btnBrowse.Click += BtnBrowse_Click;
            btnRun.Click += BtnRun_Click;
            btnCancel.Click += delegate { if (pipeline != null) pipeline.CancelRequested = true; };
            btnPredict.Click += BtnPredict_Click;
            chkAuto.CheckedChanged += delegate { SetManualEnabled(!chkAuto.Checked); };
            curvePanel.Paint += CurvePanel_Paint;
            curvePanel.Resize += delegate { curvePanel.Invalidate(); };

            cboSex.SelectedIndex = 0;
            SetManualEnabled(!chkAuto.Checked);
            SetStatus("Ready.");
        }

        private void SetStatus(string msg)
        {
            this.Text = "Insurance Cost Regression - " + msg;
        }

        private void SetManualEnabled(bool enabled)
        {
            nudHidden.Enabled = enabled; nudLr.Enabled = enabled; nudEpochs.Enabled = enabled;
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            if (openDlg.ShowDialog() == DialogResult.OK) txtPath.Text = openDlg.FileName;
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            if (worker.IsBusy)
            {
                MessageBox.Show("A training session is already in progress. Please wait or click Cancel.",
                    "Insurance Regression", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(txtPath.Text))
            {
                MessageBox.Show("CSV file not found:\n" + txtPath.Text, "Insurance Regression",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtLog.Clear();
            curveEpochs.Clear(); curveTrain.Clear(); curveTest.Clear();
            curvePanel.Invalidate();
            trained = null;
            lblResult.Text = "Predicted charges: (training...)";
            btnRun.Enabled = false; btnCancel.Enabled = true;
            SetStatus(chkAuto.Checked ? "Tuning + training..." : "Training...");

            pipeline = new Pipeline();
            pipeline.OutputDir = AppDomain.CurrentDomain.BaseDirectory;
            pipeline.Log = delegate (string msg) { OnUi(delegate { txtLog.AppendText(msg + "\r\n"); }); };
            pipeline.Curve = delegate (int epoch, double tr, double te)
            {
                OnUi(delegate
                {
                    curveEpochs.Add(epoch); curveTrain.Add(tr); curveTest.Add(te);
                    curvePanel.Invalidate();
                });
            };

            object[] args = new object[] {
                txtPath.Text, chkAuto.Checked, (int)nudHidden.Value, (double)nudLr.Value, (int)nudEpochs.Value };
            worker.RunWorkerAsync(args);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] a = (object[])e.Argument;
            e.Result = pipeline.Run((string)a[0], (bool)a[1], (int)a[2], (double)a[3], (int)a[4]);
        }

        private void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            btnRun.Enabled = true; btnCancel.Enabled = false;
            if (e.Error != null)
            {
                txtLog.AppendText("ERROR: " + e.Error.Message + "\r\n");
                SetStatus("Failed.");
                lblResult.Text = "Predicted charges: (train a model first)";
                return;
            }
            trained = e.Result as PipelineResult;
            if (trained == null)
            {
                SetStatus("Cancelled.");
                lblResult.Text = "Predicted charges: (train a model first)";
            }
            else
            {
                SetStatus("Done. You can now use Predict.");
                lblResult.Text = "Predicted charges: (press Predict)";
            }
        }

        private void OnUi(MethodInvoker action)
        {
            try
            {
                if (!this.IsDisposed && this.IsHandleCreated) this.BeginInvoke(action);
            }
            catch (InvalidOperationException) { }
        }

        private void BtnPredict_Click(object sender, EventArgs e)
        {
            if (trained == null)
            {
                MessageBox.Show("Train a model first (press Run).", "Insurance Regression",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            double[] raw = new double[] {
                (double)nudAge.Value,
                cboSex.SelectedIndex,
                (double)nudBmi.Value,
                (double)nudChildren.Value,
                chkSmoker.Checked ? 1.0 : 0.0 };
            double scaledOut = Trainer.Predict(trained.Net, trained.Scaler.ScaleFeatures(raw));
            double dollars = trained.Scaler.InverseTarget(scaledOut);
            lblResult.Text = "Predicted charges: $" + dollars.ToString("N2");
        }

        private void CurvePanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            const int left = 55, top = 15, right = 15, bottom = 30;
            Rectangle plot = new Rectangle(left, top,
                curvePanel.Width - left - right, curvePanel.Height - top - bottom);
            if (plot.Width < 20 || plot.Height < 20) return;

            using (Pen axis = new Pen(Color.Black))
            using (Pen grid = new Pen(Color.Gainsboro))
            using (Font small = new Font("Segoe UI", 8f))
            {
                g.DrawRectangle(axis, plot);
                g.DrawString("Loss curve (MSE, scaled)", small, Brushes.Black, plot.Left, 0);

                if (curveEpochs.Count < 2)
                {
                    g.DrawString("Loss curve appears during training", small, Brushes.Gray, plot.Left + 10, plot.Top + 10);
                    return;
                }

                double maxY = 0;
                for (int i = 0; i < curveEpochs.Count; i++)
                    maxY = Math.Max(maxY, Math.Max(curveTrain[i], curveTest[i]));
                maxY *= 1.05;
                if (maxY <= 0) maxY = 1;
                double maxX = curveEpochs[curveEpochs.Count - 1];
                if (maxX <= 0) maxX = 1;

                for (int k = 0; k <= 4; k++)
                {
                    float y = plot.Bottom - (float)(plot.Height * k / 4.0);
                    if (k > 0 && k < 4) g.DrawLine(grid, plot.Left, y, plot.Right, y);
                    string label = (maxY * k / 4.0).ToString("F4");
                    g.DrawString(label, small, Brushes.Black, 2, y - 7);
                }
                g.DrawString("0", small, Brushes.Black, plot.Left - 3, plot.Bottom + 3);
                g.DrawString(((int)maxX).ToString() + " epochs", small, Brushes.Black, plot.Right - 60, plot.Bottom + 3);

                PointF[] trainPts = new PointF[curveEpochs.Count];
                PointF[] testPts = new PointF[curveEpochs.Count];
                for (int i = 0; i < curveEpochs.Count; i++)
                {
                    float x = plot.Left + (float)(plot.Width * curveEpochs[i] / maxX);
                    trainPts[i] = new PointF(x, plot.Bottom - (float)(plot.Height * curveTrain[i] / maxY));
                    testPts[i] = new PointF(x, plot.Bottom - (float)(plot.Height * curveTest[i] / maxY));
                }
                using (Pen trainPen = new Pen(Color.RoyalBlue, 2f))
                using (Pen testPen = new Pen(Color.OrangeRed, 2f))
                {
                    g.DrawLines(trainPen, trainPts);
                    g.DrawLines(testPen, testPts);
                    g.DrawLine(trainPen, plot.Right - 110, plot.Top + 12, plot.Right - 90, plot.Top + 12);
                    g.DrawString("Train", small, Brushes.Black, plot.Right - 86, plot.Top + 5);
                    g.DrawLine(testPen, plot.Right - 110, plot.Top + 28, plot.Right - 90, plot.Top + 28);
                    g.DrawString("Test", small, Brushes.Black, plot.Right - 86, plot.Top + 21);
                }
            }
        }
    }
}
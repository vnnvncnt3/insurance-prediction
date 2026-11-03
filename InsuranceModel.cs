using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Backprop;

namespace HealthcareInsuranceFinal
{
    public class Sample
    {
        public double[] Features;
        public double Target;
    }

    public class ScaledSet
    {
        public double[][] X;
        public double[] Y;
    }

    public delegate void CurveHandler(int epoch, double trainMse, double testMse);

    public static class DataLoader
    {
        public static readonly string[] FeatureColumns = { "age", "sex", "bmi", "children", "smoker" };
        private const string TargetColumn = "charges";

        public static List<Sample> Load(string path, out int skipped)
        {
            List<Sample> samples = new List<Sample>();
            skipped = 0;

            string[] lines = File.ReadAllLines(path);
            if (lines.Length < 2) throw new InvalidDataException("CSV has no data rows.");

            string[] header = SplitCsv(lines[0]).Select(h => h.Trim().ToLowerInvariant()).ToArray();
            int[] featIdx = FeatureColumns.Select(c => Array.IndexOf(header, c)).ToArray();
            int targetIdx = Array.IndexOf(header, TargetColumn);
            if (featIdx.Any(i => i < 0) || targetIdx < 0)
                throw new InvalidDataException("CSV must contain columns: " +
                    string.Join(", ", FeatureColumns) + ", " + TargetColumn +
                    ". Found: " + string.Join(", ", header));

            for (int r = 1; r < lines.Length; r++)
            {
                if (lines[r].Trim().Length == 0) continue;   // blank line
                string[] cells = SplitCsv(lines[r]);
                try
                {
                    double[] f = new double[FeatureColumns.Length];
                    for (int k = 0; k < f.Length; k++)
                        f[k] = ParseCell(FeatureColumns[k], cells[featIdx[k]]);
                    double target = ParseNumber(cells[targetIdx]);
                    samples.Add(new Sample { Features = f, Target = target });
                }
                catch (Exception)   // missing cell, bad number, unknown category
                {
                    skipped++;
                }
            }
            return samples;
        }

        private static string[] SplitCsv(string line)
        {
            return line.Split(',').Select(c => c.Trim().Trim('"')).ToArray();
        }

        private static double ParseNumber(string s)
        {
            return double.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private static double ParseCell(string column, string raw)
        {
            string v = raw.Trim().ToLowerInvariant();
            if (column == "sex")
            {
                if (v == "male" || v == "m" || v == "1") return 1.0;
                if (v == "female" || v == "f" || v == "0") return 0.0;
                throw new FormatException("Unknown sex value: " + raw);
            }
            if (column == "smoker")
            {
                if (v == "yes" || v == "true" || v == "1") return 1.0;
                if (v == "no" || v == "false" || v == "0") return 0.0;
                throw new FormatException("Unknown smoker value: " + raw);
            }
            return ParseNumber(v);
        }
    }

    public class MinMaxScaler
    {
        private double[] fMin, fMax;
        private double tMin, tMax;

        public void Fit(List<Sample> data)
        {
            int n = data[0].Features.Length;
            fMin = new double[n];
            fMax = new double[n];
            for (int j = 0; j < n; j++)
            {
                int col = j;
                fMin[j] = data.Min(s => s.Features[col]);
                fMax[j] = data.Max(s => s.Features[col]);
            }
            tMin = data.Min(s => s.Target);
            tMax = data.Max(s => s.Target);
        }

        public ScaledSet Transform(List<Sample> data)
        {
            ScaledSet set = new ScaledSet { X = new double[data.Count][], Y = new double[data.Count] };
            for (int i = 0; i < data.Count; i++)
            {
                set.X[i] = ScaleFeatures(data[i].Features);
                set.Y[i] = Scale(data[i].Target, tMin, tMax);
            }
            return set;
        }

        public double[] ScaleFeatures(double[] raw)
        {
            double[] scaled = new double[raw.Length];
            for (int j = 0; j < raw.Length; j++)
                scaled[j] = Scale(raw[j], fMin[j], fMax[j]);
            return scaled;
        }

        public double InverseTarget(double scaled) { return scaled * (tMax - tMin) + tMin; }

        private static double Scale(double v, double min, double max)
        {
            double range = max - min;
            return range == 0 ? 0.0 : (v - min) / range; 
        }
    }

    public static class Trainer
    {
        public static NeuralNet Create(int inputs, int hidden, double lr)
        {
            NeuralNet net = new NeuralNet(inputs, hidden, 1);
            return net;
        }

        public static void TrainEpoch(NeuralNet net, ScaledSet d, int[] order, Random rng)
        {
            Shuffle(order, rng);
            foreach (int i in order)
            {
                for (int j = 0; j < d.X[i].Length; j++) net.setInputs(j, d.X[i][j]);
                net.setDesiredOutput(0, d.Y[i]);
                net.learn();
            }
        }

        public static double Predict(NeuralNet net, double[] x)
        {
            for (int j = 0; j < x.Length; j++) net.setInputs(j, x[j]);
            net.run();
            return net.getOuputData(0);
        }

        public static double Mse(NeuralNet net, ScaledSet d)
        {
            double sum = 0;
            for (int i = 0; i < d.X.Length; i++)
            {
                double e = d.Y[i] - Predict(net, d.X[i]);
                sum += e * e;
            }
            return sum / d.X.Length;
        }

        public static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
        }
    }

    public class PipelineResult
    {
        public NeuralNet Net;
        public MinMaxScaler Scaler;
    }

    public class Pipeline
    {
        private const int Seed = 42;
        private const double TestFraction = 0.20;
        private const double ValFraction = 0.20;  
        private const int LogEvery = 100;        
        private const int CurveEvery = 10;    

        private static readonly int[] HiddenSizes = { 4, 8, 16 };
        private static readonly double[] LearningRates = { 0.05, 0.1, 0.2, 0.4 };
        private static readonly int[] EpochCheckpoints = { 200, 500, 1000 };

        private class Config { public int Hidden; public double Lr; public int Epochs; public double ValMse; }

        public Action<string> Log;  
        public CurveHandler Curve;      
        public string OutputDir = ".";  
        public volatile bool CancelRequested;

        private void Say(string msg) { if (Log != null) Log(msg); }

        public PipelineResult Run(string csvPath, bool autoTune, int hidden, double lr, int epochs)
        {
            int skipped;
            List<Sample> all = DataLoader.Load(csvPath, out skipped);
            Say(string.Format("Loaded {0} rows ({1} skipped for missing/invalid values).", all.Count, skipped));
            if (all.Count < 20) throw new InvalidDataException("Not enough valid rows.");

            Trainer.Shuffle(all, new Random(Seed));
            int nTest = (int)(all.Count * TestFraction);
            List<Sample> test = all.Take(nTest).ToList();
            List<Sample> trainAll = all.Skip(nTest).ToList();
            int nVal = (int)(trainAll.Count * ValFraction);
            List<Sample> val = trainAll.Take(nVal).ToList();
            List<Sample> tuneTrain = trainAll.Skip(nVal).ToList();
            Say(string.Format("Split -> train: {0}, validation: {1}, test: {2}", tuneTrain.Count, val.Count, test.Count));
            Say("");

            Config cfg;
            if (autoTune)
            {
                cfg = Tune(tuneTrain, val);
                if (cfg == null) return null;
                Say("");
                Say(string.Format("Best config: hidden={0}, lr={1}, epochs={2} (val MSE {3:F6})",
                    cfg.Hidden, cfg.Lr, cfg.Epochs, cfg.ValMse));
            }
            else
            {
                cfg = new Config { Hidden = hidden, Lr = lr, Epochs = epochs };
                Say(string.Format("Manual config: hidden={0}, lr={1}, epochs={2}", hidden, lr, epochs));
            }
            Say("Training on train+validation...");
            Say("");
            return FinalTrainAndEvaluate(trainAll, test, cfg);
        }

        private Config Tune(List<Sample> train, List<Sample> val)
        {
            MinMaxScaler scaler = new MinMaxScaler();
            scaler.Fit(train);
            ScaledSet tr = scaler.Transform(train);
            ScaledSet va = scaler.Transform(val);
            int maxEpochs = EpochCheckpoints.Max();

            List<Config> results = new List<Config>();
            Say("=== Hyperparameter search (validation MSE, scaled) ===");
            Say(string.Format("{0,8} {1,6} {2,8} {3,12}", "hidden", "lr", "epochs", "val MSE"));

            foreach (int h in HiddenSizes)
            {
                foreach (double lr in LearningRates)
                {
                    NeuralNet net = Trainer.Create(tr.X[0].Length, h, lr);
                    Random rng = new Random(Seed);
                    int[] order = Enumerable.Range(0, tr.X.Length).ToArray();

                    for (int e = 1; e <= maxEpochs; e++)
                    {
                        if (CancelRequested) { Say("Cancelled."); return null; }
                        Trainer.TrainEpoch(net, tr, order, rng);
                        if (EpochCheckpoints.Contains(e))
                        {
                            double m = Trainer.Mse(net, va);
                            results.Add(new Config { Hidden = h, Lr = lr, Epochs = e, ValMse = m });
                            Say(string.Format("{0,8} {1,6} {2,8} {3,12:F6}", h, lr, e, m));
                        }
                    }
                }
            }
            return results.OrderBy(r => r.ValMse).First();
        }

        private PipelineResult FinalTrainAndEvaluate(List<Sample> trainAll, List<Sample> test, Config cfg)
        {
            MinMaxScaler scaler = new MinMaxScaler();
            scaler.Fit(trainAll);                     
            ScaledSet tr = scaler.Transform(trainAll);
            ScaledSet te = scaler.Transform(test);

            NeuralNet net = Trainer.Create(tr.X[0].Length, cfg.Hidden, cfg.Lr);
            Random rng = new Random(Seed);
            int[] order = Enumerable.Range(0, tr.X.Length).ToArray();

            Say("=== Training log ===");
            Say(string.Format("{0,8} {1,14} {2,14}", "epoch", "train MSE", "test MSE"));

            using (StreamWriter log = new StreamWriter(Path.Combine(OutputDir, "training_log.csv")))
            {
                log.WriteLine("epoch,train_mse,test_mse");
                for (int e = 0; e <= cfg.Epochs; e++)
                {
                    if (CancelRequested) { Say("Cancelled."); return null; }
                    if (e > 0) Trainer.TrainEpoch(net, tr, order, rng);

                    bool doLog = (e % LogEvery == 0);
                    bool doCurve = (e % CurveEvery == 0);
                    if (doLog || doCurve)
                    {
                        double trainMse = Trainer.Mse(net, tr);
                        double testMse = Trainer.Mse(net, te);
                        if (Curve != null) Curve(e, trainMse, testMse);
                        if (doLog)
                        {
                            Say(string.Format("{0,8} {1,14:F6} {2,14:F6}", e, trainMse, testMse));
                            log.WriteLine("{0},{1:F6},{2:F6}", e, trainMse, testMse);
                        }
                    }
                }
            }

            double sse = 0, sae = 0, sst = 0, sseBase = 0;
            double meanTrain = trainAll.Average(s => s.Target);
            double meanTest = test.Average(s => s.Target);

            using (StreamWriter pw = new StreamWriter(Path.Combine(OutputDir, "predictions.csv")))
            {
                pw.WriteLine("actual,predicted");
                for (int i = 0; i < te.X.Length; i++)
                {
                    double pred = scaler.InverseTarget(Trainer.Predict(net, te.X[i]));
                    double actual = test[i].Target;
                    double err = actual - pred;
                    sse += err * err;
                    sae += Math.Abs(err);
                    sst += (actual - meanTest) * (actual - meanTest);
                    sseBase += (actual - meanTrain) * (actual - meanTrain);
                    pw.WriteLine("{0:F2},{1:F2}", actual, pred);
                }
            }

            int n = te.X.Length;
            Say("");
            Say(string.Format("=== Test-set evaluation ({0} samples) ===", n));
            Say(string.Format("MSE  (scaled 0-1)   : {0:F6}", Trainer.Mse(net, te)));
            Say(string.Format("MSE  (dollars^2)    : {0:F2}", sse / n));
            Say(string.Format("RMSE (dollars)      : {0:F2}", Math.Sqrt(sse / n)));
            Say(string.Format("MAE  (dollars)      : {0:F2}", sae / n));
            Say(string.Format("R^2                 : {0:F4}", 1.0 - sse / sst));
            Say(string.Format("Baseline RMSE (predict train mean): {0:F2}", Math.Sqrt(sseBase / n)));
            Say("");
            Say("Saved training_log.csv and predictions.csv in: " + Path.GetFullPath(OutputDir));

            return new PipelineResult { Net = net, Scaler = scaler };
        }
    }
}

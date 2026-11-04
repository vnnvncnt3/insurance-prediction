using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Backprop;

namespace HealthcareInsuranceFinal
{

    // one row of the dataset: raw (unscaled) input features and the target value
    public class Sample
    {
        public double[] Features;   // age, sex, bmi, children, smoker
        public double Target;       // insurance charges in dollars
    }

    // dataset after min-max scaling: every value lies in [0, 1]
    public class ScaledSet
    {
        public double[][] X;   // scaled feature vectors, one per sample
        public double[] Y;     // scaled targets
    }

    public delegate void CurveHandler(int epoch, double trainMse, double testMse);


    public static class DataLoader
    {
        // columns the network uses as inputs
        public static readonly string[] FeatureColumns = { "age", "sex", "bmi", "children", "smoker" };
        // column the network learns to predict.
        private const string TargetColumn = "charges";

        // reads the CSV at <paramref name="path"/> and converts it into samples.
        // rows with missing cells, bad numbers, or unknown categories are skipped
        // and counted in <paramref name="skipped"/>
        public static List<Sample> Load(string path, out int skipped)
        {
            List<Sample> samples = new List<Sample>();
            skipped = 0;

            string[] lines = File.ReadAllLines(path);
            if (lines.Length < 2) throw new InvalidDataException("CSV has no data rows.");

            string[] header = SplitCsv(lines[0]).Select(h => h.Trim().ToLowerInvariant()).ToArray();

            // find where each required column lives (-1 means it's missing)
            int[] featIdx = FeatureColumns.Select(c => Array.IndexOf(header, c)).ToArray();
            int targetIdx = Array.IndexOf(header, TargetColumn);
            if (featIdx.Any(i => i < 0) || targetIdx < 0)
                throw new InvalidDataException("CSV must contain columns: " +
                    string.Join(", ", FeatureColumns) + ", " + TargetColumn +
                    ". Found: " + string.Join(", ", header));

            // parse each data row (row 0 is the header)
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

        // naive CSV split: breaks on commas and strips whitespace and quotes
        private static string[] SplitCsv(string line)
        {
            return line.Split(',').Select(c => c.Trim().Trim('"')).ToArray();
        }

        // parses a number using the invariant culture so "." is always the decimal separator
        private static double ParseNumber(string s)
        {
            return double.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        // converts one raw cell to a number. Categorical columns are encoded as 0/1:
        // sex (male = 1, female = 0) and smoker (yes = 1, no = 0)
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
            // numeric columns (age, bmi, children)
            return ParseNumber(v);
        }
    }

    // scales every feature and the target to [0, 1] using min-max scaling:
    // scaled = (value - min) / (max - min)
    // fit on training data only, then reuse for validation/test data to avoid leakage
    public class MinMaxScaler
    {
        private double[] fMin, fMax;   // per-feature min and max
        private double tMin, tMax;     // target min and max

        // learns the min and max of each feature column and of the target
        public void Fit(List<Sample> data)
        {
            int n = data[0].Features.Length;
            fMin = new double[n];
            fMax = new double[n];
            for (int j = 0; j < n; j++)
            {
                int col = j;   // copy so the lambda captures a stable value
                fMin[j] = data.Min(s => s.Features[col]);
                fMax[j] = data.Max(s => s.Features[col]);
            }
            tMin = data.Min(s => s.Target);
            tMax = data.Max(s => s.Target);
        }

        // applies the learned scaling to a whole dataset
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

        // scales a single raw feature vector (e.g. user input for a prediction)
        public double[] ScaleFeatures(double[] raw)
        {
            double[] scaled = new double[raw.Length];
            for (int j = 0; j < raw.Length; j++)
                scaled[j] = Scale(raw[j], fMin[j], fMax[j]);
            return scaled;
        }

        // converts a network output in [0, 1] back to dollars
        public double InverseTarget(double scaled) { return scaled * (tMax - tMin) + tMin; }

        // min-max scales one value
        private static double Scale(double v, double min, double max)
        {
            double range = max - min;
            return range == 0 ? 0.0 : (v - min) / range;
        }
    }

    public static class Trainer
    {
        // builds a network with one hidden layer and a single output neuron (regression)
        public static NeuralNet Create(int inputs, int hidden, double lr)
        {
            NeuralNet net = new NeuralNet(inputs, hidden, 1);
            net.LearningRateOut = lr;
            net.LearningRateIn = lr;
            return net;
        }

        // runs one epoch of online (per-sample) training: samples are visited in a
        // freshly shuffled order, and the network updates its weights after each one
        public static void TrainEpoch(NeuralNet net, ScaledSet d, int[] order, Random rng)
        {
            Shuffle(order, rng);   // different sample order each epoch helps convergence
            foreach (int i in order)
            {
                for (int j = 0; j < d.X[i].Length; j++) net.setInputs(j, d.X[i][j]);
                net.setDesiredOutput(0, d.Y[i]);
                net.learn();   // forward pass + backpropagation + weight update
            }
        }

        // runs a forward pass and returns the (scaled) predicted output
        public static double Predict(NeuralNet net, double[] x)
        {
            for (int j = 0; j < x.Length; j++) net.setInputs(j, x[j]);
            net.run();
            return net.getOuputData(0);
        }

        // mean squared error over a dataset, in scaled (0-1) units
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

        // in-place Fisher-Yates shuffle
        public static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
        }
    }

    // full pipeline: load -> split -> (tune) -> train -> evaluate

    public class PipelineResult
    {
        public NeuralNet Net;
        public MinMaxScaler Scaler;
    }

    public class Pipeline
    {
        private const int Seed = 42;               // fixed seed -> reproducible splits and shuffles
        private const double TestFraction = 0.20;  // 20% of all data held out for final testing
        private const double ValFraction = 0.20;   // 20% of the remaining data used for tuning validation
        private const int LogEvery = 100;          // write a log line / CSV row every N epochs
        private const int CurveEvery = 10;         // send a learning-curve point every N epochs

        // hyperparameter grid searched when auto-tuning is on
        private static readonly int[] HiddenSizes = { 4, 8, 16 };
        private static readonly double[] LearningRates = { 0.05, 0.1, 0.2, 0.4 };
        // epoch counts at which validation error is recorded during tuning
        private static readonly int[] EpochCheckpoints = { 200, 500, 1000 };

        private class Config { public int Hidden; public double Lr; public int Epochs; public double ValMse; }

        public Action<string> Log;      // receives text output 
        public CurveHandler Curve;      // receives learning-curve points 
        public string OutputDir = ".";  // folder for training_log.csv and predictions.csv 
        public volatile bool CancelRequested; 

        // sends a message to the Log callback if one is attached.
        private void Say(string msg) { if (Log != null) Log(msg); }

        // runs the whole workflow. If autoTune is true, the best
        // hidden size / learning rate / epoch count is found via grid search; otherwise the
        // supplied manual values are used. Returns null if the run is cancelled
        public PipelineResult Run(string csvPath, bool autoTune, int hidden, double lr, int epochs)
        {
            // 1. load data
            int skipped;
            List<Sample> all = DataLoader.Load(csvPath, out skipped);
            Say(string.Format("Loaded {0} rows ({1} skipped for missing/invalid values).", all.Count, skipped));
            if (all.Count < 20) throw new InvalidDataException("Not enough valid rows.");

            // 2. shuffle once (seeded) and split into test / validation / train sets.
            //    test set is held out and only touched at the very end.
            Trainer.Shuffle(all, new Random(Seed));
            int nTest = (int)(all.Count * TestFraction);
            List<Sample> test = all.Take(nTest).ToList();
            List<Sample> trainAll = all.Skip(nTest).ToList();          // train + validation combined
            int nVal = (int)(trainAll.Count * ValFraction);
            List<Sample> val = trainAll.Take(nVal).ToList();           // used only to compare configs
            List<Sample> tuneTrain = trainAll.Skip(nVal).ToList();     // used to train during tuning
            Say(string.Format("Split -> train: {0}, validation: {1}, test: {2}", tuneTrain.Count, val.Count, test.Count));
            Say("");

            // 3. choose hyperparameters (grid search or manual values)
            Config cfg;
            if (autoTune)
            {
                cfg = Tune(tuneTrain, val);
                if (cfg == null) return null;   // cancelled during tuning
                Say("");
                Say(string.Format("Best config: hidden={0}, lr={1}, epochs={2} (val MSE {3:F6})",
                    cfg.Hidden, cfg.Lr, cfg.Epochs, cfg.ValMse));
            }
            else
            {
                cfg = new Config { Hidden = hidden, Lr = lr, Epochs = epochs };
                Say(string.Format("Manual config: hidden={0}, lr={1}, epochs={2}", hidden, lr, epochs));
            }

            // 4. retrain from scratch on train+validation with the chosen config, then evaluate on test
            Say("Training on train+validation...");
            Say("");
            return FinalTrainAndEvaluate(trainAll, test, cfg);
        }

        // grid search over hidden size and learning rate. Each network is trained once up to the
        // largest epoch checkpoint, and validation MSE is recorded at each checkpoint.
        // returns the configuration with the lowest validation MSE, or null if cancelled
        private Config Tune(List<Sample> train, List<Sample> val)
        {
            // fit the scaler on the training portion only
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

                        // record validation error only at the checkpoint epochs
                        if (EpochCheckpoints.Contains(e))
                        {
                            double m = Trainer.Mse(net, va);
                            results.Add(new Config { Hidden = h, Lr = lr, Epochs = e, ValMse = m });
                            Say(string.Format("{0,8} {1,6} {2,8} {3,12:F6}", h, lr, e, m));
                        }
                    }
                }
            }
            // pick the config with the smallest validation error
            return results.OrderBy(r => r.ValMse).First();
        }

        // trains a final network on all non-test data with the chosen config, logs the learning
        // curve, then evaluates on the held-out test set and writes predictions to disk
        // returns null if cancelled
        private PipelineResult FinalTrainAndEvaluate(List<Sample> trainAll, List<Sample> test, Config cfg)
        {
            // refit the scaler on train+validation (still never on test data).
            MinMaxScaler scaler = new MinMaxScaler();
            scaler.Fit(trainAll);
            ScaledSet tr = scaler.Transform(trainAll);
            ScaledSet te = scaler.Transform(test);

            NeuralNet net = Trainer.Create(tr.X[0].Length, cfg.Hidden, cfg.Lr);
            Random rng = new Random(Seed);
            int[] order = Enumerable.Range(0, tr.X.Length).ToArray();

            Say("=== Training log ===");
            Say(string.Format("{0,8} {1,14} {2,14}", "epoch", "train MSE", "test MSE"));

            // write per-epoch error to training_log.csv as training progresses.
            using (StreamWriter log = new StreamWriter(Path.Combine(OutputDir, "training_log.csv")))
            {
                log.WriteLine("epoch,train_mse,test_mse");
                for (int e = 0; e <= cfg.Epochs; e++)
                {
                    if (CancelRequested) { Say("Cancelled."); return null; }
                    if (e > 0) Trainer.TrainEpoch(net, tr, order, rng);   // epoch 0 = untrained baseline

                    bool doLog = (e % LogEvery == 0);
                    bool doCurve = (e % CurveEvery == 0);
                    if (doLog || doCurve)
                    {
                        // MSE is only computed on the epochs where something needs it
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

            // evaluation accumulators (all in dollars):
            //   sse     = sum of squared errors of the model
            //   sae     = sum of absolute errors
            //   sst     = total variance of test targets around the test mean (for R^2)
            //   sseBase = squared errors of a naive model that always predicts the training mean
            double sse = 0, sae = 0, sst = 0, sseBase = 0;
            double meanTrain = trainAll.Average(s => s.Target);
            double meanTest = test.Average(s => s.Target);

            // predict every test sample, convert back to dollars, and save actual vs. predicted
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

            // report final test metrics
            int n = te.X.Length;
            Say("");
            Say(string.Format("=== Test-set evaluation ({0} samples) ===", n));
            Say(string.Format("MSE  (scaled 0-1)   : {0:F6}", Trainer.Mse(net, te)));
            Say(string.Format("MSE  (dollars^2)    : {0:F2}", sse / n));
            Say(string.Format("RMSE (dollars)      : {0:F2}", Math.Sqrt(sse / n)));
            Say(string.Format("MAE  (dollars)      : {0:F2}", sae / n));
            Say(string.Format("R^2                 : {0:F4}", 1.0 - sse / sst));   // 1 = perfect, 0 = no better than mean
            Say(string.Format("Baseline RMSE (predict train mean): {0:F2}", Math.Sqrt(sseBase / n)));
            Say("");
            Say("Saved training_log.csv and predictions.csv in: " + Path.GetFullPath(OutputDir));

            // return the network plus scaler so callers can make new predictions
            return new PipelineResult { Net = net, Scaler = scaler };
        }
    }
}
<div align="center">

# Insurance Charges Prediction
### Regression with a Custom Multi-Layer Perceptron in C#

A from-scratch 3-layer neural network (input, hidden, output) with backpropagation, trained to predict individual medical insurance charges.

![Language](https://img.shields.io/badge/language-C%23-239120)
![Framework](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4)
![Task](https://img.shields.io/badge/task-regression-blue)

</div>

---

## Contents

1. [About the Project](#about-the-project)
2. [Dataset](#dataset)
3. [Preprocessing Pipeline](#preprocessing-pipeline)
4. [Network Architecture and Backpropagation](#network-architecture-and-backpropagation)
5. [Getting Started](#getting-started)
6. [Training Log](#training-log)
7. [Hyperparameter Tuning](#hyperparameter-tuning)
8. [Results and Evaluation](#results-and-evaluation)
9. [Project Structure](#project-structure)
10. [Compatibility Notes](#compatibility-notes)
11. [Authors](#authors)
12. [Acknowledgements](#acknowledgements)

---

## About the Project

<!-- 2-4 sentences: what you built, why, and the headline result. -->

This project applies our custom C# neural network engine (the `Backprop` library: `INeuron`, `HNeuron`, `ONeuron`, `NeuralNet`) to a real-world regression problem: predicting a person's yearly medical insurance **charges** from demographic and lifestyle attributes.

On a held-out test set, the best model reached **TODO: RMSE $___, MAE $___, R² ___**, compared with a mean-prediction baseline of **TODO: RMSE $___**.

<!-- TODO: add a screenshot of the running app -->
<div align="center">
  <img src="screenshots/app.png" alt="Application screenshot" width="700" />
</div>

**Task type:** Regression (single continuous output).

---

## Dataset

<!-- Deliverable 3: brief explanation of the data used. -->

**File:** `insurance.csv`  
**Source:** TODO: cite where you obtained it (for example the "Medical Cost Personal Datasets" insurance data on Kaggle) and add the link.  
**Size:** 1,338 records, 7 columns, no missing values.

| Column | Type | Description |
|---|---|---|
| `age` | integer | Age of the primary beneficiary (18-64) |
| `sex` | categorical | `male` or `female` |
| `bmi` | decimal | Body mass index |
| `children` | integer | Number of dependents covered (0-5) |
| `smoker` | categorical | `yes` or `no` |
| `region` | categorical | `northeast`, `northwest`, `southeast`, `southwest` |
| **`charges`** | decimal | **Target**: individual medical costs billed by insurance |

**Observations about the data** (computed on the full dataset):

- `charges` is right-skewed (skewness of about 1.5): most people have moderate charges and a few have very large ones.
- `smoker` is by far the strongest single predictor (correlation with `charges` of about 0.79); `age` and `bmi` are weaker.
- TODO: add anything else you noticed (for example the charge range of $1,121.87 to $63,770.43).

---

## Preprocessing Pipeline

<!-- Rubric 2. Explain each step AND why it is done. -->

1. **Parsing:** `CSVLoader` reads the CSV with `CultureInfo.InvariantCulture` and `TryParse`. Malformed or incomplete rows are skipped and counted (TODO: report the count, expected 0 for this file).
2. **Split:** the records are shuffled with a fixed seed (Fisher-Yates) and divided into:

   | Set | Share | Rows | Purpose |
   |---|---|---|---|
   | Training | 70% | 936 | Fit the weights |
   | Validation | 10% | 133 | Compare hyperparameters |
   | Test | 20% | 269 | Final, untouched evaluation |

   TODO: one sentence on why you chose this split (the test set keeps the common 80/20 train/test proportion, and the validation slice is carved from the 80% so tuning never touches the test data).
3. **Encoding:** `sex` and `smoker` become 0/1 flags; `region` is one-hot encoded into four columns (no artificial ordering). This gives **9 input features**.
4. **Scaling:** Min-Max scaling to [0, 1], `x' = (x - min) / (max - min)`, applied to the features and to the target. The scaler is **fitted on the training set only** and then applied to validation and test data, which prevents data leakage.
5. **Inverse transform:** predictions are converted back to dollars before any metric is computed, so all reported errors are in dollars.

---

## Network Architecture and Backpropagation

<!-- Rubric 1. Show you understand the math, not just the code. -->

```
Input (9)  ->  Hidden (H, sigmoid)  ->  Output (1, sigmoid)
```

- **Output activation:** sigmoid, which is why the target is scaled to [0, 1].
- **Training:** per-sample stochastic gradient descent; the training rows are shuffled every epoch.
- **Loss:** mean squared error (MSE), computed on the scaled values each epoch.
- **Backpropagation (as implemented in `NeuralNet.learn()`):**
  - Output error component: `e = desired - output`
  - Output delta: `delta_out = output * (1 - output) * e`
  - Hidden error: `err_h = (sum of delta_out * weight_h_to_out) * h * (1 - h)`
  - Weight updates: `w += learningRate * activation * delta` for both layers, plus the same rule for the biases. Hidden errors are computed **before** any weights are updated.

### Modifications to the provided library

The provided classes were kept as-is except for these minimal changes in `NeuralNetwork/`:

| File | Change | Reason |
|---|---|---|
| `BackNeural.cs` | `LRPOUT` and `LRPIN` changed from `const` to regular fields; added the constructor `NeuralNet(input, hidden, output, lrOut, lrIn, seed)` | Learning rates must be tunable; reproducible runs |
| `InputN.cs`, `HiddenN.cs`, `OuputN.cs` | Added `static SetSeed(int seed)` | Reproducible random weight initialisation |

The original constructors and default behavior are unchanged.

---

## Getting Started

### Prerequisites

- Windows with **Visual Studio 2019 or later**
- **.NET Framework 4.8** (the code also avoids APIs newer than .NET 3.5; see [Compatibility Notes](#compatibility-notes))

### Run

```bash
git clone TODO-your-repository-url
cd InsuranceMLP
```

1. Open `InsuranceMLP.sln` in Visual Studio.
2. Build the solution (`Ctrl+Shift+B`). `insurance.csv` is copied next to the executable automatically.
3. Run (`F5`).
4. TODO: describe the buttons / steps in your UI (for example: set hyperparameters, click **Train**, read the log, view the loss curve and test metrics).

### Output files

| File | Content |
|---|---|
| `training_log.csv` | `epoch, trainMSE, validationMSE`, logged every TODO epochs |
| `experiments.csv` | One row per hyperparameter configuration with its validation error |

---

## Training Log

<!-- Deliverable 2: show the loss decreasing across epochs. -->

Loss is printed every TODO epochs and exported to `training_log.csv`. Excerpt from the final run:

```
Epoch    Train MSE    Validation MSE
TODO     TODO         TODO
TODO     TODO         TODO
TODO     TODO         TODO
```

<div align="center">
  <img src="screenshots/loss_curve.png" alt="Training and validation loss curve" width="600" />
</div>

TODO: 2-3 sentences interpreting the curve (does the loss still fall? does validation start rising, which would indicate overfitting? at which epoch is the best validation loss?).

---

## Hyperparameter Tuning

<!-- Rubric 3. Show a systematic search AND explain the winner. -->

Every configuration was trained **TODO: 3** times with different seeds and compared by mean **validation** MSE (the test set was not used for tuning). Full results are in `experiments.csv`.

| Hidden nodes | Learning rate (out / in) | Epochs | Val MSE (mean) | Val MSE (std) |
|---|---|---|---|---|
| TODO | TODO | TODO | TODO | TODO |
| TODO | TODO | TODO | TODO | TODO |

**Search space:** hidden nodes {4, 8, 16, 32}; learning rate {0.01, 0.05, 0.1, 0.3} (TODO: adjust to what you ran); epochs chosen from the best validation point in each run.

**Chosen configuration:** TODO hidden nodes, learning rate TODO, TODO epochs.

**Why it won:** TODO: explain in your own words, for example what happened when the learning rate was too small (slow convergence), too large (unstable loss), or when there were too many hidden nodes (overfitting or no gain).

---

## Results and Evaluation

<!-- Rubric 4. Metrics in dollars on the TEST set, a baseline, and real interpretation. -->

Final model evaluated once on the **test set** (269 rows, never used for fitting or tuning):

| Model | RMSE ($) | MAE ($) | R² |
|---|---|---|---|
| Baseline: always predict the training mean | TODO | TODO | TODO |
| **MLP (ours)** | TODO | TODO | TODO |

**Metric definitions:** MSE is the mean squared error, RMSE its square root (same units as charges), MAE the mean absolute error, and R² the share of variance in `charges` explained by the model.

**Sample predictions:**

| Actual ($) | Predicted ($) | Error ($) |
|---|---|---|
| TODO | TODO | TODO |

**Analysis:** TODO. Suggested points:

- How much better is the MLP than the baseline?
- Error for smokers vs non-smokers: where does the model struggle most?
- Do the largest charges get under-predicted? (A sigmoid output and a skewed target often cause this.)
- Limitations (per-sample SGD only, sigmoid-bounded output, small dataset) and what you would try next.

---

## Project Structure

```
InsuranceMLP/
├── NeuralNetwork/        Provided Backprop library (minimal documented edits)
│   ├── BackNeural.cs
│   ├── InputN.cs
│   ├── HiddenN.cs
│   └── OuputN.cs
├── Data/                 CSV parsing, encoding, scaling, splitting
│   ├── InsuranceRecord.cs
│   ├── CSVLoader.cs
│   ├── FeatureEncoder.cs
│   ├── MinMaxScaler.cs
│   ├── TargetScaler.cs
│   └── DatasetSplitter.cs
├── Training/             TODO: Trainer, ExperimentRunner
├── Evaluation/           TODO: Metrics
├── Form1.cs              WinForms user interface
├── insurance.csv
├── training_log.csv
├── experiments.csv
└── README.md
```

<!-- Update this tree to match your final files. -->

---

## Compatibility Notes

- Built and tested on **.NET Framework 4.8** (WinForms).
- The code deliberately sticks to .NET 3.5-compatible APIs (no `Tuple`, `Task`, `File.ReadLines`, `string.IsNullOrWhiteSpace`, or optional parameters).
- The loss curve is drawn with GDI+ in a `Panel` instead of the `Chart` control, because `Chart` is not part of .NET 3.5.
- No third-party libraries are used.

---

## Authors

- Caballero, Vonn Vincent S. 
- Sabellita, Rea Emerald 

Course: CS345 - INTELLIGENT SYSTEMS 1

---

## Acknowledgements

- Dataset: TODO (source and link)
- Neural network classes provided by our course instructor (`Backprop` library)
- Random weight initialisation, shuffling, and metric formulas follow standard textbook definitions (TODO: add any references you used)

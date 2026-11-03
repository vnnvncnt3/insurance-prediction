<div align="center">

# Insurance Charges Prediction
### Regression with a Custom Multi-Layer Perceptron in C#

A from-scratch 3-layer neural network (input, hidden, output) with backpropagation, trained to predict individual medical insurance charges.

![Language](https://img.shields.io/badge/language-C%23-239120)
![Framework](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4)
![Task](https://img.shields.io/badge/task-regression-blue)

</div>

---

## About the Project

This project applies our custom C# neural network engine (the `Backprop` library: `INeuron`, `HNeuron`, `ONeuron`, `NeuralNet`) to a real-world regression problem: predicting a person's yearly medical insurance **charges** from demographic and lifestyle attributes. It is a Windows Forms app that loads the CSV, tunes and trains the network, plots the loss curve, reports test-set metrics and predicts charges for a new person.
 
On a held-out test set of 267 records, the model reached **RMSE $4,586, MAE $2,705 and R² 0.847**, compared with a mean-prediction baseline of **RMSE $11,824, MAE $9,007 and R² ≈ 0**.

<!-- TODO: add a screenshot of the running app -->
<div align="center">
  <img src="screenshots/app.png" alt="Application screenshot" width="700" />
</div>

**Task type:** Regression (single continuous output).

---

## Dataset

**File:** `insurance.csv`  
**Source:** "Healthcare Insurance Datasets", https://www.kaggle.com/datasets/willianoliveiragibin/healthcare-insurance/data  
**Size:** 1,338 records, 7 columns, no missing values.

| Column | Type | Description | Used as input |
|---|---|---|---|
| `age` | integer | Age of the primary beneficiary (18-64) | Yes |
| `sex` | categorical | `male` or `female` | Yes |
| `bmi` | decimal | Body mass index | Yes |
| `children` | integer | Number of dependents covered (0-5) | Yes |
| `smoker` | categorical | `yes` or `no` | Yes |
| `region` | categorical | `northeast`, `northwest`, `southeast`, `southwest` | No |
| **`charges`** | decimal | **Target**: individual medical costs billed by insurance | Target |
 
- `charges` is right-skewed (skewness of about 1.5), ranging from $1,121.87 to $63,770.43 with a mean of about $13,270.
- `smoker` is the strongest single predictor (correlation with `charges` of about 0.79). About 20% of the people (274) are smokers.

---

## Preprocessing Pipeline

1. **Parsing:** `DataLoader` finds the columns by header name and skips rows with a missing or invalid cell (the skipped count is printed in the app log; expected 0 for this file).
2. **Split:** the records are shuffled with a fixed seed (`Random(42)`) and divided into test 267 (20%), validation 214 (20% of the remaining 1,071) and training 857. After tuning, the final model is **retrained on training + validation (1,071 rows)** and evaluated once on the test set. <!--TODO: one sentence on why you chose this split.-->
3. **Encoding:** `sex` and `smoker` become 0/1 flags, giving **5 input features**: age, sex, bmi, children, smoker.
4. **Scaling:** Min-Max scaling to [0, 1], `x' = (x - min) / (max - min)`, applied to the features and the target. The scaler is **fitted on the training data only**, which prevents data leakage.
5. **Inverse transform:** predictions are converted back to dollars before any metric is computed.

---

## Network Architecture and Backpropagation

```
Input (5)  ->  Hidden (H, sigmoid)  ->  Output (1, sigmoid)
```
 
- **Output activation:** sigmoid, which is why the target is scaled to [0, 1].
- **Training:** per-sample stochastic gradient descent, shuffling the training rows every epoch. **Loss:** MSE on the scaled values.
- **Backpropagation (`NeuralNet.learn()`):**
  - Output error: `e = desired - output`
  - Output delta: `delta_out = output * (1 - output) * e`
  - Hidden error: `err_h = (sum of delta_out * weight_h_to_out) * h * (1 - h)`
  - Updates: `w += learningRate * activation * delta` for both layers and the biases. Hidden errors are computed **before** any weights change.
**Library modifications:** <!--TODO: list any edits to the provided classes (for example learning rates changed from `const` to fields, seeded weight initialisation), or write "used as-is".-->

---
 
## Getting Started
 
- Windows with Visual Studio; the project targets **.NET Framework 3.5** (WinForms, no third-party libraries).
```bash
git clone https://github.com/vnnvncnt3/insurance-prediction.git
```
 
1. Open `HealthcareInsuranceFinal.slnx` in Visual Studio, build (`Ctrl+Shift+B`) and run (`F5`).
2. Tick **Auto-tune** to search the hyperparameter grid, or set hidden neurons, learning rate and epochs yourself. Click **Run** (**Cancel** stops it).
3. Read the log and loss curve, then use the **Predict** panel to estimate charges for a new person.
Output files in `bin\Debug`: `training_log.csv` (`epoch, train_mse, test_mse`, every 100 epochs) and `predictions.csv` (`actual, predicted` for each test row).
 
---
 
## Training Log
 
Loss is printed every 100 epochs and exported to `training_log.csv`. Excerpt from the final run:
 
```
epoch    train_mse    test_mse
0        0.182224     0.197050
100      0.006412     0.006434
500      0.005771     0.005383
1000     0.005742     0.005359
```
 
<div align="center">
  <img src="assets/loss_curve.png" alt="Loss curve" width="600" />
</div>
The loss drops sharply in the first 100 epochs and flattens from about epoch 200, with no sign of overfitting. The test loss is only monitored; it is not used to choose hyperparameters.
 
---
 
## Hyperparameter Tuning
 
Auto-tune trains each combination on the 857 training rows and compares them by **validation** MSE (the test set is not used). **Search space:** hidden neurons {4, 8, 16}; learning rate {0.05, 0.1, 0.2, 0.4}; epochs {200, 500, 1000}.
 
| Hidden neurons | Learning rate | Epochs | Validation MSE |
|---|---|---|---|
| <!--TODO--> | <!--TODO--> | <!--TODO--> | <!--TODO--> |
 
**Chosen configuration:** <!--TODO-->. **Why it won:** <!--TODO, in your own words.-->
 
---
 
## Results and Evaluation
 
Final model evaluated once on the **test set** (267 rows, never used for fitting or tuning):
 
| Model | RMSE ($) | MAE ($) | R² |
|---|---|---|---|
| Baseline: always predict the training mean | 11,823.67 | 9,006.87 | -0.015 |
| **MLP (ours)** | **4,586.08** | **2,705.46** | **0.8474** |
 
RMSE is the square root of the mean squared error (same units as charges), MAE the mean absolute error, and R² the share of variance in `charges` explained by the model.
 
**Analysis:** <!--TODO. Suggested points: how much better the MLP is than the baseline; smokers vs non-smokers (the model errs more on smokers: RMSE about $6,120 vs $4,237); under-prediction of the largest charges; limitations and next steps.-->
 
---
 
## Project Structure

```
<!--TODO-->

```

---

## Authors

- Caballero, Vonn Vincent S. 
- Sabellita, Rea Emerald 

Course: CS345 - INTELLIGENT SYSTEMS 1

---

## Acknowledgements

- Dataset: Healthcare Insurance — https://www.kaggle.com/datasets/willianoliveiragibin/healthcare-insurance/data
- Neural network classes provided by our course instructor (`Backprop` library)

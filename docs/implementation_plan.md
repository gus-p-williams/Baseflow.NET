# Baseflow.NET (.NET 8 WPF) Implementation Plan

## Goal
Create a standalone Windows Desktop Application using .NET 8 and WPF to perform baseflow separation analysis. This is a port of the 'baseflow' Python library.

## User Review Required
> [!NOTE]
> **Algorithm Details**: I have accessed the original Python source code from the [BYU-Hydroinformatics baseflow repository](https://github.com/BYU-Hydroinformatics/baseflow) and will port the algorithms directly to C# to ensure accuracy.

## Proposed Changes

### 1. Project Scaffolding
- **Solution Structure**:
  - `Baseflow.sln`
  - `Baseflow.App/` (WPF Project)
- **Dependencies**:
  - `ScottPlot.WPF` (Charting)
  - `CsvHelper` (CSV I/O)

### 2. Core Logic (`Baseflow.App/Engine`)
- **`BaseflowCalculator` Class**:
  - Input: `List<StreamflowRecord>` (Date, Discharge Q)
  - Output: `List<BaseflowResult>` (Date, Baseflow b)
  - **Methods to Implement**:
    - `CalculateUKIH` (United Kingdom Institute of Hydrology)
    - `CalculateLocalMin`
    - `CalculateFixedInterval`
    - `CalculateSlidingInterval`
    - `CalculateLyneHollick` (LH)
    - `CalculateChapman`
    - `CalculateCM`
    - `CalculateBoughton`
    - `CalculateFurey`
    - `CalculateEckhardt`
    - `CalculateEWMA`
    - `CalculateWillems`

### 3. User Interface (`Baseflow.App/MainWindow.xaml`)
- **Layout**:
  - **Top Bar**: "Load CSV" button, File path display.
  - **Left Sidebar**: List of Algorithm Checkboxes. "Run Analysis" button. "Export Results" button.
  - **Center**: ScottPlot `WpfPlot` control showing Streamflow (Line) and calculated Baseflows (Lines).
- **Interactions**:
  - Loading CSV parses data using `CsvHelper`.
  - Checking algorithms enables them for the "Run" command.
  - "Run" executes selected methods and updates the Plot.

## Verification Plan
### Automated Tests
- Create a simple unit test project `Baseflow.Tests` (Optional but recommended for engines) or just manual verification for now as per instructions.

### Manual Verification
1. **Launch App**: Verify window opens.
2. **Load Data**: Load a sample CSV (need to generate a dummy one).
3. **Run Algorithms**: Select "Lyne-Hollick" and "Eckhardt", run, verify graph updates.
4. **Export**: Save results, check CSV content.

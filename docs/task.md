# Baseflow.NET Development Tasks

- [x] **1. Project Initialization** <!-- id: 1 -->
    - [x] Run `git init` <!-- id: 1.1 -->
    - [x] Create WPF Project `Baseflow.App` <!-- id: 1.2 -->
    - [x] Add NuGet packages: `ScottPlot.WPF`, `CsvHelper` <!-- id: 1.3 -->
    - [x] Create Solution file and add project <!-- id: 1.4 -->

- [x] **2. Core Logic Implementation (The 'Engine')** <!-- id: 2 -->
    - [x] **Analyze Python Source** <!-- id: 2.1 -->
    - [x] Define Data Models (`StreamflowData`, `BaseflowResult`) <!-- id: 2.2 -->
    - [x] Implement Parsing Logic (Read CSV) <!-- id: 2.3 -->
    - [x] Port Recursive Separation Methods: <!-- id: 2.4 -->
        - [x] UKIH
        - [x] Local Minimum
        - [x] Fixed Interval
        - [x] Sliding Interval
        - [x] Lyne-Hollick (LH)
        - [x] Chapman
        - [x] CM
        - [x] Boughton
        - [x] Furey
        - [x] Eckhardt
        - [x] EWMA
        - [x] Willems

- [x] **3. User Interface (WPF)** <!-- id: 3 -->
    - [x] Design Main Layout (Grid/DockPanel) <!-- id: 3.1 -->
    - [x] File Selection Control <!-- id: 3.3 -->
    - [x] Method Selection <!-- id: 3.4 -->
    - [x] ScottPlot Integration <!-- id: 3.5 -->
    - [x] Export Functionality <!-- id: 3.6 -->

- [x] **4. GitHub Integration** <!-- id: 4 -->
    - [x] Initialize Repository locally <!-- id: 4.1 -->
    - [x] Create Remote Repository (`gh repo create` or manual) <!-- id: 4.2 -->
    - [x] Push initial code <!-- id: 4.3 -->

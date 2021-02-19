﻿# What are the file used for 

| folder | usage | 
| --- | --- |
| DataAsset/[city] | All raw data files, sorted by city|
| DataProcessing/Generic | Framework to inherit from when adding a new data |
| DataProcessing/[data-name] | Contains the 3 files needed to implement a new data |


# How to develop

In order to add a new data in the visuals : 

- Start by adding the raw source file in ```/DataAsset/[city-of-data]```
- Add those three file to the foled ```/DataAsset/[data-name]``` :
    1. **[data-name]Data.cs** : Modelisation of raw data in C#
    2. **[data-name]DataReader.cs** : Class to get data from raw file. It must inherit ```Generic/DataReader```
    3. **[data-name]DataManager.cs** : Hold meta data, as limitation , scalling, global properties. It also provide the access to the dataset to the visual framework. It must inherit ```Generic/DataManager```
- Create a visual to display ```/Visual/[script-of-visual]```
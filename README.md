# Project Folder Organisation

| folder | usage | 
| --- | --- |
| DataAsset/[city] | All raw data files, sorted by city|
| DataProcessing/Generic | Framework to inherit from when adding a new data |
| DataProcessing/[data-name] | Contains the 3 files needed to implement a new data |
| Bounds/ | Bounds to register during conversion for knowing how to scale things relative to the drawn data |
| Tools/ | one-liners and other specific algorithms that needed to be put aside for the sake of clarity and honoring GRASP principles |

# General Development Procedure

In order to add a new data in the visuals : 

- Start by adding the raw source file in ```/DataAsset/[city-of-data]```
- Add those three file to the foled ```/DataAsset/[data-name]``` :
    1. **[data-name]Data.cs** : Modelisation of raw data in C#
    2. **[data-name]DataReader.cs** : Class to get data from raw file. It must inherit ```Generic/DataReader```
    3. **[data-name]DataConverter.cs** : Hold meta data, as limitation , scalling, global properties. It also provide the access to the dataset to the visual framework. It must inherit ```Generic/DataConverter```
- Create a visual to display ```/Visual/[script-of-visual]``` 
    1. Import your datas from DataConverter.cs 
    2. Enrich everything you want from the Unity Engine and Editor


## Note on generation For RilData


Extrapolation Rate : 0.1

Disappearing rate : 0.01

=> 13 iterations to be all white

# KeyBindings


F1 = Toggle on screen logs

F2 = Toggle CityLine

F3 = Toggle Density display

F4 = Start/Restart Screencity display

F5 = Pause ScreenCity Display

<a rel="license" href="http://creativecommons.org/licenses/by-nc-nd/4.0/"><img alt="Creative Commons License" style="border-width:0" src="https://i.creativecommons.org/l/by-nc-nd/4.0/88x31.png" /></a><br />This work is licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-nc-nd/4.0/">Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International License</a>.

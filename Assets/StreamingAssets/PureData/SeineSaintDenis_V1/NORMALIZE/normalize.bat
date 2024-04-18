for %%i in ("res\*.wav") do (
    mkdir "res" 2>nul
	mkdir "res/nrm" 2>nul
    ffmpeg -i "%%i" -af loudnorm=I=-18:LRA=11:TP=-1.5:print_format=summary -ar 44100 -ac 2 "res\nrm\%%~ni.wav" -y
)

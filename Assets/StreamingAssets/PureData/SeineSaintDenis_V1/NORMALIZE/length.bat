for %%i in ("res/*.wav") do (ffmpeg -i "%%i" -t 120 "res/%%~ni.wav" -y)

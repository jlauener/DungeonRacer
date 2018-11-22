#!/bin/sh

cd SDR.app/Contents/MacOS/
mkbundle -o RunSDR --simple SDR.exe
rm *.dll
rm SDR.exe
chmod +x SDR

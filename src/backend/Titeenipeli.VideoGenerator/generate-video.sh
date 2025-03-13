#!/bin/bash

ffmpeg -framerate "$1" -pattern_type glob -i "$2"'/*.png' -q:v 3 -c:v libx264 -pix_fmt yuv420p -an "$3"
#!/usr/bin/env python3
import subprocess
import glob
import itertools

files = ["script", "*.json", "*.xml", "thumbnail.png"]
mod_name = "petrakat_yank"

coq_dir = "/home/petrak/.config/unity3d/Freehold Games/CavesOfQud/Mods/"

# ===

files = [glob.glob(f) for f in files]
subprocess.call(["rm", "-r", coq_dir + mod_name])
subprocess.call(["mkdir", coq_dir + mod_name])
subprocess.call(
    ["cp", "-r"] + list(itertools.chain(*files)) + [coq_dir + mod_name])

(use cbt)

(use ./src/object-blueprints)
(use ./src/population-tables)

(build-metadata
  :qud-dlls "/home/petrak/.local/share/Steam/steamapps/common/Caves of Qud/CoQ_Data/Managed/"
  :qud-mods-folder "/home/petrak/.config/unity3d/Freehold Games/CavesOfQud/Mods/")

(declare-mod
  "yank"
  "Yank!"
  "petrak@"
  "0.2.0"
  :steam-id 3014096020
  :steam-visibility "2")

(generate-xml "ObjectBlueprints.xml" object-blueprints)
(generate-xml "PopulationTables.xml" population-tables)

(set-debug-output true)

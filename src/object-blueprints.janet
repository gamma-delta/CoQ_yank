(use cbt/xml-helpers/objects)
(use cbt/colors)

(defn object-blueprints []
  [:objects
   # We modify the base recoiler to be a yank instead! Since inheritance propagates these changes downstream,
   # and we patch the inside of Teleporter to cooperate with YankMod, for the most part this should Just Work.
   (alter-object "BaseRecoiler"
                 (description "A fragile crystalline disk with a recessed button on top. It shivers in your hands.")
                 # We attach our custom Yank part, which changes the name and forces the yank to be tinkerable. 
                 (part "PKYNK_YankMod" :ReplaceName true :MakeAlwaysTinkerable true)
                 # Yanks never cost any energy... 
                 (part "Teleporter" :ChargeUse 0)
                 [:tag {:Name "ExcludeFromDynamicEncounters" :Value "*noinherit"}]
                 # ...so we remove the cell and socket too. 
                 [:removepart {:Name "EnergyCell"}]
                 [:removepart {:Name "EnergyCellSocket"}])
   (object "pkynk_geofoldyank" "BaseRecoiler"
           (part "Render" :DisplayName "{{g|geofold}} yank")
           (part "PKYNK_GeofoldYank")
           (commerce 20)
           (tag :Tier 1))
   # We edit the vanilla recoilers; since they inherit BaseRecoiler,
   #  there's not much to do, but we currently make them much cheaper. (Petra: Change this, probably.) 
   # THIS is the kind of stuff I made CBT for
   # yes yes YES oh it's so much less horrible   
   ;(seq [[name value] :in [["Joppa" 40]
                            ["Grit Gate" 60]
                            ["Golgotha" 50]
                            ["Kyakukya" 60]
                            ["Six Day Stilt" 50]
                            ["Bethesda Susa" 70]
                            ["Ezra" 70]
                            ["Yd Freehold" 100]]]
      (alter-object (string name " Recoiler")
                    (commerce value)))
   # apparently random-point recoilers "Just Work"
   (alter-object "Programmable Recoiler"
                 (commerce 100)
                 (part "ProgrammableRecoiler" :ChargeUse 0))

   # True kin are not spared: OnboardRecoiler gets the treatment too. We give it five shots before it fizzles. 
   (alter-object "OnboardRecoiler"
                 (part :CyberneticsBaseItem :BehaviorDescription "You can imprint this implant with a location (cooldown 100) and yank yourself back to it (cooldown 50), up to five times.")
                 # We attach our custom Yank part, with different params:
                 # don't make it tinkerable, but it's a 5-shot and displays as such
                 (part :PKYNK_YankMod :ReplaceName true :NumUses 5 :ModifyDisplay true))

   # given recoilers are all single-use there's no purpose for the repro one
   # so i get to keep my cislocational curiosity
   [:object {:Name "Reprogrammable Recoiler"}
    (render "Items/sw_recoiler.bmp" "cislocational curiosity" white purple :cp437 (utf8->cp437 "♦"))
    (part "Physics" :Category "Trinket")
    (description "Before the world was spun into being, two creators sought to excise from it the convenience of instantaneous transportation. But the weft and warp of the world resisted them, and small anomalies remain. This is one such anomaly; although it no longer functions, it may be sold or disassembled.")
    (part "TinkerItem" :Bits "3456788" :CanDisassemble true :CanBuild false)
    (tag "Mods" "None")
    (tag "Storied" :true)
    [:property {:Name "Story" :Value "pkynk_cislocational"}]
    (tag "BaseObject" "*noinherit")]])

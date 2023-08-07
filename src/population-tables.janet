(use cbt/xml-helpers/population-tables)
(use cbt/colors)

(defn population-tables []
  [:populations
   ;(seq [[name weight roll] :in [[:0C 1 "1"]
                                  [:OR 5 "1d2"]
                                  [:1C 5 "1d2"]
                                  [:2C 1 "1d2"]]]
      (alter-population (string "Artifact " name)
                        (items
                          (object-one :pkynk_geofoldyank roll weight))))])

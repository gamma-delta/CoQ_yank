using System;
using XRL.UI;

namespace XRL.World.Parts {

    [Serializable]
    public class PKYNK_GeofoldYank : IPart
    {
        public override void Register(GameObject obj) {
            obj.RegisterPartEvent(this, "TeleportPreflight");
            base.Register(obj);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "TeleportPreflight" && ParentObject.GetPart<Teleporter>() is Teleporter tp)
            {
                var who = E.GetParameter<GameObject>("Who");

                if (who.CurrentZone.Z == 10) {
                    if (who.IsPlayer()) {
                        Popup.ShowFail("You feel a lurch in your stomach as you end up exactly where you were.");
                    }
                    return false;
                }

                tp.DestinationZone = ZoneID.Assemble(
                    who.CurrentZone.ZoneWorld,
                    who.CurrentZone.wX,
                    who.CurrentZone.wY,
                    who.CurrentZone.X,
                    who.CurrentZone.Y,
                    10
                );
            }
            return base.FireEvent(E);
        }
    }
}

using System;
using XRL.UI;
using XRL.Language;

namespace XRL.World.Parts {
  [Serializable]
  public class PKRCX_Yanker : IActivePart {
    public static readonly string ACTIVATE_KEY = "ActivateTeleporter";

    public string Destination;
    public int DestinationX = -1, DestinationY = -1;
    public string Sound = "sfx_ability_recoiler_use";

    public PKRCX_Yanker() {
      this.IsEMPSensitive = true;
      this.IsRealityDistortionBased = true;
      this.IsTechScannable = true;
      this.ChargeUse = 1;
      this.ChargeMinimum = 0;

      // Carrier = the owner of the inventory
      this.WorksOnCarrier = true;
      this.WorksOnHolder = true; // in case you equip it whoops
    }

    public override bool WantEvent(int ID, int cascade) {
      return ID == GetInventoryActionsEvent.ID
          || ID == InventoryActionEvent.ID
          || base.WantEvent(ID, cascade);
    }

    public override bool HandleEvent(GetInventoryActionsEvent E) {
      E.AddAction("Activate", "activate", ACTIVATE_KEY, Key: 'a', Default: 100);
      return base.HandleEvent(E);
    }


    public override bool HandleEvent(InventoryActionEvent e) {
      if (e.Command == ACTIVATE_KEY) {
        AttemptTeleport(e.Actor, e);
      }
      return base.HandleEvent(e);
    }

    // Mostly vanillacopy
    public bool AttemptTeleport(GameObject user, IEvent causingEvent = null) {
      if (user.AreHostilesNearby()) {
        if (user.IsPlayer()) {
          Popup.ShowFail("You can't yank with hostiles nearby!", true, true, true);
        }
        return false;
      }

      ActivePartStatus status = GetActivePartStatus();
      if (status != 0 || !IsObjectActivePartSubject(user)) {
        if (user.IsPlayer()) {
          // Give the player a message about what is going on
          if (status != ActivePartStatus.LocallyDefinedFailure || !(GetActivePartLocallyDefinedFailureDescription() == "ProtocolMismatch")) {
            if (status == ActivePartStatus.Rusted) {
              Popup.ShowFail(Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName) + " activation button is rusted in place.", true, true, true);
            } else if (status == ActivePartStatus.Broken) {
              Popup.ShowFail(ParentObject.Itis + " broken...", true, true, true);
            } else {
              Popup.ShowFail(String.Format("Nothing happens (due to {0}).", status), true, true, true);
            }
          } else {
            string a = GetAnyBasisZone()?.ResolveWorldBlueprint()?.Protocol;
            if (a == "THIN") {
              Popup.ShowFail("You have no bodily tether to yank.", true, true, true);
            } else if (a == "CLAM") {
              Popup.ShowFail("You are stuck in a remote pocket dimension and have nowhere to be yanked to.", true, true, true);
            } else {
              Popup.ShowFail("You cannot do that here.", true, true, true);
            }
          }
        }
        return false;
      }

      string destZone;
      int destX = -1, destY = -1;
      if (this.Destination == "@SURFACE") {
        // 10 is ground level; higher numbers are deeper
        if (user.CurrentZone.Z == 10) {
          Popup.ShowFail("You feel a lurch in your stomach as you end up exactly where you were.");
          return false;
        }
        // this is horrible how does anyone get things done
        // this isn't visual basic stop passing strings around
        destZone = ZoneID.Assemble(user.CurrentZone.ZoneWorld, user.CurrentZone.wX, user.CurrentZone.wY,
          user.CurrentZone.X, user.CurrentZone.Y, 10);
      } else {
        destZone = this.Destination;
        destX = this.DestinationX;
        destY = this.DestinationY;
      }

      // zoop!
      int powerLeft = ParentObject.QueryCharge(IncludeBiological: false);
      bool finalUse = powerLeft <= 1;
      bool worked = user.ZoneTeleport(destZone, destX, destY, causingEvent, ParentObject, user, SuccessMessage:
          finalUse
            ? "The yank shatters as you are yanked!"
            : "You are yanked!");
      if (!worked) {
        return false;
      }

      // woohoo!
      if (!Sound.IsNullOrEmpty()) {
        user.PlayWorldSound(Sound, 0.5f, 0f, false, 0f);
      }
      ParentObject.UseCharge(1, IncludeBiological: false);
      if (finalUse) {
        ParentObject.Destroy();
      }
      causingEvent?.RequestInterfaceExit();
      return true;
    }
  }
}
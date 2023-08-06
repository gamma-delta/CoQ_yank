using System;

namespace XRL.World.Parts {
    /// This class serves two roles:
    /// 1. Provide a flag noting that this object is a yank, so that it will get destroyed properly during a teleport.
    ///    (See the mixin subdir for details.)
    ///
    /// 2. Modify several of the other parts on this object at initialization.
    ///    Yes, we would normally do this in ObjectBlueprints.xml, but most recoilers have parameters set per-recoiler
    ///    that need to be changed for yanks-- e.g. their name, {{COLOR|PLACE recoiler}}.
    ///    In the vanilla ObjectBlueprints.xml, these changes are all made by hand; mods that add their own recoilers
    ///    presumably follow suit.
    ///    Attaching this part feels hackier, but it's likely to produce more consistent results.
    ///    (Individual mods who want other behavior can of course unset these flags in their own objects.)
    [Serializable]
    public class PKYNK_YankMod : IPart {
        public int NumUses = 1; // Consume on the very first use.
        public bool ModifyDisplay = false; // If set, show remaining uses in display name.
        public bool ReplaceName = false; // If set, replace 'recoiler' with 'yank' in the object name.
        public bool MakeAlwaysTinkerable = false; // If set, overwrite TinkerItem.CanBuild to be true.

        public override bool WantEvent(int ID, int cascade) =>
            ID == ObjectCreatedEvent.ID || (ModifyDisplay && ID == GetDisplayNameEvent.ID) || base.WantEvent(ID, cascade);
        
        public string NUses() {
            return NumUses + (NumUses == 1 ? " use" : " uses") + " left";
        }

        public override bool HandleEvent(GetDisplayNameEvent E) {
            if (ModifyDisplay && E.Understood()) {
                E.AddTag("[{{W|" + NUses() + "}}]");
            }
            return true;
        }

        public override bool HandleEvent(ObjectCreatedEvent E) {
            if (ReplaceName && ParentObject.GetPart<Render>() is Render render && render.DisplayName != null) {
                render.DisplayName = render.DisplayName.Replace("recoiler", "yank");
            }
            if (MakeAlwaysTinkerable && ParentObject.GetPart<TinkerItem>() is TinkerItem ti) {
                ti.CanBuild = true;
            }
            return true;
        }

        // Try to consume this yank. Return the message to display.
        public string ConsumeWithMessage() {
            NumUses--;
            if (NumUses <= 0) {
                var message = "The world turns as " + ParentObject.the + ParentObject.ShortDisplayName + " shatters!";
                ParentObject.Destroy("Yank shatters", true);
                return message;
            }
            return "You are yanked away! (" + NUses() + ")";
        }
    }
}

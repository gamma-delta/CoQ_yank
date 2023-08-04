using XRL.World;
using XRL.World.Parts;

namespace Petrak.Yank {
    public class YankMixinCallee {
        /// Handle a recoiler successfully activating:
        /// destroy the game object, and change the message
        /// to reflect the destruction.
        public static string ModifyMessageAndDestroy(string message, IPart self) {
            var ob = self.ParentObject;
            if (!ob.HasPart<PKYNK_YankMod>()) {
                // oh, we're not a yank! sorry to bother you
                return message;
            }
            ob.Destroy("Yank shatters", true);
            return "The world turns as the yank shatters!";
        }
    }
}

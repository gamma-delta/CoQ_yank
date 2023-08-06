using XRL.World;
using XRL.World.Parts;

namespace Petrak.Yank {
    public class YankMixinCallee {
        /// Handle a recoiler successfully activating:
        /// destroy the game object, and change the message
        /// to reflect the destruction.
        public static string ModifyMessageAndDestroy(string message, IPart self) {
            var ob = self.ParentObject;
            if (ob.GetPart<PKYNK_YankMod>() is PKYNK_YankMod yankMod) {
                return yankMod.ConsumeWithMessage();
            }
            // oh, we're not a yank! sorry to bother you
            return message;
        }
    }
}

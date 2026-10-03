using UnityGameFramework.Runtime;

namespace Monster
{
    public class BattleBackgroundUI : UIFormLogic
    {
        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            Log.Info("Battle Background Open");
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);

            Log.Info("Battle Background Close");
        }
    }
}
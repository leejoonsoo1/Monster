using GameFramework.Event;
using Monster;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{
    public abstract class GameBase
    {
        public virtual void Initialize()
        {
            GameOver = false;
        }

        public virtual void Shutdown()
        {

        }

        public virtual void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        protected virtual void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
        }

        protected virtual void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            ShowEntityFailureEventArgs ne = (ShowEntityFailureEventArgs)e;
            Log.Warning("Show entity failure with error message '{0}'.", ne.ErrorMessage);
        }

        public abstract GameMode GameMode
        {
            get;
        }

        public bool GameOver
        {
            get;
            protected set;
        }
    }
}
using GameFramework.Sound;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class SoundHelperBase : MonoBehaviour, ISoundHelper
    {
        public abstract void ReleaseSoundAsset(object soundAsset);
    }
}

using GameFramework.UI;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class UIGroupHelperBase : MonoBehaviour, IUIGroupHelper
    {
        public abstract void SetDepth(int depth);
    }
}

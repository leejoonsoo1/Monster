using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class UIIntKey : MonoBehaviour
    {
        [SerializeField]
        private int mKey = 0;

        public int Key
        {
            get
            {
                return mKey;
            }
            set
            {
                mKey = value;
            }
        }
    }
}

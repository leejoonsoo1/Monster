using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class UIStringKey : MonoBehaviour
    {
        [SerializeField]
        private string mKey = null;

        public string Key
        {
            get
            {
                return mKey ?? string.Empty;
            }
            set
            {
                mKey = value;
            }
        }
    }
}

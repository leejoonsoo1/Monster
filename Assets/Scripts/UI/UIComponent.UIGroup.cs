using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed partial class UIComponent : GameFrameworkComponent
    {
        [Serializable]
        private sealed class UIGroup
        {
            [SerializeField]
            private string mName = null;

            [SerializeField]
            private int mDepth = 0;

            public string Name
            {
                get
                {
                    return mName;
                }
            }

            public int Depth
            {
                get
                {
                    return mDepth;
                }
            }
        }
    }
}

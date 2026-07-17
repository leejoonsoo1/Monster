using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed partial class SoundComponent : GameFrameworkComponent
    {
        [Serializable]
        private sealed class SoundGroup
        {
            [SerializeField]
            private string mName = null;

            [SerializeField]
            private bool mAvoidBeingReplacedBySamePriority = false;

            [SerializeField]
            private bool mMute = false;

            [SerializeField, Range(0f, 1f)]
            private float mVolume = 1f;

            [SerializeField]
            private int mAgentHelperCount = 1;

            public string Name
            {
                get
                {
                    return mName;
                }
            }

            public bool AvoidBeingReplacedBySamePriority
            {
                get
                {
                    return mAvoidBeingReplacedBySamePriority;
                }
            }

            public bool Mute
            {
                get
                {
                    return mMute;
                }
            }

            public float Volume
            {
                get
                {
                    return mVolume;
                }
            }

            public int AgentHelperCount
            {
                get
                {
                    return mAgentHelperCount;
                }
            }
        }
    }
}

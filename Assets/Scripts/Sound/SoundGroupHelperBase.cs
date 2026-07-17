using GameFramework.Sound;
using UnityEngine;
using UnityEngine.Audio;

namespace UnityGameFramework.Runtime
{
    public abstract class SoundGroupHelperBase : MonoBehaviour, ISoundGroupHelper
    {
        [SerializeField]
        private AudioMixerGroup mAudioMixerGroup = null;

        public AudioMixerGroup AudioMixerGroup
        {
            get
            {
                return mAudioMixerGroup;
            }
            set
            {
                mAudioMixerGroup = value;
            }
        }
    }
}

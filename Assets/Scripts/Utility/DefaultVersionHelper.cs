using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class DefaultVersionHelper : Version.IVersionHelper
    {
        public string GameVersion
        {
            get
            {
                return Application.version;
            }
        }

        public int InternalGameVersion
        {
            get
            {
                return 0;
            }
        }
    }
}

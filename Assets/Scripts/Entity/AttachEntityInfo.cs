using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    internal sealed class AttachEntityInfo : IReference
    {
        private Transform mParentTransform;
        private object mUserData;

        public AttachEntityInfo()
        {
            mParentTransform = null;
            mUserData = null;
        }

        public Transform ParentTransform
        {
            get
            {
                return mParentTransform;
            }
        }

        public object UserData
        {
            get
            {
                return mUserData;
            }
        }

        public static AttachEntityInfo Create(Transform parentTransform, object userData)
        {
            AttachEntityInfo attachEntityInfo = ReferencePool.Acquire<AttachEntityInfo>();
            attachEntityInfo.mParentTransform = parentTransform;
            attachEntityInfo.mUserData = userData;
            return attachEntityInfo;
        }

        public void Clear()
        {
            mParentTransform = null;
            mUserData = null;
        }
    }
}

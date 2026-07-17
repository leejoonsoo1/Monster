using GameFramework;
using System;

namespace UnityGameFramework.Runtime
{
    internal sealed class ShowEntityInfo : IReference
    {
        private Type mEntityLogicType;
        private object mUserData;

        public ShowEntityInfo()
        {
            mEntityLogicType = null;
            mUserData = null;
        }

        public Type EntityLogicType
        {
            get
            {
                return mEntityLogicType;
            }
        }

        public object UserData
        {
            get
            {
                return mUserData;
            }
        }

        public static ShowEntityInfo Create(Type entityLogicType, object userData)
        {
            ShowEntityInfo showEntityInfo = ReferencePool.Acquire<ShowEntityInfo>();
            showEntityInfo.mEntityLogicType = entityLogicType;
            showEntityInfo.mUserData = userData;
            return showEntityInfo;
        }

        public void Clear()
        {
            mEntityLogicType = null;
            mUserData = null;
        }
    }
}

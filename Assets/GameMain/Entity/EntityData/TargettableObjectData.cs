using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityGameFramework.Runtime;

namespace Monster
{
    [Serializable]
    public abstract class TargetableObjectData : EntityData
    {
        protected TargetableObjectData(int entityId, int typeId)
            : base(entityId, typeId)
        {

        }
    }
}
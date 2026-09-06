using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{
    public class MapEntity : EntityLogic
    {
        private MapData mMapData;

        protected internal override void OnShow(object userData)
        {
            base.OnShow(userData);

            mMapData = userData as MapData;

            if (mMapData == null)
            {
                Log.Error("MapData가 유효하지 않습니다.");

                return;
            }

            transform.SetPositionAndRotation(mMapData.Position, mMapData.Rotation);
        }
    }
}
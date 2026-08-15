using UnityEngine;

namespace Monster
{

    // <summary>
    // TargetTableObject를 상속받는 Entity들이
    // 공통으로 사용하는 데이터 클래스입니다.
    // </summary>
    public class TargetTableObjectData
    {
        // <summary> 
        // Entity의 고유 ID입니다.
        // </summary>
        public int Id
        {
            get;
            private set;
        }
        
        // <summary>
        // Entity 종류를 구분하기 위한 타입 ID입니다.
        // </summary>
        public int TypeId
        {
            get;
            private set;
        }

        // <summary>
        // Entity가 생성될 때 시작할 초기 상태입니다.
        public EObjectState InitialState
        {
            get;
            private set;
        }

        protected TargetTableObjectData(int id, int typeId, EObjectState initialState)
        {
            Id = id;
            TypeId = typeId;

            InitialState = initialState;
        }
    }


}
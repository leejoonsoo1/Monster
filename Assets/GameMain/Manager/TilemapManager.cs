using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Monster
{
    public class TilemapManager
    {
        public enum EWalkableTileType
        {
            Ground1     = 0,
            Ground2,
            Grass, 
            Tree3,
            Building3,
            LightHouse2,
            Building2,
        }

        public enum EBlockedTileType
        {
            Hill = 7,
            Props,
            Tree1,
            Tree2,
            Building1,
            Building2,
            Lighthouse1,
            Tree4,
            Door
        }

        // Gird 아래에 배치된 모든 Tilemap을 저장합니다.
        private Tilemap[] mTilemaps;

        // TilemapManager 목록을 다시 가져오기 위해 Grid를 저장합니다.
        private Grid mGrid;

        // 현재 씬에서 사용할 TilemapManager입니다.
        public static TilemapManager Instance { get; private set; }

        // RPGGame에서 호출합니다.
        public void Initialize(Tilemap[] tilemaps)
        {
            mTilemaps = tilemaps;

            if (mTilemaps == null || mTilemaps.Length == 0)
            {
                return;
            }

            // Tilemap들의 부모에 있는 Grid를 저장합니다.
            mGrid = mTilemaps[0].GetComponentInParent<Grid>();

            if (mGrid == null)
            {
                Debug.LogError("TilemapManager : Grid를 찾을 수 없습니다.");

                return;
            }

            RefreshTilemaps();
        }

        // <summary>
        // 현재 Grid 아래에 존재하는 모든 Tilemap을 다시 가져옵니다.
        // Player가 한 칸 이동할 때마다 호출합니다.
        // </summary>
        public void RefreshTilemaps()
        {
            if (mGrid == null)
            {
                return;
            }

            mTilemaps = mGrid.GetComponentsInChildren<Tilemap>(true);

            Debug.Log($"Tilemap 갱신 완료 : {mTilemaps.Length}개");
        }

        // <summary>
        // 전달받은 셀에 존재하는 이동 불가 타일 종류를 반환합니다.
        // </summary>
        public List<EBlockedTileType> GetBlockedTileTypes(Vector3Int cellPosition)
        {
            List<EBlockedTileType> blockedTileTypes = new List<EBlockedTileType>();

            if (mTilemaps == null)
            {
                return blockedTileTypes;
            }

            // 해당 셀에 타일이 있는 모든 TIlemap을 확인합니다.
            foreach (Tilemap tilemap in mTilemaps)
            {
                if (tilemap == null)
                {
                    continue;
                }

                // 현재 Tilemap의 해당 셀에 타일이 없으면 건너뜁니다.
                if (tilemap.HasTile(cellPosition) != true)
                {
                    continue;
                }

                // TIlemap 오브젝트 이름을 이동 불가 enum으로 변환합니다.
                bool isBlockedTile = Enum.TryParse(tilemap.gameObject.name, true, out EBlockedTileType blockedTileType);

                // 이동 불가 enum에 등록된 Tilemap이면 목록에 추가합니다.
                if (isBlockedTile)
                {
                    blockedTileTypes.Add(blockedTileType);

                    Debug.Log($"이동 불가 타일 발견: " + $"{blockedTileType}, 셀: {cellPosition}");
                }
            }

            return blockedTileTypes;
        }

        // <summary>
        // 이동 불가 타일이 하나라도 있으면 해당 셀로 이동할 수 없습니다.
        // </summary>
        public bool IsWalkable(List<EBlockedTileType> blockedTileTypes)
        {
            return blockedTileTypes == null || blockedTileTypes.Count == 0;
        }

        // <summary>
        // 목표 셀에 장애물이 없고 실제로 디딜 타일이 있는지 확인합니다.
        // </summary>
        public bool IsWalkable(Vector3Int cellPosition)
        {
            // 타일맵이 준비되지 않았거나 도착할 셀에 장애물이 있으면 착지할 수 없습니다.
            if (mTilemaps == null || !IsWalkable(GetBlockedTileTypes(cellPosition)))
            {
                return false;
            }

            foreach (Tilemap tilemap in mTilemaps)
            {
                // 해당 셀에 타일이 없는 타일맵은 건너뜁니다.
                if (tilemap == null || !tilemap.HasTile(cellPosition))
                {
                    continue;
                }

                // 이동 가능한 타일과 현재 맵의 수풀 레이어인 Grace를 착지할 수 있는 곳으로 봅니다.
                string layerName = tilemap.gameObject.name;
                if (Enum.TryParse(layerName, true, out EWalkableTileType _) ||
                    string.Equals(layerName, "Grace", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        // 지정한 셀에 Hill 타일이 있는지 확인합니다.
        public bool IsHillTile(Vector3Int cellPosition)
        {
            if (mTilemaps == null)
            {
                return false;
            }

            foreach (Tilemap tilemap in mTilemaps)
            {
                if (tilemap == null)
                {
                    continue;
                }

                // Tilemap 이름이 Hill이고 해당 셀에 타일이 있는지 확인합니다.
                if (string.Equals(tilemap.gameObject.name, EBlockedTileType.Hill.ToString(), StringComparison.OrdinalIgnoreCase) && tilemap.HasTile(cellPosition))
                {
                    return true;
                }
            }

            return false;
        }

        // 현재 셀이 수풀인지 확인합니다.
        public bool IsGrassTile(Vector3Int cellPosition)
        {
            if (mTilemaps == null)
            {
                return false;
            }

            // 현재 Grid Cell에 어떤 Tilemap의 타일이 존재하는지 검사합니다.
            foreach (Tilemap tilemap in mTilemaps)
            {
                if (tilemap == null)
                {
                    continue;
                }

                // 현재 Cell에 해당 Tilemap의 타일이 없으면 다음 Tilemap 검사
                if (tilemap.HasTile(cellPosition) != true)
                {
                    continue;
                }

                Debug.Log($"현재 셀 : {cellPosition}, " + $"타일 : {tilemap.gameObject.name}");

                if (string.Equals(tilemap.gameObject.name, "Grace", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(tilemap.gameObject.name,
                        EWalkableTileType.Grass.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        // 게임 종료 또는 씬 정리 시 사용 가능
        public void Shutdown()
        {
            mTilemaps   = null;
            mGrid       = null;
        }
    }
}
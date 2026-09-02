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
            Ground2     = 1,
            Grace       = 2,
            Tree3       = 3,
            Building3   = 4,
            LightHouse2 = 5,
            Building2   = 6,
        }

        public enum EBlockedTileType
        {
            Hill        = 7,
            Props       = 8,
            Tree1       = 9,
            Tree2       = 10,
            Building1   = 11,
            Building2   = 12,
            LgihtHouse1 = 13,
            Tree4       = 14
        }

        // Gird 아래에 배치된 모든 Tilemap을 저장합니다.
        private Tilemap[] mTilemaps;

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
                if (!tilemap.HasTile(cellPosition))
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

        //<summary>
        //전달받은 모든 타일맵 종류를 검사하여
        //해당 칸으로 이동할 수 있는지 반환합니다.
        //</summary>
        public bool IsWalkable(List<EBlockedTileType> blockedTileTypes)
        {
            if (blockedTileTypes == null)
            {
                return true;
            }

            // 등록된 이동 가능한 타일맵을 검사합니다.
            foreach (EBlockedTileType blockedTileType in blockedTileTypes)
            {
                switch (blockedTileType)
                {
                    case EBlockedTileType.Hill:
                        return false;
                    case EBlockedTileType.Props:
                        return false;
                    case EBlockedTileType.Tree1:
                        return false;
                    case EBlockedTileType.Tree2:
                        return false;
                    case EBlockedTileType.Building1:
                        return false;
                    case EBlockedTileType.LgihtHouse1:
                        return false;
                }
            }

            // 어떤 이동 가능 타일맵에도 타일이 없으면 이동할 수 없습니다.
            return true;
        }

        //<summary>
        //목표 셀에 이동할 수 있는지 바로 확인합니다.
        //</summary>
        public bool IsWalkable(Vector3Int cellPosition)
        {
            List<EBlockedTileType> blockedTileTypes = GetBlockedTileTypes(cellPosition);

            return IsWalkable(blockedTileTypes);
        }

        // 현재 셀이 수풀인지 확인합니다.
        public bool IsGrassTile(Vector3Int cellPosition)
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

                if (!tilemap.HasTile(cellPosition))
                {
                    continue;
                }

                bool isWalkableTile = Enum.TryParse(tilemap.gameObject.name, true, out EWalkableTileType walkableTileType);

                if (!isWalkableTile)
                {
                    continue;
                }

                // 현재 enum 이름이 Grace이므로 Grace로 검사
                if (walkableTileType == EWalkableTileType.Grace)
                {
                    return true;
                }
            }

            return false;
        }

        // 게임 종료 또는 씬 정리 시 사용 가능
        public void Shutdown()
        {
            mTilemaps = null;
        }
    }
}
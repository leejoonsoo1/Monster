using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{
    // EntityComponent가 생성하고 관리하는 전투 포켓몬입니다.
    public class BattlePokemon : EntityLogic
    {
        private SpriteRenderer mSpriteRenderer;
        private Sprite mDefaultSprite;

        public BattlePokemonData PokemonData { get; private set; }

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            mSpriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            if (mSpriteRenderer != null)
            {
                mDefaultSprite = mSpriteRenderer.sprite;
            }
        }

        protected internal override void OnShow(object userData)
        {
            base.OnShow(userData);

            PokemonData = userData as BattlePokemonData;
            if (PokemonData == null)
            {
                Log.Error("BattlePokemonData가 유효하지 않습니다.");
                return;
            }

            Name = Entity.EntityAssetName;
            transform.SetPositionAndRotation(PokemonData.Position, PokemonData.Rotation);
            transform.localScale = Vector3.one * PokemonData.Scale;

            if (mSpriteRenderer == null)
            {
                Log.Error("BattlePokemon의 SpriteRenderer를 찾을 수 없습니다.");
                return;
            }

            // 풀에서 재사용할 때도 현재 데이터 또는 프리팹의 원래 Sprite를 적용합니다.
            mSpriteRenderer.sprite = PokemonData.PokemonSprite != null
                ? PokemonData.PokemonSprite
                : mDefaultSprite;
            mSpriteRenderer.enabled = true;
        }

        protected internal override void OnHide(bool isShutdown, object userData)
        {
            PokemonData = null;
            base.OnHide(isShutdown, userData);
        }
    }
}

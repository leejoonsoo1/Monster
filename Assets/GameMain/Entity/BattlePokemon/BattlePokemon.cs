using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{
    // EntityComponent가 생성하고 관리하는 전투 포켓몬입니다.
    public class BattlePokemon : EntityLogic
    {
        private SpriteRenderer mSpriteRenderer;
        private Animator mSpriteAnimator;
        private Sprite mDefaultSprite;

        public BattlePokemonData PokemonData { get; private set; }

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            mSpriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            if (mSpriteRenderer != null)
            {
                mDefaultSprite = mSpriteRenderer.sprite;
                mSpriteAnimator = mSpriteRenderer.GetComponent<Animator>();
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

            // 직접 지정한 Sprite가 있으면 애니메이션이 덮어쓰지 않도록 합니다.
            Sprite overrideSprite = PokemonData.PokemonSprite;
            if (mSpriteAnimator != null)
            {
                mSpriteAnimator.enabled = overrideSprite == null;
            }

            if (overrideSprite != null)
            {
                mSpriteRenderer.sprite = overrideSprite;
            }
            else
            {
                mSpriteRenderer.sprite = mDefaultSprite;
            }
            mSpriteRenderer.enabled = true;
        }

        protected internal override void OnHide(bool isShutdown, object userData)
        {
            PokemonData = null;
            base.OnHide(isShutdown, userData);
        }
    }
}
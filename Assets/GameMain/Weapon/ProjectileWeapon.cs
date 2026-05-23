using UnityEngine;
using UnityGameFramework.Runtime;

namespace ToyBoxNightmare
{
    /// <summary>
    /// 투사체형 무기. 가장 가까운 적을 향해 투사체를 발사한다.
    /// </summary>
    public class ProjectileWeapon : WeaponBase
    {
        private const string ProjectileAssetPath = "Projectile";
        private const float DetectRadius = 20f;

        [SerializeField] private int   damage   = 25;
        [SerializeField] private float speed    = 10f;
        [SerializeField] private float lifetime = 3f;

        public int   Damage   { get => damage;   set => damage   = value; }
        public float Speed    { get => speed;    set => speed    = value; }
        public float Lifetime { get => lifetime; set => lifetime = value; }

        protected override void Attack()
        {
            Enemy nearest = FindNearestEnemy(DetectRadius);
            if (nearest == null) return;

            Vector3 dir = (nearest.CachedTransform.position - Owner.CachedTransform.position).normalized;

            int id = EntitySerialId.Next();
            try
            {
                GameEntry.GetComponent<EntityComponent>().ShowEntity(
                    id,
                    typeof(Projectile),
                    ProjectileAssetPath,
                    "Projectile",
                    new ProjectileData(id, 1)
                    {
                        Position  = Owner.CachedTransform.position,
                        Direction = dir,
                        Damage    = damage,
                        Speed     = speed,
                        Lifetime  = lifetime
                    });
            }
            catch (System.Exception ex)
            {
                Log.Warning("ProjectileWeapon skipped (prefab not ready): {0}", ex.Message);
            }
        }
    }
}

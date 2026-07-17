namespace UnityGameFramework.Runtime
{
    public class DefaultSoundHelper : SoundHelperBase
    {
        private ResourceComponent mResourceComponent = null;

        public override void ReleaseSoundAsset(object soundAsset)
        {
            mResourceComponent.UnloadAsset(soundAsset);
        }

        private void Start()
        {
            mResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            if (mResourceComponent == null)
            {
                Log.Fatal("Resource component is invalid.");
                return;
            }
        }
    }
}

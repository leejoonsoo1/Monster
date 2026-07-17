namespace UnityGameFramework.Runtime
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        private sealed class FpsCounter
        {
            private float mUpdateInterval;
            private float mCurrentFps;
            private int mFrames;
            private float mAccumulator;
            private float mTimeLeft;

            public FpsCounter(float updateInterval)
            {
                if (updateInterval <= 0f)
                {
                    Log.Error("Update interval is invalid.");
                    return;
                }

                mUpdateInterval = updateInterval;
                Reset();
            }

            public float UpdateInterval
            {
                get
                {
                    return mUpdateInterval;
                }
                set
                {
                    if (value <= 0f)
                    {
                        Log.Error("Update interval is invalid.");
                        return;
                    }

                    mUpdateInterval = value;
                    Reset();
                }
            }

            public float CurrentFps
            {
                get
                {
                    return mCurrentFps;
                }
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                mFrames++;
                mAccumulator += realElapseSeconds;
                mTimeLeft -= realElapseSeconds;

                if (mTimeLeft <= 0f)
                {
                    mCurrentFps = mAccumulator > 0f ? mFrames / mAccumulator : 0f;
                    mFrames = 0;
                    mAccumulator = 0f;
                    mTimeLeft += mUpdateInterval;
                }
            }

            private void Reset()
            {
                mCurrentFps = 0f;
                mFrames = 0;
                mAccumulator = 0f;
                mTimeLeft = 0f;
            }
        }
    }
}

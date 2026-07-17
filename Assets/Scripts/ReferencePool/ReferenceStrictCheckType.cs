namespace UnityGameFramework.Runtime
{
    public enum ReferenceStrictCheckType : byte
    {
        AlwaysEnable = 0,

        OnlyEnableWhenDevelopment,

        OnlyEnableInEditor,

        AlwaysDisable,
    }
}

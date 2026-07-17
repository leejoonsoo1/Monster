namespace UnityGameFramework.Runtime
{
    public enum DebuggerActiveWindowType : byte
    {
        AlwaysOpen = 0,

        OnlyOpenWhenDevelopment,

        OnlyOpenInEditor,

        AlwaysClose,
    }
}

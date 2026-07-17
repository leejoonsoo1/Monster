using GameFramework.FileSystem;
using System;

namespace UnityGameFramework.Runtime
{
    public class DefaultFileSystemHelper : FileSystemHelperBase
    {
        private const string AndroidFileSystemPrefixString = "jar:";

        public override FileSystemStream CreateFileSystemStream(string fullPath, FileSystemAccess access, bool createNew)
        {
            if (fullPath.StartsWith(AndroidFileSystemPrefixString, StringComparison.Ordinal))
            {
                return new AndroidFileSystemStream(fullPath, access, createNew);
            }
            else
            {
                return new CommonFileSystemStream(fullPath, access, createNew);
            }
        }
    }
}

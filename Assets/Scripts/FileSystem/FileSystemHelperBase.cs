using GameFramework.FileSystem;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class FileSystemHelperBase : MonoBehaviour, IFileSystemHelper
    {
        public abstract FileSystemStream CreateFileSystemStream(string fullPath, FileSystemAccess access, bool createNew);
    }
}

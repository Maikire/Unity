using System.IO;
using UnityEditor;

/// <summary>
/// GenerateResourceConfig
/// </summary>
public class GenerateResourceConfig : Editor
{
    //使用了C#特性，使这个方法的调用方式变成了点特定的按钮
    //按钮的位置在左上角，在Tools中的Resource中的Generate Resource Config File
    [MenuItem("Tools/Resource/Generate Resource Config File")]
    public static void Generate()
    {
        //生成资源配置文件

        //1.查找Resource目录下所有的预制件的路径
        //AssetDatabase包含了只适用于在编译器中操作资源的相关功能
        //第一个参数是过滤器（指定格式的字符串）
        //例："t:prefab"  筛选出prefab类型的文件（预制件的后缀是 .prefab）
        //第二个参数是指定路径
        //返回值是Unity中的资源ID，称之为 GUID
        string filter = "t:prefab";
        string searchInFolders = "Assets/Resources";
        string[] resourceFile = AssetDatabase.FindAssets(filter, new string[] { searchInFolders });

        for (int i = 0; i < resourceFile.Length; i++)
        {
            //GUID转换为资源路径
            resourceFile[i] = AssetDatabase.GUIDToAssetPath(resourceFile[i]);

            //2.生成对应关系（名称=路径）
            string fileName = Path.GetFileNameWithoutExtension(resourceFile[i]);
            string filePath = resourceFile[i].Replace(searchInFolders + "/", string.Empty).Replace("." + filter.Split(':')[1], string.Empty);
            resourceFile[i] = fileName + '=' + filePath;
        }

        //3.在 .txt 文件中写入
        //为了兼容所有的平台，需要放到 StreamingAssets 中
        //StreamingAssets是Unity的特殊目录之一，
        //该目录下的文件不会被压缩，适合在移动端读取资源，可以在PC端写入,
        //在运行时，这个文件夹是只读的
        File.WriteAllLines("Assets/StreamingAssets/ConfigFile.txt", resourceFile);

        //刷新文件夹（Unity不会实时刷新StreamingAssets中的文件，可以用代码手动刷新）
        //AssetDatabase 包含了只适用于在编辑器中操作资源的相关功能
        AssetDatabase.Refresh();

        //持久化路径 Application.persistentDataPath 是Unity外部目录（安装程序时才会产生），
        //这个路径可以在运行时进行读写操作，只有在游戏发布时，这个路径才能使用
    }

}


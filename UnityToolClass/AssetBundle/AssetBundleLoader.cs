namespace Common
{
    /// <summary>
    /// AB 包加载器
    /// </summary>
    public class AssetBundleLoader : MonoSingleton<AssetBundleLoader>
    {
        protected override void Init()
        {
            base.Init();
            DontDestroyOnLoad(this.gameObject);
        }

    }
}

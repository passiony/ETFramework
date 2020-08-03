
namespace ETModel
{
    public abstract class BaseChannel
    {
        public abstract void Init();

        public virtual string GetCompanyName()
        {
            return "gillar";
        }

        public abstract string GetBundleID();

        public abstract string GetProductName();

        public virtual bool IsInternalChannel()
        {
            return false;
        }

        public virtual bool IsGooglePlay()
        {
            return false;
        }

        public abstract void DownloadGame(params object[] args);

        public abstract void InstallApk();

        public abstract void Login();

        public abstract void Logout();

        public abstract void Pay(params object[] paramList);

    }
}
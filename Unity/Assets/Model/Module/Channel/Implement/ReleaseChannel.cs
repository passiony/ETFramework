
namespace ETModel
{
    public class ReleaseChannel : BaseChannel
    {
        public override void Init()
        {
            //AndroidSDKHelper.FuncCall("TestChannelInit");
        }

        public override string GetBundleID()
        {
            return "com.gillar.word.magicalletters";
        }

        public override string GetProductName()
        {
            return "Magicalletters";
        }

        public override void InstallApk()
        {
            //AndroidSDKHelper.FuncCall("InstallApk");
        }

        public override void DownloadGame(params object[] args)
        {
            //string url = paramList[0] as string;
            //string saveName = paramList[1] as string;
            //AndroidSDKHelper.FuncCall("DownloadGame", url, saveName);
        }

        public override void Login()
        {

        }

        public override void Logout()
        {

        }

        public override void Pay(params object[] paramList)
        {

        }

    }
}
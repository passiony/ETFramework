/// <summary>
/// 全局URL配置
/// </summary>
namespace ETModel
{
    public static class URLSetting
    {
        public static string BASE_URL
        {
            get
            {
#if DEVELOPMENT
                return "http://10.200.10.54:8080";
#else
                return "https://wordsletter.gillar.com";
#endif
            }
        }

        public static string START_UP_URL
        {
            get
            {
                return BASE_URL+"/startup";
            }
        }

        public static string REPORT_ERROR_URL
        {
            get
            {
                return BASE_URL + "/trace";
            }
        }

        public static string LOGIN_URL
        {
            get
            {
                return BASE_URL + "/login";
            }
        }

        public static string VERSION_URL
        {
            get
            {
                return BASE_URL + "/admin/version";
            }
        }
        
        public static string APP_DOWNLOAD_URL
        {
            get;
            set;
        }

        public static string RES_DOWNLOAD_URL
        {
            get;
            set;
        }
        
        public const string APP_URL = "https://apps.apple.com/us/app/magical-letters-wordcross/id1508275839";
        public const string S3_URL = "s3://statics-word.gillar.com";
        public const string RES_CDN_URL = "https://statics-word.gillar.com/AssetBundles";
        public const string TOKEN = "577847378416391261";

    }
}

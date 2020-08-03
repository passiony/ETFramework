namespace ETModel
{
    public static class Define
    {
#if UNITY_EDITOR && !ASYNC
        public static bool IsAsync = false;
#else
        public static bool IsAsync = true;
#endif

#if UNITY_EDITOR
        public static bool IsEditorMode = true;
#else
	public static bool IsEditorMode = false;
#endif

#if DEVELOPMENT
	public static bool IsDevelopment = true;
#else
        public static bool IsDevelopment = false;
#endif

#if ILRuntime
        public static bool IsILRuntime = true;
#else
	public static bool IsILRuntime = false;
#endif

#if LOGGER_ON
        public static bool IsLoggerOn = true;
#else
	public static bool IsLoggerOn = false;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        public static bool IsAndroidMode = true;
#else
        public static bool IsAndroidMode = false;
#endif

#if ENCRYPT
	public static bool IsEncrypt = true;
#else
        public static bool IsEncrypt = false;
#endif
    }
}
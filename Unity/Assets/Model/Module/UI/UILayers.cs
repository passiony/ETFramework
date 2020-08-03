namespace ETModel
{
    public enum ELayer
    {
        Scene,//场景UI，如：点击建筑查看建筑信息---一般置于场景之上，界面UI之下
        Backgroud,//背景UI，如：主界面---一般情况下用户不能主动关闭，永远处于其它UI的最底层
        Normal, //普通UI，一级、二级、三级等窗口---一般由用户点击打开的多级窗口
        Info,//信息UI---如：跑马灯、广播等---一般永远置于用户打开窗口顶层
        Tip,//提示UI，如：错误弹窗，网络连接弹窗等
        Top//顶层UI，如：场景加载
    }

    public enum OpenType
    {
        None,   //可重复添加
        NoRepeat//检查重复
    }

    public class LayerConfig
    {
        public string Name;
        public int PlaneDistance;
        public int OrderInLayer;

        public LayerConfig(string name, int planeDistance, int oderInLayer)
        {
            Name = name;
            PlaneDistance = planeDistance;
            OrderInLayer = oderInLayer;
        }
    }

    //窗体的层级类型
    public static class UILayers
    {
        public static LayerConfig[] Layers = {
            new LayerConfig("SceneLayer", 1000, 0),
            new LayerConfig("BackgroudLayer", 900, 1000),
            new LayerConfig("NormalLayer", 800, 2000),
            new LayerConfig("InfoLayer", 700, 3000),
            new LayerConfig("TipLayer", 600, 4000),
            new LayerConfig("TopLayer", 500, 5000),
        };

        public static LayerConfig GetLayer(ELayer type)
        {
            return Layers[(int)type];
        }
    }
}
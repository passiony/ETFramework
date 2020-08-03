using System;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
    [ObjectSystem]
    public class OperaComponentAwakeSystem : AwakeSystem<OperaComponent>
    {
	    public override void Awake(OperaComponent self)
	    {
		    self.Awake();
	    }
    }

	[ObjectSystem]
	public class OperaComponentUpdateSystem : UpdateSystem<OperaComponent>
	{
		public override void Update(OperaComponent self)
		{
			self.Update();
		}
	}

	public class OperaComponent: Entity
    {
        public Vector3 ClickPoint;

	    public int mapMask;

	    public void Awake()
	    {
		    this.mapMask = LayerMask.GetMask("Map");
	    }

        //private readonly Frame_ClickMap frameClickMap = new Frame_ClickMap();
        private Vector3 frameClickMap = new Vector3();

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000, this.mapMask))
                {
                    this.ClickPoint = hit.point;
                    frameClickMap.x = this.ClickPoint.x;
                    frameClickMap.y = this.ClickPoint.y;
                    frameClickMap.z = this.ClickPoint.z;
                    //ETModel.SessionComponent.Instance.Session.Send(frameClickMap);

                    //移动到目的地
                    Unit unit = ETModel.Game.Scene.GetComponent<UnitComponent>().Get(1);
                    unit.GetComponent<AnimatorComponent>().SetFloatValue("Speed", 5f);
                    UnitPathComponent unitPathComponent = unit.GetComponent<UnitPathComponent>();

                    unitPathComponent.StartMove(frameClickMap).Coroutine();

                    // 测试actor rpc消息
                    //this.TestActor().Coroutine();
                }
            }
        }

	    public async ETVoid TestActor()
	    {
		    try
		    {
			   //  M2C_TestActorResponse response = (M2C_TestActorResponse) await SessionComponent.Instance.Session.Call(
						// new C2M_TestActorRequest() { Info = "actor rpc request" });
			   //  Log.Debug(response.Info);
			}
		    catch (Exception e)
		    {
				Log.Error(e);
		    }
		}
    }
}

﻿using System;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler]
	public class ObjectRemoveRequestHandler : AMActorRpcHandler<Scene, ObjectRemoveRequest, ObjectRemoveResponse>
	{
		protected override async ETTask Run(Scene scene, ObjectRemoveRequest request, ObjectRemoveResponse response, Action reply)
		{
			await scene.GetComponent<LocationComponent>().Remove(request.Key);
			reply();
			await ETTask.CompletedTask;
		}
	}
}
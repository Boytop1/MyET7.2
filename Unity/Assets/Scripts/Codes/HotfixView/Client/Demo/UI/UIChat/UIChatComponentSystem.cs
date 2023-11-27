//自动生成文件，请勿编辑
using UnityEngine.UI;
using UnityEngine;


namespace ET.Client
{
	[FriendOf(typeof(UIChatComponent))]
	public static class UIChatComponentSystem
	{
		[ObjectSystem]
		public class UIChatComponentAwakeSystem: AwakeSystem<UIChatComponent>
		{
			protected override void Awake(UIChatComponent self)
			{
				ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
				self.Content = rc.Get<GameObject>("Content");
				self.Message = rc.Get<GameObject>("Message");
				self.SendBtn = rc.Get<GameObject>("SendBtn");

				
			}
		}
	}
}

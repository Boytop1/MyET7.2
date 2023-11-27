using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [FriendOf(typeof(UILobbyComponent))]
    public static class UILobbyComponentSystem
    {
        [ObjectSystem]
        public class UILobbyComponentAwakeSystem: AwakeSystem<UILobbyComponent>
        {
            protected override void Awake(UILobbyComponent self)
            {
                ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

                self.enterMap = rc.Get<GameObject>("EnterMap");
                self.enterMap.GetComponent<Button>().onClick.AddListener(() => { self.EnterMap().Coroutine(); });
                self.sendMessageToServer = rc.Get<GameObject>("SendMessageToServer");
                self.sendMessageToServer.GetComponent<Button>().onClick.AddListener(() => { self.SendMessageToServer(); });
                self.requestServerMessage = rc.Get<GameObject>("RequestServerMessage");
                self.requestServerMessage.GetComponent<Button>().onClick.AddListener(() => { self.RequestMessageToServer().Coroutine(); });
            }
        }
        
        public static async ETTask EnterMap(this UILobbyComponent self)
        {
            await EnterMapHelper.EnterMapAsync(self.ClientScene());
            await UIHelper.Remove(self.ClientScene(), UIType.UILobby);
        }
        
        public static void SendMessageToServer(this UILobbyComponent self)
        {
            Session session = self.ClientScene().GetComponent<SessionComponent>().Session;
            C2G_HelloWorldSend msg = new C2G_HelloWorldSend() { Message = "你好！服务器。(send)" };
            session.Send(msg);
            Log.Info(msg.Message);
        }
        
        public static async ETTask RequestMessageToServer(this UILobbyComponent self)
        {
            Session session = self.ClientScene().GetComponent<SessionComponent>().Session;
            G2C_HelloWorld g2CHelloWorld_Res = await session.Call(new C2G_HelloWorld(){Message = "你好！服务器。(req)"}) as G2C_HelloWorld;
            Log.Info(g2CHelloWorld_Res.Message);
        }
    }
}
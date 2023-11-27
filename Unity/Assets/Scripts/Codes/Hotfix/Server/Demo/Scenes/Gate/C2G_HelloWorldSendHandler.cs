namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class C2G_HelloWorldSendHandler : AMHandler<C2G_HelloWorldSend>
	{
		protected override async ETTask Run(Session session, C2G_HelloWorldSend send)
		{
            Log.Info(send.Message);
            await ETTask.CompletedTask;
		}
	}
}
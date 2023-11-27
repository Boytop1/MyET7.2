namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class C2G_HelloWorldHandler : AMRpcHandler<C2G_HelloWorld, G2C_HelloWorld>
	{
		protected override async ETTask Run(Session session, C2G_HelloWorld request, G2C_HelloWorld response)
		{
            response.Message = "你好！客户端。";
            await ETTask.CompletedTask;
		}
	}
}
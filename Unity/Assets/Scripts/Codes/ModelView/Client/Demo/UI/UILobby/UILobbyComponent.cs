
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILobbyComponent : Entity, IAwake
	{
		public GameObject enterMap;
		public Text enterMapText;
		public GameObject sendMessageToServer;
		public Text sendMessageToServerText;
		public GameObject requestServerMessage;
		public Text requestServerMessageText;
	}
}

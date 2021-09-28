using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


public class TCPClient : MonoBehaviour
{
	public string m_Ip = "192.168.1.9";
	public int m_Port = 50003;
	private TcpClient m_Client;
	private Thread m_ThrdClientReceive;
	//private Text myText;
	//private static string myResult;
	private static int myCnt = 1;
	//private string faceData;
	//private ARFaceBlendShapeVisualizer FaceBlendshape;


	void Start()
	{
		//myResult = "(준비중)";
		//myText = GameObject.Find("Text33").GetComponent<Text>();
		//FaceBlendshape = GetComponent<ARFaceBlendShapeVisualizer>();
		//faceData = FaceBlendshape.Facedata;
		ConnectToTcpServer();
	}

	void Update()
	{
		//SendMessage(faceData);
		SendMessage(myCnt++.ToString() + ". hello?");
		//myText.text = "서버가 보낸값: " + myResult;
	}

	void ConnectToTcpServer()
	{
		try
		{
			m_ThrdClientReceive = new Thread(new ThreadStart(ListenForData));
			m_ThrdClientReceive.IsBackground = true;
			m_ThrdClientReceive.Start();
		}
		catch (Exception ex)
		{
			Debug.Log(ex);
		}
	}

	void ListenForData()
	{
		try
		{
			m_Client = new TcpClient(m_Ip, m_Port);
			Byte[] bytes = new Byte[1024];
			while (true)
			{
				if (m_Client.Connected)
				{
					using (NetworkStream stream = m_Client.GetStream())
					{
						int length;

						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);

							string serverMessage = Encoding.Default.GetString(incommingData);
							Debug.Log(serverMessage); // 받은 값
							print(serverMessage);
							//myResult = serverMessage;
						}
					}
				}

			}


		}

		catch (SocketException ex)
		{
			Debug.Log(ex);
		}
	}


	void SendMessage(string message)
	{
		if (m_Client == null)
		{
			return;
		}

		try
		{
			if (m_Client.Connected)
			{
				NetworkStream stream = m_Client.GetStream();

				if (stream.CanWrite)
				{
					byte[] clientMessageAsByteArray = Encoding.Default.GetBytes(message);
					stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				}
			}
		}

		catch (SocketException ex)
		{
			Debug.Log(ex);
		}
	}

	void OnApplicationQuit()
	{
		m_ThrdClientReceive.Abort();

		if (m_Client != null)
		{
			m_Client.Close();
			m_Client = null;
		}
	}

}
using UnityEngine;
using System.IO.Ports;

public class Foot : MonoBehaviour
{
    private SerialPort sp;

    void Start()
    {
        sp = new SerialPort("COM5", 115200);
        sp.Open();
    }

    void Update()
    {

    }

    public void startFoot()
    {
        SendCommand("BackOn");
        SendCommand("HapticOn");
    }

    public void stopFoot()
    {
        SendCommand("BackOff");
        SendCommand("HapticOff");
    }

    void SendCommand(string command)
    {
        if (sp != null && sp.IsOpen)
        {
            sp.WriteLine(command);
        }
    }

    void OnApplicationQuit()
    {
        if (sp != null && sp.IsOpen)
        {
            // 安全のため全停止
            sp.WriteLine("BackOff");
            sp.WriteLine("HapticOff");

            sp.Close();
        }
    }
}

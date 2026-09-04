using UnityEngine;
using System.IO.Ports;

public class Foot : MonoBehaviour
{
    private SerialPort sp;

    // Vp4 èÛë‘
    private bool vp4On = false;

    // HAPTIC REACTOR èÛë‘
    private bool hapticOn = false;

    void Start()
    {
        sp = new SerialPort("COM4", 115200);
        sp.Open();
    }

    void Update()
    {
      
    }

    public void startFoot()
    {
        vp4On = true;
        hapticOn = true;

        SendCommand("BackOn");
        SendCommand("HapticOn");
    }

    public void stopFoot()
    {
        vp4On = false;
        hapticOn = false;

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
            // à¿ëSÇÃÇΩÇﬂëSí‚é~
            sp.WriteLine("BackOff");
            sp.WriteLine("HapticOff");

            sp.Close();
        }
    }
}

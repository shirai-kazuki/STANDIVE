using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

using UnityEngine.InputSystem;

public class Hardware : MonoBehaviour
{
    private SerialPort serial;
    private Coroutine currentRoutine;

    //中央を0とした現在位置
    private float currentPosition = 0f;

    //現在動いている方向
    private int currentDirection = 0;

    //現在動いている時間
    private float currentTimer = 0f;

    //左右の最大
    private const float maxMove = 2.5f;

    void Start()
    {
        serial = new SerialPort("COM3", 115200);
        serial.Open();

    }

    void Update()
    {
        //アクチュエータ
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            CancelAutoMove();
            currentRoutine = StartCoroutine(MoveToStop("AB_Up", 3.0f, 0, "AB_Up_Slow"));
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            CancelAutoMove();
            float limit = maxMove + currentPosition;
            currentRoutine = StartCoroutine(MoveToStop("A_Up_B_Down", limit, -1, "A_Up_B_Down_Slow"));
        }

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            CancelAutoMove();
            float limit = maxMove - currentPosition;
            currentRoutine = StartCoroutine(MoveToStop("B_Up_A_Down", limit, 1, "B_Up_A_Down_Slow"));
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            CancelAutoMove();
            currentRoutine = StartCoroutine(MoveToStop("AB_Down", 10.0f, 0, "AB_Down_Slow"));
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            CancelAutoMove();
            SendCommand("AB_Stop");
        }

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            CancelAutoMove();
            SendCommand("AB_DownA");
        }

        if (Keyboard.current.uKey.wasPressedThisFrame)
        {
            CancelAutoMove();
            SendCommand("AB_UpA");
        }
    }

    //飛び込み
    public void MoveJampingOut()
    {
        CancelAutoMove();
        currentRoutine = StartCoroutine(MoveToStop("AB_Up", 3.0f, 0, "AB_Up_Slow"));
    }

    //右移動
    public void MoveRight()
    {
        CancelAutoMove();
        float limit = maxMove - currentPosition;
        currentRoutine = StartCoroutine(MoveToStop("B_Up_A_Down", limit, 1, "B_Up_A_Down_Slow"));
    }

    //左移動
    public void MoveLeft()
    {
        CancelAutoMove();
        float limit = maxMove + currentPosition;
        currentRoutine = StartCoroutine(MoveToStop("A_Up_B_Down", limit, -1, "A_Up_B_Down_Slow"));
    }

    //パラシュート
    public void MoveParachute()
    {
        CancelAutoMove();
        currentRoutine = StartCoroutine(MoveToStop("AB_Down", 10.0f, 0, "AB_Down_Slow"));
    }

    //センサーによるストップ(キー入力入り)
    IEnumerator MoveToStop(string startCommand, float timeLimit, int direction, string slowCommand)
    {

        SendCommand(startCommand);

        //現在の移動情報を保存
        currentDirection = direction;
        currentTimer = 0f;

        while (currentTimer < timeLimit)
        {
            currentTimer += Time.deltaTime;
            yield return null;
        }

        SendCommand("AB_Stop");

        //実際に動いた時間だけ現在位置を更新
        currentPosition += currentDirection * currentTimer;

        //範囲を超えないようにする
        currentPosition = Mathf.Clamp(currentPosition, -maxMove, maxMove);

        currentDirection = 0;
        currentTimer = 0f;

        currentRoutine = null;
    }

    //コマンド送信
    void SendCommand(string command)
    {
        if (serial != null && serial.IsOpen)
        {
            serial.WriteLine(command);
            Debug.Log("Sent: " + command);
        }
    }

    //入力での制御を途中中断できる
    void CancelAutoMove()
    {
        if (currentRoutine != null)
        {
            //途中まで動いた分を現在位置へ反映
            currentPosition += currentDirection * currentTimer;
            currentPosition = Mathf.Clamp(currentPosition, -maxMove, maxMove);

            StopCoroutine(currentRoutine);

            currentRoutine = null;
            currentDirection = 0;
            currentTimer = 0f;
        }
    }

    void OnDestroy()
    {
        if (serial != null && serial.IsOpen)
        {
            serial.Close();
        }
    }
}
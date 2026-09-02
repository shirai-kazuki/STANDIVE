using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.InputSystem;

public class Hardware : MonoBehaviour
{
    private SerialPort serial;
    private Coroutine currentRoutine;

    //アクチュエータ

    //中央を0とした現在位置
    private float currentPosition = 0f;

    //現在動いている方向
    private int currentDirection = 0;

    //現在動いている時間
    private float currentTimer = 0f;

    //スタートの傾き
    private float startMove = 3.0f;

    //左右の最大
    private float maxMove = 2.5f;

    //パラシュート着地の排他制御用変数
    private Coroutine paratyakutiRoutine;

    //体験終了
    private bool Finish = true;

    //送風機
    private int sendValue = 94;


    //圧力センサ

    //圧力センサーの値
    private int sensorData1 = 0;
    private int sensorData2 = 0;
    private int sensorData3 = 0;
    private int sensorData4 = 0;

    //加圧か排気か（false:加圧　true:排気）
    private bool relay1Off;
    private bool relay2Off;
    private bool relay3Off;
    private bool relay4Off;
    //false:排気　true:Stop
    private bool relay7Off;

    //エアバック動作させる=true
    private bool AirMove;

    //飛び出し加圧max時間
    private float AirTimeLimit = 1.1f;
    //飛び出し加圧maxセンサー値
    private int sensorLimit0 = 600;

    //パラシュート後、AirMoveを禁止する
    private bool airMoveLocked = false;

    // パラシュートを開ける高度に到達したか
    public bool parachuteOK = false;

    //ソフトのほうに送る時間用
    public float hardTime = 0f;

    //スタート可能か
    public bool startOk = false;

    private Playermanager playermanager;

    void Start()
    {
        serial = new SerialPort("COM3", 115200);
        serial.ReadTimeout = 100;
        serial.Open();

        playermanager = FindAnyObjectByType<Playermanager>();
    }

    void Update()
    {
        //パラシュート再設置
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            paratyakutiRoutine = StartCoroutine(ParachuteReset(1));
        }

        //パラシュート再設置
        if (Keyboard.current.uKey.wasPressedThisFrame)
        {
            paratyakutiRoutine = StartCoroutine(ParachuteReset(2));
        }

        //シリンダストップ
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            CancelAutoMove();
            SendCommand("AB_Stop");
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            CancelAutoMove();

            // パラシュート着地処理停止
            if (paratyakutiRoutine != null)
            {
                StopCoroutine(paratyakutiRoutine);
                paratyakutiRoutine = null;
            }

            // エアバッグ停止
            for (int i = 1; i <= 4; i++)
            {
                SendCommand(i + "_Off");
            }

            // 7番も停止
            SendCommand("7_On");

            // 状態をリセット
            relay1Off = true;
            relay2Off = true;
            relay3Off = true;
            relay4Off = true;
            relay7Off = false;

            AirMove = false;

            SendCommand("AB_Stop");
            SendCommand("C_Stop");
        }

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            SendCommand("AB_Down");
        }

        //パラシュート後はAirMoveを絶対にfalseにする
        if (airMoveLocked)
        {
            AirMove = false;
        }

    }

    public void SetParachute(bool value)
    {
        parachuteOK = value;
    }

    public void SetStart(bool value)
    {
        startOk = value;
    }


    //飛び出し
    public void MoveJampingOut()
    {
        if (!Finish) return;
        CancelAutoMove();
        AirMove = true;
        currentRoutine = StartCoroutine(MoveToStop("AB_Up", startMove, 0, 0));
        //送信コマンド,アクチュエーター動作リミット時間,動作時間用ステータス,圧力用ステータス
    }

    //右移動
    public void MoveRight()
    {
        if (!Finish) return;
        // パラシュート処理中は右移動禁止
        if (paratyakutiRoutine != null) return;
        CancelAutoMove();
        float limit = maxMove - currentPosition;
        currentRoutine = StartCoroutine(MoveToStop("B_Up_A_Down", limit, 1, 2));
    }

    //左移動
    public void MoveLeft()
    {
        if (!Finish) return;
        // パラシュート処理中は右移動禁止
        if (paratyakutiRoutine != null) return;
        CancelAutoMove();
        float limit = maxMove + currentPosition;
        currentRoutine = StartCoroutine(MoveToStop("A_Up_B_Down", limit, -1, 1));
    }

    //パラシュート
    public void MoveParachute()
    {
        if (!Finish) return;
        CancelAutoMove();
        // 現在の傾きを確認
        float adjustTime = Mathf.Abs(currentPosition);

        hardTime = startMove + 0.1f + Mathf.Abs(currentPosition);

        // Aが高い場合
        if (currentPosition < 0)
        {
            currentRoutine = StartCoroutine(MoveToStop("B_Up_A_Down", adjustTime, 0, 3));
        }
        // Bが高い場合
        else if (currentPosition > 0)
        {
            currentRoutine = StartCoroutine(MoveToStop("A_Up_B_Down", adjustTime, 0, 3));
        }
        paratyakutiRoutine = StartCoroutine(ParachuteDeploy());

        //パラシュート後の左右傾き
        maxMove = 2.0f;
        startMove = maxMove;
    }

    //着地前
    public void BeforeLanding()
    {
        if (!Finish) return;
        CancelAutoMove();
        // 現在の傾きを確認
        float adjustTime = Mathf.Abs(currentPosition) * 2;

        // Aが高い場合
        if (currentPosition < 0)
        {
            currentRoutine = StartCoroutine(MoveToStop("A_Down", adjustTime, 0, 4));
        }
        // Bが高い場合
        else if (currentPosition > 0)
        {
            currentRoutine = StartCoroutine(MoveToStop("B_Down", adjustTime, 0, 4));
        }
        Finish = false;
    }

    //着地
    public void MoveLanding()
    {
        paratyakutiRoutine = StartCoroutine(LandingImpact());
    }

    //送風機初期
    public void MoveKaze()
    {
        SendCommand("Kaze");
        Debug.Log("Kaze");
    }
    

//センサー値取得
void ReceiveSensor()
    {
        if (serial != null && serial.IsOpen && serial.BytesToRead > 0)
        {
            try
            {
                string data = serial.ReadLine().Trim();

                if (data == "P")
                {
                    if (startOk)
                    {
                        playermanager.SetStartTrigger(true);//飛び降りる
                        startOk = false;
                    }
                    if (parachuteOK)
                    {
                        playermanager.SetParachuteOpen(true);//パラシュートの処理を可能にする
                        parachuteOK = false;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    IEnumerator MoveToStop(string startCommand, float timeLimit, int direction, int nowState)
    {
        SendCommand(startCommand);

        //現在の移動情報を保存
        currentDirection = direction;
        currentTimer = 0f;
        float sendTimer = 0f;

        //圧力制御が終了したか
        bool pressureFinished = false;

        //飛び出し時
        if (nowState == 0 && AirMove)
        {
            //送風機
            SendCommand("W");

            relay1Off = false;
            relay2Off = false;
            relay3Off = false;
            relay4Off = false;

            //コマンド送信
            SendCommand("7_Off");
            relay7Off = true;

            for (int i = 1; i <= 4; i++)
            {
                SendCommand(i + "_On");
            }
        }

        //傾き
        if ((nowState == 1 || nowState == 2) && AirMove)
        {

        }

        //パラシュート
        if (nowState == 3 && AirMove)
        {
            SendCommand("V");

            relay1Off = true;//排気
            relay2Off = true;
            relay3Off = true;
            relay4Off = true;

            //コマンド送信
            SendCommand("7_On");
            relay7Off = false;//排気

            for (int i = 1; i <= 4; i++)
            {
                SendCommand(i + "_Off");
            }

            // AirMoveを強制的にOFF
            AirMove = false;
            //以降AirMoveを禁止
            airMoveLocked = true;
        }

        while (currentTimer < timeLimit)
        {
            currentTimer += Time.deltaTime;
            sendTimer += Time.deltaTime;

            //秒ごとに1ずつ変化
            if (sendTimer >= ((maxMove * 2) / 11))
            {
                sendTimer = 0f;

                if (nowState == 2)
                {
                    sendValue--;
                    SendCommand(sendValue.ToString());
                }
                else if (nowState == 1)
                {
                    sendValue++;
                    SendCommand(sendValue.ToString());
                }
                else if (nowState == 3 || nowState == 4 )
                {
                    if (sendValue < 94)
                    {
                        sendValue++;
                        SendCommand(sendValue.ToString());
                    }
                    else if (sendValue > 94)
                    {
                        sendValue--;
                        SendCommand(sendValue.ToString());
                    }
                }
            }


            //飛び出し
            if (AirMove)
            {
                if (nowState == 0 && !pressureFinished)
                {
                    if (currentTimer < AirTimeLimit)
                    {
                        
                    }
                    else
                    {
                        //AirTimeLimit経過後
                        if (!relay1Off)
                        {
                            SendCommand("1_Off");
                            relay1Off = true;
                        }

                        if (!relay2Off)
                        {
                            SendCommand("2_Off");
                            relay2Off = true;
                        }

                        if (!relay3Off)
                        {
                            SendCommand("3_Off");
                            relay3Off = true;
                        }

                        if (!relay4Off)
                        {
                            SendCommand("4_Off");
                            relay4Off = true;
                        }
                        pressureFinished = true;
                    }
                }
            }

            yield return null;
        }

        if (nowState == 3 || nowState == 4)
        {
            if (nowState == 4)
            {
                SendCommand("X");
            }

            currentTimer = 0f;
            SendCommand("AB_Down");
            while (currentTimer <= (startMove + 0.1f))
            {
                currentTimer += Time.deltaTime;
            }
            yield return null;
        }

        else
        {
            SendCommand("AB_Stop");
        }

        //実際に動いた時間だけ現在位置を更新
        currentPosition += currentDirection * currentTimer;

        //範囲を超えないようにする
        currentPosition = Mathf.Clamp(currentPosition, -maxMove, maxMove);

        currentDirection = 0;
        currentTimer = 0f;

        if (nowState == 3)
        {
            currentPosition = 0f;
        }

        currentRoutine = null;
    }

    //パラシュート提示
    IEnumerator ParachuteDeploy()
    {
        Debug.Log("パラシュート展開開始");
        SendCommand("C_Relay_NO");

        float timer = 0f;
        bool retractSent = false;
        bool stopSent = false;

        while (!stopSent)
        {
            timer += Time.deltaTime;

            // 秒後にC_Retract
            if (!retractSent && timer >= 0f)
            {
                SendCommand("C_Down");
                retractSent = true;
            }

            // 開始から6秒後にC_Stop
            if (!stopSent && timer >= 6.0f)
            {
                SendCommand("C_Stop");
                stopSent = true;

                Debug.Log("パラシュート展開終了");
                paratyakutiRoutine = null;
            }

            yield return null;
        }
    }

    //パラシュート再接地
    IEnumerator ParachuteReset(int state)
    {
        SendCommand("C_Relay_NO");
        float timer = 0f;
        bool stopSent = false;
        if (state == 1)
        {
            Debug.Log("パラシュート再設置開始");
            SendCommand("C_Up");
            while (!stopSent)
            {
                timer += Time.deltaTime;

                //2秒後
                if (timer >= 2.0f)
                {
                    SendCommand("C_Stop");
                    stopSent = true;

                    Debug.Log("パラシュート再設置終了");
                    paratyakutiRoutine = null;
                }

                yield return null;
            }
        }

        else if (state == 2)
        {
            Debug.Log("パラシュート縮む");
            SendCommand("C_Down");
            while (!stopSent)
            {
                timer += Time.deltaTime;

                //2秒後
                if (timer >= 3.0f)
                {
                    SendCommand("C_Stop");
                    stopSent = true;

                    Debug.Log("パラシュート再設置終了");
                    paratyakutiRoutine = null;
                }

                yield return null;
            }
        }
    }

    //着地感覚提示
    IEnumerator LandingImpact()
    {
        Debug.Log("着地感覚提示開始");
        SendCommand("C_Relay_NC");

        float timer = 0f;
        bool upSent = false;
        bool retractSent = false;
        bool stopSent = false;

        while (!stopSent)
        {
            timer += Time.deltaTime;

            if (!upSent && timer >= 0.8)
            {
                SendCommand("C_Up");
                upSent = true;
            }

            //5秒後
            if (!retractSent && timer >= 3.8f)
            {
                SendCommand("C_Down");
                retractSent = true;
            }

            //10秒後
            if (!stopSent && timer >= 8.0f)
            {
                SendCommand("C_Stop");
                stopSent = true;

                Debug.Log("着地感覚提示終了");
                paratyakutiRoutine = null;
            }

            yield return null;
        }
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
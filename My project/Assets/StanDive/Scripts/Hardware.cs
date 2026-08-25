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

    private Coroutine pressureRoutine;

    //飛び出し加圧max時間
    private float AirTimeLimit = 0.7f;
    //飛び出し加圧maxセンサー値
    private int sensorLimit0 = 500;

    //加圧時間
    private float timerUp1 = 0f;
    private float timerUp2 = 0f;
    private float timerUp3 = 0f;
    private float timerUp4 = 0f;

    private float timeLimitUp = 0.1f;

    //傾き
    //加圧傾き方向のmaxセンサー値
    private int sensorAMaxLimit = 600;
    //加圧傾きと逆のmaxセンサー値
    private int sensorBMaxLimit = 570;
    //加圧傾き方向のminセンサー値
    private int sensorAMinLimit = 550;
    //加圧傾きと逆のminセンサー値
    private int sensorBMinLimit = 520;

    //加圧右maxセンサー値
    private int sensorRMaxLimit = 0;
    //加圧左maxセンサー値
    private int sensorLMaxLimit = 0;
    //加圧右minセンサー値
    private int sensorRMinLimit = 0;
    //加圧左maxセンサー値
    private int sensorLMinLimit = 0;

    //パラシュート後、AirMoveを禁止する
    private bool airMoveLocked = false;

    public float hardTime = 0f;

    IEnumerator Start()
    {
        serial = new SerialPort("COM3", 115200);
        serial.ReadTimeout = 100;
        serial.Open();

        yield return new WaitForSeconds(1.0f);
    }

    void Update()
    {
        //センサー受信
        ReceiveSensor();

        //センサー値による常時安全制御
        if (AirMove)
        {
            CheckSensorSafety();
        }

        //アクチュエータ

        if (Finish)
        {
            //飛び出し（キー）
            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                CancelAutoMove();
                AirMove = true;
                currentRoutine = StartCoroutine(MoveToStop("AB_Up", startMove, 0, 0));
                //送信コマンド,アクチュエーター動作リミット時間,動作時間用ステータス,圧力用ステータス
            }

            //右移動（キー）
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                CancelAutoMove();
                if (pressureRoutine != null)
                {
                    StopCoroutine(pressureRoutine);
                }
                float limit = maxMove - currentPosition;
                if (AirMove)
                {
                    pressureRoutine = StartCoroutine(Air(2));
                }
                currentRoutine = StartCoroutine(MoveToStop("B_Up_A_Down", limit, 1, 2));
            }

            //左移動（キー）
            if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                CancelAutoMove();
                if (pressureRoutine != null)
                {
                    StopCoroutine(pressureRoutine);
                }
                float limit = maxMove + currentPosition;
                if (AirMove)
                {
                    pressureRoutine = StartCoroutine(Air(1));
                }
                currentRoutine = StartCoroutine(MoveToStop("A_Up_B_Down", limit, -1, 1));
            }

            //パラシュート（キー）
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                CancelAutoMove();
                if (pressureRoutine != null)
                {
                    StopCoroutine(pressureRoutine);
                }
                // 現在の傾きを確認
                float adjustTime = Mathf.Abs(currentPosition) * 2;

                // Aが高い場合
                if (currentPosition < 0)
                {
                    currentRoutine = StartCoroutine(MoveToStop("A_Down", adjustTime, 0, 3));
                }
                // Bが高い場合
                else if (currentPosition > 0)
                {
                    currentRoutine = StartCoroutine(MoveToStop("B_Down", adjustTime, 0, 3));
                }
                paratyakutiRoutine = StartCoroutine(ParachuteDeploy());

                //パラシュート後の左右傾き
                maxMove = 1.5f;
                startMove = maxMove;
            }

            //着地前横移動できなくなったタイミング
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                CancelAutoMove();
                // 現在の傾きを確認
                float adjustTime = Mathf.Abs(currentPosition) * 2;


                // Aが高い場合
                if (currentPosition < 0)
                {
                    currentRoutine = StartCoroutine(MoveToStop("A_Down", adjustTime, 0, 3));
                }
                // Bが高い場合
                else if (currentPosition > 0)
                {
                    currentRoutine = StartCoroutine(MoveToStop("B_Down", adjustTime, 0, 3));
                }
            }
        }

        //着地感覚提示（キー）
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            paratyakutiRoutine = StartCoroutine(LandingImpact());
            Finish = false;
        }

        //パラシュート再設置
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            paratyakutiRoutine = StartCoroutine(ParachuteReset());
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
            // 圧力制御停止
            if (pressureRoutine != null)
            {
                StopCoroutine(pressureRoutine);
                pressureRoutine = null;
            }

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
        if (pressureRoutine != null)
        {
            StopCoroutine(pressureRoutine);
        }
        float limit = maxMove - currentPosition;
        if (AirMove)
        {
            pressureRoutine = StartCoroutine(Air(2));
        }
        currentRoutine = StartCoroutine(MoveToStop("B_Up_A_Down", limit, 1, 2));
    }

    //左移動
    public void MoveLeft()
    {
        if (!Finish) return;
        // パラシュート処理中は右移動禁止
        if (paratyakutiRoutine != null) return;
        CancelAutoMove();
        if (pressureRoutine != null)
        {
            StopCoroutine(pressureRoutine);
        }
        float limit = maxMove + currentPosition;
        if (AirMove)
        {
            pressureRoutine = StartCoroutine(Air(1));
        }
        currentRoutine = StartCoroutine(MoveToStop("A_Up_B_Down", limit, -1, 1));
    }

    //パラシュート
    public void MoveParachute()
    {
        if (!Finish) return;
        CancelAutoMove();
        if (pressureRoutine != null)
        {
            StopCoroutine(pressureRoutine);
        }
        // 現在の傾きを確認
        float adjustTime = Mathf.Abs(currentPosition) * 2;

        hardTime = startMove + 0.1f + Mathf.Abs(currentPosition);

        // Aが高い場合
        if (currentPosition < 0)
        {
            currentRoutine = StartCoroutine(MoveToStop("A_Down", adjustTime, 0, 3));
        }
        // Bが高い場合
        else if (currentPosition > 0)
        {
            currentRoutine = StartCoroutine(MoveToStop("B_Down", adjustTime, 0, 3));
        }
        paratyakutiRoutine = StartCoroutine(ParachuteDeploy());

        //パラシュート後の左右傾き
        maxMove = 1.5f;
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
            currentRoutine = StartCoroutine(MoveToStop("A_Down", adjustTime, 0, 3));
        }
        // Bが高い場合
        else if (currentPosition > 0)
        {
            currentRoutine = StartCoroutine(MoveToStop("B_Down", adjustTime, 0, 3));
        }
        Finish = false;
    }

    //着地
    public void MoveLanding()
    {
        paratyakutiRoutine = StartCoroutine(LandingImpact());
    }

    //センサー値取得
    void ReceiveSensor()
    {
        if (serial != null && serial.IsOpen && serial.BytesToRead > 0)
        {
            try
            {
                string data = serial.ReadLine().Trim();

                if (data.StartsWith("FSR:"))
                {
                    data = data.Substring(4);

                    string[] d = data.Split(',');

                    if (d.Length == 4)
                    {
                        sensorData1 = int.Parse(d[0]);
                        sensorData2 = int.Parse(d[1]);
                        sensorData3 = int.Parse(d[2]);
                        sensorData4 = int.Parse(d[3]);

                        CorrectSensorValues();


                        Debug.Log(
                            sensorData1 + "," +
                            sensorData2 + "," +
                            sensorData3 + "," +
                            sensorData4
                        );

                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    void CorrectSensorValues()
    {
        // 1,3
        int correctRValue = 0;

        if (sensorData1 > 200)
        {
            correctRValue = sensorData1;
        }
        else if (sensorData3 > 200)
        {
            correctRValue = sensorData3;
        }

        if (sensorData1 <= 200 && correctRValue > 200)
            sensorData1 = correctRValue;

        if (sensorData3 <= 200 && correctRValue > 200)
            sensorData3 = correctRValue;

        // 2,4
        int correctLValue = 0;

        if (sensorData2 > 200)
        {
            correctLValue = sensorData2;
        }
        else if (sensorData4 > 200)
        {
            correctLValue = sensorData4;
        }

        if (sensorData2 <= 200 && correctLValue > 200)
            sensorData2 = correctLValue;

        if (sensorData4 <= 200 && correctLValue > 200)
            sensorData4 = correctLValue;

    }

    IEnumerator Air(int nowState)
    {
        float pressureTimer = 0f;

        //nowState == 1,2 の時間処理用
        bool CommandSent1 = false;
        bool CommandSent2 = false;
        bool CommandSent3 = false;
        bool CommandSent4 = false;

        relay1Off = false;//加圧
        relay2Off = false;
        relay3Off = false;
        relay4Off = false;

        SendCommand("7_Off");
        relay7Off = true;

        while (true)
        {
            pressureTimer += Time.deltaTime;

            // 常に圧力センサーを監視
            CheckPressure();

            if (nowState == 1)
            {
                //加圧右maxセンサー値
                sensorRMaxLimit = sensorAMaxLimit;
                //加圧左maxセンサー値
                sensorLMaxLimit = sensorBMaxLimit;
                //加圧右minセンサー値
                sensorRMinLimit = sensorAMinLimit;
                //加圧左maxセンサー値
                sensorLMinLimit = sensorBMinLimit;
            }

            else if (nowState == 2)
            {
                //加圧右maxセンサー値
                sensorRMaxLimit = sensorBMaxLimit;
                //加圧左maxセンサー値
                sensorLMaxLimit = sensorAMaxLimit;
                //加圧右minセンサー値
                sensorRMinLimit = sensorBMinLimit;
                //加圧左maxセンサー値
                sensorLMinLimit = sensorAMinLimit;
            }

            // 1.5秒で一度だけ
            if (!CommandSent1 && pressureTimer >= 1.5f)
            {
                CommandSent1 = true;
                PressureMove();
            }

            // 5.0秒で一度だけ
            if (!CommandSent2 && pressureTimer >= 5.0f)
            {
                CommandSent2 = true;
                PressureMove();
            }

            // 10.0秒で一度だけ
            if (!CommandSent3 && pressureTimer >= 10.0f)
            {
                CommandSent3 = true;
                PressureMove();
            }

            // 15.0秒で一度だけ
            if (!CommandSent4 && pressureTimer >= 15.0f)
            {
                CommandSent4 = true;
                PressureMove();
            }

            yield return null;
        }
    }

    IEnumerator MoveToStop(string startCommand, float timeLimit, int direction, int nowState)
    {
        SendCommand(startCommand);

        //現在の移動情報を保存
        currentDirection = direction;
        currentTimer = 0f;

        //圧力制御が終了したか
        bool pressureFinished = false;

        //飛び出し時
        if (nowState == 0 && AirMove)
        {
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

            //飛び出し
            if (AirMove)
            {
                if (nowState == 0 && !pressureFinished)
                {
                    if (currentTimer < AirTimeLimit)
                    {
                        //1
                        if (!relay1Off && sensorData1 >= sensorLimit0)
                        {
                            SendCommand("1_Off");
                            relay1Off = true;
                        }

                        //2
                        if (!relay2Off && sensorData2 >= sensorLimit0)
                        {
                            SendCommand("2_Off");
                            relay2Off = true;
                        }

                        //3
                        if (!relay3Off && sensorData3 >= sensorLimit0)
                        {
                            SendCommand("3_Off");
                            relay3Off = true;
                        }

                        //4
                        if (!relay4Off && sensorData4 >= sensorLimit0)
                        {
                            SendCommand("4_Off");
                            relay4Off = true;
                        }

                        if (relay1Off && relay2Off && relay3Off && relay4Off)
                        {
                            pressureFinished = true;
                        }
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

        if (nowState == 3)
        {
            currentTimer = 0f;
            SendCommand("AB_Down");
            while (currentTimer <= (startMove + 0.1f - (timeLimit / 2)))
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

    void PressureMove()
    {
        // 1
        if (relay1Off && sensorData1 <= sensorRMinLimit)
        {
            SendCommand("1_On");
            relay1Off = false;
            timerUp1 = 0f;
        }

        // 2
        if (relay2Off && sensorData2 <= sensorLMinLimit)
        {
            SendCommand("2_On");
            relay2Off = false;
            timerUp2 = 0f;
        }

        // 3
        if (relay3Off && sensorData3 <= sensorRMinLimit)
        {
            SendCommand("3_On");
            relay3Off = false;
            timerUp3 = 0f;
        }

        // 4
        if (relay4Off && sensorData4 <= sensorLMinLimit)
        {
            SendCommand("4_On");
            relay4Off = false;
            timerUp4 = 0f;
        }
    }

    void CheckPressure()
    {
        // 1
        if (!relay1Off)
        {
            timerUp1 += Time.deltaTime;

            if (sensorData1 >= sensorRMaxLimit || timerUp1 >= timeLimitUp)
            {
                SendCommand("1_Off");
                relay1Off = true;
            }
        }

        // 2
        if (!relay2Off)
        {
            timerUp2 += Time.deltaTime;

            if (sensorData2 >= sensorLMaxLimit || timerUp2 >= timeLimitUp)
            {
                SendCommand("2_Off");
                relay2Off = true;
            }
        }

        // 3
        if (!relay3Off)
        {
            timerUp3 += Time.deltaTime;

            if (sensorData3 >= sensorRMaxLimit || timerUp3 >= timeLimitUp)
            {
                SendCommand("3_Off");
                relay3Off = true;
            }
        }

        // 4
        if (!relay4Off)
        {
            timerUp4 += Time.deltaTime;

            if (sensorData4 >= sensorLMaxLimit || timerUp4 >= timeLimitUp)
            {
                SendCommand("4_Off");
                relay4Off = true;
            }
        }
    }

    //センサー値による常時制御
    void CheckSensorSafety()
    {
        //センサー値630を超えたら、そのセンサーに対応する電磁弁をOFF、7_On

        if (sensorData1 > 630)
        {
            SendCommand("1_Off");
            relay1Off = true;

            if (relay7Off)
            {
                SendCommand("7_On");
                relay7Off = false;
            }
        }

        if (sensorData2 > 630)
        {
            SendCommand("2_Off");
            relay2Off = true;

            if (relay7Off)
            {
                SendCommand("7_On");
                relay7Off = false;
            }
        }

        if (sensorData3 > 630)
        {
            SendCommand("3_Off");
            relay3Off = true;

            if (relay7Off)
            {
                SendCommand("7_On");
                relay7Off = false;
            }
        }

        if (sensorData4 > 630)
        {
            SendCommand("4_Off");
            relay4Off = true;

            if (relay7Off)
            {
                SendCommand("7_On");
                relay7Off = false;
            }
        }

        //570を下回ったら7_Off
        if (sensorData1 < 570 && sensorData2 < 570 && sensorData3 < 570 && sensorData4 < 570)
        {
            if (!relay7Off && AirMove)
            {
                SendCommand("7_Off");
                relay7Off = true;
            }
        }
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

            // 1秒後にC_Retract
            if (!retractSent && timer >= 1.0f)
            {
                SendCommand("C_Down");
                retractSent = true;
            }

            // 開始から4秒後にC_Stop
            if (!stopSent && timer >= 4.0f)
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
    IEnumerator ParachuteReset()
    {
        Debug.Log("パラシュート再設置開始");
        SendCommand("C_Relay_NO");

        float timer = 0f;
        bool stopSent = false;

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

    //着地感覚提示
    IEnumerator LandingImpact()
    {
        Debug.Log("着地感覚提示開始");
        SendCommand("C_Relay_NC");

        float timer = 0f;
        bool retractSent = false;
        bool stopSent = false;

        SendCommand("C_Up");

        while (!stopSent)
        {
            timer += Time.deltaTime;

            //5秒後
            if (!retractSent && timer >= 2.0f)
            {
                SendCommand("C_Down");
                retractSent = true;
            }

            //10秒後
            if (!stopSent && timer >= 5.0f)
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

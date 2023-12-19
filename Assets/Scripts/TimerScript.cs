using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerScript : MonoBehaviour
{
    public int countDownTime;
    public Server _server;
    void Start()
    {
        InvokeRepeating("CountDown", 0, 1);
    }

    private void CountDown()
    {
        countDownTime--;
        if (countDownTime == -1)
        {
            CancelInvoke("CountDown");
            _server.SendGameEnd();
            return;
        }
        _server.SendCountDown(countDownTime);

    }
}

using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : MonoBehaviour
{
    Player scriptPlayer;
    private StarterAssetsInputs starterAssetsInputs;

    void Awake()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        scriptPlayer = GetComponent<Player>();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (starterAssetsInputs.pass)
        {
            starterAssetsInputs.pass = false;
            scriptPlayer.Pass();
        }

        if (starterAssetsInputs.shoot)
        {
            if (scriptPlayer.HasBall)
            {
                scriptPlayer.ShootingPower += 1.5f * Time.deltaTime;
                Game.Instance.SetPowerBar(scriptPlayer.ShootingPower);
                if (scriptPlayer.ShootingPower > 1)
                {
                    scriptPlayer.ShootingPower = 1;
                }
            }
        }
        else if (scriptPlayer.ShootingPower>0)
        {
            scriptPlayer.Shoot();
        }
    }
}

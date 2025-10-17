using UnityEngine;
using UnityEngine.UI;


public class AbilityHUD : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public PlayerMovement playerMovement;

    public Image slideCoolDownBar;
    public Image stickCoolDownBar;
    public Image glideCoolDownBar;
    public Image chargeCoolDownBar;


    private void Start()
    {
        
        if (playerMovement == null) Debug.Log("playermovement null in abilityhud");
    }
    // Update is called once per frame
    void Update()
    {

        if (playerMovement == null) Debug.Log("playermovement null in abilityhud");

        slideCoolDownBar.fillAmount = playerMovement.slideLeft / playerMovement.maxSlideCharge;
        stickCoolDownBar.fillAmount = playerMovement.stickLeft / playerMovement.maxStickCharge;
        glideCoolDownBar.fillAmount = playerMovement.glideLeft / playerMovement.maxGlideCharge;
        chargeCoolDownBar.fillAmount = playerMovement.chargeTime / playerMovement.maxChargeTime;
    }
}

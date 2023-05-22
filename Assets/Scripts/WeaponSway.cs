using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponSway : MonoBehaviour
{
    public float swaySensibility = 0.02f;
    public float swayClamp = 20f;
    public float swaySmothness = 20f;

    private Vector3 startPosition;
    private Vector3 nextPosition;
    private Vector3 currentVelocity = Vector3.zero;
    public PhotonView photonView;
    void Start()
    {
        startPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }
        float mouseX = Input.GetAxis("Mouse X")*swaySensibility*Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * swaySensibility * Time.deltaTime;

        mouseX = Mathf.Clamp(mouseX, -swayClamp, swayClamp);
        mouseY = Mathf.Clamp(mouseY, -swayClamp, swayClamp);

        nextPosition=new Vector3(mouseX, mouseY,0);
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, nextPosition+startPosition,ref currentVelocity,swaySmothness*Time.deltaTime);
    }
}

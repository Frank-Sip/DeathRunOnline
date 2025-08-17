using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private LayerMask collisionMask;
    
    private PhotonView photonView;

    public PhotonView PhotonView => photonView ?? GetComponent<PhotonView>();

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (PhotonView.IsMine)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

            transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!PhotonView.IsMine) return;

        if ((collisionMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            PhotonView.RPC("RPC_InformCollision", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName);
        }
    }
    
    [PunRPC]
    public void RPC_InformCollision(string playerName)
    {
        Debug.Log($"{playerName} has collided with an object.");
    }

    [ContextMenu("GetID")]
    public void PrintID()
    {
        print(PhotonView.ViewID);
        print(PhotonNetwork.NickName);
    }
}

using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public interface ITrap
{
    [PunRPC]
    void RPC_ActivateTrap();
}

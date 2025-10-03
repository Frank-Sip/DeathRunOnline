using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BallController : MonoBehaviourPun
{
    [Header("Movement Settings")]
    [SerializeField] private float initialSpeed = 5f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float speedIncrease = 0.2f;

    [Header("Boundaries")]
    [SerializeField] private float topBoundary = 4.5f;
    [SerializeField] private float bottomBoundary = -4.5f;
    [SerializeField] private float leftBoundary = -9f;
    [SerializeField] private float rightBoundary = 9f;

    private Vector3 velocity;
    private float currentSpeed;

    private void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(InitialDelay());
        }
    }

    private IEnumerator InitialDelay()
    {
        yield return new WaitForSeconds(1f);
        ResetBall();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        MoveBall();
        CheckBoundaries();
    }

    private void MoveBall()
    {
        transform.position += velocity * Time.deltaTime;
    }

    private void ResetBall()
    {
        transform.position = Vector3.zero;
        currentSpeed = initialSpeed;

        // Dirección aleatoria inicial
        float randomAngle = Random.Range(-45f, 45f);
        float direction = Random.value > 0.5f ? 1f : -1f;

        velocity = Quaternion.Euler(0, 0, randomAngle) * Vector3.right * direction * currentSpeed;
    }

    private void CheckBoundaries()
    {
        Vector3 pos = transform.position;

        // Rebote superior e inferior
        if (pos.y >= topBoundary && velocity.y > 0)
        {
            velocity.y = -velocity.y;
            pos.y = topBoundary;
            transform.position = pos;
        }
        else if (pos.y <= bottomBoundary && velocity.y < 0)
        {
            velocity.y = -velocity.y;
            pos.y = bottomBoundary;
            transform.position = pos;
        }

        // Gol en el lado izquierdo (punto para Team 2)
        if (pos.x <= leftBoundary)
        {
            photonView.RPC("RPC_ScorePoint", RpcTarget.MasterClient, 2);
        }
        // Gol en el lado derecho (punto para Team 1)
        else if (pos.x >= rightBoundary)
        {
            photonView.RPC("RPC_ScorePoint", RpcTarget.MasterClient, 1);
        }
    }

    [PunRPC]
    private void RPC_ScorePoint(int team)
    {
        if (PhotonNetwork.IsMasterClient && GameManager2.Instance != null)
        {
            GameManager2.Instance.AddScore(team);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;

        if (collision.CompareTag("Paddle"))
        {
            HandlePaddleCollision(collision);
        }
    }

    private void HandlePaddleCollision(Collider2D paddle)
    {
        // Cambiar dirección horizontal
        velocity.x = -velocity.x;

        // Calcular ángulo según posición de impacto
        float paddleHeight = paddle.bounds.size.y;
        float relativeIntersectY = transform.position.y - paddle.transform.position.y;
        float normalizedIntersect = relativeIntersectY / (paddleHeight / 2f);

        // Ajustar ángulo vertical basado en dónde golpea
        float bounceAngle = normalizedIntersect * 60f; // Máximo 60 grados

        // Incrementar velocidad
        currentSpeed = Mathf.Min(currentSpeed + speedIncrease, maxSpeed);

        // Aplicar nueva dirección
        float direction = Mathf.Sign(velocity.x);
        velocity = Quaternion.Euler(0, 0, bounceAngle) * Vector3.right * direction * currentSpeed;

        // Alejar la bola de la paleta para evitar colisiones múltiples
        Vector3 pushDirection = (transform.position - paddle.transform.position).normalized;
        transform.position += pushDirection * 0.1f;
    }

    // Sincronizar posición para clientes
    private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(velocity);
            stream.SendNext(currentSpeed);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            velocity = (Vector3)stream.ReceiveNext();
            currentSpeed = (float)stream.ReceiveNext();
        }
    }
}
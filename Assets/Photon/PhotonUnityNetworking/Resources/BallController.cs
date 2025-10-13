using UnityEngine;
using Photon.Pun;

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
            photonView.RPC("RPC_LaunchBall", RpcTarget.AllBuffered);
        }
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

    [PunRPC]
    public void RPC_LaunchBall()
    {
        if (photonView.IsMine)
        {
            currentSpeed = initialSpeed;

            float randomAngle = Random.Range(-45f, 45f);
            float direction = Random.value > 0.5f ? 1f : -1f;

            velocity = Quaternion.Euler(0, 0, randomAngle) * Vector3.right * direction * currentSpeed;
        }
    }

    private void CheckBoundaries()
    {
        Vector3 pos = transform.position;

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

        if (pos.x <= leftBoundary)
        {
            OnGoalScored(2);
        }
        else if (pos.x >= rightBoundary)
        {
            OnGoalScored(1);
        }
    }

    private void OnGoalScored(int team)
    {
        photonView.RPC("RPC_ScorePoint", RpcTarget.AllBuffered, team);
    }

    [PunRPC]
    private void RPC_ScorePoint(int team)
    {
        if (PhotonNetwork.IsMasterClient && GameManager2.Instance != null)
        {
            GameManager2.Instance.OnGoalScored(team);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!photonView.IsMine) return;

        PaddleController paddle = collision.gameObject.GetComponent<PaddleController>();
        
        if (paddle != null)
        {
            HandlePaddleCollision(paddle);
        }
    }

    private void HandlePaddleCollision(PaddleController paddle)
    {
        velocity.x = -velocity.x;

        float paddleHeight = paddle.GetComponent<Collider2D>().bounds.size.y;
        float relativeIntersectY = transform.position.y - paddle.transform.position.y;
        float normalizedIntersect = relativeIntersectY / (paddleHeight / 2f);

        float bounceAngle = normalizedIntersect * 60f;

        currentSpeed = Mathf.Min(currentSpeed + speedIncrease, maxSpeed);

        float direction = Mathf.Sign(velocity.x);
        velocity = Quaternion.Euler(0, 0, bounceAngle) * Vector3.right * direction * currentSpeed;

        Vector3 pushDirection = (transform.position - paddle.transform.position).normalized;
        transform.position += pushDirection * 0.1f;
    }


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
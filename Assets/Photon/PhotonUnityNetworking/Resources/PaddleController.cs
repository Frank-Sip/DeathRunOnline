using UnityEngine;
using Photon.Pun;

public class PaddleController : MonoBehaviourPun
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    [Header("Input Settings")]
    [SerializeField] private KeyCode upKey = KeyCode.W;
    [SerializeField] private KeyCode downKey = KeyCode.S;

    private int teamNumber;
    private bool isLocalPlayer;

    private void Start()
    {
        isLocalPlayer = photonView.IsMine;

        // Desactivar collisión entre paletas
        gameObject.layer = LayerMask.NameToLayer("Paddle");
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        float verticalInput = 0f;

        if (Input.GetKey(upKey))
            verticalInput = 1f;
        else if (Input.GetKey(downKey))
            verticalInput = -1f;

        if (verticalInput != 0f)
        {
            Vector3 movement = Vector3.up * verticalInput * moveSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + movement;

            // Limitar movimiento
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            transform.position = newPosition;
        }
    }

    public void SetTeam(int team)
    {
        teamNumber = team;
    }

    public int GetTeam()
    {
        return teamNumber;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
		// Las paletas no colisionan entre ellas.
        if (collision.gameObject.CompareTag("Paddle"))
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
        }
    }
}

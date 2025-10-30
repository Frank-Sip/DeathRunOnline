using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;

public class ChuckNorrisJokeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text jokeText;
    [SerializeField] private Image avatarImage; 

    private const string API_URL = "https://api.chucknorris.io/jokes/random";
    private bool isLoading = false;

    void Start()
    {
        GetNewJoke();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isLoading)
        {
            GetNewJoke();
        }
    }

    public void GetNewJoke()
    {
        if (!isLoading)
        {
            StartCoroutine(FetchJokeFromAPI());
        }
    }

    private IEnumerator FetchJokeFromAPI()
    {
        isLoading = true;
        jokeText.text = "Cargando chiste...";

        using (UnityWebRequest request = UnityWebRequest.Get(API_URL))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;

                ChuckNorrisJoke joke = JsonConvert.DeserializeObject<ChuckNorrisJoke>(jsonResponse);


                if (joke != null && !string.IsNullOrEmpty(joke.value))
                {
                    jokeText.text = joke.value;
                    StartCoroutine(LoadIcon(joke.icon_url));
                }
                else
                {
                    jokeText.text = "Error: No se pudo obtener el chiste.";
                }
            }
            else
            {
                jokeText.text = $"Error: {request.error}";
            }
        }

        isLoading = false;
    }

    private IEnumerator LoadIcon(string iconUrl)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(iconUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);

                Sprite iconSprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                avatarImage.sprite = iconSprite;
            }
        }
    }
}

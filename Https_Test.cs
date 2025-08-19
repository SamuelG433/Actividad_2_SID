using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class HttpTest : MonoBehaviour
{
    [SerializeField]

    private string APIUrl = "https://my-json-server.typicode.com/SamuelG433/Actividad_2_SID/users/";
    private string RicKandMortyUrl = "https://rickandmortyapi.com/api/character/";

    [SerializeField]
    private RawImage rawImage;

    [SerializeField] private RawImage[] rawImages = new RawImage[4];
    [SerializeField] private TMP_Text[] nameTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text userText;
    [SerializeField] private Button nextButton;
    [SerializeField] private int[] availableUserIds = new int[] { 1, 2, 3};

    private int currentUserId;
    private User currentUser;

    void Start()
    {
        currentUserId = availableUserIds[Random.Range(0, availableUserIds.Length)];
        if (nextButton) nextButton.onClick.AddListener(Next);
        StartCoroutine(GetUser(currentUserId));
     
    }

    IEnumerator GetUser(int userId)
    {
        UnityWebRequest request = UnityWebRequest.Get(APIUrl + userId);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                string json = request.downloadHandler.text;
                currentUser = JsonUtility.FromJson<User>(json);
                if (userText) userText.text = "Usuario: " + currentUser.username;
                ShowRandomFromDeck(5);
            }
            else
            {
                string mensaje = "status:" + request.responseCode + "\nError: " + request.error;
                Debug.Log(mensaje);
            }
        }
    }

    IEnumerator GetCharacter(int characterId)
    {
        UnityWebRequest request = UnityWebRequest.Get(RicKandMortyUrl + characterId);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                string json = request.downloadHandler.text;
                Character character = JsonUtility.FromJson<Character>(json);
                StartCoroutine(GetImage(character.image));
            }
            else
            {
                string mensaje = "status:" + request.responseCode + "\nError: " + request.error;
                Debug.Log(mensaje);
            }
        }
    }

    IEnumerator GetCharacterToSlot(int characterId, int slotIndex)
    {
        UnityWebRequest request = UnityWebRequest.Get(RicKandMortyUrl + characterId);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                string json = request.downloadHandler.text;
                Character character = JsonUtility.FromJson<Character>(json);

                if (slotIndex >= 0 && slotIndex < nameTexts.Length && nameTexts[slotIndex])
                    nameTexts[slotIndex].text = character.name;

                StartCoroutine(GetImageToSlot(character.image, slotIndex));
            }
            else
            {
                string mensaje = "status:" + request.responseCode + "\nError: " + request.error;
                Debug.Log(mensaje);
            }
        }
    }

    IEnumerator GetImage(string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                rawImage.texture = texture;
            }
            else
            {
                string mensaje = "status:" + request.responseCode + "\nError: " + request.error;
                Debug.Log(mensaje);
            }
        }
    }

    IEnumerator GetImageToSlot(string imageUrl, int slotIndex)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                if (slotIndex >= 0 && slotIndex < rawImages.Length && rawImages[slotIndex])
                    rawImages[slotIndex].texture = texture;
            }
            else
            {
                string mensaje = "status:" + request.responseCode + "\nError: " + request.error;
                Debug.Log(mensaje);
            }
        }
    }

    void ShowRandomFromDeck(int k)
    {
        if (currentUser == null || currentUser.deck == null || currentUser.deck.Length == 0) return;

        for (int i = 0; i < rawImages.Length; i++)
        {
            if (i < nameTexts.Length && nameTexts[i]) nameTexts[i].text = "Cargando...";
            if (rawImages[i]) rawImages[i].texture = null;
        }

        int need = Mathf.Min(k, Mathf.Min(rawImages.Length, nameTexts.Length));
        var picked = new System.Collections.Generic.HashSet<int>();
        int tries = 0;

        for (int i = 0; i < need; i++)
        {
            int pick;
            do { pick = currentUser.deck[Random.Range(0, currentUser.deck.Length)]; tries++; }
            while (picked.Contains(pick) && tries < 100);
            picked.Add(pick);
            StartCoroutine(GetCharacterToSlot(pick, i));
        }
    }

    public void Next()
    {
        if (availableUserIds == null || availableUserIds.Length == 0) return;
        int newId = currentUserId;
        int safe = 0;
        while (newId == currentUserId && safe < 20)
        {
            newId = availableUserIds[Random.Range(0, availableUserIds.Length)];
            safe++;
        }
        currentUserId = newId;
        StartCoroutine(GetUser(currentUserId));
    }
}

public class Character
{
    public int id;
    public string name;
    public string species;
    public string image;
}

public class User
{
    public int id;
    public string username;
    public string state;
    public int[] deck;
}

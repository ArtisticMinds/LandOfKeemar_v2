using Firebase;
using Firebase.Messaging;
using UnityEngine;
using System.Threading.Tasks; // Aggiungi questo per Task

public class FirebaseMessagingManager : MonoBehaviour
{
    private const string TAG = "FirebaseFCM"; // Tag per i log

    void Awake()
    {
      //  Debug.Log($"[{TAG}] Awake: Checking and fixing Firebase dependencies...");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
              //  Debug.Log($"[{TAG}] Firebase è stato inizializzato con successo.");
                InitializeFirebaseMessaging();
            }
            else
            {
                Debug.LogError(
                    $"[{TAG}] Could not resolve all Firebase dependencies: {dependencyStatus}");
                // Firebase non è disponibile per l'uso.
            }
        });
    }

    void InitializeFirebaseMessaging()
    {
       // Debug.Log($"[{TAG}] Initializing Firebase Messaging.");
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;

        // Richiedi il token di registrazione FCM
        GetFCMToken();
    }

    async void GetFCMToken()
    {
       // Debug.Log($"[{TAG}] Getting FCM registration token...");
        Task<string> tokenTask = FirebaseMessaging.GetTokenAsync();
        await tokenTask;

        if (tokenTask.IsCompleted)
        {
            string token = tokenTask.Result;
            Debug.LogFormat($"[{TAG}] FCM registration token: {0}", token);
            // Questo token è unico per ogni installazione della app su un dispositivo.
            // È FONDAMENTALE che questo token venga generato .

        }
        else if (tokenTask.IsFaulted)
        {
            Debug.LogErrorFormat($"[{TAG}] Failed to get FCM registration token: {0}", tokenTask.Exception);
        }
    }

    public void OnTokenReceived(object sender, TokenReceivedEventArgs tokenInfo)
    {
      //  Debug.LogFormat($"[{TAG}] Received Registration Token via event: {0}", tokenInfo.Token);
    }

    public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log($"[{TAG}] Received a new message!");
        if (e.Message.From != null && e.Message.From.Length > 0)
            Debug.Log($"[{TAG}] From: {e.Message.From}");
        if (e.Message.Data != null && e.Message.Data.Count > 0)
        {
         //   Debug.Log($"[{TAG}] Data:");
            foreach (System.Collections.Generic.KeyValuePair<string, string> iter in e.Message.Data)
            {
                Debug.Log($"[{TAG}]   {iter.Key}: {iter.Value}");
            }
        }
        if (e.Message.Notification != null)
        {
            Debug.Log($"[{TAG}] Notification Title: {e.Message.Notification.Title}");
            Debug.Log($"[{TAG}] Notification Body: {e.Message.Notification.Body}");
        }
        // Qui puoi aggiungere la logica per mostrare la notifica o elaborare i dati
        // Ad esempio, mostrare un messaggio in UI:
        // YourNotificationDisplayManager.DisplayNotification(e.Message.Notification.Title, e.Message.Notification.Body);
    }

    void OnDestroy()
    {
        // Rimuovi gli handler quando l'oggetto viene distrutto per evitare memory leaks
        // La verifica di null non è necessaria per la rimozione di handler da eventi.
        FirebaseMessaging.TokenReceived -= OnTokenReceived;
        FirebaseMessaging.MessageReceived -= OnMessageReceived;
    }
}

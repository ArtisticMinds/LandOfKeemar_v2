using BarcodeScanner;
using BarcodeScanner.Scanner;
using System;
using System.Collections;
using System.IO;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.UI;
using Wizcorp.Utils.Logger;
using ZXing.PDF417.Internal;

//Sript che si trova su QRCamera
public class QR_ScanCode : MonoBehaviour {

	private IScanner BarcodeScanner;
    public static string deviceID;
    public static CanvasGroup camGroup;
    public TMP_Text TextHeader;
    public RawImage CameraImage;
	public AudioSource Audio;
    public AudioClip ScanOk;

    public AudioClip Error;
    public Button Scannerizza;
    public Button Stop;
    public RectTransform foregr;
    [Space(10)]
    public string[] CorrectCodes;
    //Array che contiene i riferimenti a tutti i TappaMapMarker presenti nella scena si trova su QRCamera
    //La posizione nella lista corrisponde al sui QR Code (e.s. ID0 --> Tappa1, ID1-->Tappa2, etc)
    public TappaMapMarker[] tappaMaker;
    public static TappaMapMarker lastTappaMapMarker;
    public static string lastBarCodeValue;
    public TMP_Text loacationText;
    public GameObject fadeToBlack;

    private Animator anim;

    // Disable Screen Rotation on that screen
    void Awake()
	{
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Stop.gameObject.SetActive(false);
        Scannerizza.interactable = true;
        deviceID = SystemInfo.deviceUniqueIdentifier;
        deviceID = deviceID.Substring(0, 16);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        camGroup = CameraImage.GetComponent<CanvasGroup>();
        fadeToBlack.SetActive(false);
        loacationText.text = "";
        camGroup.alpha = 0;
    }



	void Start () {


        lastBarCodeValue = "";
        lastTappaMapMarker=null;
        anim = MainMenu.instance.QR_ScanPanel.GetComponent<Animator>(); //L'animator si trova sul pannello mentre questo script sulla camera


        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }


        StartCoroutine(InitializeCamera());

	}

    IEnumerator InitializeCamera()
    {
        // Wait a bit
        yield return new WaitForSeconds(0.1f);
        // Create a basic scanner
        BarcodeScanner = new Scanner();
        BarcodeScanner.Camera.Play();
        camGroup.alpha = 1;

        // Display the camera texture through a RawImage
        BarcodeScanner.OnReady += (sender, arg) => {
            // Set Orientation & Texture
            CameraImage.transform.localEulerAngles = BarcodeScanner.Camera.GetEulerAngles();
            CameraImage.transform.localScale = BarcodeScanner.Camera.GetScale();
            CameraImage.texture = BarcodeScanner.Camera.Texture;

            // Keep Image Aspect Ratio
            var rect = CameraImage.GetComponent<RectTransform>();
            var newHeight = rect.sizeDelta.x * BarcodeScanner.Camera.Height / BarcodeScanner.Camera.Width;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, newHeight);
            foregr.sizeDelta = new Vector2(rect.sizeDelta.x + 3, newHeight);
            foregr.rotation = rect.rotation;
            foregr.gameObject.SetActive(false);
        };

        // Track status of the scanner
        BarcodeScanner.StatusChanged += (sender, arg) => {
            TextHeader.text = "Stato: " + BarcodeScanner.Status;

            if (BarcodeScanner.Status.Equals(ScannerStatus.Running))
            {
                TextHeader.text = "INQUADRA IL CODICE";
                Stop.gameObject.SetActive(true);
                foregr.gameObject.SetActive(true);
                Scannerizza.gameObject.SetActive(false);
            }
            else

            if (BarcodeScanner.Status.Equals(ScannerStatus.Paused))
            {
                TextHeader.text = "PREMI \"START\"";
                Stop.gameObject.SetActive(false);
                foregr.gameObject.SetActive(false);
                Scannerizza.gameObject.SetActive(true);
            }
            else

            if (BarcodeScanner.Status.Equals(ScannerStatus.Initialize))
            {
                TextHeader.text = "PREMI \"START\"";
                Stop.gameObject.SetActive(false);
                foregr.gameObject.SetActive(false);
                Scannerizza.gameObject.SetActive(true);
            }
        };
    }

    public void OpenInstructions()
    {
       anim.SetTrigger("OpenInstructions");
    }

    void Update()
	{


       //f (!connectionON) return;


        if (BarcodeScanner == null)
		{
			return;
		}
		BarcodeScanner.Update();

       
	}

    bool CheckCodes(string barCodeValue)
    {
        foreach(string code in CorrectCodes)
        {
            if (barCodeValue.Equals(code))
                return true;
        }

        return false;
    }

	#region UI Buttons

	public void ClickStart()
	{

		if (BarcodeScanner == null)
        {
            TextHeader.text = "<color=#FF0000>CAMERA NON AVVIATA</color>\nPROVA A RIAVVIARE L'APPLICAZIONE";
            Log.Warning("No valid camera - Click Start");
            foregr.gameObject.SetActive(false);
            return; 

        }

		// Start Scanning
		BarcodeScanner.Scan((barCodeType, barCodeValue) => {
			BarcodeScanner.Stop();
            if (CheckCodes(barCodeValue))
            {
                StopCamera(null);
                CodiceAcquisito(barCodeValue);
                TextHeader.text = "<color=#FFFFAA>CODICE ACQUISITO!</color>"; //"Found: " + barCodeType + " / " + barCodeValue;
                Stop.gameObject.SetActive(false);

                foregr.gameObject.SetActive(false);
                Scannerizza.interactable = false;
               
                // Feedback
                if(ScanOk)
                AudioManager.instance.PlayAudioClip(ScanOk);

                Debug.Log("Codice: " + barCodeValue );
                Debug.Log("BarCodeType: " + barCodeType);
            }
            else 
            {
                Stop.gameObject.SetActive(false);
                Scannerizza.interactable = true;
                TextHeader.text = "<color=#FF0000>CODICE NON VALIDO</color> \nRIPROVA"; //"Found: " + barCodeType + " / " + barCodeValue;

                Debug.Log("Codice: " + barCodeValue);
                Debug.Log("BarCodeType: " + barCodeType);

                if (Error)
                AudioManager.instance.PlayAudioClip(Error);
            }

#if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
			#endif
		});
	}




    //Eseguito dopo aver scanzionato correttamente un QR
    //Apre il pannello con le info sulla tappa e salva il codice acquisito in una variabile statica (lastBarCodeValue) per poterlo usare dopo quando si apre la mappa
    public void CodiceAcquisito(string barCodeValue)
    {
        Debug.Log("CodiceAcquisito");
        //Apre il pannello con le info sulla tappa
        anim.SetTrigger("OpenLocationInfos");
        //Salva il codice acquisito in una variabile statica per poterlo usare dopo quando si apre la mappa
        lastBarCodeValue = barCodeValue;

        GetMarkerByLastBarCodeValue();

        //Imposta la tappa come giocabile(isOpen = true) e salva lo stato su PlayerPrefs (va eseguito dopo GetMarkerByLastBarCodeValue)
        lastTappaMapMarker.tappa.OpenTappa();

    }



    //Trova il marker corrispondente al codice acquisito (lastBarCodeValue) e salva il riferimento in una variabile statica (lastTappaMapMarker) per poterlo usare dopo quando si apre la mappa
    public void GetMarkerByLastBarCodeValue()
    {
        int markerID = -1;
        markerID=(lastBarCodeValue.Substring(lastBarCodeValue.Length - 1, 1)[0] - '0')-1; //Prende l'ultimo carattere del codice acquisito e lo converte in numero -1 (es. "Tappa1" -> 0 "Tappa2" -> 1)
        Debug.Log("markerID: "+ markerID);

        lastTappaMapMarker = tappaMaker[markerID];
        loacationText.text = lastTappaMapMarker.tappa.tappaName;

        //NOTA il riferimento tra QR e tappa va in base alla sua posizione in TappaMaker, su QRCamera
    }


    //Da pulsante "PROSEGUI" dopo aver scanzionato correttamente un QR (salvato su lastBarCodeValue)
    public void OpenMapFromQR() 
    {
        Debug.Log("OpenMapFromQR");

        Stop.gameObject.SetActive(false);
        Scannerizza.interactable = true; //Torna ad essere attivo per una eventuale prossima scansione
        TextHeader.text = "PREMI \"START\""; //Torna al testo iniziale per una eventuale prossima scansione
        fadeToBlack.SetActive(true);
        StartCoroutine(OpenMapInfoCoroutine());
    }




    IEnumerator OpenMapInfoCoroutine()
    {
        Debug.Log("OpenMapInfoCoroutine");

        anim.enabled = false;
        yield return new WaitForSeconds(0.6F);
      

        //Imposta la mappa e apre il libro
        lastTappaMapMarker.SetTappa();
      

        yield return new WaitForSeconds(0.8F);
        CloseQRPanel();
        anim.enabled = true;
    }

    [ContextMenu("Debug Test Scan")]
    public void DebugTestScan()
    {
 
        StopCamera(null);
        CodiceAcquisito("Tappa2");
        TextHeader.text = "<color=#FF3333>CODICE ACQUISITO!</color>"; //"Found: " + barCodeType + " / " + barCodeValue;
        Stop.gameObject.SetActive(false);

        foregr.gameObject.SetActive(false);
        Scannerizza.interactable = false;
        // Feedback
        Audio.PlayOneShot(ScanOk);
    }

    public void CloseQRPanel()
    {
        MainMenu.instance.QR_ScanPanel.gameObject.SetActive(false);
        // Stop Scanning
        BarcodeScanner.Stop();
        lastBarCodeValue = "";
        loacationText.text = "";
        lastTappaMapMarker = null;
        fadeToBlack.SetActive(false);
    }



	public void ClickStop()
	{
		if (BarcodeScanner == null)
		{
			Log.Warning("No valid camera - Click Stop");
			return;
		}

		// Stop Scanning
		BarcodeScanner.Stop();
	}



	/// <summary>
	/// This coroutine is used because of a bug with unity (http://forum.unity3d.com/threads/closing-scene-with-active-webcamtexture-crashes-on-android-solved.363566/)
	/// Trying to stop the camera in OnDestroy provoke random crash on Android
	/// </summary>
	/// <param name="callback"></param>
	/// <returns></returns>
	public IEnumerator StopCamera(Action callback)
	{
        // Stop Scanning
        BarcodeScanner.Camera.Stop();
       CameraImage = null;
		BarcodeScanner.Destroy();
		BarcodeScanner = null;

		// Wait a bit
		yield return new WaitForSeconds(0.2f);

		callback.Invoke();
	}

	#endregion
}

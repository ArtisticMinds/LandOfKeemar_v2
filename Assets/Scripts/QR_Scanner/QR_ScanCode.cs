using BarcodeScanner;
using BarcodeScanner.Scanner;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Wizcorp.Utils.Logger;
using TMPro;
using UnityEngine.Android;
using System.IO;

public class QR_ScanCode : MonoBehaviour {

	private IScanner BarcodeScanner;
    public static string deviceID;
    public static CanvasGroup camGroup;
    public TMP_Text TextHeader;
    public RawImage CameraImage;
	public AudioSource Audio;
    public AudioClip ScanOk;
    public AudioClip InvioOk;
    public AudioClip Error;
    public AudioClip RegOk;
    public Button Scannerizza;
    public Button Stop;
    public RectTransform foregr;
    [Space(10)]
    public string[] CorrectCodes;
    public TappaMapMarker[] tappaMaker;
    public static string lastBarCodeValue;

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

        
        camGroup.alpha = 0;
    }



	void Start () {


        lastBarCodeValue = "";

       // StartCoroutine(CheckConnection());

        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

       // StartCoroutine(GetCorrectCode());
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



 


    //Texture2D texture1;
    //IEnumerator TakePhoto()  // Start this Coroutine on some button click
    //{

    //    texture1= ScreenCapture.CaptureScreenshotAsTexture();
    //    //So that the screenshot is taken
    //    yield return new WaitForEndOfFrame();
    //    yield return new WaitForSecondsRealtime(1.5f);
    //    SaveTextureAsPNG(texture1, Application.persistentDataPath+"\\shot.PNG");
    //}

    //public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    //{//first Make sure you're using RGB24 as your texture format
    //    Texture2D texture = new Texture2D(1024, 1024, TextureFormat.RGB24, false);

    //    //then Save To Disk as PNG
    //    byte[] bytes = texture.EncodeToPNG();

    //    if (!Directory.Exists(_fullPath))
    //    {
    //        Directory.CreateDirectory(_fullPath);
    //    }
    //    File.WriteAllBytes(_fullPath, bytes);

    //    //File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
    //    //byte[] _bytes = _texture.EncodeToPNG();
    //    //System.IO.File.WriteAllBytes(_fullPath, _bytes);
    //    //Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
    //}

    /// <summary>
    /// The Update method from unity need to be propagated to the scanner
    /// </summary>
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
            TextHeader.text = "<color=#FAAA55>CAMERA NON AVVIATA</color>\nPROVA A RIAVVIARE L'APPLICAZIONE";
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
                OpenInfos(barCodeValue);
                TextHeader.text = "<color=#FF3333>CODICE ACQUISITO!</color>"; //"Found: " + barCodeType + " / " + barCodeValue;
                Stop.gameObject.SetActive(false);

                foregr.gameObject.SetActive(false);
                Scannerizza.interactable = false;
                // Feedback
                Audio.PlayOneShot(ScanOk);
            }
            else {
                Stop.gameObject.SetActive(false);
                Scannerizza.interactable = true;
                TextHeader.text = "<color=#FAAA55>CODICE NON VALIDO</color> \nRIPROVA"; //"Found: " + barCodeType + " / " + barCodeValue;
                Audio.PlayOneShot(Error);
            }

#if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
			#endif
		});
	}

    public void OpenInfos(string barCodeValue)
    {
        MainMenu.instance.QR_ScanPanel.GetComponent<Animator>().SetTrigger("OpenLocationInfos");

        lastBarCodeValue = barCodeValue;
    }

    
    public void OpenMapFromQR() 
    {
        MainMenu.instance.mapPanel.gameObject.SetActive(true);

        Stop.gameObject.SetActive(false);
        Scannerizza.interactable = true;
        TextHeader.text = "PREMI \"START\"";

        
        StartCoroutine(OpenMapInfo());
    }



    IEnumerator OpenMapInfo()
    {
        MainMenu.instance.QR_ScanPanel.GetComponent<Animator>().enabled = false;
        yield return new WaitForSeconds(0.1F);

        if (lastBarCodeValue.Equals("ROCCA VARANO"))
            tappaMaker[0].SetTappa();
        if (lastBarCodeValue.Equals("MAGALOTTI"))
            tappaMaker[1].SetTappa();
        if (lastBarCodeValue.Equals("BORGIA"))
            tappaMaker[0].SetTappa();

        yield return new WaitForSeconds(0.5F);
        CloseQRPanel();
        MainMenu.instance.QR_ScanPanel.GetComponent<Animator>().enabled = true;
    }

    public void CloseQRPanel()
    {
        MainMenu.instance.QR_ScanPanel.gameObject.SetActive(false);
        lastBarCodeValue = "";
        // Stop Scanning
        BarcodeScanner.Stop();
    }

    //public static bool connectionON;
    //public IEnumerator CheckConnection()
    //{
    //    //Spedisci dati al database
    //    WWWForm form = new WWWForm();


    //    using (UnityWebRequest www = UnityWebRequest.Get("http://wilez.it/timbratureEskigel/scrivi_timbratura.php"))
    //    {
    //        yield return www.SendWebRequest();
    //        Debug.Log("output:  " + www.downloadHandler.text);

    //        if (www.isNetworkError || www.isHttpError)
    //        {
    //            TextHeader.text = "E' AVVENUTO UN ERRORE\nControlla la tua connessione.";
    //            Audio.PlayOneShot(Error);
    //            connectionON = false;
    //            yield break;
    //        }
    //    }
    //    connectionON = true;

    //}

    //public void ClickSendAndClose() {

    //    StartCoroutine(Send());

    //}

    //IEnumerator GetCorrectCode()
    //{
    //    if (!connectionON) yield break;

    //    //Spedisci dati al database
    //    WWWForm form = new WWWForm();
    //    using (UnityWebRequest www = UnityWebRequest.Get("http://wilez.it/timbratureEskigel/getCorrectCode.php"))
    //    {
    //        yield return www.SendWebRequest();

    //        if (www.isNetworkError || www.isHttpError)
    //        {
    //            Debug.Log(www.error);
    //            TextHeader.text = "E' AVVENUTO UN ERRORE:\n " + www.error;
    //            Audio.PlayOneShot(Error);
    //        }
    //        else
    //        {
    //            if (www.downloadHandler.text.Contains("Error"))
    //            {
    //                TextHeader.text = "E' AVVENUTO UN ERRORE.";
    //                Audio.PlayOneShot(Error);
    //                yield break;
    //            }

    //            //Prendo il codice corretto (stringa) dalla rete
    //            CorrectCode = www.downloadHandler.text;
    //        }
    //    }

    //    }




    //    IEnumerator Send()
    //{
    //    if (!connectionON) yield break;

    //    //Spedisci dati al database
    //    WWWForm form = new WWWForm();

    //    using (UnityWebRequest www = UnityWebRequest.Get("http://wilez.it/timbratureEskigel/scrivi_timbratura.php?nome="+nameID+"&deviceID="+ deviceID))
    //    {
    //        yield return www.SendWebRequest();
    //        Debug.Log("output " + www.downloadHandler.text);


            
    //        if (www.isNetworkError || www.isHttpError)
    //        {
    //            Debug.Log(www.error);
    //            TextHeader.text = "E' AVVENUTO UN ERRORE\nControlla la tua connessione.";
    //            Audio.PlayOneShot(Error);
    //        }
    //        else
    //        {
    //            if (www.downloadHandler.text.Contains("Error"))
    //            {
    //                TextHeader.text = "E' AVVENUTO UN ERRORE\nNon puoi tibrare due volte.";
    //                Audio.PlayOneShot(Error);
    //                connectionON=false;
    //                yield break;
    //            }
    //            Audio.PlayOneShot(InvioOk);

    //            TextHeader.text = "TIMBRATURA EFFETTUATA!";
    //            yield return new WaitForSeconds(2);
    //            TextHeader.text = "CHIUSURA APPLICAZIONE....";
    //            yield return new WaitForSeconds(1);
    //            Application.Quit();
    //            Debug.Log("QUIT");
    //        }
            
    //    }



    //}

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

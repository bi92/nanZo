using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NinevaStudios.GoogleMaps;
using NinevaStudios.GoogleMaps.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using Vuforia;

public class LoadingTest : MonoBehaviour
{    
    public enum View
    {
        None,
        Message,
        Map,
        Camera
    }

    public RectTransform MessageView;
    public RectTransform MapView;
    public Text Output;
    public RectTransform Content;
    public VuforiaBehaviour ARCamera;
    public Transform ARObject2;
    public VideoPlayer ARObject1;

    public AudioSource test_audio_01;

    private View currentView = View.None;
    private GoogleMapsView map;
    private float latitude;
    private float longitude;
    Circle circle;
    
    IEnumerator Start()
    {
        AppManager manager = AppManager.Instance;
        
        StartCoroutine(LoadBundle());
        StartCoroutine(DownloadFile());

        MessageView.gameObject.SetActive(false);
        MapView.gameObject.SetActive(false);

        Debug.Log("save path :" + Application.persistentDataPath);
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield break;

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            Log("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Log("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
        }

        
        StartCoroutine(UpdateGps());
        
        InitMap();
    }

    //downLoadSourceArea
    IEnumerator DownloadFile()
    {
        string source = "https://qa.bien.ltd/nanzo/ar1.mp4";
        //string source = "https://qa.bien.ltd/nanzo/map_open.mp3";

        string[] files = source.Split('/');
        string file = files[files.Length - 1];

        string targetPath = Application.persistentDataPath + "/" + file;
        
        if (!File.Exists(targetPath))
        {
            WWW www = new WWW(source);
            yield return www;
            byte[] bytes = www.bytes;
            File.WriteAllBytes(targetPath, bytes);
            Log("Downloaded file - " + targetPath);
            ARObject1.url = targetPath;
            
            ARObject1.Play();
        }
        else
        {
            Log("File existed - " + targetPath);
            ARObject1.url = targetPath;
            ARObject1.Play();
        }
    }

    IEnumerator LoadBundle()
    {
        Log("Request asset bundle");

        string url = string.Empty;
#if UNITY_ANDROID
        url = "https://qa.bien.ltd/nanzo/ar2.android";
#elif UNITY_IOS
        url = "https://qa.bien.ltd/nanzo/ar2.ios";
#endif
        var uwr = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return uwr.SendWebRequest();

        Log("Get content asset bundle");
        // Get an asset from the bundle and instantiate it.
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
        var loadAsset = bundle.LoadAssetAsync<GameObject>("Assets/NanZo/Prefabs/House.prefab");
        yield return loadAsset;

        Log("Instantiate content from prefab");
        GameObject house = (GameObject)Instantiate(loadAsset.asset);
        house.transform.parent = ARObject2;
        house.transform.localPosition = Vector3.zero;
        house.transform.localRotation = Quaternion.identity;
        house.transform.localScale = Vector3.one;
    }

    void Log(string msg)
    {
        Output.text = DateTime.Now + " " + msg + "\n" + Output.text;
        var line = Output.text.Split('\n');
        if (line.Length > 100) Output.text = DateTime.Now + " " + msg + "\n";
        Content.sizeDelta = new Vector2 (0, (line.Length * 16) + 20);
    }
    
    IEnumerator UpdateGps()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude);
            
        }
    }

    void InitMap()
    {
        map = new GoogleMapsView(CreateMapViewOptions());
        map.Show(RectTransformToScreenSpace(MapView), OnMapReady);
    }
    
    GoogleMapsOptions CreateMapViewOptions()
    {
        var options = new GoogleMapsOptions();
        options.MapType(GoogleMapType.Normal);

        var cameraPosition = new CameraPosition(
            new LatLng(latitude, longitude), 16, 0, 0);
        // Camera position
        options.Camera(cameraPosition);

        return options;
    }

    void OnMapReady()
    {
        Log("The map is ready!");
        map.IsVisible = false;
        
        map.IsMyLocationEnabled = true;
        map.UiSettings.IsMyLocationButtonEnabled = true;
        map.OnOrientationChange += () => { map.SetRect(RectTransformToScreenSpace(MapView)); };
        
        AddCircle(new LatLng(37.5499464,126.9603536));
    }
    
//    CameraPosition CameraPosition
//    {
//        get
//        {
//            return new CameraPosition(
//                new LatLng(camPosLat.value, camPosLng.value),
//                camPosZoom.value,
//                camPosTilt.value,
//                camPosBearing.value);
//        }
//    }
    
    public void ShowMessage()
    {
        ToggleView(View.Message);
    }

    public void ShowMap()
    {
        ToggleView(View.Map);
    }

    public void ShowCamera()
    {
        ToggleView(View.Camera);
    }
    

    private void ToggleView(View view)
    {
        if (currentView == view) return;
        
        // 이전 뷰 제거 처리
        switch (currentView)
        {
            case View.Message:
                MessageView.gameObject.SetActive(false);
                break;
            case View.Map:
                map.IsVisible = false;
                MapView.gameObject.SetActive(false);
                break;
            case View.Camera:
                ARCamera.enabled = false;
                break;
        }
        
        // 신규 부 표시 처리
        switch (view)
        {
            case View.Message:
                MessageView.gameObject.SetActive(true);
                break;
            case View.Map:
                MapView.gameObject.SetActive(true);
                map.IsVisible = true;
                break;
            case View.Camera:
                ARCamera.enabled = true;
                break;
        }
        
        currentView = view;
    }
    
    public void TestVibrate()
    {
        Handheld.Vibrate();
    }
    
    static Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
        rect.x -= transform.pivot.x * size.x;
        rect.y -= (1.0f - transform.pivot.y) * size.y;
        rect.x = Mathf.CeilToInt(rect.x);
        rect.y = Mathf.CeilToInt(rect.y);
        return rect;
    }
    
    public static CircleOptions ColorCircleOptions(LatLng point, float radius = 10)
    {
        return new CircleOptions()
            .Center(point)
            .Radius(radius)
            .FillColor(new Color(0,0,0,0))
            .StrokeColor(new Color(0,0,1,1));
    }
    
    void CenterCamera(LatLng latLng)
    {
        if (map != null)
        {
            map.AnimateCamera(CameraUpdate.NewLatLng(latLng));
        }
    }
    
    void AddCircle(LatLng latlng, float radius = 5)
    {
        circle = map.AddCircle(CreateInitialCircleOptions(latlng, radius));
        CenterCamera(circle.Center);
    }
    
    public static CircleOptions CreateInitialCircleOptions(LatLng latlng, float radius = 5)
    {
        // on iOS width is in iOS points, and pixels on Android
//        var width = GoogleMapUtils.IsAndroid ? 20 : 2;
        var width = GoogleMapUtils.IsAndroid ? 10 : 5;

        // Create a circle in Sydney, Australia
        return new CircleOptions()
            .Center(latlng)
            .Radius(radius)
            .StrokeWidth(width)
            .StrokeColor(Color.blue)
            .FillColor(new Color(0, 0, 0, 0f))
            .Visible(true)
            .Clickable(true)
            .ZIndex(1);
    }


}

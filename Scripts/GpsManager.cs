using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinevaStudios.GoogleMaps;
using NinevaStudios.GoogleMaps.Internal;
using GeoCoordinatePortable;
using UnityEngine.UI;
using StructCollection;

public class GpsManager : MonoBehaviour
{

    public GoogleMapsView map;
    private float myLatitude;
    private float myLongitude;
    Circle outCircle;

    [Header("안쪽 원 색상")]
    public Color outCircleColor;
    [Header("바깥 원 색상")]
    public Color inCircleColor;
    Circle inCircle;

    CircleOptions outCircleOption;
    CircleOptions inCircleOption;


    public Text Output;
    public RectTransform MapView;
    public RectTransform Content;

    //target trigger gps location
    public GeoCoordinate targetLocation;
    //curlocation
    GeoCoordinate myLocation;

    Trig_location curTriggerData;

    double curDistanceToTarget;
    float targetRange;
    public Trigger debug_trigger;
    //get gps and check it is spot area
    //Gps check + map marker check

    //0- target gps
    // 1- curGps
    //2 - target range : curRange
    //3- curstate
    public Text[] debugGps;
    public SourceLoader loader;
    public GameObject arObject;

    //json trigger range
    float circleRange;
    private void Awake()
    {
        //call when dialog changed
        DataSorter.instance.OnCurDialogChanged += SetTargetLocation;
        targetLocation = new GeoCoordinate();
        myLocation = new GeoCoordinate();
        //arObject.gameObject.SetActive(false);

   
    }
    IEnumerator Start()
    {
        MapView.gameObject.SetActive(false);

        Log("map is start to load");
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Log("not able to use GPS");
            yield break;
        }
        
        
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
           // Log("Timed out");
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
            Log("Location is start:" + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            myLatitude = Input.location.lastData.latitude;
            myLongitude = Input.location.lastData.longitude;
        }


        StartCoroutine(UpdateGps());

    }
    int a;
    IEnumerator UpdateGps()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            myLatitude = Input.location.lastData.latitude;
            myLongitude = Input.location.lastData.longitude;

            // float to double to specify distance
            myLocation.Latitude = myLatitude * 1.0f;
            myLocation.Longitude = myLongitude * 1.0f;
            a++;
         
            Log("gps update: " + a);
        
            if(isLocationTrigger)
            CheckGpsDistance();
         
       
        }

    }

   
    public void InitMap()
    {
      
        map = new GoogleMapsView(CreateMapViewOptions());
        map.Show(RectTransformToScreenSpace(MapView, gameObject), OnMapReady);

    }

    GoogleMapsOptions CreateMapViewOptions()
    {
        var options = new GoogleMapsOptions();
        options.MapType(GoogleMapType.Normal);
        var cameraPosition = new CameraPosition(
            new LatLng(myLatitude, myLongitude), 18, 0, 0);
        // Camera position
        options.Camera(cameraPosition);

        return options;
    }

    void OnMapReady()
    {
        Log("The map is ready!");
        //map.IsVisible = false;
        map.IsMyLocationEnabled = true;
        map.UiSettings.IsMyLocationButtonEnabled = true;
        map.OnOrientationChange += () => { map.SetRect(RectTransformToScreenSpace(MapView,gameObject)); };
        SetTargetLocation();
    }

    static Rect RectTransformToScreenSpace(RectTransform transform, GameObject this_obj)
    {
        this_obj.GetComponent<GpsManager>().Log("map is re load beacause, orientation is change");
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
        rect.x -= transform.pivot.x * size.x;
        rect.y -= (1.0f - transform.pivot.y) * size.y;
        rect.x = Mathf.CeilToInt(rect.x);
        rect.y = Mathf.CeilToInt(rect.y);
        return rect;
    }

    public static CircleOptions ColorCircleOptions(LatLng point,float radius = 10)
    {
        
        return new CircleOptions()
            .Center(point)
            .Radius(radius)
            .FillColor(new Color(0, 0, 0, 0))
            .StrokeColor(new Color(0, 0, 1, 1));
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
        map.Clear();
        
        Debug.Log("++++++++++++++++create Circle");
      //  outCircle = map.AddCircle(ColorCircleOptions(latlng, radius));
        inCircle = map.AddCircle(CreateInitialCircleOptions(latlng, radius ));
       
        CenterCamera(inCircle.Center);
    }

    public static CircleOptions CreateInitialCircleOptions(LatLng latlng, float radius = 5 )
    {
        // on iOS width is in iOS points, and pixels on Android
        //        var width = GoogleMapUtils.IsAndroid ? 20 : 2;
        var width = GoogleMapUtils.IsAndroid ? 10 : 5;

        // Create a circle in Sydney, Australia
        return new CircleOptions()
            .Center(latlng)
            .Radius(radius)
            .StrokeWidth(width+3)
            .StrokeColor(new Color32(248, 176, 4, 255))
            .FillColor(new Color32(145,2,2,255))
            .Visible(true)
            .Clickable(true)
            .ZIndex(1);
    }

    bool isLocationTrigger;
    //call when cur dialogs trigger is location

    bool CurTriggerCheckEnd;
   public void SetTargetLocation()
    {
        int dialogIndex = DataSorter.instance.curDialogIndex;
        Trigger curTrigger =  DataSorter.instance.curDialogs[dialogIndex].trigger;
        //  Debug.Log("trigger :" + dialogIndex + curTrigger.type.location.latitude);

        targetLocation.Latitude = curTrigger.type.location.latitude;
        targetLocation.Longitude = curTrigger.type.location.longitude;
       // Debug.Log("----------------------targetLocation :" + targetLocation.Latitude);
        debug_trigger = curTrigger;
        //check only once 
        CurTriggerCheckEnd = false;

        if (curTrigger.u_type == _triggertype.location)
        {
            debug_trigger = curTrigger;
            //get target gps location
            targetLocation.Latitude = curTrigger.type.location.latitude;
            targetLocation.Longitude = curTrigger.type.location.longitude;
            targetRange = curTrigger.type.location.range;
          //  Debug.Log("targetLocation :" + targetLocation.Latitude);

            if(map !=null)
            {
                //add circle to target position
                AddCircle(new LatLng(targetLocation.Latitude, targetLocation.Longitude), targetRange);
                Debug.Log("AddCircle on -> "+ targetLocation.Latitude+"///"+ targetLocation.Longitude);
            }
            else
            {
                Debug.Log("map is null value");
                Debug.Log("targetLocation is ->" + targetLocation.Latitude + "///" + targetLocation.Longitude);
            }
         
            isLocationTrigger = true;
            DialogSequencer.isEnterTriggerArea = false;
           
        }
        else
        {
            isLocationTrigger = false;
          
        }
    
    }


    //check gps distance
    void CheckGpsDistance()
    {
     
       if(!CurTriggerCheckEnd)
       {
            curDistanceToTarget = myLocation.GetDistanceTo(targetLocation);
            debugGps[0].text = ("target :" + targetLocation.Latitude + targetLocation.Longitude);
            debugGps[1].text = ("my location :" + myLocation.Latitude + myLocation.Longitude);
            debugGps[2].text = ("현재거리" + curDistanceToTarget + "타겟거리" + targetRange);

            if (curDistanceToTarget < targetRange)
            {
                //get to target
                debugGps[3].text = ("is  on target area");
                Debug.Log("range small " + curDistanceToTarget + "than" + targetRange);
                Debug.Log("isEnterTriggerArea " + DialogSequencer.isEnterTriggerArea);
                DialogSequencer.isEnterTriggerArea = true;

                //finish check
                CurTriggerCheckEnd = true;
            }
            else
            {
                debugGps[3].text = ("out of target area");

                Debug.Log("range large " + curDistanceToTarget + "than" + targetRange);
                Debug.Log("isEnterTriggerArea " + DialogSequencer.isEnterTriggerArea);
                DialogSequencer.isEnterTriggerArea = false;

            }
       }
       
        
      

    }


    public  void Log(string msg)
    {
        
        Output.text = System.DateTime.Now + " " + msg + "\n" + Output.text;
        var line = Output.text.Split('\n');
        if (line.Length > 100) Output.text = System.DateTime.Now + " " + msg + "\n";
        Content.sizeDelta = new Vector2(0, (line.Length * 16) + 20);
    }
}

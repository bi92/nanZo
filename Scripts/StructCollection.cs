using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System;


//대화 컨텐츠 타입 dialog { trigger,content[message, media, message .....] }
namespace StructCollection 
{


    //root Class
    [Serializable]
    public class Data_NJ
    {
        public FileInfo file_info;
        public int dataOrder;
        public string dataName;

        //sources to download
        public Source[] sources;
        public Dialog[] dialogs;

    }


    #region list

    [Serializable]
    public class DataCollection
    {
        public Data_NJ data;
    }



    [Serializable]
    public class Dialog_list
    {
        public Dialog[] dialogs;
    }

  

    #endregion

 


    #region File Information type - ---------------------------------


    [Serializable]
    public struct FileInfo
    {
        public string title;
        public int created_date;
        public string description;
        public int version;


    }

    #endregion






    #region Dialog


   
    [Serializable]
    public class Source
    {
        public string name;
        public string dataName;
        public string sourcelink;
 
        //output path link
        public string resultlink;

        //json data
        public string sourcetype;
        public GameObject loadObject;

        //used in unity
        public sourceType type;
        public bool isUpdate;

    }


    [Serializable]
    public class Dialog
    {

        public string title;
        public string description;
    
        public Trigger trigger;
        public Content[] contents;
        public string dataName;

    }

    #endregion




    #region Trigger Area---------------------------------------------


    [Serializable]
    public struct Trigger
    {
        public TriggerType type;
       
        //types used in unity
        public _triggertype u_type;
        public bool isEnter;
    }


    public enum _triggertype
    {
        notSet,
        time,
        location,
        marker
    }

    [Serializable]
    public struct TriggerType
    {
        public Trig_location location;
        public Trig_marker marker;
        public Trig_time time;

    }

    [Serializable]
    public struct Trig_location
    {
        public double latitude;
        public double longitude;
        public float range;

    }

    //vuforia marker
    [Serializable]
    public struct Trig_marker
    {
        //source name
        public string name;
        public string sourcelink;
        public GameObject object_3d;
        public VideoClip video;

    }

    //vuforia marker
    [Serializable]
    public struct Trig_time
    {
        public float delay;

    }

    #endregion







    #region ContentStruct----------------------------------------------

    [Serializable]
    public class _content
    {
        //id of content
        public string id;
    
        
        //일반 메세지 
        public _message message;

        //인풋 버튼
        public _messageBtn messageBtn;

        public Audio audio;
        public Vibrate vibrate;

        //input field content
        public _input input;

        public Button button;
    }


    [Serializable]
    public class Content
    {


        //id of content
        public int id;
        public contentType type;
        public int dialogNumber;
        public string dataName;

        //if this bool is true - lock the sequence
        public bool hold;

        //일반 메세지 
        public _message message;

        //인풋 버튼
        public _messageBtn messageBtn;

        public Audio audio;
        public Vibrate vibrate;

        //input field content
        public _input input;
        
        public Button button;

        //used only in unity
        public bool isUpdate;


    }



    //------------------------------------struct-----------------------------


    [Serializable]
    public enum contentType
    {
        none,
        message,
        button,
        audio,
        vibrate,
        input,
        multiple,
      
    }

    public enum sourceType
    {
        bundle,
        video
    }

    #region Media Struct -----------------------------------------------------
    [Serializable]
    //audio or video text
    public struct Audio
    {
        //message delay
        public float delay;

        //오디오 주소 - 소스
        public string sourcelink;
        public AudioClip audioClip;

        //video + audio autoplay
        public bool autoplay;

    }

    [Serializable]
    public struct Vibrate
    {
        public float delay;
        public bool autoplay;
    }



    [Serializable]
    //audio or video text
    public struct Video
    {

        //message delay
        public float delay;

        //비디오 주소 - 소스
        public string sourcelink;
        public VideoClip videoClip;

        //video + audio autoplay
        public bool autoplay;


    }


   

    [Serializable]
    public struct ARObject
    {
        //message delay
        public float delay;

        //3d object 주소 - 소스 
        public string sourcelink;
        public GameObject Source_3d;

        //3d object scale + position
        public Vector3 scale;
        public Vector3 position;

    }



    #endregion

  

    #region MessaageType Struct--------------------------------------

    [Serializable]
    public struct _message
    {
        //message delay
        public float delay;

        //txt 내용 
        public txt_const txt_const;

    }

     [Serializable]
    public struct Action
    {
    
        //content link
        public string gotoMap;
        public string gotoCamera;
        public int gotoContent;

    }


    public enum actiontype
    {
        notSet,
        gotoMap,
        gotoCamera,
        gotoContent
    }
        

    [Serializable]
    public struct _input
    {
        public float delay;
        public _inputOption[] options;
        
    }


    [Serializable]
    //type which has input field
    public struct _inputOption
    {

        public string keyword;
        public Action action;
        public actiontype type;

    }


    [Serializable]
    public struct Button
    {
        public float delay;
        public txt_const option;
        public Action action;
        public actiontype type;
    }


    [Serializable]
    public struct txt_const
    {
        public string ko;
        public string en;
    }



    // message which have buttons

    [Serializable]
    //버튼이 있는 메세지
    public struct _messageBtn
    {

        // delay Time
        public float delay;

        //txt 내용 
        public txt_const description;



        #endregion
    }


    #endregion


    //btn 형 질문 +대답
    [Serializable]
    public struct Options
    {
        public txt_const txt_const;

    }


}

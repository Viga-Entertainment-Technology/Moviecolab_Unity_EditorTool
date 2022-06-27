using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Linq;
using SimpleJSON;
//using Unity.Plastic.Newtonsoft.Json;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Datatypes;
namespace ServerResponse
{
    public class ServerLinks
    {
        public static  async Task RequestWebAction(string eml,string pswd,Action<string[]> Callback)
        {
            //Uri loginUri = new Uri("https://viga-sso-stage-u44si7afja-as.a.run.app/api/token/");
            Uri loginUri = new Uri("https://sso-prod-service-txevaj6mmq-as.a.run.app/api/token/");
            JSONObject payload = new JSONObject();
            payload.Add("email", eml);
            payload.Add("password", pswd);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload.ToString());

            using (var Request = new UnityWebRequest("" + loginUri, "POST"))
            {
                Request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                Request.downloadHandler = new DownloadHandlerBuffer();
                Request.SetRequestHeader("Content-Type", "application/json");
                Request.SendWebRequest();
                while (!Request.isDone)
                {
                    await Task.Delay(10);
                    EditorUtility.DisplayProgressBar("loging in.....", "please wait...", Request.uploadProgress+Request.downloadProgress / 2);
                }
                if (Request.error != null)
                {
                    EditorUtility.ClearProgressBar();
                    Debug.Log("Error: " + Request.error);
                    EditorUtility.DisplayDialog("Error:", Request.error +": Please Check Login Credentials","Ok");
                }
                else
                {
                    //Debug.Log("Status Code: " + request.downloadHandler.text);
                    JSONNode res = JSON.Parse(Request.downloadHandler.text);

                    string AccessToken = res["access"];
                    string RefreshToken = res["refresh"];
                    string[] str = { AccessToken, RefreshToken };
                    Callback(str);
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        internal static async Task RefreshAccess()
        {
            //Uri RefreshUri = new Uri("https://viga-sso-stage-u44si7afja-as.a.run.app/api/token/refresh/");
            Uri RefreshUri = new Uri("https://sso-prod-service-txevaj6mmq-as.a.run.app/api/token/refresh/");
            JSONObject payload = new JSONObject();
            payload.Add("refresh", EditorPrefs.GetString("RefreshToken"));
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload.ToString());

            using (var Request = new UnityWebRequest("" + RefreshUri, "POST"))
            {
                Request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                Request.downloadHandler = new DownloadHandlerBuffer();
                
                Request.SetRequestHeader("Content-Type", "application/json");
                Request.SendWebRequest();
                while (!Request.isDone)
                {
                    await Task.Delay(10);
                    EditorUtility.DisplayProgressBar("Getting Access Token.....", "please wait...", Request.uploadProgress + Request.downloadProgress / 2);
                }
                if (Request.error != null)
                {
                    EditorUtility.ClearProgressBar();
                    Debug.Log("Error: " + Request.error);
                }
                else
                {
                    JSONNode res = JSON.Parse(Request.downloadHandler.text);
                    string AccessToken = res["access"];
                    EditorPrefs.SetString("AccessToken", AccessToken);
                    EditorUtility.ClearProgressBar();
                }
            }
            
        }

        internal static async Task UploadtoServer(string filename, byte[] bytes, Action<string> uploadaction,string type)
        {
            await RefreshAccess();
            
            UnityWebRequest wr= new UnityWebRequest(); 
            
            if (type=="Shot")
            {
                List<IMultipartFormSection> form = new List<IMultipartFormSection>();
                form.Add(new MultipartFormDataSection("name", filename));
                form.Add(new MultipartFormFileSection("file", bytes, filename + ".png", "image/png"));
                form.Add(new MultipartFormDataSection("shot", EditorPrefs.GetString("Current_Shot")));
                //wr = UnityWebRequest.Post("https://movie-colab-stage-5etrn5mvdq-as.a.run.app/trackables/shot-version/",form);
                wr = UnityWebRequest.Post("https://movie-colab-prod-service-facgkefmpa-as.a.run.app/trackables/shot-version/", form);

                form.Clear();
            }
            else if (type=="Sequence")
            {
                WWWForm form = new WWWForm();
                form.AddField("code",filename);
                form.AddBinaryData("file", bytes);
                form.AddField("project", EditorPrefs.GetString("Current_Project"));
                form.AddField("file_path","");
                //wr =UnityWebRequest.Post("https://movie-colab-stage-5etrn5mvdq-as.a.run.app/trackables/shot-sequence/",form);
                wr = UnityWebRequest.Post("https://movie-colab-prod-service-facgkefmpa-as.a.run.app/trackables/shot-sequence/", form);

            }
            wr.SetRequestHeader("Authorization", "Bearer " + EditorPrefs.GetString("AccessToken"));
            wr.SendWebRequest();

            while (!wr.isDone)
            {
                await Task.Delay(10);
                EditorUtility.DisplayProgressBar("Processing Request .....", "Uploading data to server please wait...", wr.uploadProgress / 1);
            }
            if (wr.error != null)
            {
                EditorUtility.ClearProgressBar();
                Debug.Log("Error: " + wr.error);
            }
            else
            {
                uploadaction(filename);
                EditorUtility.ClearProgressBar();
            }
        }
        internal static async Task ActionTask
        (
         string ActionTag, Action<List<ProjectData>> Call_Proj = null,
         Action<List<SequenceData>> call_seq = null,
         Action<List<ShotsData>> call_shot = null,
         Action<List<ShotVersionData>> call_shotver = null,
         Action<List<Taskdata>> call_task=null
        )
        {
            await RefreshAccess();
            List<ProjectData> str_Proj = new List<ProjectData>();
            List<SequenceData> str_Seq = new List<SequenceData>();
            List<ShotsData> str_Shot = new List<ShotsData>();
            List<ShotVersionData> str_ShotVersion = new List<ShotVersionData>();
            List<Taskdata> str_Task = new List<Taskdata>();
            
            UnityWebRequest request;
            //string backend = "https://movie-colab-stage-5etrn5mvdq-as.a.run.app/";
            string backend = "https://movie-colab-prod-service-facgkefmpa-as.a.run.app/";
            Uri Moviecollaburi=null;

            switch (ActionTag)
            {
                case "Project":
                    Moviecollaburi = new Uri(backend + "project/");
                    break;

                case "Sequence":
                    Moviecollaburi = new Uri(backend + "trackables/shot-sequence?project=" + EditorPrefs.GetString("Current_Project"));
                    break;

                case "Shot":
                    Moviecollaburi = new Uri(backend + "trackables/shot?project=" + EditorPrefs.GetString("Current_Project") + "&parent_sequence=" + EditorPrefs.GetString("Current_Sequence"));
                    break;

                case "Shot_Version":
                    Moviecollaburi = new Uri(backend + "trackables/shot-version?shot=" + EditorPrefs.GetString("Current_Shot"));
                    break;

                case "Task":
                    Moviecollaburi = new Uri(backend + "trackables/task/?project=" + EditorPrefs.GetString("Current_Project") + "&linked=" + EditorPrefs.GetString("Current_Shot") + "&assigned=1");
                    break;

                default:
                    Debug.Log("Unknown or Invalid Tag");
                    break;
            }
            request = new UnityWebRequest(Moviecollaburi, "GET");
            request.SetRequestHeader("Authorization", "Bearer " + EditorPrefs.GetString("AccessToken"));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            request.SendWebRequest();
            while (!request.isDone)
            {
                await Task.Delay(10);
                EditorUtility.DisplayProgressBar("Proccessing Request .....", " Getting data from server ,please wait...",request.downloadProgress /1);
            }
            if (request.error != null)
            {
                EditorUtility.ClearProgressBar();
                Debug.Log("Error: " + request.error);
            }
            else
            {
                switch (ActionTag)
                {
                    case "Project":
                        str_Proj = Jsonresolver.JsonReader_Project(request.downloadHandler.text);
                        Call_Proj(str_Proj);
                        break;

                    case "Sequence":
                        str_Seq = Jsonresolver.JsonReader_Sequence(request.downloadHandler.text);
                        call_seq(str_Seq);
                        break;

                    case "Shot":
                        str_Shot = Jsonresolver.JsonReader_Shot(request.downloadHandler.text);
                        call_shot(str_Shot);
                        break;

                    case "Shot_Version":
                        str_ShotVersion = Jsonresolver.JsonReader_ShotVersion(request.downloadHandler.text);
                        call_shotver(str_ShotVersion);
                        break;

                    case "Task":
                        str_Task = Jsonresolver.JsonReader_Task(request.downloadHandler.text);
                        call_task(str_Task);
                        break;

                    default:
                        break;
                }
                EditorUtility.ClearProgressBar();
            }
            
           
        }

    }
    [System.Serializable]
    internal static  class Jsonresolver
    {
        internal static List<ProjectData> JsonReader_Project(string fetcheddata)
        {
            
            List<ProjectData> projlist = new List<ProjectData>();
            
            Root_Projects parsed_data = JsonConvert.DeserializeObject<Root_Projects>(fetcheddata);
            if (fetcheddata != null)
            {
                 foreach(var item in parsed_data.results)
                 {
                    var tempholder = new ProjectData
                    {
                        Project_ID = item.id,
                        Project_Name = item.name
                    };
                    projlist.Add(tempholder);
                 }
            }
            return projlist;
        }

        internal static List<SequenceData> JsonReader_Sequence(string fetcheddata)
        {
            List<SequenceData> seqlist = new List<SequenceData>();
            Root_Sequence parsed_data = JsonConvert.DeserializeObject<Root_Sequence>(fetcheddata);
            if(fetcheddata!=null)
            {
                foreach(var item in parsed_data.results)
                {
                    var tempholder = new SequenceData
                    {
                        Sequence_Name = item.code,
                        Sequence_ID=item.id  
                    };
                    seqlist.Add(tempholder);
                }
            }
            return seqlist;
        }

        internal static List<ShotsData> JsonReader_Shot(string fetcheddata)
        {
            List<ShotsData> shotlist = new List<ShotsData>();
            Root_Shot parsed_data = JsonConvert.DeserializeObject<Root_Shot>(fetcheddata);
            if(fetcheddata!=null)
            {
                foreach(var item in parsed_data.results)
                {
                    var tempholder = new ShotsData
                    {
                        Shots_Name = item.code,
                        Shots_ID = item.id
                    };
                    shotlist.Add(tempholder);
                }
            }
            return shotlist;
        }

        internal static List<ShotVersionData> JsonReader_ShotVersion(string fetcheddata)
        {
            List<ShotVersionData> ShotVersionList = new List<ShotVersionData>();
            Root_SHOTVERSION paresed_data = JsonConvert.DeserializeObject<Root_SHOTVERSION>(fetcheddata);
            if(fetcheddata!=null)
            {
                foreach(var item in paresed_data.results)
                {
                    var tempholder = new ShotVersionData
                    {
                        ShotVersion_Name = item.name,
                        ShotVersion_ID = item.id,
                    };
                    ShotVersionList.Add(tempholder);
                } 
            }
            return ShotVersionList;
        }
        internal static List<Taskdata> JsonReader_Task(string fetcheddata)
        {
            List<Taskdata> tasklist = new List<Taskdata>();
            Root_Task parsed_data = JsonConvert.DeserializeObject<Root_Task>(fetcheddata);
            if(fetcheddata!=null )
            {
                foreach(var item in parsed_data.results)
                {
                    var tempholder = new Taskdata
                    {
                        Task_ID = item.id,
                        Task_Name = item.name
                    };
                    tasklist.Add(tempholder);
                }
            }
            return tasklist;
        }
        public class Result_Project
        {
            
            public int id { get; set; }
            public string name { get; set; }
        }
        public class Root_Projects
        {
            public List<Result_Project> results { get; set; }
        }
        public class Result_Sequence
        {
            public int id { get; set; }
            public string code { get; set; }
        }
        public class Root_Sequence
        {
            public List<Result_Sequence> results { get; set; }
        }

        public class Result_Shot
        {
            public int id { get; set; }
            public string code { get; set; }
        }

        public class Root_Shot
        {
            public int count { get; set; }
            public List<Result_Shot> results { get; set; }
        }


        public class Result_Task
        {
            public int id { get; set; }
            public string name { get; set; }

        }

        public class Root_Task
        {
            public int count { get; set; }
            public List<Result_Task> results { get; set; }
        }

        public class Result_SHOTVERSION
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        public class Root_SHOTVERSION
        {
            public int count { get; set; }
            public List<Result_SHOTVERSION> results { get; set; }
        }
    }

}


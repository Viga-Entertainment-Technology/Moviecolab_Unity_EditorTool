using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ServerResponse;
using System.Collections.Generic;
using Datatypes;
using UnityEditor.Media;
using System.Threading.Tasks;
using System.Threading;
using System.IO;


public class MovielCollab : EditorWindow
{
    //[MenuItem("Tools/MovielCollab_Editor")]
    public static void Showtool()
    {
        EditorWindow wnd = GetWindow<MovielCollab>();
        wnd.titleContent = new GUIContent("MovielCollab");
        wnd.Show();
    }

    List<ProjectData> list_Projects = new List<ProjectData>();
    List<SequenceData> list_Sequence = new List<SequenceData>();
    List<ShotsData> list_Shots = new List<ShotsData>();
    List<Taskdata> list_Task = new List<Taskdata>();
    List<ShotVersionData> List_SHOTVERSION = new List<ShotVersionData>();

    bool flag_projectlist = false;
    bool flag_Sequence = false;
    bool flag_Shots = false;
    bool flag_SHOTVERSION = false;
    bool flag_Tasks = false;
    //bool flag_Project_Asset = false;
    //bool flag_Asset = false;
    bool flag_Render = false;
    //bool flag_rendersequence = false;
    string Selected_Project = null;
    string Selected_Sequence = null;
    string selected_Shot = null;
    string selected_SHOTVERSION = null;
    string selected_Task = null;


    byte[] bytes;
    Image RenderImage;


    public void CreateGUI()
    {
        RenderImage = new Image();

        VisualElement root = rootVisualElement;
        var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);


        rootVisualElement.Add(splitView);


        var leftPane = new VisualElement();

        var sequence = new Foldout
        {
            text = "Sequene"
        };

       /* var Assets = new Foldout
        {
            text = "Assets"
        };*/

        splitView.Add(leftPane);

        var rightPane = new VisualElement();
        splitView.Add(rightPane);

        leftPane.Add(sequence);
        //leftPane.Add(Assets);


        var rectpanel_sequence = new ScrollView();
        if (EditorPrefs.HasKey("scrollpos"))
        {
            rectpanel_sequence.verticalScroller.value=EditorPrefs.GetFloat("scrollpos");
        }

        rectpanel_sequence.verticalScroller.RegisterCallback<ChangeEvent<float>>((x) => { EditorPrefs.SetFloat("scrollpos", rectpanel_sequence.verticalScroller.value); });

        //var rectpanel_asset = new ScrollView();
        sequence.Add(rectpanel_sequence);
        //Assets.Add(rectpanel_asset);
        var previewlabel = new ToolbarToggle()
        {
            text = "Render Preview"
        };

        var render_preview = new VisualElement();
        var splitView2 = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        rightPane.Add(render_preview);

        //////==========================================SEQUENCE INTERNAL LAYOUT=============================================================

        var GetProject_Seq = new Button(ProjectAction);
        GetProject_Seq.text = "Get Projects";
        rectpanel_sequence.Add(GetProject_Seq);

        if (flag_projectlist)
        {

            var tod = new Toolbar();
            rectpanel_sequence.Add(tod);
            var projectlist = new ToolbarMenu
            {
                text = "Project : "

            };
            foreach (var project in list_Projects)
            {
                projectlist.menu.AppendAction(project.Project_Name, ProjectMenuAction, DropdownMenuAction.AlwaysEnabled);
                projectlist.menu.AppendSeparator();
            }
            tod.Add(projectlist);
            var lav = new ToolbarToggle()
            {
                text = Selected_Project
            };
            tod.Add(lav);
        }

        var GetSequence = new Button(SequenceAction);
        GetSequence.text = "Get Sequence";
        rectpanel_sequence.Add(GetSequence);

        if (flag_Sequence)
        {
            var tod = new Toolbar();
            rectpanel_sequence.Add(tod);
            var Sequencelist = new ToolbarMenu
            {
                text = "Sequences : "
            };
            foreach (var sequences in list_Sequence)
            {
                Sequencelist.menu.AppendAction(sequences.Sequence_Name, SequenceMenuAction, DropdownMenuAction.AlwaysEnabled);
                Sequencelist.menu.AppendSeparator();
            }
            tod.Add(Sequencelist);
            var lav = new ToolbarToggle()
            {
                text = Selected_Sequence
            };
            tod.Add(lav);
        }


        var GetShots = new Button(ShotAction);
        GetShots.text = "Get Shots";
        rectpanel_sequence.Add(GetShots);

        if (flag_Shots)
        {
            var tod = new Toolbar();
            rectpanel_sequence.Add(tod);
            var Shotslist = new ToolbarMenu
            {
                text = "Shots : "
            };
            foreach (var shot in list_Shots)
            {
                Shotslist.menu.AppendAction(shot.Shots_Name, ShotMenuAction, DropdownMenuAction.AlwaysEnabled);
                Shotslist.menu.AppendSeparator();
            }
            tod.Add(Shotslist);
            var lav = new ToolbarToggle()
            {
                text = selected_Shot
            };
            tod.Add(lav);
        }

        var GetShotVersion = new Button(ShotVersionAction);
        GetShotVersion.text = "Get Shot Version";
        rectpanel_sequence.Add(GetShotVersion);

        if (flag_SHOTVERSION)
        {
            var tod = new Toolbar();
            rectpanel_sequence.Add(tod);
            var versionlist = new ToolbarMenu
            {
                text = "Shot Version : "
            };
            foreach (var version in List_SHOTVERSION)
            {
                versionlist.menu.AppendAction(version.ShotVersion_Name, ShotVersionMenuAction, DropdownMenuAction.AlwaysEnabled);
                versionlist.menu.AppendSeparator();
            }
            tod.Add(versionlist);
            var lav = new ToolbarToggle()
            {
                text = selected_SHOTVERSION
            };
            tod.Add(lav);
        }



        var GetAssigned = new Button(Taskaction);
        GetAssigned.text = "View Assigned Task";
        rectpanel_sequence.Add(GetAssigned);

        if (flag_Tasks)
        {
            var tod = new Toolbar();
            rectpanel_sequence.Add(tod);
            var Tasklist = new ToolbarMenu
            {
                text = "Assigned Tasks : "
            };
            foreach (var task in list_Task)
            {
                Tasklist.menu.AppendAction(task.Task_Name, TaskMenuAction, DropdownMenuAction.AlwaysEnabled);
                Tasklist.menu.AppendSeparator();

            }
            tod.Add(Tasklist);
            var lav = new ToolbarToggle()
            {
                text = selected_Task
            };
            tod.Add(lav);
        }

        var RenderScene = new Button(Renderaction);
        RenderScene.text = "Render Scene";
        rectpanel_sequence.Add(RenderScene);

        if (flag_Render)
        {
            if (Application.isPlaying)
            {
                var imag = RTImage_play();
                RenderImage.image = imag;
                bytes = imag.EncodeToPNG();
            }
            else if (Application.isEditor)
            {
                var imag = RTImage_Edit();
                RenderImage.image = imag;
                bytes = imag.EncodeToPNG();
            }

            RenderImage.scaleMode = ScaleMode.ScaleToFit;
            RenderImage.focusable = true;

            render_preview.Add(RenderImage);

            var upload = new Button(UploadAction)
            {
                text = "Upload"
            };
            var Discard = new Button(DiscardAction)
            {
                text = "Discard"
            };

            var bar = new ToolbarToggle();
            rightPane.Add(bar);
            bar.Add(upload);
            bar.Add(Discard);
            flag_Render = false;
        }

        var RenderSequence = new Button(renderseqaction);
        RenderSequence.text = "Upload Sequence";
        rectpanel_sequence.Add(RenderSequence);



        var Logout = new Button(logout);
        Logout.text = "Log Out ";
        rectpanel_sequence.Add(Logout);
        /////=====================================ASSETS INTERNAL LAYOUT================================================================
        /*var GETproject_ASSET = new Button();
        GETproject_ASSET.text = "Get Project ";
        rectpanel_asset.Add(GETproject_ASSET);

        if (flag_Project_Asset)
        {
            var Project_Asset_list = new ToolbarMenu
            {
                text = "Projects"
            };
            rectpanel_sequence.Add(Project_Asset_list);

        }
        var Get_Asset = new Button();
        Get_Asset.text = "Get Assets";
        rectpanel_asset.Add(Get_Asset);

        if (flag_Asset)
        {
            var Asset_list = new ToolbarMenu
            {
                text = "Assets"
            };
            rectpanel_sequence.Add(Asset_list);
        }

        var Get_Asset_version = new Button();
        Get_Asset_version.text = "Get Assets Version";
        rectpanel_asset.Add(Get_Asset_version);

        var Renderassetseq = new Button();
        Renderassetseq.text = "Render Asset Sequence";
        rectpanel_asset.Add(Renderassetseq);

        var uploadseq = new Button();
        uploadseq.text = "Upload Sequence";
        rectpanel_asset.Add(uploadseq);
        rootVisualElement.MarkDirtyRepaint();
        */

    }

    ////==============================================VISUAL ELEMENT ACTIONS==========================================================
    async void ProjectAction()
    {
        flag_projectlist = true;
        await ServerLinks.ActionTask("Project", Projectcall).ConfigureAwait(true);
    }

    async void SequenceAction()
    {

        flag_Sequence = true;
        await ServerLinks.ActionTask("Sequence", null, Sequencecall).ConfigureAwait(true);

    }

    async void ShotAction()
    {
        if (!flag_projectlist || !flag_Sequence || Selected_Project == null || Selected_Sequence == null)
        {
            var emptyfields = "";

            if (!flag_projectlist || Selected_Project == null)
            {
                emptyfields += " ::Project";
            }
            if (!flag_Sequence || Selected_Sequence == null)
            {
                emptyfields += " ::Sequence";
            }
            EditorUtility.DisplayDialog("Missing Data OR Empty Field", " Please Check For Empty Fields : " + emptyfields, "Close");
        }
        else
        {
            flag_Shots = true;
            await ServerLinks.ActionTask("Shot", null, null, Shotcall).ConfigureAwait(true);
        }
    }

    async void ShotVersionAction()
    {
        if (!flag_projectlist || !flag_Sequence || !flag_Shots || Selected_Project == null || Selected_Sequence == null || selected_Shot == null)
        {
            var emptyfields = "";
            if (!flag_projectlist || Selected_Project == null)
            {
                emptyfields += " ::Project";
            }
            if (!flag_Sequence || Selected_Sequence == null)
            {
                emptyfields += " ::Sequence";
            }
            if (!flag_Shots || selected_Shot == null)
            {
                emptyfields += "::Shots";
            }
            EditorUtility.DisplayDialog("Missing Data OR Empty Field", " Please Check For Empty Fields : " + emptyfields, "Close");
        }
        else
        {
            flag_SHOTVERSION = true;
            await ServerLinks.ActionTask("Shot_Version", null, null, null, ShotVersionCall).ConfigureAwait(true);
        }
    }

    async void Taskaction()
    {
        if (!flag_projectlist || !flag_Sequence || !flag_Shots || Selected_Project == null || Selected_Sequence == null || selected_Shot == null)
        {
            var emptyfields = "";
            if (!flag_projectlist || Selected_Project == null)
            {
                emptyfields += " ::Project";
            }
            if (!flag_Sequence || Selected_Sequence == null)
            {
                emptyfields += " ::Sequence";
            }
            if (!flag_Shots || selected_Shot == null)
            {
                emptyfields += "::Shots";
            }

            EditorUtility.DisplayDialog("Missing Data OR Empty Field", " Please Check For Empty Fields : " + emptyfields, "Close");
        }
        else
        {
            flag_Tasks = true;
            await ServerLinks.ActionTask("Task", null, null, null, null, Taskcall).ConfigureAwait(true);
        }

    }

    async void UploadAction()
    {
        if (!flag_projectlist || !flag_Sequence || !flag_Shots || !flag_Tasks)
        {
            var emptyfields = "";
            if (!flag_projectlist || Selected_Project == null)
            {
                emptyfields += " ::Project";
            }
            if (!flag_Sequence || Selected_Sequence == null)
            {
                emptyfields += " ::Sequence";
            }
            if (!flag_Shots || selected_Shot == null)
            {
                emptyfields += "::Shots";
            }
            if(!flag_Tasks || selected_Task==null)
            {
                emptyfields += "::Task";
            }
            EditorUtility.DisplayDialog("Missing Data OR Empty Field", " Please Check For Empty Fields : " + emptyfields, "Close");
        }
        else
        {
            await ServerLinks.ActionTask("Shot_Version", null, null, null, ShotVersionCall).ConfigureAwait(true);
            var versionpad = 4 - List_SHOTVERSION.Count.ToString().Length;
            var filename = selected_Shot + "_" + selected_Task + "_ver_" + (List_SHOTVERSION.Count + 1).ToString().PadLeft(versionpad, '0');
            await ServerLinks.UploadtoServer(filename, bytes, UploadCall,"Shot").ConfigureAwait(true);
        }
    }

    void DiscardAction()
    {
        flag_Render = false;
        RenderImage = null;
        refresh();
    }


    void Renderaction()
    {
        flag_Render = true;
        refresh();
    }

    void renderseqaction()
    {
        var proceed = false;
        string path=null;
        if (!flag_projectlist || Selected_Project == null)
        {
            EditorUtility.DisplayDialog("Missing Data OR Empty Field", " Please Check For Empty Fields : Project", "Close");
            proceed = false;

        }
        else
        {
            path = EditorUtility.OpenFilePanel("Overwrite with png", "", "mp4");
            if (path.Length!= 0) 
            {
                SequenceAction();
                proceed = true;
            }
        }

        foreach (var i in list_Sequence)
        {
            var cmp1 = (path.Substring(path.Length - (i.Sequence_Name.Length+4)));
            var cmp2 = (i.Sequence_Name + ".mp4");
            Debug.Log("cmp1 : " + cmp1);
            Debug.Log("cmp2 : " + cmp2);
            if (cmp1==cmp2)
            {
                EditorUtility.DisplayDialog("Sequence already exist", " A sequence with same name already exist, please either remove old sequence or Change the name of current sequence ", "OK");
                proceed = false;
            }
        }
        if(proceed)
        {
            Debug.Log(path);
            var sequencename = path.Substring(path.LastIndexOf('/')+1);
            sequencename = sequencename.Remove(sequencename.LastIndexOf("."));
            Debug.Log(sequencename);
            
            if (EditorUtility.DisplayDialog("Upload the current sequence?", " do you want to upload the current sequence " + sequencename, "Upload", "discard"))
            {
                bytes = File.ReadAllBytes(path);
                ServerLinks.UploadtoServer(sequencename, bytes, UploadCall, "Sequence").ConfigureAwait(true);
            }
        }
    }


    void refresh()
    {
        rootVisualElement.Clear();
        CreateGUI();
    }

    //=============================================================Actions CALLBACKS =============================================


    void Projectcall(List<ProjectData> temprory_project)
    {
        list_Projects = temprory_project;
        if (list_Projects.Count == 0)
        {
            EditorUtility.DisplayDialog("Empty Project List", " No Project Found In This Account", "Ok");
            Selected_Project = "";
            flag_projectlist = false;
        }
        refresh();
    }

    void Sequencecall(List<SequenceData> temprory_sequence)
    {
        list_Sequence = temprory_sequence;
        if (list_Sequence.Count == 0)
        {
            EditorUtility.DisplayDialog("Empty Sequence List", " No Sequence Found In This Project", "Ok");
            Selected_Sequence = "";
            flag_Sequence = false;
        }
        refresh();
    }

    void Shotcall(List<ShotsData> temprory_shots)
    {
        list_Shots = temprory_shots;
        if (list_Shots.Count == 0)
        {
            EditorUtility.DisplayDialog("Empty Shot List", " No Shot Found In This Sequence", "Ok");
            selected_Shot = "";
            flag_Shots = false;
        }
        refresh();
    }

    void ShotVersionCall(List<ShotVersionData> temprory_shotversion)
    {
        List_SHOTVERSION = temprory_shotversion;
        if (List_SHOTVERSION.Count == 0)
        {
            EditorUtility.DisplayDialog("Empty Shot_version List", " No Shot_Version Found For This Shot", "Ok");
            selected_SHOTVERSION = "";
            flag_SHOTVERSION = false;
        }
        refresh();
    }

    void UploadCall(string filename)
    {
        EditorUtility.DisplayDialog("Upload Complete ",filename + " Uploaded Succesfully ", "Ok");
    }

    void Taskcall(List<Taskdata> temprory_task)
    {
        list_Task = temprory_task;
        if (list_Task.Count == 0)
        {
            EditorUtility.DisplayDialog("Empty Shot List", " No Task Found For This Shot", "Ok");
            selected_Task = "";
            flag_Tasks = false;
        }
        refresh();
    }

    void logout()
    {
        EditorPrefs.DeleteKey("RefreshToken");
        EditorWindow window = GetWindow<MovieCollab_Login>("Movie Collab Editor", typeof(MovielCollab));
        window.Show();
        this.Close();
    }
    //====================================MENU ACTIONS=========================================================================

    void ProjectMenuAction(DropdownMenuAction action)
    {
        Selected_Project = action.name;
        var id = list_Projects.Find((x) => x.Project_Name == action.name);
        Debug.Log(id);
        EditorPrefs.SetString("Current_Project", id.Project_ID.ToString());
        flag_Sequence = false;
        list_Sequence.Clear();
        Selected_Sequence = null;
        flag_Shots = false;
        list_Shots.Clear();
        selected_Shot = null;
        flag_SHOTVERSION = false;
        List_SHOTVERSION.Clear();
        selected_SHOTVERSION = null;
        flag_Tasks = false;
        list_Task.Clear();
        selected_Task = null;
        refresh();
    }

    void SequenceMenuAction(DropdownMenuAction action)
    {
        Selected_Sequence = action.name;
        var id = list_Sequence.Find((x) => x.Sequence_Name == action.name);
        EditorPrefs.SetString("Current_Sequence", id.Sequence_ID.ToString());

        flag_Shots = false;
        list_Shots.Clear();
        selected_Shot = null;
        flag_SHOTVERSION = false;
        List_SHOTVERSION.Clear();
        selected_SHOTVERSION = null;
        flag_Tasks = false;
        list_Task.Clear();
        selected_Task = null;
        refresh();
    }

    void ShotMenuAction(DropdownMenuAction action)
    {
        selected_Shot = action.name;
        var id = list_Shots.Find((x) => x.Shots_Name == action.name);
        EditorPrefs.SetString("Current_Shot", id.Shots_ID.ToString());
        flag_SHOTVERSION = false;
        List_SHOTVERSION.Clear();
        flag_Tasks = false;
        list_Task.Clear();
        selected_Task = null;
        refresh();
    }

    void ShotVersionMenuAction(DropdownMenuAction action)
    {
        selected_SHOTVERSION = action.name;
        var id = List_SHOTVERSION.Find((x) => x.ShotVersion_Name == action.name);
        EditorPrefs.SetString("Current_ShotVersion", id.ShotVersion_ID.ToString());
        refresh();
    }



    void TaskMenuAction(DropdownMenuAction action)
    {
        selected_Task = action.name;
        var id = list_Task.Find((x) => x.Task_Name == action.name);
        EditorPrefs.SetString("Current_Task", id.Task_ID.ToString());
        refresh();
    }

    //====================================================RENDER CALLS====================================================================
    private Texture2D RTImage_play()
    {

        var mCamera = Camera.main;
        if (mCamera == null)
        {
            mCamera = Camera.current;
        }
        Rect rect = new Rect(0, 0, mCamera.pixelWidth, mCamera.pixelHeight);
        RenderTexture renderTexture = new RenderTexture(mCamera.pixelWidth, mCamera.pixelHeight, 32);
        Texture2D screenShot = new Texture2D(mCamera.pixelWidth, mCamera.pixelHeight, TextureFormat.RGBA32, false);

        
        mCamera.targetTexture = renderTexture;
        
        mCamera.Render();
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();

        mCamera.targetTexture = null;
        RenderTexture.active = null;

        Destroy(renderTexture);
        renderTexture = null;
        return screenShot;
    }

    private Texture2D RTImage_Edit()
    {
        var temp = (SceneView)SceneView.sceneViews[0];
        var mCamera = temp.camera;
        mCamera.allowHDR = true;
        Rect rect = new Rect(0, 0, 1920, 1080);
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 32);
        Texture2D screenShot = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
        mCamera.targetTexture = renderTexture;
        mCamera.Render();
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();

        mCamera.targetTexture = null;
        RenderTexture.active = null;
        renderTexture = null;
        DestroyImmediate(renderTexture);

        return screenShot;

    }  
}

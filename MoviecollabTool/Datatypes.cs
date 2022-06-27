using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Datatypes
{
    [System.Serializable]
    public struct ProjectData
    {
        public string Project_Name { get; set; }
        public  int Project_ID{ get; set; }
        public string Project_Description { get; set; }
    }
    [System.Serializable]
    public struct SequenceData
    {
        public string Sequence_Name { get; set; }
        public int Sequence_ID { get; set; }
        public string description { get; set; }
        public int status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string abbreviation { get; set; }
        public string thumbnail { get; set; }
        public int notification_events { get; set; }
        public int email_events { get; set; }
        public List<string> admin_users { get; set; }
        public List<string> users { get; set; }
    }
    [System.Serializable]
    public struct ShotsData
    {
        public string Shots_Name { get; set; }
        public int Shots_ID { get; set; }
    }
    [System.Serializable]
    public struct ShotVersionData
    {
        public string ShotVersion_Name { get; set; }
        public int ShotVersion_ID { get; set; }
        public int Shotvercount { get; set; }
    }
    [System.Serializable]
    public struct Taskdata
    {
        public string Task_Name { get; set; }
        public int Task_ID { get; set; }
    }

}

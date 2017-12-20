﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;
using System.IO;
using Database;
using Mono.Data.Sqlite;
namespace Database
{
    class MainUI : EditorWindow
    {

        static string dbPath = "";
        static DatabaseTable dbTable=null;
        static DatabaseTableGUI dbGUI = null;
        [MenuItem("Tools/Database Editor/Editor")]
        public static void ShowWindow()
        {
            var ui = EditorWindow.GetWindow(typeof(MainUI));
            ui.name = "Main";
            ui.Show();
        }
        public void OnEnable()
        {
            dbPath = Application.dataPath + "/Database";
            Debug.Log(dbPath);
            dbTable = new DatabaseTable();
        }
        public MainUI()
        {
            Debug.Log(dbPath);


        }
        bool isDbOpen = false;
        bool isTableOpen = false;
        bool isUIOpen = false;
        bool openDb = false;
        string tableName = " ";
        public void OnGUI()
        {

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Mic Check.", GUILayout.ExpandHeight(false));
            GUILayout.Space(5);

            if (!isDbOpen)
            {

                string dbName = "wepons";//EditorGUILayout.TextField("Enter Database Name", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                openDb = true;//GUILayout.Button("Open Database", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));

                if (openDb)
                {
                    if (dbName != "")
                    {
                        isDbOpen = dbTable.SetDatabase(dbPath, dbName);

                    }
                    else
                    {
                        Debug.Log("Enter the database name.");
                    }
                }
            }
            if (!isTableOpen)
            {

                if (isDbOpen)
                {
                    bool openTable = true;//GUILayout.Button("Open Table", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                    if (openTable)
                    {
                        isTableOpen = dbTable.SetTable("WeaponNames");
                        dbGUI = new DatabaseTableGUI(ref dbTable);
                        dbGUI.ShowWindow();
                    }
                    else
                    {
                        tableName = EditorGUILayout.TextField("Test", tableName);
                        Debug.Log(tableName);
                            
                    }


                }
            }
            }
            

        }
        
    }
    /**********************DATABASETABLE GUI CLASS****************************/


    public class DatabaseTableGUI : EditorWindow
    {
        DatabaseTable dbTable = null;
        int numRows = 0;
        int numCols = 0;
    

        public DatabaseTableGUI(ref DatabaseTable dbTable)
        {
            
            this.dbTable = dbTable;
            numRows = dbTable.NumRows();
            numCols = dbTable.NumColumns();
            this.titleContent = new GUIContent("Database Editor.");
        }
        public void ShowWindow()
        {
         var ui = EditorWindow.GetWindow(typeof(DatabaseTableGUI));
         ui.name = "Database Editor";
         ui.Show();
        }
        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Mic Check Mic Check.", GUILayout.ExpandHeight(false));

            System.Data.DataRowCollection dbRow = dbTable.dbTable.Rows;
        
            for (int i = 0; i < numRows; i++)
            {
                var element = dbRow[i];
               EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < numCols; j++)
                {
                    GUILayout.Label(new GUIContent(element[j].ToString()), GUILayout.ExpandHeight(false));
                }
               EditorGUILayout.EndHorizontal();
                
             }
                      
           EditorGUILayout.EndVertical();
        }
    }



    /***********************DATABASETABLE CLASS*******************************/



    public class DatabaseTable
    {
        string dbPath = "";
        SqliteConnection conn = null;
        SqliteCommand cmd = null;
        public List<string> tableNames = new List<string>();
        ISQLDatabase ISQL = new ISQLDatabase();
        SqliteDataAdapter dbAdapter = null;
        public System.Data.DataTable dbTable = null;
        SqliteCommandBuilder cmdBuilder = null;
        string tableName = "";
        

        public DatabaseTable()
        {

        }
        public bool SetDatabase(string dbPath, string active)
        {
            this.dbPath = dbPath;
            ISQL.SetDatabaseDirectory(dbPath, active);
            conn = ISQL.GetConn();
            conn.Open();
            cmd = ISQL.GetCommand(ref conn);
            cmd.CommandText = "SELECT name FROM sqlite_master;";
            var sqle = ISQL.ExecuteNonQuery(ref cmd);
            
            if (sqle != null)
            {

                Debug.Log("Enter a proper database name.");
                return false;
            }
            else
            {
                return true;
            }
        }
        public int NumColumns()
        {
            int numCols = 0;
            cmd.CommandText = "PRAGMA table_info("+tableName+")";
            var sqlE = ISQL.ExecuteNonQuery(ref cmd);
            if (sqlE != null)
                return -1;
            var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                numCols = reader.GetInt32(0);
            }
            reader.Close();
            Debug.Log("Number of Columns:"+numCols);
            return (numCols+1);
        }




        public int NumRows()
        {
            int numRows = 0;
            cmd.CommandText = "SELECT COUNT(*) FROM [" + tableName + "];";
            var sqlE =ISQL.ExecuteNonQuery(ref cmd);
            if (sqlE != null)
                return -1;
            var reader = cmd.ExecuteReader();
            reader.Read();
            numRows = reader.GetInt32(0);
            reader.Close();
            Debug.Log("Number of Rows:" + numRows);
            return numRows;
        }

        public bool SetTable(string tableName)
        {
            this.tableName = tableName;
            dbTable = new System.Data.DataTable(tableName);
            cmd.CommandText = "SELECT * FROM [" + tableName + "];";
            var sqle = ISQL.ExecuteNonQuery(ref cmd);
            if (sqle == null)
            {
                dbAdapter = new SqliteDataAdapter(cmd);
                cmdBuilder = new SqliteCommandBuilder(dbAdapter);
                dbAdapter.AcceptChangesDuringFill = true;
                dbAdapter.Fill(dbTable);
                dbTable.Rows[0][1] = "murdureurs pistal";
                dbAdapter.Update(dbTable);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SaveTable()
        {
            dbAdapter.Update(dbTable);
            Debug.Log("Table Updated Successfully");
            return true;
        }
        /************************DATAROW CLASS***************************************/
    }



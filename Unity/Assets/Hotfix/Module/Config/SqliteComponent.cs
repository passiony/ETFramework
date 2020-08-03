/*
 * 脚本名称：EnergyValue
 * 项目名称：FrameWork
 * 脚本作者：黄哲智
 * 创建时间：2018-05-17 10:18:35
 * 脚本作用：
*/

using ETModel;
using SqlCipher4Unity3D;
using SQLite.Attribute;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ETHotfix
{
    [ObjectSystem]
    public class SqliteComponentAwakeSystem : AwakeSystem<SqliteComponent>
    {
        public override void Awake(SqliteComponent self)
        {
            self.Awake();
        }
    }

    public class SqliteComponent :Entity
    {
        private string dbName="";
        private string password = "";
        private SQLiteOpenFlags flag= SQLiteOpenFlags.ReadOnly;
        SQLiteConnection connection;

        public void Awake()
        {
            connection = GetSqlConnection(dbName, flag);
        }

        public override void Dispose()
        {
            base.Dispose();
            connection?.Dispose();
        }

        public List<T> SelectTable<T>() where T : IConfig, new()
        {
            List<T> dict = new List<T>();
            try
            {
                TableQuery<T> table = connection.Table<T>();
                table.Count();

                Log.Debug(table.Table.TableName);
                foreach (var value in table)
                {
                    dict.Add(value);
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
            return dict;
        }

        public SQLiteConnection GetSqlConnection(string dbName, SQLiteOpenFlags flag)
        {
            if (connection == null)
            {
#if UNITY_EDITOR
                string dbPath = string.Format(@"Assets/StreamingAssets/{0}", dbName);
#else
                string dbPath = GetFilePath();
#endif
                connection = new SQLiteConnection(dbPath, password, flag);
            }
            return connection;
        }

        // check if file exists in Application.persistentDataPath
        string GetFilePath()
        {
            string filepath = string.Format("{0}/{1}", Application.persistentDataPath, dbName);

            if (!File.Exists(filepath))
            {
#if UNITY_ANDROID
                WWW loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + dbName);
                while (!loadDb.isDone)
                {
                } // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check

                // then save to Application.persistentDataPath
                File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
                string loadDb =
 Application.dataPath + "/Raw/" + dbName;
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#else
                string loadDb = Application.dataPath + "/StreamingAssets/" + dbName;
                File.Copy(loadDb, filepath);
#endif
            }
            return filepath;
        }
    }
}
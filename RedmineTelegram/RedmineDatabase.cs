﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using SD = System.Data;

namespace RedmineTelegram
{
    public sealed class RedmineDatabase
    {


        // активные(незакрытые) задачи у типа, отсортировано от новых к старым
        public List<NormalIssue> GetUserIssues(long userId)
        {
            var table = ExecuteScript(@"
            select i.id, i.subject, i.description, iss.name, e.name , i.created_on, i.estimated_hours, i.closed_on
            from bitnami_redmine.issues i
            join bitnami_redmine.issue_statuses iss on iss.id = i.status_id
            join bitnami_redmine.enumerations e on i.priority_id = e.id
            where i.assigned_to_id = " + userId + @" and i.created_on is not null
            and iss.is_closed = 0");


            List<NormalIssue> a = new();

            for (int i = 1; i < table.GetLength(0); i++)
            {
                int estHours;
                Int32.TryParse(table[i, 6].ToString(), out estHours);
                a.Add(new NormalIssue((int)table[i, 0], table[i, 1].ToString(), table[i, 2].ToString(), table[i, 3].ToString(), table[i, 4].ToString(), table[i, 5].ToString(), estHours, table[i, 7].ToString()));

            }
            return a;
        }

        // кол-во на вход, на выход issue, сортировка по времени (updated_on), на выход count последних задач 
        public List<Issue> LoadLastEditedIssues(int count)
        {
            var table = ExecuteScript(@"
            select i.id, i.assigned_to_id, i.closed_on, i.status_id
            from bitnami_redmine.issues i 
            order by i.updated_on desc
            limit " + count);

            List<Issue> a = new();

            for (int i = 1; i < table.GetLength(0); i++)
            {
                var closedOn = table[i, 2].ToString().Length;
                var status = closedOn > 2 ? true : false;
                a.Add(new Issue((int)table[i, 0], (int)table[i, 1], status, (int)table[i, 3]));
            }
            return a;
        }

        public List<string> GetStatusesList() //возвращает список всех статусов
        {
            var list = new List<string>();
            var table = ExecuteScript(@"
            select is2.name 
            from bitnami_redmine.issue_statuses is2 
           ");

            for (int i = 1; i < table.GetLength(0); i++)
                list.Add(table[i, 0].ToString());
            return list;
        }

        public int GetStatusIdByName(string statusName)
        {
            var table = ExecuteScript(@"
            select is2.id
            from bitnami_redmine.issue_statuses is2
            where is2.name = '" + statusName + "\'");

            return (int)table[1, 0];
        }

        public int GetRedmineUserIdByTelegram(string tgId)
        {
            var table = ExecuteScript(@"
            select cv.customized_id 
            from bitnami_redmine.custom_values cv 
            where cv.value = '" + tgId + "\'");

            if (table.GetLength(0) == 1)
                return 0;
            return (int)table[1, 0];
        }

        public bool ChangeIssueStatus(long issueId, int statusId) //проверка на то что статус есть, да и на айди задачи наверн тоже
        {


            var table = ExecuteScript(@"
            update bitnami_redmine.issues i
            set i.status_id = " + statusId +
            " where i.id = " + issueId);

            return true;
        }

        public void ChangeLaborCost(long issueId, int laborCost)
        {
            var table = ExecuteScript(@"
            update bitnami_redmine.issues i
            set i.estimated_hours = " + laborCost +
            " where i.id = " + issueId);
        }

        public bool TryGetTelegramUsernameByRedmineId(int assignedTo, out string username)
        {
            var table = ExecuteScript(@"
            select cv.value 
            from bitnami_redmine.custom_values cv 
            where cv.customized_id = " + assignedTo);

            if (table.GetLength(0) == 1)
            {
                username = "";
                return false;
            }
            username = table[1, 0].ToString();

            return true;
        }

        public bool TryGetRedmineUserIdByTelegram(string telegramUsername, out long redmineUserId)
        {
            var table = ExecuteScript(@"
            select cv.customized_id 
            from bitnami_redmine.custom_values cv 
            where cv.value = '" + telegramUsername + "\'");

            if (table.GetLength(0) == 1)
            {
                redmineUserId = 0;
                return false;
            }
            redmineUserId = (long)((int)table[1, 0]);

            return true;
        }

        public NormalIssue GetNormalIssue(int issueId) //
        {
            var table = ExecuteScript(@"
            select i.id, i.subject, i.description, iss.name, e.name , i.created_on, i.estimated_hours, i.closed_on
            from bitnami_redmine.issues i
            join bitnami_redmine.issue_statuses iss on iss.id = i.status_id
            join bitnami_redmine.enumerations e on i.priority_id = e.id
            where i.id = " + issueId);
            int estHours;
            Int32.TryParse(table[1, 6].ToString(), out estHours);
            return new NormalIssue((int)table[1, 0], table[1, 1].ToString(), table[1, 2].ToString(), table[1, 3].ToString(), table[1, 4].ToString(), table[1, 5].ToString(), estHours, table[1, 7].ToString());
        }

        private object[,] ExecuteScript(string script)
        {
            MySqlConnection suka = DBUtils.GetDBConnection();
            suka.Open();
            var ms_data = new MySqlDataAdapter(script, suka);
            var table = new SD.DataTable();
            ms_data.Fill(table);
            suka.Close();

            object[,] multiDimensionalArry = new Object[table.Rows.Count + 1, table.Columns.Count];

            for (int j = table.Rows.Count - 1; j >= 0; j--)
            {
                // get current row items array to loop through
                var rowItem = table.Rows[j].ItemArray;

                // inner loop - loop through current row items, and add to resulting multi dimensional array
                for (int k = 0; k <= rowItem.Length - 1; k++)
                {
                    multiDimensionalArry[j + 1, k] = rowItem[k];
                }
            }

            return multiDimensionalArry; //multiDimensionalArry.GetLength(0) размер столбца, multiDimensionalArry.GetLength(1) размер строчки
        }
    }
}
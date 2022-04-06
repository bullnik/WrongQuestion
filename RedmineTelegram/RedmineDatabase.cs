﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using SD = System.Data;

namespace RedmineTelegram
{
    public sealed class RedmineDatabase
    {
        static readonly string host = "localhost";

        // активные(незакрытые) задачи у типа, отсортировано от новых к старым
        public List<NormalIssue> GetUserIssues(long userId)
        {
            var table = ExecuteScript(@"
            select i.id, i.subject, i.description, iss.name, e.name , i.created_on, i.estimated_hours, i.closed_on, i.assigned_to_id, i.author_id, u.lastname, u.firstname
            from bitnami_redmine.issues i
            join bitnami_redmine.issue_statuses iss on iss.id = i.status_id
            join bitnami_redmine.enumerations e on i.priority_id = e.id 
            join bitnami_redmine.users u on i.author_id = u.id 
            where i.assigned_to_id = " + userId + @" and i.created_on is not null
            and iss.is_closed = 0");


            List<NormalIssue> a = new();

            for (int i = 1; i < table.GetLength(0); i++)
            {
                int estHours;
                Int32.TryParse(table[i, 6].ToString(), out estHours);
                a.Add(new NormalIssue((int)table[i, 0], table[i, 1].ToString(), table[i, 2].ToString(), table[i, 3].ToString(), table[i, 4].ToString(), table[i, 5].ToString(), estHours, table[i, 7].ToString(), (int)table[i, 8], (int)table[i, 9], GetLabourCostByIssueId((int)table[i, 0]), $"{table[i, 10].ToString()} {table[i, 11].ToString()}"));

            }
            return a;
        }

        public List<NormalIssue> LoadLastCreatedIssues(DateTime date)
        {
            var strDate = date.ToString("yyyy-MM-dd HH:mm:ss");

            var table = ExecuteScript(@$"
            select i.id, i.subject, i.description, iss.name, e.name , i.created_on, i.estimated_hours, i.closed_on, i.assigned_to_id, i.author_id, u.lastname, u.firstname
            from bitnami_redmine.issues i
            join bitnami_redmine.issue_statuses iss on iss.id = i.status_id
            join bitnami_redmine.enumerations e on i.priority_id = e.id 
            join bitnami_redmine.users u on i.author_id = u.id 
            where i.created_on >= '{strDate}'");


            List<NormalIssue> a = new();

            for (int i = 1; i < table.GetLength(0); i++)
            {
                int estHours;
                Int32.TryParse(table[i, 6].ToString(), out estHours);
                a.Add(new NormalIssue((int)table[i, 0], table[i, 1].ToString(), table[i, 2].ToString(), table[i, 3].ToString(), table[i, 4].ToString(), table[i, 5].ToString(), estHours, table[i, 7].ToString(), (int)table[i, 8], (int)table[i, 9], GetLabourCostByIssueId((int)table[i, 0]), $"{table[i, 10].ToString()} {table[i, 11].ToString()}"));

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

        public List<Issue> LoadLastEditedIssues(DateTime date)
        {
            var strDate = date.ToString("yyyy-MM-dd HH:mm:ss");

            var table = ExecuteScript(@$"
            select i.id, i.assigned_to_id, i.created_on 
            from bitnami_redmine.issues i 
            where i.updated_on >= '{strDate}'
            order by i.updated_on desc");



            List<Issue> a = new();

            for (int i = 1; i < table.GetLength(0); i++)
            {
                var closedOn = table[i, 2].ToString().Length;
                var status = closedOn > 2 ? true : false;
                a.Add(new Issue((int)table[i, 0], (int)table[i, 1], status, (int)table[i, 3]));
            }
            return a;
        }

        public List<JournalItem> LoadLastJournalsLine(int count)
        {
            var table = ExecuteScript(@"
            select j.journalized_id, j.user_id, j.notes,
(select is2.name
            from bitnami_redmine.issue_statuses is2
            where is2.id = jd.value), 
(select is2.name
            from bitnami_redmine.issue_statuses is2
            where is2.id = jd.old_value), 
u.lastname, u.firstname 
            from bitnami_redmine.journals j left join bitnami_redmine.journal_details jd on j.id = jd.journal_id 
            join bitnami_redmine.users u on u.id = j.user_id
            where j.journalized_type  = 'Issue'
            order by j.created_on desc
            limit " + count);

            List<JournalItem> a = new();

            for (int i = 1; i < table.GetLength(0); i++)
            {
                var issueId = (int)table[i, 0];
                var comment = table[i, 2].ToString();
                var curStatus = table[i, 3].ToString();
                var oldStatus = table[i, 4].ToString();
                var isComment = comment != "";
                var isStatusChanged = curStatus != "";
                a.Add(new JournalItem(issueId, (int)table[i, 1], comment, isComment, isStatusChanged, curStatus, oldStatus, $"{table[i, 5].ToString()} {table[i, 6].ToString()}"));
            }
            return a;
        }

        public List<JournalItem> LoadLastJournalsLine(DateTime date)
        {
            var strDate = date.ToString("yyyy-MM-dd HH:mm:ss");

            var table = ExecuteScript(@$"
            select j.journalized_id, j.user_id, j.notes,
(select is2.name
            from bitnami_redmine.issue_statuses is2
            where is2.id = jd.value), 
(select is2.name
            from bitnami_redmine.issue_statuses is2
            where is2.id = jd.old_value), 
u.lastname, u.firstname 
            from bitnami_redmine.journals j left join bitnami_redmine.journal_details jd on j.id = jd.journal_id 
            join bitnami_redmine.users u on u.id = j.user_id
            where j.journalized_type  = 'Issue'
            and (jd.prop_key = 'status_id' or jd.prop_key is null)
            and j.created_on >= '{strDate}'
            order by j.created_on desc");

            List<JournalItem> a = new();

            for (int i = 1; i < table.GetLength(0); i++)
            {
                var issueId = (int)table[i, 0];
                var comment = table[i, 2].ToString();
                var curStatus = table[i, 3].ToString();
                var oldStatus = table[i, 4].ToString();
                var isComment = comment != "";
                var isStatusChanged = curStatus != "";
                a.Add(new JournalItem(issueId, (int)table[i, 1], comment, isComment, isStatusChanged, curStatus, oldStatus, $"{table[i, 5].ToString()} {table[i, 6].ToString()}"));
            }
            return a;
        }

        public string GetStatusNameByIssueId(long issueId) 
        {
            var table = ExecuteScript(@$"
            select iss.name
            from bitnami_redmine.issues i
            join bitnami_redmine.issue_statuses iss on iss.id = i.status_id
            where i.id = {issueId}");

            
            return table[1, 0].ToString();
        }

        public NormalIssue GetIssueByIssueId(long issueId)
        {
            var table = ExecuteScript(@"
            select i.id, i.subject, i.description, iss.name, e.name , i.created_on, i.estimated_hours, i.closed_on, i.assigned_to_id, i.author_id, u.lastname, u.firstname
            from bitnami_redmine.issues i
            join bitnami_redmine.issue_statuses iss on iss.id = i.status_id
            join bitnami_redmine.enumerations e on i.priority_id = e.id 
            join bitnami_redmine.users u on i.author_id = u.id  
            where i.id  =  " + issueId);

            int estHours;
            Int32.TryParse(table[1, 6].ToString(), out estHours);
            return new NormalIssue((int)table[1, 0], table[1, 1].ToString(), table[1, 2].ToString(), table[1, 3].ToString(), table[1, 4].ToString(), table[1, 5].ToString(), estHours, table[1, 7].ToString(), (int)table[1, 8], (int)table[1, 9], GetLabourCostByIssueId((int)table[1, 0]), $"{table[1, 10].ToString()} {table[1, 11].ToString()}");

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

        public double GetLabourCostByIssueId(long issueId)
        {
            var table = ExecuteScript(@"
            select sum(t.hours)
            from bitnami_redmine.time_entries t
            where t.issue_id = " + issueId);

            if (table.GetLength(0) == 1 || table[1, 0] is System.DBNull)
                return 0;
            return (double)table[1, 0];
        }

        public bool ChangeIssueStatus(long issueId, long statusId, string updTime) //проверка на то что статус есть, да и на айди задачи наверн тоже
        {
            var check = ExecuteScript(@"
            select i.id 
            from bitnami_redmine.issues i 
            join bitnami_redmine.issue_statuses iss on iss.id = i.status_id
            where i.id = " + issueId);

            if (check.GetLength(0) == 1)
                return false;

            var table = ExecuteScript(@$"
            update bitnami_redmine.issues i
            join bitnami_redmine.issue_statuses iss on iss.id = i.status_id
            set i.status_id = {statusId} 
            where i.id = {issueId}");

            var ttable = ExecuteScript(@$"
            update bitnami_redmine.issues i
            join bitnami_redmine.issue_statuses iss on iss.id = i.status_id
            set i.updated_on = cast('{updTime}' as datetime)
            where i.id = {issueId}");

            return true;
        }

        //public void ChangeLaborCost(long issueId, int laborCost)
        //{
        //    var table = ExecuteScript(@"
        //    update bitnami_redmine.issues i
        //    join bitnami_redmine.issue_statuses iss on iss.id = i.status_id
        //    set i.estimated_hours = " + laborCost +
        //    " where i.id = " + issueId
        //    + " and iss.is_closed = 0");
        //}

        public void ChangeLaborCost(long issueId, int hours, string comment, string tgName)
        {
            TryGetRedmineUserIdByTelegram(tgName, out long userId);

            var table = ExecuteScript(@"
            insert into bitnami_redmine.time_entries (user_id, project_id, hours, activity_id, spent_on , tyear, tmonth, tweek, created_on, updated_on, author_id, issue_id, comments)"
+ $" values({userId}, {GetProjectIdByIssueId(issueId)}, {hours}, 9, now(), year(now()), month(now()), week(now()), now(), now(), {userId}, {issueId}, '{comment}')") ;

        }

        public void AddComment(long issueId, string comment, string tgName)
        {
            TryGetRedmineUserIdByTelegram(tgName, out long userId);

            var table = ExecuteScript(@$"
            insert into bitnami_redmine.journals (journalized_id, journalized_type, user_id, notes, created_on, private_notes)
values({issueId}, 'Issue', {userId}, '{comment}', now(), 0)");

        }

        public int GetProjectIdByIssueId(long issueId)
        {
            var table = ExecuteScript(@"
            select i.project_id
            from bitnami_redmine.issues i
            where i.id = " + issueId);

            return (int)table[1, 0];
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

        public List<int> GetWatchersIdList(long issueId)
        {
            var list = new List<int>();
            var table = ExecuteScript(@$"
            select w.user_id 
            from bitnami_redmine.watchers w 
            where w.watchable_type = 'Issue' and watchable_id = {issueId} 
           ");

            for (int i = 1; i < table.GetLength(0); i++)
                list.Add((int)table[i, 0]);
            return list;
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
            select i.id, i.subject, i.description, iss.name, e.name , i.created_on, i.estimated_hours, i.closed_on, i.assigned_to_id, i.author_id, u.lastname, u.firstname
            from bitnami_redmine.issues i
            join bitnami_redmine.issue_statuses iss on iss.id = i.status_id
            join bitnami_redmine.enumerations e on i.priority_id = e.id 
            join bitnami_redmine.users u on i.author_id = u.id 
            where i.id = " + issueId);
            int estHours;
            Int32.TryParse(table[1, 6].ToString(), out estHours);
            return new NormalIssue((int)table[1, 0], table[1, 1].ToString(), table[1, 2].ToString(), table[1, 3].ToString(), table[1, 4].ToString(), table[1, 5].ToString(), estHours, table[1, 7].ToString(), (int)table[1, 8], (int)table[1, 9], GetLabourCostByIssueId((int)table[1, 0]), $"{table[1, 10].ToString()} {table[1, 11].ToString()}");
        }

        public bool AddLaborCost(long issueId, double hours, string comment, long redmineUserId) 
        {
            var table = ExecuteScript(@"
            insert into bitnami_redmine.time_entries (user_id, project_id, hours, activity_id, spent_on , tyear, tmonth, tweek, created_on, updated_on, author_id, issue_id, comments)"
+ $" values({redmineUserId}, {GetProjectIdByIssueId(issueId)}, {hours}, 9, now(), year(now()), month(now()), week(now()), now(), now(), {redmineUserId}, {issueId}, '{comment}')");

            return true;
        }

        public bool AddComment(long issueId, string comment, long redmineUserId)
        {
            var table = ExecuteScript(@$"
            insert into bitnami_redmine.journals (journalized_id, journalized_type, user_id, notes, created_on, private_notes)
values({issueId}, 'Issue', {redmineUserId}, '{comment}', now(), 0)");

            return true;
        }

        public bool ChangeStatus(long issueId, long statusId, long redmineUserId) 
        {
            var strDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var fTable = ExecuteScript(@$"
            insert into bitnami_redmine.journals (journalized_id, journalized_type, user_id, notes, created_on, private_notes)
values({issueId}, 'Issue', {redmineUserId}, '', cast('{strDate}' as datetime), 0)");


            var sTable = ExecuteScript(@$"
            insert into bitnami_redmine.journal_details (journal_id, property, prop_key, old_value, value)
values((select j.id
from bitnami_redmine.journals j 
order by j.id desc 
limit 1), 'attr', 'status_id', {GetStatusIdByName(GetStatusNameByIssueId(issueId))}, {statusId})");

            ChangeIssueStatus(issueId, statusId, strDate);

            return true;
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
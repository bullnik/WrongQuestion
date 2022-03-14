using System;
using System.Collections.Generic;

namespace RedmineTelegram
{
    public sealed class RedmineDatabase
    {

        // активные(незакрытые) задачи у типа, отсортировано от новых к старым
        public List<NormalIssue> GetUserIssues()
        {
            /*
            select i.id, i.assigned_to_id, i.closed_on, iss.name
            from bitnami_redmine.issues i join bitnami_redmine.issue_statuses iss on i.status_id = iss.id 
            order by i.updated_on desc
            limit count
            */
            List<NormalIssue> a = new();
            a.Add(new(2, "jopa", "jija", 3, 2, "vchera", 40, "da"));
            a.Add(new(4, "jo342pa", "jij432a", 354, 342, "vch324era", 440, "da32"));
            return a;
        }
        // кол-во на вход, на выход issue, сортировка по времени (updated_on), на выход count последних задач 
        public List<Issue> LoadLastEditedIssues(int count)
        {
            /*
               
            select i.id, i.assigned_to_id, i.closed_on, iss.name
            from bitnami_redmine.issues i join bitnami_redmine.issue_statuses iss on i.status_id = iss.id 
            order by i.updated_on desc
            limit count
            */
            throw new Exception();
        }

        public void GetStatusesList() //возвращает список всех статусов
        {
            /*
            select is2.name 
            from bitnami_redmine.issue_statuses is2 
           */
        }

        public void GetStatusIdByName(string statusName) //возвращает список всех статусов
        {
            /*
             select is2.id
            from bitnami_redmine.issue_statuses is2
            where is2.name = statusName
           */
        }

        public void GetIssueDescription(int issueId) //возвращает список всех статусов
        {
            /*
             select is2.id
            from bitnami_redmine.issue_statuses is2
            where is2.name = statusName
           */
        }

        public string GetRedmineUserIdByTelegram(string tgId)
        {
            /*
             select cv.customized_id 
            from bitnami_redmine.custom_values cv 
            where cv.value = tgId
           */
            throw new Exception();
        }

        public void GetNormalIssue(int issueId) //возвращает список всех статусов
        {
            /*
            select i.id, i.subject, i.description, iss.name, e.name , i.created_on, i.estimated_hours, i.closed_on
            from bitnami_redmine.issues i
            join bitnami_redmine.issue_statuses iss on iss.id = i.status_id
            join bitnami_redmine.enumerations e on i.priority_id = e.id
            where i.id = issueId
            */
        }
    }
}
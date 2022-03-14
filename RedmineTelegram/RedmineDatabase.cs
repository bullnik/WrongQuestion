using System;
using System.Collections.Generic;

namespace RedmineTelegram
{
    public sealed class RedmineDatabase
    {
        // активные(незакрытые) задачи у типа, отсортировано от новых к старым
        public List<NormalIssue> GetUserIssues(long userId)
        {
            List<NormalIssue> a = new();
            a.Add(new(1, "Subject1", "Description1", "Status1", "Priority1", "CreatedOn1", 10, "ClosedOn1"));
            a.Add(new(1, "Subject2", "Description2", "Status2", "Priority2", "CreatedOn2", 20, "ClosedOn2"));
            a.Add(new(1, "Subject3", "Description3", "Status3", "Priority3", "CreatedOn3", 30, "ClosedOn3"));
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
            List<Issue> a = new();
            a.Add(new(1, 3, false, 4));
            a.Add(new(2, 2, false, 8));
            a.Add(new(3, 5, false, 5));
            a.Add(new(4, 6, false, 2));
            a.Add(new(5, 7, false, 1));
            return a;
        }

        public List<string> GetStatusesList() //возвращает список всех статусов
        {
            /*
            select is2.name 
            from bitnami_redmine.issue_statuses is2 
           */
            List<string> a = new();
            a.Add("Jopa");
            a.Add("Pizda");
            a.Add("Govno");
            a.Add("Xyi");
            a.Add("Kal");
            return a;
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

        public bool TryGetRedmineUserIdByTelegram(string telegramUsername, out long redmineUserId)
        {
            /*
             select cv.customized_id 
            from bitnami_redmine.custom_values cv 
            where cv.value = tgId
           */
            redmineUserId = 1;
            return true;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTelegram
{
    public class CallbackData
    {
        public CallbackDataCommand Command { get; private set; }
        public long TargetIssueId { get; private set; }
        public string AdditionalData { get; private set; }

        public CallbackData(CallbackDataCommand command, long targetIssueId, string additionalData)
            : this(command, targetIssueId)
        {
            AdditionalData = additionalData;
        }

        public CallbackData(CallbackDataCommand command, long targetIssueId)
            : this(command)
        {
            TargetIssueId = targetIssueId;
        }

        public CallbackData(CallbackDataCommand command)
        {
            Command = command;
        }

        public static CallbackData GetFromString(string callbackData)
        {
            string[] parts = callbackData.Split(' ');

            CallbackDataCommand command = (CallbackDataCommand)int.Parse(parts[0]);
            long targetIssueId = long.Parse(parts[1]);
            string additionalData = "";
            for (int i = 2; i < parts.Length - 1; i++)
            {
                additionalData += parts[i] + " ";
            }
            additionalData += parts[^1];

            return new(command, targetIssueId, additionalData.ToString());
        }

        public static bool TryGetFromString(string stringCallbackData, out CallbackData callbackData)
        {
            try
            {
                callbackData = GetFromString(stringCallbackData);
                return callbackData.TargetIssueId >= 0;
            }
            catch
            {
                callbackData = null;
                return false;
            }
        }

        public override string ToString()
        {
            return $"{(int)Command} {TargetIssueId} {AdditionalData}";
        }
    }
}

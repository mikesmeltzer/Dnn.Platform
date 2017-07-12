﻿using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;

namespace Dnn.PersonaBar.Prompt.Commands.User
{
    [ConsoleCommand("reset-password", "Resets the user's password", new string[]{
        "id",
        "notify"
    })]
    public class ResetPassword : ConsoleCommandBase
    {
        private const string FLAG_ID = "id";
        private const string FLAG_NOTIFY = "notify";


        public bool? Notify { get; private set; }
        public int? UserId { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            int tmpId = 0;
            if (HasFlag(FLAG_ID))
            {
                if (int.TryParse(Flag(FLAG_ID), out tmpId))
                    UserId = tmpId;
            }
            else
            {
                if (int.TryParse(args[1], out tmpId))
                    UserId = tmpId;
            }

            bool tmpNotify = false;
            if (HasFlag(FLAG_NOTIFY))
            {
                if (!bool.TryParse(Flag(FLAG_NOTIFY), out tmpNotify))
                {
                    sbErrors.AppendFormat("The --{0} flag takes True or False as its value (case-insensitive)", FLAG_NOTIFY);
                }
                else
                {
                    Notify = tmpNotify;
                }
            }
            else
            {
                Notify = true; // reset password triggers sending the email to reset password. It's kind of useless if _Notify is False;
            }


            if (!UserId.HasValue)
            {
                sbErrors.Append("You must specify a number for the User's ID");
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            bool sendEmail = false;
            if (Notify.HasValue)
                sendEmail = (bool)Notify;

            var user = UserController.GetUserById(PortalId, (int)UserId);
            if (user == null)
            {
                return new ConsoleErrorResultModel(string.Format("No user found with the ID of '{0}'", UserId));
            }
            else
            {
                UserController.ResetPasswordToken(user, sendEmail);
            }

            return new ConsoleResultModel("User password has been reset." + ((bool)Notify ? " An email has been sent to the user" : ""));
        }

    }
}
﻿using ScsAuditLog;
using ScsAuditLog.Model;
using ScsAuditLog.Pipelines;
using Sitecore.Diagnostics;
using Sitecore.Publishing.Pipelines.Publish;
using Sitecore.Security.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;

namespace ScsAuditLog.Pipelines.Publish
{
	public class AuditPublish : PublishProcessor
	{
		public override void Process(PublishContext context)
		{
			try
			{
				StringBuilder sb = new StringBuilder($"<tr><td>Created</td><td>{context.Statistics.Created}</td></tr>");
				sb.Append($"<tr><td>Deleted</td><td>{context.Statistics.Deleted}</td></tr>");
				sb.Append($"<tr><td>Skipped</td><td>{context.Statistics.Skipped}</td></tr>");
				sb.Append($"<tr><td>Updated</td><td>{context.Statistics.Updated}</td></tr>");
				string statistics = $"<table><th>Type</th><th>Items Processed</th>{sb}</table>";
				string publishInfo =
					$"<table><th>Source</th><th>Target</th><tr><td>{context.PublishOptions.SourceDatabase.Name}</td><td>{context.PublishOptions.TargetDatabase.Name}</td></tr></table>";
                if (context.PublishOptions.RootItem != null)
                    AuditLogger.Current.Log(context.PublishOptions.RootItem, "5", $"{publishInfo}{statistics}");
                else
                    AuditLogger.Current.Log(new ItemAuditEntry("5", "", "")
                    {
                        Database = context.PublishOptions.SourceDatabase.Name,
                        Note = $"{publishInfo}{statistics}",
                        User = context.User.Name,
                        Role = context.User.Roles.Select(x => x.Name).ToList(),
                        Id = ID.Null,
                        TimeStamp = DateTime.Now,
                        Path = context.PublishOptions.RootItem != null ? context.PublishOptions.RootItem.Paths.FullPath : "full site",
                        Language = context.PublishOptions.Language.Name
					});
			}
			catch (Exception ex)
			{
				Log.Error("problem auditing publish", ex, this);
			}
		}
	}
}

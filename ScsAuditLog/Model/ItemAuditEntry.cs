﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScsAuditLog.Model.Interface;
using ScsAuditLog.Pipelines;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace ScsAuditLog.Model
{
	public class ItemAuditEntry : IAuditEntry
	{
		public string Uid { get; set; }
		public string EventId { get; set; }
		public string User { get; set; }
		public List<string> Role { get; set; }
		public ID Id { get; set; }
		public string Database { get; set; }
		public string Path { get; set; }
		public DateTime TimeStamp { get; set; }
		public string Note { get; set; } = "";
		public List<string> Tokens { get; set; }
		public string Label { get; set; }
		public string Color { get; set; }
        public string Language { get; set; }

        public ItemAuditEntry()
		{
		}

		public ItemAuditEntry(string eventId, string label, string color)
		{
			Id = ID.Null;
			EventId = eventId;
			Label = label;
			Color = color;
		}

		public ItemAuditEntry(string eventId, string label, string color, Item item)
		{
			EventId = eventId;
			User = Context.GetUserName();
			Role = Context.User.Roles.Select(x => x.Name).ToList();
			if (item != null)
			{
				Id = item.ID;
				Path = item.Paths.FullPath;
				Database = item.Database.Name;
			}
			TimeStamp = DateTime.Now;
			Label = label;
			Color = color;
            Language = item.Language.Name;
        }
        public void FromLine(string line)
		{
			try
			{
				string[] entries = line.Split(new string[] {"|"}, StringSplitOptions.None);
				if (entries.Length > 0)
					Uid = entries[0];
				if (entries.Length > 1)
					User = entries[1];
				if (entries.Length > 2)
					Role =
						entries[2].Split(new string[] {","}, StringSplitOptions.None).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
				if (entries.Length > 3)
					TimeStamp = DateUtil.IsoDateToDateTime(entries[3]);
				if (entries.Length > 4)
					EventId = entries[4];
				if (entries.Length > 5)
					Id = new ID(entries[5]);
				if (entries.Length > 6)
					Note = entries[6];
				if (entries.Length > 7)
					Path = entries[7];
				if (entries.Length > 8)
					Database = entries[8];
                if (entries.Length > 9)
                    Language = entries[9];
            }
            catch (Exception e)
			{
				Log.Error("Problem reading entry from string\n" + line, e, this);
			}
		}
		public string Collapse()
		{
			StringBuilder sb = new StringBuilder();
			try
			{
				sb.Append(Uid).Append("|");
				sb.Append(User).Append("|");
				if (Role.Count > 0)
					sb.Append(Role.Aggregate((roles, x) => roles + x + ",")).Remove(sb.Length - 1, 1).Append("|");
				else
					sb.Append("|");
				sb.Append(DateUtil.ToIsoDate(TimeStamp)).Append("|");
				sb.Append(EventId).Append("|");
				sb.Append(Id.ToShortID()).Append("|");
				sb.Append(Note).Append("|");
				sb.Append(Path).Append("|");
                sb.Append(Database).Append("|");
                sb.Append(Language);

            }
            catch (Exception e)
			{
				Log.Error("problem collapsing entry", e, this);
			}
			return sb.ToString();
		}

		public override int GetHashCode()
		{
			return Uid.GetHashCode();
		}
	}
}
